using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelMapConverter
{
    class IntChunkData : IChunkData
    {
        int data;
        
        public IntChunkData(int data)
        {
            this.data = data;
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(data);
        }

        public int ByteSize()
        {
            return 4;
        }
    }
}
