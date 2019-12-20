using System;
using System.IO;

namespace VoxelMapConverter
{
    class Program
    {
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
            byte[] AoSMapData;
            Console.WriteLine("Welcome to the Ace of Spades to MagicaVoxel map conversion tool by Dodo");
            Console.WriteLine("Please give me the name of the Ace of Spades .vxl map file (Excluding the .vxl):");
            string mapinput;
            while (true) 
            { 
                try
                {
                    mapinput = Console.ReadLine();
                    AoSMapData = File.ReadAllBytes(mapinput + ".vxl");
                    break;
                } catch (Exception)
                {
                    Console.WriteLine("I couldn't read that. Try again. Make sure the map file is placed in the same folder as the tool program:");
                }
            }
            Console.WriteLine("Read file. Sadly the map is formatted in a way that makes it impossible to tell the height.");
            Console.WriteLine("However I can find the lowest surface block. This is likely the ocean which you would want to cut away.");
            Console.WriteLine("But if the ocean isn't exposed on you map, you would want to keep the lowest block. Keep lowest block? (y/n)");
            bool keepOcean = ReadBool();
            Console.WriteLine("Alright, here I go parsing the Ace of Spades map. This might take a few seconds...");
            IntermediateMap map = AceOfSpadesToIM.ToIntermediateMap(AoSMapData, keepOcean);
            int colorCount = map.palette.palette.Count; //Should really have internal functions to do this but eh. 
            Console.WriteLine("Parsing went well. Map height detected to be " + map.sizeZ + " with " + colorCount + " unique colors");
            int maxcolors = 254 - Palette.reservedIDs.Count;
            Console.WriteLine("How many colors should the map have? Maximum different colors is " + maxcolors + ". Enter nothing for maximum colors.");
            int colorInput = ReadInt(1, maxcolors);
            Console.WriteLine("Condensing colors. This might take a second...");
            map.PaletteShrink(Math.Min(colorInput, colorCount)); //Use shrink even if at acceptable number of colors to shuffle indexes away from reserved spots
            Console.WriteLine("Shrunk colors.");
            Console.WriteLine("Now I just need to write it to a vox file.");
            Console.WriteLine("What do you want me to call the output file (Excluding the .vox). Input nothing place the map ready to load in appdata:");
            string outname = Console.ReadLine();
            if (outname == "")
            {
                outname = VercidiumDirectory + "\\map";
            }
            Console.WriteLine("Alright, I'll try write the vox file with that. Might take a couple of seconds...");
            IMToVXL.IMToVoxel(map, outname + ".vox");
            Console.WriteLine("There we are. There should now be a nice " + outname + ".vox with your map :)");
            Console.WriteLine("Want me to also make a " + outname + ".txt template with dimensions and things? (y/n)");
            bool writeTxt = ReadBool();
            if (writeTxt)
            {
                CreateSupportTxt.CreateSupportingTxt(map, outname + ".txt");
                Console.WriteLine("Should also have a " + outname + ".txt now.");
            }
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
