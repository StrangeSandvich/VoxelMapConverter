using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class ModelLeaf : IModelTreeComponent
    {
        public int ModelID;
        public int sizeX, sizeY, sizeZ;
        public byte[,,] data;

        public ModelLeaf(int modelID, int sizeX, int sizeY, int sizeZ, byte[,,] data)
        {
            ModelID = modelID;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;
            this.data = data;
        }
    }
}
