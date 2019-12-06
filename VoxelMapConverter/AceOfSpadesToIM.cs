using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class AceOfSpadesToIM
    {
        private const int aosSizeX = 512;
        private const int aosSizeY = 512;
        //private const int IMSizeZ = 256;
        //Size Z varies by map, but should be 256 at most. Figure out map height by lowest block and then add that in. 
        public static IntermediateMap ToIntermediateMap(byte[] AoSMapData, bool keepOcean)
        {
            
            //Go through each column
            int columnStart = 0;
            int groundHeight = GroundHeight(AoSMapData);
            if (!keepOcean)
            {
                groundHeight++;
            }
            int mapHeight = 256 - groundHeight;
            IntermediateMap mapResult = new IntermediateMap(aosSizeX, aosSizeY, mapHeight, Block.AOSSOLID);
            Palette palette = mapResult.palette;
            Block airBlock = new Block(Block.AIR, palette);
            for (int x = 0; x < aosSizeX; x++)
            {
                for (int y = 0; y < aosSizeY; y++)
                {
                    int spanStart = columnStart;
                    int z = mapHeight - 1;
                    while (true)
                    {
                        //Read span metadata
                        int number_4_byte_chunks = (int)AoSMapData[spanStart];
                        int top_color_start = (int)AoSMapData[spanStart + 1];
                        int top_color_end = (int)AoSMapData[spanStart + 2]; //Inclusive
                        int surfaceBlockCount = top_color_end - top_color_start + 1;
                        int i; //Offset into data bytes
                        
                        //Value where z = 0 is the bottom of the map
                        int top_color_switched = mapHeight - top_color_start - 1;

                        //Air run. Start at the latest z and go down to top_color_start
                        for (/*Use z*/; z > top_color_switched; z--){
                            mapResult.setBlockAt(x, y, z, airBlock);
                        }

                        //Surface/top run
                        for(i = 0; i < surfaceBlockCount; i++)
                        {
                            if(z < groundHeight)
                            {
                                break;
                            }
                            int blue = AoSMapData[spanStart + 4 + i * 4];
                            int green = AoSMapData[spanStart + 5 + i * 4];
                            int red = AoSMapData[spanStart + 6 + i * 4];
                            mapResult.setBlockAt(x, y, z, new Block(palette, new RGBColor(red, green, blue)));
                            z--;
                        }

                        if(number_4_byte_chunks == 0) //Last span of column
                        {
                            //Update if this column was the highest.
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
                        z = mapHeight - ceiling_color_start - 1; //Jump down to top of bottom blocks
                        //Continue from previous set of data bytes
                        for(int j = i; j < i + ceilingBlockCount; j++)
                        {
                            int blue = AoSMapData[spanStart + 4 + j * 4];
                            int green = AoSMapData[spanStart + 5 + j * 4];
                            int red = AoSMapData[spanStart + 6 + j * 4];
                            mapResult.setBlockAt(x, y, z, new Block(palette, new RGBColor(red, green, blue)));
                            z--;
                        }

                        //Set spanstart to next span. Z is already at the end of the previous ceiling
                        spanStart += 4 * number_4_byte_chunks;
                    }
                }
                Console.Write("|");
            }

            Console.WriteLine("");
            return mapResult;
        }

        public static int GroundHeight(byte[] AoSMapData)
        {
            //Go through each column
            int columnStart = 0;
            int groundHeight = 256; //Start at the highest value so first column Z will be smaller 
            for (int x = 0; x < aosSizeX; x++)
            {
                for (int y = 0; y < aosSizeY; y++)
                {
                    int spanStart = columnStart;
                    while (true)
                    {
                        //Read span metadata
                        int number_4_byte_chunks = (int)AoSMapData[spanStart];

                        if (number_4_byte_chunks != 0)
                        {
                            //Set spanstart to next span
                            spanStart += 4 * number_4_byte_chunks;
                        }
                        else
                        {
                            int top_color_start = (int)AoSMapData[spanStart + 1];
                            int top_color_end = (int)AoSMapData[spanStart + 2]; //Inclusive
                            int surfaceBlockCount = top_color_end - top_color_start + 1;

                            //Value where z = 0 is the bottom of the map
                            int top_color_switched = 256 - top_color_start - 1;

                            int z = top_color_switched;
                            z -= surfaceBlockCount;
                            //Update if this column was the highest. 
                            groundHeight = Math.Min(groundHeight, z);
                            //Rest of the column is already solid, leave it
                            //Next column is past first 4 span bytes and then past the number of surface blocks * the four bytes one of those takes
                            columnStart = spanStart + 4 + 4 * surfaceBlockCount;
                            break;

                        }
                    }
                }
            }
            return groundHeight + 1;
        }
    }
}
