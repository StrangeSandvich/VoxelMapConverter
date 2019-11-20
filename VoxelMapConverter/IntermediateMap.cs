using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class IntermediateMap
    {
        private int sizeX, sizeY, sizeZ;
        private List<List<List<Block>>> mapXYZI;

        public IntermediateMap(int sizeX, int sizeY, int sizeZ, int filltype)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;

            mapXYZI = new List<List<List<Block>>>(sizeX);
            //Creates map with all blocks of filltype
            for (int x = 0; x < sizeX; x++)
            {
                List<List<Block>> slice = new List<List<Block>>(sizeY);
                for (int y = 0; y < sizeY; y++)
                {
                    List<Block> column = new List<Block>(sizeZ);
                    for (int z = 0; z < sizeZ; z++)
                    {
                        column.Add(new Block(filltype));
                    }
                    slice.Add(column);
                }
                mapXYZI.Add(slice);
            }
        }

        public Block getBlockAt(int x, int y, int z)
        {
            if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ)
            {
                Console.WriteLine("GetBlockAt got invalid coordinate " + x + ", " + y + ", " + z);
                throw new ArgumentOutOfRangeException();
            }
            return mapXYZI[x][y][z];
        }

        public void setBlockAt(int x, int y, int z, Block block)
        {
            if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ)
            {
                Console.WriteLine("SetBlockAt got invalid coordinate " + x + ", " + y + ", " + z);
                throw new ArgumentOutOfRangeException();
            }
            mapXYZI[x][y][z] = block;
        }

        public List<Voxel> getListOfVoxels(int lowx, int highx, int lowy, int highy, int lowz, int highz)
        {
            List<Voxel> resultList = new List<Voxel>();
            for(int x = lowx; x < highx; x++)
            {
                for(int y = lowy; y < highy; y++)
                {
                    for(int z = lowz; z < highz; z++)
                    {
                        Block block = getBlockAt(x, y, z);
                        if(block.ID != Block.AIR)
                        {
                            resultList.Add(new Voxel(x, y, z, block));
                        }
                    }
                }
            }
            return resultList;
        }
    }
}
