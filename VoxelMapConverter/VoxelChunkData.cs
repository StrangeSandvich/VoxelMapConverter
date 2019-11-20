using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelMapConverter
{
    class VoxelChunkData : IChunkData
    {
        byte x, y, z, colorIndex;
        
        public VoxelChunkData(byte x, byte y, byte z, byte colorIndex)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.colorIndex = colorIndex;
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
            writer.Write(colorIndex);
        }

        public int ByteSize()
        {
            return 4;
        }
    }
}
