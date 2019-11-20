using System;

namespace VoxelMapConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input AoS map file name:");
            string input = Console.ReadLine();
            if(input == "l")
            {
                input = "Limeville.vxl";
            }
            try
            {
                IntermediateMap map = AceOfSpadesToIMUsingKarp.ToIntermediateMap(input);
                IMToVXL.IMToVoxel(map);
            } catch (Exception e)
            {
                Console.WriteLine("Error. Are you sure that was the right file?");
                Console.WriteLine("If you are, tell Dodo to fix his stuff because you got an Exception:");
                Console.WriteLine(e.ToString());
            }
        }
    }
}
