#AoS Converter Readme

Download the Program folder with the .exe and support files 

Place Ace of Spades .vxl map in the Program folder and run the program

The function name for conversion is "aosConverter"

Parameters taken by the converter:

For conversion:
"aosFile", string, name of the Ace of Spades map
"keepOcean", boolean, determines if the lowest block, which is usually ocean, should be kept
"colourCount", int, Number of unique colours to keep. Maximum of 230, though Sectors Edge itself can only handle about 40 unique block textures

For writing the map:
"mirrorMap", boolean, will mirror the map on the y axis if true
"modelSizeX", int, size of MagicaVoxel model, max of 256, optional, defaults to max
"modelSizeY", int, size of MagicaVoxel model, max of 256, optional, defaults to max
"modelSizeZ", int, size of MagicaVoxel model, max of 256, optional, defaults to max
"outputName", string, name of output. If none will randomize.

Note that Ace of Spades maps have a bunch of "solid" blocks that it doesn't store color for. All these blocks are given color index 1 and can be changed in MagicaVoxel.
Sectors Edge has a number of block indexes that are reserved for special purposes. See the map making guide on the forum.