using CsharpVoxReader.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelMapConverter
{
    class ParcelVoxReader : CsharpVoxReader.IVoxLoader
    {
        private int modelID = 0; //Starts at 0
        private Dictionary<int, ModelTree> chunkComponents = new Dictionary<int, ModelTree>();
        private Dictionary<int, ModelLeaf> leafComponents = new Dictionary<int, ModelLeaf>();

        public ModelTree masterNode;

        public byte[,] palette;

        public void LoadModel(int sizeX, int sizeY, int sizeZ, byte[,,] data)
        {
            ModelLeaf leaf = new ModelLeaf(modelID, sizeX, sizeY, sizeZ, data);
            leafComponents.Add(modelID, leaf);
            modelID++;
        }

        public void LoadPalette(byte[,] palette)
        {
            this.palette = palette;
        }

        public void NewGroupNode(int id, Dictionary<string, byte[]> attributes, int[] childrenIds)
        {
            ModelTree group = new ModelTree(id);
            group.childChunks = new List<int>(childrenIds);
            chunkComponents.Add(id, group);
        }

        public void NewLayer(int id, Dictionary<string, byte[]> attributes)
        {
            //Ignore layers
        }

        public void NewMaterial(int id, Dictionary<string, byte[]> attributes)
        {
            //Ignore material
        }

        public void NewShapeNode(int id, Dictionary<string, byte[]> attributes, int[] modelIds, Dictionary<string, byte[]>[] modelsAttributes)
        {
            ModelTree shape = new ModelTree(id);
            shape.childModels = new List<int>(modelIds);
            chunkComponents.Add(id, shape);
        }

        public void NewTransformNode(int id, int childNodeId, int layerId, Dictionary<string, byte[]>[] framesAttributes)
        {
            ModelTree translation = new ModelTree(id);
            translation.childChunks.Add(childNodeId);
            byte[] offsets = framesAttributes[0].GetValueOrDefault("_t", null);
            if(offsets != null)
            {
                string s = Encoding.UTF8.GetString(offsets, 0, offsets.Length);

                string[] splits = s.Split(" ");
                if(splits.Length == 3)
                {

                }
            }

            chunkComponents.Add(id, translation);
        }

        public void SetMaterialOld(int paletteId, MaterialOld.MaterialTypes type, float weight, MaterialOld.PropertyBits property, float normalized)
        {
            //Ignore whatever this is
        }

        public void SetModelCount(int count)
        {
            //I don't care.
        }

        void assembleTree()
        {
            if(chunkComponents.Count == 0)
            {
                masterNode = new ModelTree(0);
                foreach(ModelLeaf leaf in leafComponents.Values)
                {
                    masterNode.components.Add(leaf);
                }
            } else
            {
                masterNode = chunkComponents.GetValueOrDefault(0, null);
                if(masterNode == null)
                {
                    throw new Exception("Parcel did not contain master node!");
                }
                foreach (int chunkID in masterNode.childChunks)
                {
                    IModelTreeComponent component = fetchComponent(chunkID, 0, 0, 0);
                    if (component != null)
                    {
                        masterNode.components.Add(component);
                    }
                }

            }
        }

        IModelTreeComponent fetchComponent(int id, int translationX, int translationY, int translationZ)
        {
            ModelTree result = chunkComponents.GetValueOrDefault(id, null);
            if(result == null)
            {
                throw new Exception("Vox referenced chunk ID " + id + " but didn't contain it");
            }
            if(result.childChunks.Count == 0)
            {
                if(result.childModels.Count == 0)
                {
                    Console.WriteLine("Encountered component with no children ID " + id);
                    return null;
                } else
                {
                    if(translationX + translationY + translationZ == 0 && result.childModels.Count == 1)
                    {
                        return leafComponents.GetValueOrDefault(0, null); //Squeeze node away
                    } else
                    {
                        result.offsetX += translationX;
                        result.offsetY += translationY;
                        result.offsetZ += translationZ;
                        foreach(int modelID in result.childModels)
                        {
                            result.components.Add(leafComponents.GetValueOrDefault(modelID));
                        }
                        return result;
                    }
                }
            } else if(result.childChunks.Count == 1)
            {
                return fetchComponent(result.childChunks[0], translationX + result.offsetX, translationY + result.offsetY, translationZ + result.offsetZ); //Squeeze node away
            } else
            {
                result.offsetX += translationX;
                result.offsetY += translationY;
                result.offsetZ += translationZ;
                foreach (int chunkID in result.childChunks)
                {
                    IModelTreeComponent component = fetchComponent(chunkID, 0, 0, 0);
                    if(component != null)
                    {
                        result.components.Add(component);
                    }
                }
                return result;
            }
        }

        public void insertReadParcel(IntermediateMap map, int offsetX, int offsetY, int offsetZ)
        {
            assembleTree();
            traverseCompositeTree(masterNode, map, offsetX, offsetY, offsetZ);
        }

        void traverseCompositeTree(IModelTreeComponent component, IntermediateMap map, int offsetX, int offsetY, int offsetZ)
        {
            if(component is ModelLeaf)
            {
                ModelLeaf model = (ModelLeaf) component;
                for(int x = 0; x < model.sizeX; x++)
                {
                    for (int y = 0; y < model.sizeY; y++)
                    {
                        for (int z = 0; z < model.sizeZ; z++)
                        {
                            int colourIndex = model.data[x, y, z]; //Implicit conversion
                            map.setBlockAt(offsetX + x, offsetY + y, offsetZ + z, new Block(colourIndex));
                        }
                    }
                }
            } else if(component is ModelTree)
            {
                ModelTree tree = (ModelTree) component;
                foreach(IModelTreeComponent child in tree.components)
                {
                    traverseCompositeTree(child, map, offsetX + tree.offsetX, offsetY + tree.offsetY, offsetZ + tree.offsetZ);
                }
            }
        }
    }
}
