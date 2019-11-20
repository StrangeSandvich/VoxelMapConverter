using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelMapConverter
{
    class TextChunkData : IChunkData
    {
        string data;
        
        public TextChunkData(string data)
        {
            this.data = data;
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(data.ToCharArray());
        }

        public int ByteSize()
        {
            return data.Length;
        }
    }
}
