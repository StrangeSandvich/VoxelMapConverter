using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class AceOfSpadesToIMUsingKarp
    {
        private const int aosSizeX = 512;
        private const int aosSizeY = 512;
        private const int aosSizeZ = 256;
        public static IntermediateMap ToIntermediateMap(string aosFilename)
        {
            IntermediateMap mapResult = new IntermediateMap(aosSizeX, aosSizeY, aosSizeZ, Block.AIR);
            
            /*
            Vector3s sizeorig = new Vector3s(512, 250, 512);
            orig_blocks2.Change(delegate (ref Block block, ref Vector3s pos)
            {
                block.Id = 0;
            });
            Block dirt = new Block();
            dirt.Id = 53;
            */

            string filename1 = "output.txt";
            File.Create(filename1).Close();
            byte[] data = File.ReadAllBytes(aosFilename);

            int x = 0;
            int zz = 0;
            int mapSize = 512;
            int columnCount = mapSize * mapSize;
            int columnI = 0;
            int i = 0;
            int Z; int M;
            while (columnI < columnCount)
            {
                ConsoleColor cc;
                byte N = data[i];
                byte S = data[i + 1];
                byte E = data[i + 2];
                byte A = data[i + 3];
                //K = lenght of span
                int K = E - S + 1;

                if (N == 0)
                {
                    Z = 0;
                    M = 64;
                }
                else
                {
                    Z = (N - 1) - K;
                    // A of the next span
                    M = data[i + N * 4 + 3];
                }
                //Console.WriteLine("end of air run: " + (S - 1));
                //Console.WriteLine("end of top run: " + E);
                //Console.WriteLine("end of air run: " + (M - Z - 1));
                //Console.WriteLine("end of air run: " + (M - 1));
                int sizey = 249;
                int sizex = 511;
                int sizez = 511;
                //air run
                //for (int j = A; j < S - 1; j++)
                //{
                //    orig_blocks2.Set2(x, sizey - j, zz, brick);
                //}

                byte red2;
                byte green2;
                byte blue2;
                try
                {
                    red2 = data[i + 6 + 0 * 4];
                    green2 = data[i + 5 + 0 * 4];
                    blue2 = data[i + 4 + 0 * 4];
                }
                catch
                {
                    break;
                }

                mapResult.setBlockAt(sizex - x, zz, sizey - (S - 1) - 0, new Block(red2, green2, blue2));

                try
                {
                    red2 = data[i + 6 + 1 * 4];
                    green2 = data[i + 5 + 1 * 4];
                    blue2 = data[i + 4 + 1 * 4];
                }
                catch
                {
                    break;
                }

                int j2 = 1;

                //top run
                for (int j = S; j < E; j++)
                {
                    byte red;
                    byte green;
                    byte blue;
                    try
                    {
                        red = data[i + 6 + j2 * 4];
                        green = data[i + 5 + j2 * 4];
                        blue = data[i + 4 + j2 * 4];
                    }
                    catch
                    {
                        break;
                    }

                    mapResult.setBlockAt(sizex - x, zz, sizey - j, new Block(red, green, blue));
                    j2++;

                }
                //solid run
                for (int j = E + 1; j < M - Z - 1; j++)
                {
                    mapResult.setBlockAt(sizex - x, zz, sizey - j, new Block(Block.AOSSOLID));
                }
                //bottom run
                for (int j = M - Z; j < M - 1; j++)
                {
                    byte red;
                    byte green;
                    byte blue;
                    try
                    {
                        red = data[i + 6 + j2 * 4];
                        green = data[i + 5 + j2 * 4];
                        blue = data[i + 4 + j2 * 4];
                    }
                    catch
                    {
                        break;
                    }

                    j2++;
                    mapResult.setBlockAt(sizex - x, zz, sizey - j, new Block(red, green, blue));
                }

                if (N == 0)
                {
                    columnI += 1;
                    i += 4 * (1 + K);
                    x += 1;
                    if (x >= mapSize)
                    {
                        x = 0;
                        zz += 1;
                    }
                }
                else
                {
                    i += N * 4;
                }
            }

            return mapResult;
        }
    }

    /* Silverman code
     * z = 0;
         for(;;) {
            uint32 *color;
            int i;
            int number_4byte_chunks = v[0];
            int top_color_start = v[1];
            int top_color_end   = v[2]; // inclusive
            int bottom_color_start;
            int bottom_color_end; // exclusive
            int len_top;
            int len_bottom;

            for(i=z; i < top_color_start; i++)
               setgeom(x,y,i,0);

            color = (uint32 *) (v+4);
            for(z=top_color_start; z <= top_color_end; z++)
               setcolor(x,y,z,*color++);

            len_bottom = top_color_end - top_color_start + 1;

            // check for end of data marker
            if (number_4byte_chunks == 0) {
               // infer ACTUAL number of 4-byte chunks from the length of the color data
               v += 4 * (len_bottom + 1);
               break;
            }

            // infer the number of bottom colors in next span from chunk length
            len_top = (number_4byte_chunks-1) - len_bottom;

            // now skip the v pointer past the data to the beginning of the next span
            v += v[0]*4;

            bottom_color_end   = v[3]; // aka air start
            bottom_color_start = bottom_color_end - len_top;

            for(z=bottom_color_start; z < bottom_color_end; ++z) {
               setcolor(x,y,z,*color++);
            }
         }
     */


    /* Timothee code
     * private void button12_Click(object sender, EventArgs e)
        {
            string json2 = MapStoreUtils.Decode(File.ReadAllBytes(bnlmap.Lines[0]));
            //string json2 = MapStoreUtils.Decode(File.ReadAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\BlockNLoad\UserData\CustomMaps\mountainexpress.bnlbin"));
            JsonData json = JsonParser.Parse(json2);


            json.Object["map"].Object["color_palette"] = Write.String(utils.genpallete());

            Vector3s sizeorig = new Vector3s(512, 250, 512);
            //MapGenerator.FixSize(ref sizeorig);
            //Byte[] colors = Read.Binary(json.Object["map"].Object["colors_data"]);

            //MapBinary mb = new MapBinary(1, Read.Binary(json.Object["map"].Object["blocks_data"]), sizeorig);
            //BlockMap3D orig_blocks = mb.ToMap3D();
            BlockArrayMap3D2 orig_blocks2 = new BlockArrayMap3D2(sizeorig);
            orig_blocks2.Change(delegate (ref Block block, ref Vector3s pos)
            {
                block.Id = 0;
            });
            Block air = new Block();
            air.Id = 0;
            Block sand = new Block();
            sand.Id = 2;
            Block stone = new Block();
            stone.Id = 3;
            Block dirt = new Block();
            dirt.Id = 53;
            Block brick = new Block();
            brick.Id = 53;


            string filename1 = "output.txt";
            string filename2 = "DoubleDragon.vxl";
            File.Create(filename1).Close();
            byte[] data = File.ReadAllBytes(filename2);

            int x = 0;
            int zz = 0;
            int mapSize = 512;
            int columnCount = mapSize * mapSize;
            int columnI = 0;
            int i = 0;
            int Z; int M;
            while (columnI < columnCount)
            {
                ConsoleColor cc;
                byte N = data[i];
                byte S = data[i + 1];
                byte E = data[i + 2];
                byte A = data[i + 3];
                //K = lenght of span
                int K = E - S + 1;

                if (N == 0)
                {
                    Z = 0;
                    M = 64;
                }
                else
                {
                    Z = (N - 1) - K;
                    // A of the next span
                    M = data[i + N * 4 + 3];
                }
                //Console.WriteLine("end of air run: " + (S - 1));
                //Console.WriteLine("end of top run: " + E);
                //Console.WriteLine("end of air run: " + (M - Z - 1));
                //Console.WriteLine("end of air run: " + (M - 1));
                int sizey = 249;
                int sizex = 511;
                int sizez = 511;
                //air run
                //for (int j = A; j < S - 1; j++)
                //{
                //    orig_blocks2.Set2(x, sizey - j, zz, brick);
                //}

                dirt.Color = 0;
                byte red2;
                byte green2;
                byte blue2;
                try
                {
                    red2 = data[i + 6 + 0 * 4];
                    green2 = data[i + 5 + 0 * 4];
                    blue2 = data[i + 4 + 0 * 4];
                }
                catch
                {
                    break;
                }

                cc = utils.ClosestConsoleColor(red2, green2, blue2);
                if (cc == ConsoleColor.Black) { dirt.Color = Black; }
                if (cc == ConsoleColor.DarkBlue) { dirt.Color = DarkBlue; }
                if (cc == ConsoleColor.DarkGreen) { dirt.Color = DarkGreen; }
                if (cc == ConsoleColor.DarkCyan) { dirt.Color = DarkCyan; }
                if (cc == ConsoleColor.DarkRed) { dirt.Color = DarkRed; }
                if (cc == ConsoleColor.DarkMagenta) { dirt.Color = DarkMagenta; }
                if (cc == ConsoleColor.Gray) { dirt.Color = Gray; }
                if (cc == ConsoleColor.DarkGray) { dirt.Color = DarkGray; }
                if (cc == ConsoleColor.Blue) { dirt.Color = Blue; }
                if (cc == ConsoleColor.Green) { dirt.Color = Green; }
                if (cc == ConsoleColor.Cyan) { dirt.Color = Cyan; }
                if (cc == ConsoleColor.Red) { dirt.Color = Red; }
                if (cc == ConsoleColor.Magenta) { dirt.Color = Magenta; }
                if (cc == ConsoleColor.Yellow) { dirt.Color = Yellow; }
                if (cc == ConsoleColor.White) { dirt.Color = White; }
                orig_blocks2.Set2(sizex - x, sizey - (S - 1) - 0, zz, dirt);
                dirt.Color = 0;

                try
                {
                    red2 = data[i + 6 + 1 * 4];
                    green2 = data[i + 5 + 1 * 4];
                    blue2 = data[i + 4 + 1 * 4];
                }
                catch
                {
                    break;
                }

                cc = utils.ClosestConsoleColor(red2, green2, blue2);
                if (cc == ConsoleColor.Black) { dirt.Color = Black; }
                if (cc == ConsoleColor.DarkBlue) { dirt.Color = DarkBlue; }
                if (cc == ConsoleColor.DarkGreen) { dirt.Color = DarkGreen; }
                if (cc == ConsoleColor.DarkCyan) { dirt.Color = DarkCyan; }
                if (cc == ConsoleColor.DarkRed) { dirt.Color = DarkRed; }
                if (cc == ConsoleColor.DarkMagenta) { dirt.Color = DarkMagenta; }
                if (cc == ConsoleColor.Gray) { dirt.Color = Gray; }
                if (cc == ConsoleColor.DarkGray) { dirt.Color = DarkGray; }
                if (cc == ConsoleColor.Blue) { dirt.Color = Blue; }
                if (cc == ConsoleColor.Green) { dirt.Color = Green; }
                if (cc == ConsoleColor.Cyan) { dirt.Color = Cyan; }
                if (cc == ConsoleColor.Red) { dirt.Color = Red; }
                if (cc == ConsoleColor.Magenta) { dirt.Color = Magenta; }
                if (cc == ConsoleColor.Yellow) { dirt.Color = Yellow; }
                if (cc == ConsoleColor.White) { dirt.Color = White; }
                //orig_blocks2.Set2(sizex - x, sizey - (S - 1) - 1, zz, dirt);
                dirt.Color = 0;

                //orig_blocks2.Set2(sizex - x, sizey - (S - 1) - 0, zz, dirt);
                //orig_blocks2.Set2(sizex - x, sizey - (S - 1) - 1, zz, dirt);

                int j2 = 1;

                //top run
                for (int j = S; j < E; j++)
                {
                    dirt.Color = 0;
                    byte red;
                    byte green;
                    byte blue;
                    try
                    {
                        red = data[i + 6 + j2 * 4];
                        green = data[i + 5 + j2 * 4];
                        blue = data[i + 4 + j2 * 4];
                    }
                    catch
                    {
                        break;
                    }

                    cc = utils.ClosestConsoleColor(red, green, blue);
                    if (cc == ConsoleColor.Black) { dirt.Color = Black; }
                    if (cc == ConsoleColor.DarkBlue) { dirt.Color = DarkBlue; }
                    if (cc == ConsoleColor.DarkGreen) { dirt.Color = DarkGreen; }
                    if (cc == ConsoleColor.DarkCyan) { dirt.Color = DarkCyan; }
                    if (cc == ConsoleColor.DarkRed) { dirt.Color = DarkRed; }
                    if (cc == ConsoleColor.DarkMagenta) { dirt.Color = DarkMagenta; }
                    if (cc == ConsoleColor.Gray) { dirt.Color = Gray; }
                    if (cc == ConsoleColor.DarkGray) { dirt.Color = DarkGray; }
                    if (cc == ConsoleColor.Blue) { dirt.Color = Blue; }
                    if (cc == ConsoleColor.Green) { dirt.Color = Green; }
                    if (cc == ConsoleColor.Cyan) { dirt.Color = Cyan; }
                    if (cc == ConsoleColor.Red) { dirt.Color = Red; }
                    if (cc == ConsoleColor.Magenta) { dirt.Color = Magenta; }
                    if (cc == ConsoleColor.Yellow) { dirt.Color = Yellow; }
                    if (cc == ConsoleColor.White) { dirt.Color = White; }
                    orig_blocks2.Set2(sizex - x, sizey - j, zz, dirt);
                    j2++;
                    dirt.Color = 0;

                }
                //solid run
                for (int j = E + 1; j < M - Z - 1; j++)
                {
                    orig_blocks2.Set2(sizex - x, sizey - j, zz, dirt);
                }
                //bottom run
                for (int j = M - Z; j < M - 1; j++)
                {
                    dirt.Color = 0;
                    byte red;
                    byte green;
                    byte blue;
                    try
                    {
                        red = data[i + 6 + j2 * 4];
                        green = data[i + 5 + j2 * 4];
                        blue = data[i + 4 + j2 * 4];
                    }
                    catch
                    {
                        break;
                    }

                    cc = utils.ClosestConsoleColor(red, green, blue);
                    if (cc == ConsoleColor.Black) { dirt.Color = Black; }
                    if (cc == ConsoleColor.DarkBlue) { dirt.Color = DarkBlue; }
                    if (cc == ConsoleColor.DarkGreen) { dirt.Color = DarkGreen; }
                    if (cc == ConsoleColor.DarkCyan) { dirt.Color = DarkCyan; }
                    if (cc == ConsoleColor.DarkRed) { dirt.Color = DarkRed; }
                    if (cc == ConsoleColor.DarkMagenta) { dirt.Color = DarkMagenta; }
                    if (cc == ConsoleColor.Gray) { dirt.Color = Gray; }
                    if (cc == ConsoleColor.DarkGray) { dirt.Color = DarkGray; }
                    if (cc == ConsoleColor.Blue) { dirt.Color = Blue; }
                    if (cc == ConsoleColor.Green) { dirt.Color = Green; }
                    if (cc == ConsoleColor.Cyan) { dirt.Color = Cyan; }
                    if (cc == ConsoleColor.Red) { dirt.Color = Red; }
                    if (cc == ConsoleColor.Magenta) { dirt.Color = Magenta; }
                    if (cc == ConsoleColor.Yellow) { dirt.Color = Yellow; }
                    if (cc == ConsoleColor.White) { dirt.Color = White; }
                    j2++;
                    orig_blocks2.Set2(sizex - x, sizey - j, zz, dirt);
                    dirt.Color = 0;
                }

                for (int y = 200; y > 8; y--)
                {
                    if (orig_blocks2.Get2(sizex - x, y, zz).Id != 0)
                    {

                    }
                }



                if (N == 0)
                {
                    columnI += 1;
                    i += 4 * (1 + K);
                    x += 1;
                    if (x >= mapSize)
                    {
                        x = 0;
                        zz += 1;
                    }
                }
                else
                {
                    i += N * 4;
                }
            }
            for (int xx = 0; xx < 512; xx++)
            {
                for (int zzz = 0; zzz < 512; zzz++)
                {
                    for (int yy = 9; yy < 200; yy++)//fill up those holes
                    {
                        if (orig_blocks2.Get2(xx, yy, zzz).Id != 0)
                        {
                            break;
                        }
                        orig_blocks2.Set2(xx, yy, zzz, dirt);
                    }
                }
            }




            byte[] colors2 = MapBinary.EncodeColors(orig_blocks2);
            json.Object["map"].Object["colors_data"] = Write.Optional<byte[]>(new Func<byte[], JsonData>(Write.Binary)).Invoke(colors2);
            json.Object["map"].Object["size"].Object["x"] = Write.Int(sizeorig.x);
            json.Object["map"].Object["size"].Object["y"] = Write.Int(sizeorig.y);
            json.Object["map"].Object["size"].Object["z"] = Write.Int(sizeorig.z);
            json.Object["map"].Object["blocks_data"] = Write.Optional<byte[]>(new Func<byte[], JsonData>(Write.Binary)).Invoke(MapBinary.Pack(orig_blocks2));


            //plane_position
            //json.Object["map"].Object["properties"].Object["plane_position"] = Write.Float(0.5f);


            string json_fixed = json.ToString().Replace("\\", "").Replace("\"color_palette\":\"[", "\"color_palette\":[").Replace(",]\",\"spawn", "],\"spawn");
            byte[] compressed = MapStoreUtils.Encode(json_fixed);
            try
            {
                File.Create(@"C:\Program Files (x86)\Steam\steamapps\common\BlockNLoad\UserData\CustomMaps\map-generated.bnlbin").Close();
                File.WriteAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\BlockNLoad\UserData\CustomMaps\map-generated.bnlbin", compressed);
            }
            catch
            {

            }
            File.Create(@"map-generated.bnlbin").Close();
            File.WriteAllBytes(@"map-generated.bnlbin", compressed);
        }
        */
}
