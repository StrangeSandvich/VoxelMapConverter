using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class IntermediateMap
    {
        private int sizeX, sizeY, sizeZ;
        //private List<List<List<Block>>> mapXYZI;
        private Block[,,] mapXYZI;
        public int groundHeight { get; set; }
        public Block fillBlock;
        public Palette palette { get; }

        public IntermediateMap(int sizeX, int sizeY, int sizeZ, int filltype)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.sizeZ = sizeZ;
            this.groundHeight = 0;
            this.palette = new Palette();
            this.fillBlock = new Block(filltype, palette);

            mapXYZI = new Block[sizeX, sizeY, sizeZ];
            //Leave all values in the array null. If asked for, the function will give the fillblock. 
        }

        public Block getBlockAt(int x, int y, int z)
        {
            if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ - groundHeight)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (mapXYZI[x,y,z+groundHeight] == null){
                return fillBlock;
            }
            return mapXYZI[x,y,z+groundHeight];
        }

        public void setBlockAt(int x, int y, int z, Block block)
        {
            if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ-groundHeight)
            {
                throw new ArgumentOutOfRangeException();
            }
            mapXYZI[x,y,z+groundHeight] = block;
        }

        //Reduce number of colors in palette to count 
        public void PaletteShrink(int count)
        {
            //Make the palette shrink, have it give the new index values
            List<int> reIndexingList = palette.PaletteShrink(count);

            //Update the color index of every block
            for (int x = 0; x < sizeX; x++)
            {
                for(int y = 0; y < sizeY; y++)
                {
                    for(int z = groundHeight; z < sizeZ; z++)
                    {
                        Block block = mapXYZI[x, y, z];
                        //Don't bother with air, fill or solid blocks
                        if(block == null)
                        {
                            continue;
                        } else if(block.ID < 0) //Block is air or something
                        {
                            continue;
                        }//Else
                        block.colorIndex = reIndexingList[block.colorIndex];
                        
                    }
                }
                if (x % 64 == 0)
                {
                    Console.Write("|");
                }
            }
            Console.WriteLine("");
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
                        if (block.ID != Block.AIR)
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
