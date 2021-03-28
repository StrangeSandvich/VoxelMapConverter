using Newtonsoft.Json;
using System;
using System.IO;

namespace VoxelMapConverter
{
    class Program
    {
        public const string PARCEL_FUNCTION_NAME = "parcel";
        public const string CONVERTER_FUNCTION_NAME = "aosConverter";
        const string DEFAULT_CONFIG = "mapMakerConfig";
        public const int maxColours = 230;

        public static string VercidiumDirectory
        {
            get
            {
                var s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vercidium");
                Directory.CreateDirectory(s);
                return s;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Dodo's map making tool");
            string function = DEFAULT_CONFIG;
            ProgramConfiguration programConfig = null;
            while (!MatchesFunction(function)){
                try
                {
                    string programConfigText = File.ReadAllText(function + ".json");
                    programConfig = JsonConvert.DeserializeObject<ProgramConfiguration>(programConfigText);
                    function = programConfig.function;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not read " + function + ".json, please specify a program config or function");
                    function = Console.ReadLine();
                }
            }

            if(programConfig == null)
            {
                programConfig = new ProgramConfiguration();
                programConfig.function = function;
                if(function == PARCEL_FUNCTION_NAME)
                {
                    Console.WriteLine("Give me the name of the configuration JSON file (without the .json):");
                    programConfig.parcelConfiguration = Console.ReadLine();
                } else if(function == CONVERTER_FUNCTION_NAME)
                {
                    Console.WriteLine("Please give me the name of the Ace of Spades .vxl map file (Excluding the .vxl):");
                    programConfig.aosFile = Console.ReadLine();

                    Console.WriteLine("The map is formatted in a way that makes it impossible to tell the height.");
                    Console.WriteLine("However I can find the lowest surface block. This is likely the ocean which you would want to cut away.");
                    Console.WriteLine("But if the ocean isn't exposed on you map, you would want to keep the lowest block. Keep lowest block? (y/n)");
                    programConfig.keepOcean = ReadBool();

                    Console.WriteLine("How many colors indexes should the map have? Maximum different colors is " + maxColours + ". Enter nothing for maximum colors.");
                    programConfig.colourCount = ReadInt(1, maxColours);
                }

                Console.WriteLine("Do you want the map mirrored on the y axis? (y/n)");
                programConfig.mirrorMap = ReadBool();

                Console.WriteLine("MagicaVoxel divides the map into models with a maximum size of 256. Can you please give me the X, Y and Z you want your models to be:");
                programConfig.modelSizeX = ReadInt(1, 256);
                programConfig.modelSizeY = ReadInt(1, 256);
                programConfig.modelSizeZ = ReadInt(1, 256);
                Console.WriteLine("What do you want me to call the output file (Excluding the .vox). Input nothing for a random name");
                programConfig.outputName = Console.ReadLine();
            }

            if (programConfig.Verify())
            {
                runWithConfig(programConfig);
            }

        }

        static bool MatchesFunction(string config)
        {
            return config == PARCEL_FUNCTION_NAME || config == CONVERTER_FUNCTION_NAME;
        }

        static void runWithConfig(ProgramConfiguration config)
        {
            if(config.function == PARCEL_FUNCTION_NAME)
            {
                ParcelProgram(config);
            } else if(config.function == CONVERTER_FUNCTION_NAME)
            {
                ConverterProgram(config);
            }
        }


        static void ParcelProgram(ProgramConfiguration programConfig)
        {
            Console.WriteLine("Running parcel program");
            ParcelJsonConfiguration configuration;
            try
            {
                string configFileContent = File.ReadAllText(programConfig.parcelConfiguration + ".json");
                configuration = JsonConvert.DeserializeObject<ParcelJsonConfiguration>(configFileContent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing the parcel config json. Maybe some of this text will help you find the issue: ");
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("Read parcel config");
            IntermediateMap map = CreateParcelMap.CreateMap(configuration);
            Console.WriteLine("Generated parcel map");
            
            PrintMapProgram(map, programConfig);
        }

        static void ConverterProgram(ProgramConfiguration programConfig)
        {
            byte[] AoSMapData;
            Console.WriteLine("Running Ace of Spades to MagicaVoxel map conversion tool");   
            try
            {
                AoSMapData = File.ReadAllBytes(programConfig.aosFile + ".vxl");

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to read AoS map file");
                return;
            }
            
            Console.WriteLine("Alright, here I go parsing the Ace of Spades map. This might take a few seconds...");
            IntermediateMap map = AceOfSpadesToIM.ToIntermediateMap(AoSMapData, programConfig.keepOcean);
            int colorCount = map.palette.palette.Count;
            Console.WriteLine("Parsing went well. Map height detected to be " + map.sizeZ + " with " + colorCount + " unique colors");
            Console.WriteLine("Condensing colors. This might take a second...");
            map.PaletteShrink(Math.Min(programConfig.colourCount, colorCount)); //Use shrink even if at acceptable number of colors to shuffle indexes away from reserved spots
            Console.WriteLine("Shrunk colors.");
            PrintMapProgram(map, programConfig);
        }

        static void PrintMapProgram(IntermediateMap map, ProgramConfiguration programConfig)
        {
            
            if (programConfig.mirrorMap)
            {
                map.mirrorOnY();
                Console.WriteLine("Mirrored map");
            }

            string outname = programConfig.outputName;
            if (outname == null || outname == "")
            {
                var random = new Random();
                int index = random.Next(int.MaxValue);
                outname = "DodoTool" + index;
            }
            Console.WriteLine("Writting map to file. Might take a couple of seconds...");
            IMToVXL.IMToVoxel(map, outname + ".vox", programConfig.modelSizeX, programConfig.modelSizeY, programConfig.modelSizeZ);
            Console.WriteLine("There we are. There should now be a nice " + outname + ".vox with your map :)");
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
        }

        static int ReadInt(int min, int max)
        {
            while(true)
            {
                try
                {
                    string input = Console.ReadLine();
                    if(input == "")
                    {
                        return max;
                    }
                    int res = int.Parse(input);
                    if(res > max)
                    {
                        Console.WriteLine("That number is too high. It must at most be " + max + ". Try again");
                    }
                    else if (res < min) {
                        Console.WriteLine("That number is too small. It must at least be " + min + ". Try again");
                    }
                    else
                    {
                        return res;
                    }
                } catch (Exception)
                {
                    Console.WriteLine("That did not seem like a number. Try again:");
                }
            }
        }

        static int ReadInt()
        {
            while (true)
            {
                try
                {
                    return int.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.WriteLine("That did not seem like a number. Try again:");
                }
            }
        }

        static bool ReadBool()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if(input == "y")
                {
                    return true;
                } else if (input == "n" || input == "")
                {
                    return false;
                }//Else
                Console.WriteLine("That did not seem like a y or n. Try again:");
            }
        }
    }
}
