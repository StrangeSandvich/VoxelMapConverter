using System;
using System.IO;

namespace VoxelMapConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] AoSMapData;
            int aoSMapHeight;
            Console.WriteLine("Welcome to the Ace of Spades to MagicaVoxel map conversion tool by Dodo");
            Console.WriteLine("Please give me the name of the Ace of Spades .vxl map file (Excluding the .vxl):");      
            while (true) 
            { 
                try
                {
                    string mapinput = Console.ReadLine() + ".vxl";
                    AoSMapData = File.ReadAllBytes(mapinput);
                    break;
                } catch (Exception e)
                {
                    Console.WriteLine("I couldn't read that. Try again. Make sure the map file is placed in the same folder as the tool program:");
                }
            }
            Console.WriteLine("Read file. Sadly the map is formatted in a way that makes it impossible to tell the height.");
            Console.WriteLine("Can you please give me the height of the map? For beta AoS it should be 64 and for Jagex AoS it could be up to 256");
            Console.WriteLine("If you overshoot, there will just be some extra ground layers. If you undershoot, I will have a crash:");
            while (true)
            {
                try
                {
                    aoSMapHeight = int.Parse(Console.ReadLine());
                    break;
                } catch (Exception e)
                {
                    Console.WriteLine("That did not seem like a number. Try again:");
                }
            }
            Console.WriteLine("Alright, here I go parsing the Ace of Spades map. This might take a few seconds...");
            IntermediateMap map = AceOfSpadesToIM.ToIntermediateMap(AoSMapData, aoSMapHeight);
            Console.WriteLine("Parsing went well! Now I just need to write it to a vox file.");
            Console.WriteLine("However, a vox file can only have 255 different colours and an AoS map can easily have more. Therefore I will likely need to combine some colors.");
            Console.WriteLine("Please input the level of color approximation I should go for. 0 will make a new color for every AoS color, but if I run out I will just make all remaining colors black.");
            Console.WriteLine("I'll warn you if I run out though. Give me a number between 0 and 16:");
            int colorApprox;
            while (true)
            {
                try
                {
                    colorApprox = int.Parse(Console.ReadLine());
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("That did not seem like a number. Try again:");
                }
            }
            Console.WriteLine("Alright, I'll try write the vox file with that. Might take a couple of seconds...");
            IMToVXL.IMToVoxel(map, colorApprox);
            Console.WriteLine("There we are. There should now be a nice output.vox with your map :)");
            Console.WriteLine("Press enter to close");
            Console.ReadLine();
        }
    }
}
