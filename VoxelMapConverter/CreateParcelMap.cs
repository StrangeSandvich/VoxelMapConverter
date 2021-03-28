using CsharpVoxReader;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class CreateParcelMap
    {
        public static List<string> emptySourceFileNames = new List<string>(new string[] { "None", "", "Empty", "none", "empty", "null"});
        public static IntermediateMap CreateMap(ParcelJsonConfiguration configuration)
        {
            IntermediateMap emptyMap = new IntermediateMap(configuration.primaryParcel.sizeX, configuration.primaryParcel.sizeY, configuration.primaryParcel.sizeZ, Block.AIR, new Palette(0));

            List<Parcel> loadedParcels = createDivisionParcels(configuration.plotDivisions);
            loadedParcels.AddRange(configuration.insertParcels);

            FillParcel(emptyMap, configuration.primaryParcel, loadedParcels, 0, 0, 0, false, configuration);
            return emptyMap;
        }

        static void FillParcel(IntermediateMap map, Parcel parcel, List<Parcel> loadedParcels, int offsetX, int offsetY, int offsetZ, bool hasDonePalette, ParcelJsonConfiguration configuration)
        {
            //If no sourcefile, skip, well, sourcefile! and just plop in plots
            if (!emptySourceFileNames.Contains(parcel.sourceFile))
            {
                ParcelVoxReader parcelReader = new ParcelVoxReader();
                VoxReader r = new VoxReader(parcel.sourceFile, parcelReader);
                r.Read();

                parcelReader.insertReadParcel(map, offsetX, offsetY, offsetZ);

                //Do the palette
                if (!hasDonePalette)
                {
                    hasDonePalette = loadPalette(map, parcelReader.palette);
                }
            }

            foreach(Plot plot in parcel.parcelPlots)
            {
                Parcel insertParcel = findMatchingParcel(plot, parcel.parcelPlots, loadedParcels, configuration);

                FillParcel(map, insertParcel, loadedParcels, offsetX + plot.offsetX, offsetY + plot.offsetY, offsetZ + plot.offsetZ, hasDonePalette, configuration);
            }
        }

        static Parcel findMatchingParcel(Plot plot, List<Plot> otherPlots, List<Parcel> parcels, ParcelJsonConfiguration configuration)
        {
            List<string> connectionsNamesMatch = new List<string>();
            for(int i = 0; i < 6; i++)
            {
                string connectionName = plot.connections.connectionNames[i];
                if (configuration.connections.Exists(candidate => candidate.name == connectionName)){
                    connectionsNamesMatch.Add(connectionName);
                } else
                {
                    Plot adjacentPlot = otherPlots.Find(candidate => candidate.plotID == connectionName);
                    if(adjacentPlot == null)
                    {
                        Console.WriteLine("Plot had connection that matched no connection name or plot ID: " + connectionName);
                        throw new Exception("Plot had connection that matched no connection name or plot ID: " + connectionName);
                    } else
                    {
                        int matchingDirection;
                        if(i % 2 == 0)
                        {
                            matchingDirection = i + 1;
                        } else
                        {
                            matchingDirection = i - 1;
                        }
                        string matchingName = adjacentPlot.connections.connectionNames[matchingDirection];
                        if(matchingName == plot.plotID)
                        {
                            connectionsNamesMatch.Add("");
                        } else
                        {
                            connectionsNamesMatch.Add(matchingName);
                        }
                    }
                }
            }
            Connections matchingConnections = new Connections(connectionsNamesMatch);

            List<Parcel> candidates = parcels.FindAll(candidate => matchingConnections.matchesParcel(candidate.connections)
                    && candidate.sizeX == plot.sizeX && candidate.sizeY == plot.sizeY && candidate.sizeZ == plot.sizeZ);

            if (candidates.Count == 0)
            {
                Console.WriteLine("Found no parcel matching plot size X" + plot.sizeX + "Y" + plot.sizeY + "Z" + plot.sizeZ +
                    " Connections " + plot.connections.ToString());
                throw new Exception("No matching parcel!");
            }

            var random = new Random();
            int index = random.Next(candidates.Count);
            Parcel chosenParcel= candidates[index];

            plot.connections.addParcelConnections(chosenParcel.connections);
            return chosenParcel;
        }

        static List<Parcel> createDivisionParcels(List<PlotDivision> plotDivisions)
        {
            List<Parcel> result = new List<Parcel>();
            foreach(PlotDivision plotDivision in plotDivisions)
            {
                foreach(int divisionX in plotDivision.divisionsX)
                {
                    if(plotDivision.sizeX % divisionX != 0)
                    {
                        throw new ArgumentException("Division " + divisionX + " does not match size " + plotDivision.sizeX);
                    }
                    foreach (int divisionY in plotDivision.divisionsX)
                    {
                        if (plotDivision.sizeY % divisionY != 0)
                        {
                            throw new ArgumentException("Division " + divisionY + " does not match size " + plotDivision.sizeY);
                        }
                        foreach (int divisionZ in plotDivision.divisionsZ)
                        {
                            if (plotDivision.sizeZ % divisionZ != 0)
                            {
                                throw new ArgumentException("Division " + divisionZ + " does not match size " + plotDivision.sizeZ);
                            }

                            result.Add(createDivisionParcel(plotDivision, divisionX, divisionY, divisionZ));
                        }
                    }
                }
            }
            return result;
        }

        static Parcel createDivisionParcel(PlotDivision division, int divisionX, int divisionY, int divisionZ)
        {
            List<Plot> plots = new List<Plot>();
            string nameBase = "PlotdX" + divisionX + "dY" + divisionY + "dz" + divisionZ;

            for(int x = 0; x < division.sizeX; x += divisionX)
            {
                for (int y = 0; y < division.sizeY ; y += divisionY)
                {
                    for (int z = 0; z < division.sizeZ ; z += divisionZ)
                    {
                        List<string> connectionNames = new List<string>();
                        string plotname = nameBase + "x" + x + "y" + y + "z" + z;
                        if (x == division.sizeX - divisionX)
                        {
                            connectionNames.Add(division.connections.connectionNames[0]);
                        } else
                        {
                            connectionNames.Add(nameBase + "x" + (x+divisionX) + "y" + y + "z" + z);
                        }
                        if (x == 0)
                        {
                            connectionNames.Add(division.connections.connectionNames[1]);
                        }
                        else
                        {
                            connectionNames.Add(nameBase + "x" + (x-divisionX) + "y" + y + "z" + z);
                        }
                        if (y == division.sizeY - divisionY)
                        {
                            connectionNames.Add(division.connections.connectionNames[2]);
                        }
                        else
                        {
                            connectionNames.Add(nameBase + "x" + x + "y" + (y+divisionY) + "z" + z);
                        }
                        if (y == 0)
                        {
                            connectionNames.Add(division.connections.connectionNames[3]);
                        }
                        else
                        {
                            connectionNames.Add(nameBase + "x" + x + "y" + (y-divisionY) + "z" + z);
                        }
                        if (z == division.sizeZ - divisionZ)
                        {
                            connectionNames.Add(division.connections.connectionNames[4]);
                        }
                        else
                        {
                            connectionNames.Add(nameBase + "x" + x + "y" + y + "z" + (z+divisionZ));
                        }
                        if (z == 0)
                        {
                            connectionNames.Add(division.connections.connectionNames[5]);
                        }
                        else
                        {
                            connectionNames.Add(nameBase + "x" + x + "y" + y + "z" + (z-divisionZ));
                        }
                        Connections plotConnections = new Connections(connectionNames);
                        plots.Add(new Plot(plotname, divisionX, divisionY, divisionZ, x, y, z, plotConnections));
                    }
                }
            }
            Parcel result = new Parcel(division.sizeX, division.sizeY, division.sizeZ, "", division.connections , plots);
            return result;
        }

        static bool loadPalette(IntermediateMap map, byte[,] paletteData)
        {
            try
            {
                Palette palette = new Palette(paletteData);
                map.palette = palette;
                return true;
            } catch(Exception ignored)
            {
                Console.WriteLine("Exception loading map palette");
            }
            return false;
        }
        
    }
}
