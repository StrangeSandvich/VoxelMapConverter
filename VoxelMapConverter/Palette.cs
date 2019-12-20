using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace VoxelMapConverter
{
    class Palette
    {
        //REMEMBER TO SUBTRACT 1. ADDS 1 LATER BECAUSE MAGICAVOXEL INDICES FROM 1
        public static List<int> reservedIDs = new List<int>(new int[]{8, 16, 17 });

        //Please don't shuffle around your indexes
        public List<RGBColor> palette = new List<RGBColor>();
        public List<int> indexCount = new List<int>(); //Keep track of how many blocks use one color. 
        public List<int> fillerIndexes = new List<int>();

        public Palette()
        {
            //Solid ground color
            palette.Add(new RGBColor(0,0,0, true));
            indexCount.Add(0);
        }

        public int getExactColorIndex(RGBColor color)
        {
            int index = palette.FindIndex(x => x.Compare(color));
            if (index != -1)
            {
                indexCount[index]++;
                return index;
            }
            palette.Add(color);
            indexCount.Add(1);
            return palette.Count-1; //Last new insert
        }

        public int[,] CalculateDistanceForIndex(int index, int[,] distances)
        {
            for(int i = 1; i < distances.GetLength(0); i++)
            {
                int difference;
                if(i == index)
                {
                    difference = int.MaxValue;
                } else
                {
                    difference = palette[index].Difference(palette[i]);
                }
                distances[index, i] = difference;
                distances[i, index] = difference;
            }
            return distances;
        }

        //Find the index pair with the smallest distance. Can be optimized
        public (int, int) SmallestDistance(int[,] distances, List<int> resultList)
        {
            int smalli = 1;
            int smallj = 2;
            int smallValue = int.MaxValue;
            for (int i = 1; i < resultList.Count; i++)
            {
                if(resultList[i] != i) //Index no longer in use
                {
                    continue;
                }
                for(int j = 1; j < resultList.Count; j++)
                {
                    if (resultList[j] != j) //Index no longer in use
                    {
                        continue;
                    }
                    if (distances[i,j] < smallValue)
                    {
                        smallValue = distances[i, j];
                        smalli = i;
                        smallj = j;
                    }
                }
            }
            return (smalli, smallj);
        }

        //Reduce number of color indexes to count. Return list of new indexes
        public List<int> PaletteShrink(int count)
        {
            List<int> resultList = new List<int>();
            int paletteCount = palette.Count;
            for(int i = 0; i < paletteCount; i++)
            {
                //At the start, all indexes should just be replaced by themselves
                resultList.Add(i);
            }
            int[,] distances = new int[paletteCount, paletteCount];
            //Find all distances, except to index 0
            for(int i = 1; i < paletteCount; i++)
            {
                for(int j = 1; j < paletteCount; j++)
                {
                    if(i == j)
                    {
                        distances[i, j] = int.MaxValue;
                    } else
                    {
                        distances[i, j] = palette[i].Difference(palette[j]);
                    }
                }
            }
            while(paletteCount > count+1) //+1 for solid index
            {
                //Find indexes with smallest distance
                (int, int) combining = SmallestDistance(distances, resultList);
                //Combine the two indexes
                int one = combining.Item1;
                int two = combining.Item2;
                RGBColor newColor = RGBColor.Combine(palette[one], indexCount[one], palette[two], indexCount[two]);
                int newIndexCount = indexCount[one] + indexCount[two];
                //Write new color and count to smallest index. Refer higher index to smaller in result list
                int smallest = Math.Min(one, two);
                int highest = Math.Max(one, two);
                palette[smallest] = newColor;
                indexCount[smallest] = newIndexCount;
                resultList[highest] = smallest;
                //Recalculate distance matrix
                distances = CalculateDistanceForIndex(smallest, distances);
                //Lower paletteCount
                paletteCount--;
                if(paletteCount % 3 == 0)
                {
                    Console.Write("|");
                }
            }
            //Now there is at most count palette colors. We now have to condense the indexes so they go from 0 to count

            //Get all indexes to point directly at a valid endpoint
            List<RGBColor> newPalette = new List<RGBColor>();
            for(int i = 0; i < palette.Count; i++)
            {
                if(resultList[i] != i) //Index is not valid endpoint
                {
                    while(resultList[i] != resultList[resultList[i]])
                    {
                        resultList[i] = resultList[resultList[i]];
                    }
                }
            }
            //Now all indexes in the resultlist point directly to a valid colorindex
            for (int i = 0; i < palette.Count; i++)
            {
                if (resultList[i] == i) //Index is valid endpoint
                {
                    newPalette.Add(palette[i]);
                    resultList[i] = newPalette.Count - 1;
                } else //Index is pointing to valid endpoint and should follow it to final index
                {
                    resultList[i] = resultList[resultList[i]];
                }
            }
            foreach (int reservedID in reservedIDs)
            {
                if(reservedID >= newPalette.Count)
                {
                    while(reservedID != newPalette.Count)
                    {
                        fillerIndexes.Add(newPalette.Count);
                        newPalette.Add(new RGBColor(0, 0, 0));

                    }
                    newPalette.Add(new RGBColor(0, 0, 0));
                } else
                {
                    int replacementID = newPalette.Count;
                    newPalette.Add(newPalette[reservedID]);
                    resultList[reservedID] = replacementID;
                    newPalette[reservedID] = new RGBColor(0, 0, 0);
                }
            }
            for(int i = 0; i < resultList.Count; i++)
            {
                int index = resultList[i];
                if (reservedIDs.Contains(index))
                {
                    resultList[i] = resultList[index]; //Forward to the replacement ID
                }
            }

            //Write in the new condensed palette and return the resultlist for the map to update block indexes
            palette = newPalette;
            return resultList;
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
            for(int i = palette.Count; i < 255; i++)
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
