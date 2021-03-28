using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class ProgramConfiguration
    {
        public string function;

        //Parcel input
        public string parcelConfiguration;

        //Converter input
        public string aosFile;
        public bool keepOcean;
        public int colourCount;


        //Printer input
        public bool mirrorMap;

        public int modelSizeX;
        public int modelSizeY;
        public int modelSizeZ;

        public string outputName;

        public bool Verify()
        {
            if(function == Program.PARCEL_FUNCTION_NAME)
            {
                
            } else if(function == Program.CONVERTER_FUNCTION_NAME)
            {
                if(colourCount < 1 ||colourCount > Program.maxColours)
                {
                    Console.WriteLine("colourCount is outside of bounds");
                    return false;
                }

            } else
            {
                //Function is pointing at another config
                return true;
            }
            if(modelSizeX < 1 || modelSizeX > 256)
            {
                Console.WriteLine("Set modelSizeX to 256");
                modelSizeX = 256;
            }
            if (modelSizeY < 1 || modelSizeY > 256)
            {
                Console.WriteLine("Set modelSizeY to 256");
                modelSizeY = 256;
            }
            if (modelSizeZ < 1 || modelSizeZ > 256)
            {
                Console.WriteLine("Set modelSizeZ to 256");
                modelSizeZ = 256;
            }
            return true;
        }
    }
}
