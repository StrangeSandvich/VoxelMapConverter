using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelMapConverter
{
    class ByteChunkData : IChunkData
    {
        byte data;
        
        public ByteChunkData(int data)
        {
            this.data = Convert.ToByte(data);
        }

        public ByteChunkData(byte data)
        {
            this.data = data;
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(data);
        }

        public int ByteSize()
        {
            return 1;
        }
    }
}
