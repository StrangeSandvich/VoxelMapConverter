using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    interface IChunkData
    {
        void WriteData(BinaryWriter writer);
        int ByteSize();
    }
}
