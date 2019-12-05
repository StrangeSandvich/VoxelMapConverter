﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class AceOfSpadesToIM
    {
        private const int aosSizeX = 512;
        private const int aosSizeY = 512;
        private const int IMSizeZ = 256;
        //Size Z varies by map, but should be 256 at most. Figure out map height by lowest block and then add that in. 
        public static IntermediateMap ToIntermediateMap(byte[] AoSMapData, bool keepOcean)
        {
            IntermediateMap mapResult = new IntermediateMap(aosSizeX, aosSizeY, IMSizeZ, Block.AOSSOLID);
            
            //Go through each column
            int columnStart = 0;
            int groundHeight = IMSizeZ; //Start at the highest value so first column Z will be smaller 
            Block airBlock = new Block(Block.AIR);
            for (int x = 0; x < aosSizeX; x++)
            {
                for (int y = 0; y < aosSizeY; y++)
                {
                    int spanStart = columnStart;
                    int z = IMSizeZ - 1;
                    while (true)
                    {
                        //Read span metadata
                        int number_4_byte_chunks = (int)AoSMapData[spanStart];
                        int top_color_start = (int)AoSMapData[spanStart + 1];
                        int top_color_end = (int)AoSMapData[spanStart + 2]; //Inclusive
                        int surfaceBlockCount = top_color_end - top_color_start + 1;
                        int i; //Offset into data bytes
                        int top_color_switched;
                        
                        //Value where z = 0 is the bottom of the map
                        top_color_switched = IMSizeZ - top_color_start - 1;

                        //Air run. Start at the latest z and go down to top_color_start
                        for (/*Use z*/; z > top_color_switched; z--){
                            mapResult.setBlockAt(x, y, z, airBlock);
                        }

                        //Surface/top run
                        for(i = 0; i < surfaceBlockCount; i++)
                        {
                            int blue = AoSMapData[spanStart + 4 + i * 4];
                            int green = AoSMapData[spanStart + 5 + i * 4];
                            int red = AoSMapData[spanStart + 6 + i * 4];
                            mapResult.setBlockAt(x, y, z, new Block(new RGBColor(red, green, blue)));
                            z--;
                        }

                        if(number_4_byte_chunks == 0) //Last span of column
                        {
                            //Update if this column was the highest. 
                            groundHeight = Math.Min(groundHeight, z);
                            //Rest of the column is already solid, leave it
                            //Next column is past first 4 span bytes and then past the number of surface blocks * the four bytes one of those takes
                            columnStart = spanStart + 4 + 4 * surfaceBlockCount;
                            break;
                        }

                        //Don't run over the solid area. It is already there. 

                        //Do the ceiling at the bottom of the span
                        int ceilingBlockCount = (number_4_byte_chunks - 1) - surfaceBlockCount;
                        int ceiling_color_end = AoSMapData[spanStart + number_4_byte_chunks * 4 + 3]; //Read next span air start;
                        int ceiling_color_start = ceiling_color_end - ceilingBlockCount;
                        z = IMSizeZ - ceiling_color_start - 1;
                        for(int j = i; j < i + ceilingBlockCount; j++)
                        {
                            int blue = AoSMapData[spanStart + 4 + j * 4];
                            int green = AoSMapData[spanStart + 5 + j * 4];
                            int red = AoSMapData[spanStart + 6 + j * 4];
                            mapResult.setBlockAt(x, y, z, new Block(new RGBColor(red, green, blue)));
                            z--;
                        }

                        //Set spanstart to next span. Z is already at the end of the previous ceiling
                        spanStart += 4 * number_4_byte_chunks;
                    }
                }
            }

            if (keepOcean)
            {
                mapResult.groundHeight = groundHeight + 1;
            } else
            {
                mapResult.groundHeight = groundHeight + 2;
            }

            Console.WriteLine("Groundheight is " + groundHeight);

            return mapResult;
        }
    }
}
