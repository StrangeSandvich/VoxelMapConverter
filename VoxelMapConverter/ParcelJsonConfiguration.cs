using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class ParcelJsonConfiguration
    {
        public Parcel primaryParcel;
        public List<Parcel> insertParcels;
        public List<ConnectionConfiguration> connections;
        public List<PlotDivision> plotDivisions;
    }
}
