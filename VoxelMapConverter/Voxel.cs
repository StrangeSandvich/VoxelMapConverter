using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Voxel
    {
        public int x, y, z;
        public RGBColor color { get; set; }

        public Voxel(int x, int y, int z, RGBColor color)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.color = color;
        }

        public Voxel(int x, int y, int z, Block block)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.color = block.color;
        }
    }
}
