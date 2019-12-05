﻿using System;
using System.IO;

namespace VoxelMapConverter
{
    class Program
    {
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
            Console.WriteLine("Parsing went well! Now I just need to write it to a vox file.");
            Console.WriteLine("What do you want me to call the output file (Excluding the .vox). Input nothing to use the name of the input map:");
            string outname = Console.ReadLine();
            if (outname == "")
            {
                outname = mapinput;
            }
            Console.WriteLine("However, a vox file can only have 255 different colours and an AoS map can easily have more. Therefore I will likely need to combine some colors.");
            Console.WriteLine("Please input the level of color approximation I should go for. 0 will make a new color for every AoS color, but if I run out I will just make all remaining colors black.");
            Console.WriteLine("I'll warn you if I run out though. Give me a number between 0 and 41:");
            int colorApprox = ReadInt();
            Console.WriteLine("Alright, I'll try write the vox file with that. Might take a couple of seconds...");
            IMToVXL.IMToVoxel(map, colorApprox, outname + ".vox");
            Console.WriteLine("There we are. There should now be a nice " + outname + ".vox with your map :)");
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
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
                } else if (input == "n")
                {
                    return false;
                }//Else
                Console.WriteLine("That did not seem like a y or n. Try again:");
            }
        }
    }
}
