using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    interface IChunkData
    {
        public void WriteData(BinaryWriter writer);
        public int ByteSize();
    }
}
