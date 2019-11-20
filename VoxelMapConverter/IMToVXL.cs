﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VoxelMapConverter
{
    class IMToVXL
    {
        public static int nodeID;
        public static List<int> IDForGroup;
        public static void IMToVoxel(IntermediateMap map)
        {
            FileStream stream = new FileStream("output.vox", FileMode.Create, FileAccess.Write,
                FileShare.Write);
            BinaryWriter writer = new BinaryWriter(stream);

            //Standard opening
            writer.Write("VOX ".ToCharArray()); 
            writer.Write(150); //Version
            writer.Write("MAIN".ToCharArray());
            writer.Write(0); //No chunk data in main

            //List of chunks
            List<RiffChunk> chunks = new List<RiffChunk>();

            //Hardcode size to be 32x32x32
            RiffChunk size = new RiffChunk("SIZE");
            size.addData(new IntChunkData(32));
            size.addData(new IntChunkData(32));
            size.addData(new IntChunkData(32));

            Palette palette = new Palette();

            RiffChunk xyzi = new RiffChunk("XYZI");
            List<Voxel> voxels = map.getListOfVoxels(0, 32, 0, 32, 0, 32);
            xyzi.addData(new IntChunkData(voxels.Count));
            foreach(Voxel voxel in voxels)
            {
                int colorIndex = palette.getColorIndex(voxel.getColorTuple());

                xyzi.addData(new VoxelChunkData(Convert.ToByte(voxel.x), Convert.ToByte(voxel.y), Convert.ToByte(voxel.z), Convert.ToByte(colorIndex)));
            }

            //The tiny part that actually matters
            chunks.Add(size);
            chunks.Add(xyzi);

            //Make all the staging chunks
            resetNodeID();
            IDForGroup = new List<int>();
            int masterGRPID = getNextNodeID(); //Pass to master Translate

            //Make translate+shape node for each model
            List<RiffChunk> modelStagings = new List<RiffChunk>();
            modelStagings.AddRange(createStagingChunksForModel(0, 0, 20, 0)); //Hardcode single model

            //Master group node
            RiffChunk masterGRP = new RiffChunk("nGRP");
            masterGRP.addData(new IntChunkData(masterGRPID));
            masterGRP.addData(new DictChunkData()); //empty dict
            masterGRP.addData(new IntChunkData(modelStagings.Count / 2)); //Number of models
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

            chunks.Add(masterTRN);
            chunks.Add(masterGRP);
            chunks.AddRange(modelStagings);


            //Add in all the layer chunks
            chunks.AddRange(createLayrChunks());

            //RGBA palette chunk
            chunks.Add(palette.getPaletteChunk());

            //Material chunks
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

            //Write all those chunks
            foreach (RiffChunk chunk in chunks)
            {
                chunk.writeChunk(writer);
            }

            writer.Close();
            stream.Close();
        }

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
            dict.Add(("_t", "" + offsetX + " " + offsetY + " " + offsetZ)); //Translation
            TranslateChunk.addData(new DictChunkData(dict));

            return new List<RiffChunk>(){TranslateChunk, ShapeChunk};
        }

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

            /*          Used in the 3x3x3 example but not the newer one
                        RiffChunk robjchunk3 = new RiffChunk("rOBJ");
                        List<(string, string)> dict3 = new List<(string, string)>();
                        dict3.Add(("_type", "_rayleigh"));
                        dict3.Add(("_d", "0.4"));
                        dict3.Add(("_k", "77 153 255"));
                        robjchunk3.addData(new DictChunkData(dict3));
                        chunksResult.Add(robjchunk3);

                        RiffChunk robjchunk4 = new RiffChunk("rOBJ");
                        List<(string, string)> dict4 = new List<(string, string)>();
                        dict4.Add(("_type", "_mie"));
                        dict4.Add(("_d", "0.4"));
                        dict4.Add(("_k", "255 255 255"));
                        dict4.Add(("_g", "0.78"));
                        robjchunk4.addData(new DictChunkData(dict4));
                        chunksResult.Add(robjchunk4);

                        RiffChunk robjchunk5 = new RiffChunk("rOBJ");
                        List<(string, string)> dict5 = new List<(string, string)>();
                        dict5.Add(("_type", "_fog"));
                        dict5.Add(("_d", "0.2"));
                        dict5.Add(("_k", "255 255 255"));
                        dict5.Add(("_enable", "0"));
                        robjchunk5.addData(new DictChunkData(dict5));
                        chunksResult.Add(robjchunk5);

                        RiffChunk robjchunk6 = new RiffChunk("rOBJ");
                        List<(string, string)> dict6 = new List<(string, string)>();
                        dict6.Add(("_type", "_len"));
                        dict6.Add(("_fov", "45"));
                        dict6.Add(("_dof", "0.25"));
                        dict6.Add(("_exp", "0"));
                        dict6.Add(("_vig", "0"));
                        dict6.Add(("_sg", "0"));
                        dict6.Add(("_gam", "2.2"));
                        dict6.Add(("_blade_n", "0"));
                        dict6.Add(("_blade_r", "0"));
                        robjchunk6.addData(new DictChunkData(dict6));
                        chunksResult.Add(robjchunk6);
            */

            return chunksResult;
        }
    }
}