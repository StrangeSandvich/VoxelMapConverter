using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Block
    {
        public const int UNASSIGNED = 0;
        public const int AIR = -1;
        public const int AOSSOLID = -2;
        public RGBColor color { get; set; }
        public int ID { get; set; }

        public Block(RGBColor color)
        {
            ID = UNASSIGNED;
            this.color = color;
        }

        public Block(int ID)
        {
            this.ID = ID;
            this.color = new RGBColor(0, 0, 0);
        }

        public Block(int ID, RGBColor color)
        {
            this.ID = ID;
            this.color = color;
        }
    }
}
