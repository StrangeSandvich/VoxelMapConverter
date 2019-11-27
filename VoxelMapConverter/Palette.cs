using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace VoxelMapConverter
{
    class Palette
    {

        public int approximation;

        //Please don't shuffle around your indexes
        public List<RGBColor> palette = new List<RGBColor>();

        public Palette(int approximation)
        {
            this.approximation = approximation;
            palette.Add(new RGBColor(0,0,0));
        }

        public int getColorIndex(RGBColor color)
        {
            int index = palette.FindIndex(x => x.red + approximation > color.red && x.red - approximation < color.red && x.green + approximation > color.green && x.green - approximation < color.green && x.blue + approximation > color.blue && x.blue - approximation < color.blue);
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
            foreach(RGBColor color in palette)
            {
                //RGBA
                resultChunk.addData(new ByteChunkData(color.red)); 
                resultChunk.addData(new ByteChunkData(color.green));
                resultChunk.addData(new ByteChunkData(color.blue));
                resultChunk.addData(new ByteChunkData(255)); //Magicavoxel doesn't seem to use this, but keeps all of them at 255
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
