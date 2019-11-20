using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelMapConverter
{
    class DictChunkData : IChunkData
    {
        List<(string, string)> dictionary;
        
        public DictChunkData(List<(string, string)> dictionary)
        {
            this.dictionary = dictionary;
        }

        public DictChunkData() //Empty
        {
            dictionary = new List<(string, string)>();
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach((string, string) pair in dictionary)
            {
                writer.Write(pair.Item1.Length);
                writer.Write(pair.Item1.ToCharArray());
                writer.Write(pair.Item2.Length);
                writer.Write(pair.Item2.ToCharArray());
            }
        }

        public int ByteSize()
        {
            int result = 4;//Count field
            foreach((string, string) pair in dictionary)
            {
                result += 8; //String size fields
                result += pair.Item1.Length;
                result += pair.Item2.Length;
            }
            return result;
        }
    }
}
