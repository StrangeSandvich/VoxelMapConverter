# VoxelMapConverter
Tool to convert Ace of Spades maps to MagicaVoxel files for use in Sectors Edge map making.

Ace of Spades was originally by Ben Aksoy. http://ace-spades.com

Later by Jagex. https://store.steampowered.com/app/224540/Ace_of_Spades_Battle_Builder/

MagicaVoxel by Ephtracy. https://ephtracy.github.io/

Check out Sectors Edge by Vercidium: https://sectorsedge.com

Special thanks to Timotheeee1 for letting me see his code for a similar project.

Ace of Spades map format: https://silverspaceship.com/aosmap/aos_file_format.html 

Note that Jagex AoS map files can be higher than 64 blocks tall

Vox format: https://github.com/ephtracy/voxel-model

# How to use
Download the Program folder with the .exe and support files 

Place Ace of Spades .vxl map in the Program folder and run the program

Put in the Ace of Spades map name. (If you want to convert MyMap.vxl, put in "MyMap")

Tell the program if you want it to keep the ocean blocks (Sectors edge will not use them...)

Let the program parse the map.

If the map has more than 255 colors the program will need to condense them. 

Give the program the name of the output file. Leave empty to use input name for output name.

Let the program write the .vox file.

Find the output file next to the program when done. 
