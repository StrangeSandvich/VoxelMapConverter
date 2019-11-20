﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace VoxelMapConverter
{
    class Palette
    {

        public int approximation;

        //Please don't shuffle around your indexes
        public List<(int, int, int)> palette = new List<(int, int, int)>();

        public Palette(int approximation)
        {
            this.approximation = approximation;
            palette.Add((0, 0, 0));
        }

        public int getColorIndex((int, int, int) color)
        {
            int index = palette.FindIndex(x => x.Item1 + approximation > color.Item1 && x.Item1-approximation < color.Item1 && x.Item2 + approximation > color.Item2 && x.Item2 - approximation < color.Item2 && x.Item3 + approximation > color.Item3 && x.Item3 - approximation < color.Item3);
            if(index != -1)
            {
                return index+1;
            }
            if(palette.Count >= 255)
            {
                Console.WriteLine("Warning: Ran out of color indexes. Printing color black.");
                return 1;
            }
            palette.Add(color);
            return palette.Count; //Index+1 of new insert
        }

        public RiffChunk getPaletteChunk()
        {
            RiffChunk resultChunk = new RiffChunk("RGBA");
            foreach((int, int, int) color in palette)
            {
                //RGBA
                resultChunk.addData(new ByteChunkData(color.Item1)); 
                resultChunk.addData(new ByteChunkData(color.Item2));
                resultChunk.addData(new ByteChunkData(color.Item3));
                resultChunk.addData(new ByteChunkData(255));
            }
            for(int i = 0; i < 255-palette.Count; i++)
            {
                //Add completely white pixels to the rest of the palette
                resultChunk.addData(new ByteChunkData(255));
                resultChunk.addData(new ByteChunkData(255));
                resultChunk.addData(new ByteChunkData(255));
                resultChunk.addData(new ByteChunkData(255));
            }
            //Let number 256 be 0 0 0 0 as in example
            resultChunk.addData(new ByteChunkData(0));
            resultChunk.addData(new ByteChunkData(0));
            resultChunk.addData(new ByteChunkData(0));
            resultChunk.addData(new ByteChunkData(0));

            return resultChunk;
        }

    }
}
