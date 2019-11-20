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
        public int red { get; set; }
        public int green { get; set; }
        public int blue { get; set; }
        public int ID { get; set; }

        public Block(int red, int green, int blue)
        {
            ID = UNASSIGNED;
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public Block(int ID)
        {
            this.ID = ID;
            red = 0;
            green = 0;
            blue = 0;
        }
    }
}
