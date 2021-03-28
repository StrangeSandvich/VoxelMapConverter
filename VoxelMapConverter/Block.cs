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
        public int ColorIndex { get; set; }
        public int ID { get;}

        public Block(int ID, Palette palette)
        {
            this.ID = ID;
            if(ID == AIR)
            {
                ColorIndex = -1;
            } else
            {
                ColorIndex = Palette.SECTORS_EDGE_RESERVED_INDEXES;
            }
        }

        public Block(Palette palette, RGBColor color)
        {
            this.ID = UNASSIGNED;
            ColorIndex = palette.getExactColorIndex(color);
        }

        public Block(int colorIndex)
        {
            this.ID = UNASSIGNED;
            this.ColorIndex = colorIndex;
        }
    }
}
