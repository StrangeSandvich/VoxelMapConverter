using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class IMToVXL
    {
        public static int nodeID;
        public static List<int> IDForGroup;

        public static void IMToVoxel(IntermediateMap map, string outfileName)
        {
            FileStream stream = new FileStream(outfileName, FileMode.Create, FileAccess.Write,
                FileShare.Write);
            BinaryWriter writer = new BinaryWriter(stream);

            //Standard opening
            writer.Write("VOX ".ToCharArray()); 
            writer.Write(150); //Version
            writer.Write("MAIN".ToCharArray());
            writer.Write(0); //No chunk data in main

            //List of chunks
            List<RiffChunk> chunks = new List<RiffChunk>();

            Palette palette = map.palette;

            //Setup for staging nodes
            resetNodeID();
            IDForGroup = new List<int>();
            int masterGRPID = getNextNodeID(); //Pass to master Translate
            List<RiffChunk> modelStagings = new List<RiffChunk>();
            int modelID = 0;

            //Seperate the world into 64x64x64 regions that become a model each
            int mapheight = 256 - map.groundHeight;
            for (int x = 0; x < 512; x+= 64)
            {
                Console.Write("|");
                for(int y = 0; y < 512; y += 64)
                {
                    for(int z = 0; z < mapheight+64; z += 64) //Map may not be that high, but getListOfVoxels will just return nothing for out of bounds values. 
                    {
                        //Make sure not to go out of bounds
                        int upperBlock = z + 64;
                        if (upperBlock > mapheight)
                        {
                            upperBlock = mapheight;
                        }
                        List<Voxel> voxels = map.getListOfVoxels(x, x + 64, y, y + 64, z, upperBlock);
                        //Find out if chunk even has any voxels
                        if(voxels.Count == 0)
                        {
                            //Don't create the object if there are no voxels
                            continue;
                        }

                        //Hardcode size to be 64x64x64
                        RiffChunk size = new RiffChunk("SIZE");
                        size.addData(new IntChunkData(64));
                        size.addData(new IntChunkData(64));
                        size.addData(new IntChunkData(64));
                        chunks.Add(size);

                        //Get a region of voxels and make them a model
                        RiffChunk xyzi = new RiffChunk("XYZI");
                        xyzi.addData(new IntChunkData(voxels.Count));
                        foreach (Voxel voxel in voxels)
                        {
                            //Remember that magicavoxel index starts at 1
                            int colorIndex = voxel.colorIndex+1;
                            try
                            {
                                xyzi.addData(new VoxelChunkData(Convert.ToByte(voxel.x-x), Convert.ToByte(voxel.y-y), Convert.ToByte(voxel.z-z), Convert.ToByte(colorIndex)));
                            } catch(Exception e)
                            {
                                Console.WriteLine("Byte overflow with voxel " + voxel.x + ", " + voxel.y + ", " + voxel.z + ". With object xyz being " + x + ", " + y + "," + z);
                                Console.WriteLine("Color index of " + colorIndex);
                                throw e;
                            }
                        }
                        chunks.Add(xyzi);

                        //Create shape and translate node for the model
                        modelStagings.AddRange(createStagingChunksForModel(x, y, z, modelID)); //Create translate and shape noded for model
                        //ModelID isn't written to file, it's just the order the models appear
                        modelID++;
                    }
                }
            }

            //Master group node
            RiffChunk masterGRP = new RiffChunk("nGRP");
            masterGRP.addData(new IntChunkData(masterGRPID));
            masterGRP.addData(new DictChunkData()); //empty dict
            masterGRP.addData(new IntChunkData(modelID + 1)); //Number of models
            foreach(int nodeID in IDForGroup)
            {
                masterGRP.addData(new IntChunkData(nodeID));
            }

            //Master translate node
            RiffChunk masterTRN = new RiffChunk("nTRN");
            masterTRN.addData(new IntChunkData(0)); //Assuming top node has to be number 0
            masterTRN.addData(new DictChunkData()); //Empty dict
            masterTRN.addData(new IntChunkData(masterGRPID));
            masterTRN.addData(new IntChunkData(-1)); //Reserved
            masterTRN.addData(new IntChunkData(-1)); //Layer ID
            masterTRN.addData(new IntChunkData(1)); //Number of frames is always 1
            masterTRN.addData(new DictChunkData()); //Empty dict. No translation of master

            //Put in all the staging chunks
            chunks.Add(masterTRN);
            chunks.Add(masterGRP);
            chunks.AddRange(modelStagings);


            //Add in all the layer chunks. This may be unneeded
            chunks.AddRange(createLayrChunks());

            //RGBA palette chunk
            chunks.Add(palette.getPaletteChunk());

            //Material chunks. May be unneeded
            for(int i = 1; i <= 256; i++)
            {
                chunks.Add(createMatlChunk(i));
            }

            //Whatever the robj chunks are...
            chunks.AddRange(createROBJChunks());

            //Write in the main chunk child size field
            int mainChunkChildDataSize = 0;
            foreach(RiffChunk chunk in chunks)
            {
                mainChunkChildDataSize += chunk.getChunkSize();
            }
            writer.Write(mainChunkChildDataSize);

            Console.Write("|");
            //Write all those chunks
            foreach (RiffChunk chunk in chunks)
            {
                chunk.writeChunk(writer);
            }
            Console.Write("|");

            //Done
            writer.Close();
            stream.Close();
            Console.WriteLine("");
        }

        //We don't use the layer chunks. All objects are layer 1
        public static List<RiffChunk> createLayrChunks()
        {
            List<RiffChunk> chunksResult = new List<RiffChunk>();
            for(int i = 0; i < 8; i++)
            {
                chunksResult.Add(createLayrChunk(i));
            }
            return chunksResult;
        }

        public static RiffChunk createLayrChunk(int number)
        {
            RiffChunk chunkResult = new RiffChunk("LAYR");
            chunkResult.addData(new IntChunkData(number));
            List<(string, string)> dict = new List<(string, string)>();
            dict.Add(("name_", number.ToString()));
            chunkResult.addData(new DictChunkData(dict)); //presumably has to include dictionary with name
            chunkResult.addData(new IntChunkData(-1)); //Reserved field
            return chunkResult;
        }

        //For staging nodes
        public static void resetNodeID()
        {
            nodeID = 0; //Starts at 1.
        }
        public static int getNextNodeID()
        {
            nodeID++;
            return nodeID;
        }

        public static List<RiffChunk> createStagingChunksForModel(int offsetX, int offsetY, int offsetZ, int modelID)
        {
            int transformID = getNextNodeID();
            int shapeID = getNextNodeID();

            RiffChunk ShapeChunk = new RiffChunk("nSHP");
            ShapeChunk.addData(new IntChunkData(shapeID));
            ShapeChunk.addData(new DictChunkData()); //Empty dict
            ShapeChunk.addData(new IntChunkData(1)); //1 model
            ShapeChunk.addData(new IntChunkData(modelID)); //ID of model
            ShapeChunk.addData(new DictChunkData()); //Empty dict for model

            RiffChunk TranslateChunk = new RiffChunk("nTRN");
            IDForGroup.Add(transformID);
            TranslateChunk.addData(new IntChunkData(transformID));
            TranslateChunk.addData(new DictChunkData()); //Empty dict
            TranslateChunk.addData(new IntChunkData(shapeID));
            TranslateChunk.addData(new IntChunkData(-1)); //Reserved
            TranslateChunk.addData(new IntChunkData(0)); //Layer ID
            TranslateChunk.addData(new IntChunkData(1)); //Number of frames is always 1
            List<(string, string)> dict = new List<(string, string)>();
            dict.Add(("_t", "" + (offsetX+32) + " " + (offsetY+32) + " " + (offsetZ+32))); //Translation
            TranslateChunk.addData(new DictChunkData(dict));

            return new List<RiffChunk>(){TranslateChunk, ShapeChunk};
        }

        //Might not be needed. Create 256 identical material chunks
        public static RiffChunk createMatlChunk(int matlID)
        {
            RiffChunk matlChunk = new RiffChunk("MATL");
            matlChunk.addData(new IntChunkData(matlID));
            List<(string, string)> dict = new List<(string, string)>();
            dict.Add(("_type", "_diffuse")); //Copy of what the 3x3x3 file has
            dict.Add(("_weight", "1"));
            dict.Add(("_rough", "0.1"));
            dict.Add(("_spec", "0.5"));
            dict.Add(("_spec_p", "0.5"));
            dict.Add(("_ior", "0.3"));
            dict.Add(("_att", "0"));
            dict.Add(("_g0", "-0.5"));
            dict.Add(("_g1", "0.8"));
            dict.Add(("_gw", "0.7"));
            dict.Add(("_flux", "0"));
            dict.Add(("_idr", "0"));
            matlChunk.addData(new DictChunkData(dict));
            return matlChunk;
        }

        //This method is a mess. But no reason to waste time cleaning it.
        public static List<RiffChunk> createROBJChunks()
        {
            List<RiffChunk> chunksResult = new List<RiffChunk>();

            //Completely copied from the 3x3x3 model file

            RiffChunk robjchunkenv = new RiffChunk("rOBJ");
            List<(string, string)> dictenv = new List<(string, string)>();
            dictenv.Add(("_type", "_env"));
            dictenv.Add(("_mode", "0"));
            robjchunkenv.addData(new DictChunkData(dictenv));
            chunksResult.Add(robjchunkenv);

            RiffChunk robjchunkinf = new RiffChunk("rOBJ");
            List<(string, string)> dictinf = new List<(string, string)>();
            dictinf.Add(("_type", "_inf"));
            dictinf.Add(("_i", "0.7"));
            dictinf.Add(("_k", "255 255 255"));
            dictinf.Add(("_angle", "50 50"));
            dictinf.Add(("_area", "0.07"));
            dictinf.Add(("_disk", "0"));
            robjchunkinf.addData(new DictChunkData(dictinf));
            chunksResult.Add(robjchunkinf);

            RiffChunk robjchunk2 = new RiffChunk("rOBJ");
            List<(string, string)> dict2 = new List<(string, string)>();
            dict2.Add(("_type", "_uni"));
            dict2.Add(("_i", "0.7"));
            dict2.Add(("_k", "255 255 255"));
            robjchunk2.addData(new DictChunkData(dict2));
            chunksResult.Add(robjchunk2);

            RiffChunk robjchunkibl = new RiffChunk("rOBJ");
            List<(string, string)> dictibl = new List<(string, string)>();
            dictibl.Add(("_type", "_ibl"));
            dictibl.Add(("_i", "1"));
            dictibl.Add(("_rot", "0"));
            robjchunkibl.addData(new DictChunkData(dictibl));
            chunksResult.Add(robjchunkibl);

            RiffChunk robjchunkatm = new RiffChunk("rOBJ");
            List<(string, string)> dictatm = new List<(string, string)>();
            dictatm.Add(("_type", "_atm"));
            dictatm.Add(("_ray_d", "0.4"));
            dictatm.Add(("_ray_k", "45 104 255"));
            dictatm.Add(("_mie_d", "0.4"));
            dictatm.Add(("_mie_k", "255 255 255"));
            dictatm.Add(("_mie_g", "0.85"));
            dictatm.Add(("_o3_d", "0"));
            dictatm.Add(("_o3_k", "105 255 110"));
            robjchunkatm.addData(new DictChunkData(dictatm));
            chunksResult.Add(robjchunkatm);

            RiffChunk robjchunkfoguni = new RiffChunk("rOBJ");
            List<(string, string)> dictfoguni = new List<(string, string)>();
            dictfoguni.Add(("_type", "_fog_uni"));
            dictfoguni.Add(("_d", "0"));
            dictfoguni.Add(("_k", "255 255 255"));
            dictfoguni.Add(("_g", "0"));
            robjchunkfoguni.addData(new DictChunkData(dictfoguni));
            chunksResult.Add(robjchunkfoguni);

            RiffChunk robjchunklens = new RiffChunk("rOBJ");
            List<(string, string)> dictlens = new List<(string, string)>();
            dictlens.Add(("_type", "_lens"));
            dictlens.Add(("_prof", "0"));
            dictlens.Add(("_fov", "45"));
            dictlens.Add(("_aperture", "0.25"));
            dictlens.Add(("_blade_n", "0"));
            dictlens.Add(("_blade_r", "0"));
            robjchunklens.addData(new DictChunkData(dictlens));
            chunksResult.Add(robjchunklens);

            RiffChunk robjchunkfilm = new RiffChunk("rOBJ");
            List<(string, string)> dictfilm = new List<(string, string)>();
            dictfilm.Add(("_type", "_film"));
            dictfilm.Add(("_expo", "1"));
            dictfilm.Add(("_vig", "0"));
            dictfilm.Add(("_aces", "0"));
            dictfilm.Add(("_gam", "2.2"));
            robjchunkfilm.addData(new DictChunkData(dictfilm));
            chunksResult.Add(robjchunkfilm);

            RiffChunk robjchunk7 = new RiffChunk("rOBJ");
            List<(string, string)> dict7 = new List<(string, string)>();
            dict7.Add(("_type", "_bloom"));
            dict7.Add(("_mix", "0.5"));
            dict7.Add(("_scale", "0"));
            dict7.Add(("_aspect", "0"));
            dict7.Add(("_treshhold", "1"));
            robjchunk7.addData(new DictChunkData(dict7));
            chunksResult.Add(robjchunk7);

            RiffChunk robjchunk8 = new RiffChunk("rOBJ");
            List<(string, string)> dict8 = new List<(string, string)>();
            dict8.Add(("_type", "_ground"));
            dict8.Add(("_color", "80 80 80"));
            dict8.Add(("_hor", "0.1"));
            robjchunk8.addData(new DictChunkData(dict8));
            chunksResult.Add(robjchunk8);

            RiffChunk robjchunk9 = new RiffChunk("rOBJ");
            List<(string, string)> dict9 = new List<(string, string)>();
            dict9.Add(("_type", "_bg"));
            dict9.Add(("_color", "0 0 0"));
            robjchunk9.addData(new DictChunkData(dict9));
            chunksResult.Add(robjchunk9);

            RiffChunk robjchunk10 = new RiffChunk("rOBJ");
            List<(string, string)> dict10 = new List<(string, string)>();
            dict10.Add(("_type", "_edge"));
            dict10.Add(("_color", "0 0 0"));
            dict10.Add(("_width", "0.2"));
            robjchunk10.addData(new DictChunkData(dict10));
            chunksResult.Add(robjchunk10);

            RiffChunk robjchunk11 = new RiffChunk("rOBJ");
            List<(string, string)> dict11 = new List<(string, string)>();
            dict11.Add(("_type", "_grid"));
            dict11.Add(("_color", "0 0 0"));
            dict11.Add(("_spacing", "1"));
            dict11.Add(("_width", "0.02"));
            dict11.Add(("_display", "0"));
            robjchunk11.addData(new DictChunkData(dict11));
            chunksResult.Add(robjchunk11);

            RiffChunk robjchunksetting = new RiffChunk("rOBJ");
            List<(string, string)> dictsetting = new List<(string, string)>();
            dictsetting.Add(("_type", "_setting"));
            dictsetting.Add(("_ground", "1"));
            dictsetting.Add(("_grid", "0"));
            dictsetting.Add(("_edge", "0"));
            dictsetting.Add(("_bg_c", "0"));
            dictsetting.Add(("_bg_a", "0"));
            dictsetting.Add(("_scale", "1 1 1"));
            dictsetting.Add(("_cell", "1"));
            robjchunksetting.addData(new DictChunkData(dictsetting));
            chunksResult.Add(robjchunksetting);

            return chunksResult;
        }
    }
}
