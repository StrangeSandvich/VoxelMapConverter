using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Parcel
    {
        public int sizeX, sizeY, sizeZ;
        public string sourceFile;
        public Connections connections;

        public List<Plot> parcelPlots;

        public Parcel(int sizeX, int sizeY, int sizeZ, string sourceFile, Connections connections, List<Plot> parcelPlots)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;
            this.sourceFile = sourceFile;
            this.connections = connections;
            this.parcelPlots = parcelPlots;
        }
    }
}
