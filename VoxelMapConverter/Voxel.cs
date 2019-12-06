using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Voxel
    {
        public int x, y, z;
        public int colorIndex;

        public Voxel(int x, int y, int z, int colorIndex)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.colorIndex = colorIndex;
        }

        public Voxel(int x, int y, int z, Block block)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.colorIndex = block.colorIndex;
        }
    }
}
