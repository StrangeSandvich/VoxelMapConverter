using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class Connections
    {
        public List<string> connectionNames;
        public int[,] rotationMatrix;
        //PX, MX, PY, MY, PZ, MZ
        // 1 0 0
        // 0 1 0
        // 0 0 1

        //Flip X
        //MX, PX, PY, MY, PZ, MZ
        //-1 0 0
        // 0 1 0
        // 0 0 1

        public Connections(List<string> connectionNames)
        {
            this.connectionNames = connectionNames;
            this.rotationMatrix = (int[,]) standardMatrix.Clone();
        }

        static int[,] standardMatrix = new int[3, 3] {
            { 1, 0, 0},
            { 0, 1, 0},
            { 0, 0, 1}
        };

        static int[,] flipXMatrix = new int[3, 3] {
            {-1, 0, 0},
            { 0, 1, 0},
            { 0, 0, 1}
        };

        static int[,] flipYMatrix = new int[3, 3] {
            { 1, 0, 0},
            { 0, -1, 0},
            { 0, 0, 1}
        };

        static int[,] rotateZMatrix = new int[3, 3] {
            { 0, -1, 0},
            { 1, 0, 0},
            { 0, 0, 1}
        };

        void flipX()
        {
            List<string> newConnectionNames = new List<string>();
            newConnectionNames.Add(connectionNames[1]);
            newConnectionNames.Add(connectionNames[0]);
            newConnectionNames.Add(connectionNames[2]);
            newConnectionNames.Add(connectionNames[3]);
            newConnectionNames.Add(connectionNames[4]);
            newConnectionNames.Add(connectionNames[5]);
            connectionNames = newConnectionNames;

            combineMatrixes(flipXMatrix);   
        }

        //Flip Y
        //PX, MX, MY, PY, PZ, MZ
        // 1 0 0
        // 0 -1 0
        // 0 0 1
        void flipY()
        {
            List<string> newConnectionNames = new List<string>();
            newConnectionNames.Add(connectionNames[0]);
            newConnectionNames.Add(connectionNames[1]);
            newConnectionNames.Add(connectionNames[3]);
            newConnectionNames.Add(connectionNames[2]);
            newConnectionNames.Add(connectionNames[4]);
            newConnectionNames.Add(connectionNames[5]);
            connectionNames = newConnectionNames;

            combineMatrixes(flipYMatrix);
        }

        //Rotate Z one
        //MY, PY, PX, MX, PZ, MZ
        // 0 -1 0
        // 1 0 0
        // 0 0 1

        void rotateZ()
        {
            List<string> newConnectionNames = new List<string>();
            newConnectionNames.Add(connectionNames[3]);
            newConnectionNames.Add(connectionNames[2]);
            newConnectionNames.Add(connectionNames[0]);
            newConnectionNames.Add(connectionNames[1]);
            newConnectionNames.Add(connectionNames[4]);
            newConnectionNames.Add(connectionNames[5]);
            connectionNames = newConnectionNames;

            combineMatrixes(rotateZMatrix);
        }

        void combineMatrixes(int[,] addition)
        {
            int[,] res = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    res[i, j] = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        res[i, j] += addition[i, k] * rotationMatrix[k, j];
                    }
                }
            }
            rotationMatrix = res;
        }

        public bool matchesParcel(Connections parcelConnections)
        {
            for(int i = 0; i < 6; i++)
            {
                if(connectionNames[i] != null && connectionNames[i] != "" && parcelConnections.connectionNames[i] != connectionNames[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void addParcelConnections(Connections parcelConnections)
        {
            for (int i = 0; i < 6; i++)
            {
                connectionNames[i] = parcelConnections.connectionNames[i];
            }
        }

        public override string ToString()
        {
            return connectionNames.ToString();
        }
    }
}
