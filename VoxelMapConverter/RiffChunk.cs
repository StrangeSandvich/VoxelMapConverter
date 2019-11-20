using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class RiffChunk
    {
        public string id;
        public List<IChunkData> dataList = new List<IChunkData>();

        public RiffChunk(string ID)
        {
            if(ID.Length != 4)
            {
                throw new ArgumentException("A riff chunk ID must be 4 chars");
            }
            id = ID;
        }

        public void addData(IChunkData data)
        {
            dataList.Add(data);
        }

        public int getChunkContentSize()
        {
            int res = 0;
            foreach (IChunkData chunkData in dataList)
            {
                res += chunkData.ByteSize();
            }
            return res;
        }

        public int getChunkSize()
        {
            return getChunkContentSize() + 12; //ID, content size and child size fields;
        }

        public void writeChunk(BinaryWriter writer)
        {
            writer.Write(id.ToCharArray());
            writer.Write(getChunkContentSize());
            writer.Write(0);
            foreach (IChunkData chunkData in dataList)
            {
                chunkData.WriteData(writer);
            }
        }
    }
}
