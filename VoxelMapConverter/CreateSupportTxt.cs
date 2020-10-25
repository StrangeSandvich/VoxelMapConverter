using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class CreateSupportTxt
    {
        //Deprecated, Vercidium now uses a json file for SE specific map data
        public static void CreateSupportingTxt(IntermediateMap map, string outfileName)
        {
            List<string> lines = new List<string>();

            int numberOfIndexes = map.palette.palette.Count;
            int numberOfTextures = numberOfIndexes - map.palette.fillerIndexes.Count;
            int numberOfSFX = numberOfTextures - Palette.reservedIDs.Count;

            lines.Add("# Map width, length and height dimensions");
            lines.Add(StretchSize(map.sizeX).ToString());
            lines.Add(StretchSize(map.sizeY).ToString());
            lines.Add(StretchSize(map.sizeZ).ToString());
            lines.Add("");
            lines.Add("# Skybox type (Space/AD/CF/MC/ST)");
            lines.Add("ST");
            lines.Add("");
            lines.Add("# Water level and RGB colour");
            lines.Add("0.5");
            lines.Add(map.oceanColor.ToString());
            lines.Add("");
            lines.Add("# RGB fog colour");
            lines.Add("135 160 174");
            lines.Add("");
            lines.Add("# RGB Sun colour");
            lines.Add("203 214 221");
            lines.Add("");
            lines.Add("# Number of textures");
            lines.Add(numberOfTextures.ToString());
            lines.Add("");
            lines.Add("# BlockIndex, TextureName, Width/Height Dimensions, Reflectivity (/255)");
            for(int i = 1; i <= numberOfIndexes; i++)
            {
                if (Palette.reservedIDs.Contains(i-1)){
                    lines.Add(i.ToString() + " cfhazard 2 2 200");
                } else if (!map.palette.fillerIndexes.Contains(i-1))
                {
                    lines.Add(i.ToString() + " metal 4 4 200");
                } //Don't add anything for empty indexes
            }
            lines.Add("");
            lines.Add("");
            lines.Add("# Default FootstepSound");
            lines.Add("FootstepMetal");
            lines.Add("");
            lines.Add("# Number of FootstepSounds");
            lines.Add(numberOfSFX.ToString());
            lines.Add("");
            lines.Add("# BlockIndex, FootstepSound");
            List<int> ignorables = new List<int>(Palette.reservedIDs);
            ignorables.AddRange(map.palette.fillerIndexes);
            for (int i = 1; i <= numberOfIndexes; i++)
            {
                if (!ignorables.Contains(i-1))
                {
                    lines.Add(i.ToString() + " FootstepDirt");
                }
            }
            lines.Add("");
            lines.Add("# Default BlockDestroySound");
            lines.Add("DigMetal");
            lines.Add("");
            lines.Add("# Number of block destroy sounds");
            lines.Add(numberOfSFX.ToString());
            lines.Add("");
            lines.Add("# BlockIndex, BlockDestroySounds");
            for (int i = 1; i <= numberOfIndexes; i++)
            {
                if (!ignorables.Contains(i-1))
                {
                    lines.Add(i.ToString() + " DigRock");
                }
            }
            File.WriteAllLines(outfileName, lines);
        }

        public static int StretchSize(int size)
        {
            if(size % 32 == 0)
            {
                return size;
            }
            return size + 32 - (size % 32);
        }
    }
}
