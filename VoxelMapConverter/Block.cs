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
        public int colorIndex { get; set; }
        public int ID { get;}

        public Block(int ID, Palette palette)
        {
            this.ID = ID;
            if(ID == AIR)
            {
                colorIndex = -1;
            } else
            {
                colorIndex = 0;
            }
        }

        public Block(Palette palette, RGBColor color)
        {
            this.ID = UNASSIGNED;
            colorIndex = palette.getExactColorIndex(color);
        }
    }
}
