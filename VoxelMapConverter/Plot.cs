using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Plot
    {
        public string plotID;

        public int sizeX, sizeY, sizeZ;
        public int offsetX, offsetY, offsetZ;

        public Connections connections;

        public Plot(string plotID, int sizeX, int sizeY, int sizeZ, int offsetX, int offsetY, int offsetZ, Connections connections)
        {
            this.plotID = plotID;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.offsetZ = offsetZ;
            this.connections = connections;
        }
    }
}
