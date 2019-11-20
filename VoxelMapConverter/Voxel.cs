using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Voxel
    {
        public int x, y, z;
        public int red, green, blue;

        public Voxel(int x, int y, int z, int red, int green, int blue)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public Voxel(int x, int y, int z, Block block)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.red = block.red;
            this.green = block.green;
            this.blue = block.blue;
        }

        public (int, int, int) getColorTuple()
        {
            return (red, green, blue);
        }
    }
}
