using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class ModelTree : IModelTreeComponent
    {
        public int offsetX, offsetY, offsetZ;
        //Maybe add some rotation, LOL

        public int chunkID;
        public List<int> childChunks;
        public List<int> childModels;

        public List<IModelTreeComponent> components;

        public ModelTree(int chunkID)
        {
            this.chunkID = chunkID;
            offsetX = 0;
            offsetY = 0;
            offsetZ = 0;
            childChunks = new List<int>();
            childModels = new List<int>();
            components = new List<IModelTreeComponent>();
        }
    }
}
