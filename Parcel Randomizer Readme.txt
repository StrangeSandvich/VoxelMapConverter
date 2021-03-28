#parcel randomizer README

The parcel randomizer takes a list of parcels, a primary "layout" parcel and then fills any plots in the parcels with the provided parcels

The function name for the parcel randomizer is "parcel"

The input for the parcel randomizer is just the configuration file, called "parcelConfiguration"

The input for then writting out the map is the same as for the converter:
"mirrorMap", boolean, will mirror the map on the y axis if true
"modelSizeX", int, size of MagicaVoxel model, max of 256, optional, defaults to max
"modelSizeY", int, size of MagicaVoxel model, max of 256, optional, defaults to max
"modelSizeZ", int, size of MagicaVoxel model, max of 256, optional, defaults to max
"outputName", string, name of output. If none will randomize.

# Parcel configuration file
Configuration of the parcels is stored in a .json configuration file
Each parcel has a size, map file name, connections and a list of plots inside it
Every plot has a size, an offset into the map and connections. Here is an example parcel:

{
    "sizeX": 32,
	"sizeY": 32,
	"sizeZ": 32,
	"sourceFile": "32_32_32_crater.vox",
	"connections": {
		"connectionNames": [
			"8ground",
			"8ground",
			"8ground",
			"8ground",
			"sky",
			"ground",
		]
	},
	"parcelPlots": [
		{
			"sizeX": 8,
			"sizeY": 8,
			"sizeZ": 32,
			"offsetX": 0,
			"offsetY": 24,
			"offsetZ": 0,
			"connections": {
				"connectionNames": [
				"8ground",
				"8ground",
				"8ground",
				"8ground",
				"sky",
				"ground",
				]
			}
		}
	],
}

This parcel is 32 x 32 x 32, it's stored in 32_32_32_crater.vox and it has a 8x8x32 plot inside it.

The connections defines which parcels fit in what plots. There is a connection for each direction, starting with positive X, then negative X, positive Y, negative Y, positive Z and negative Z.
In the frost biome I use "8ground" for horizontal connections, "sky" for the sky and "ground" for the negative Z direction into the ground.

The full configuration file is built as such:

"primaryParcel": Parcel
"insertParcels": List of Parcel
"connections": List of Connections
"plotDivisions": List of plotDivisions

The connections list should hold every connection used, a connection looks like this:
{
	"name": "8ground",
	"flip": true,
	"rotate": false,
}
This defines if the connection face can be mirrored, and if it can be rotated. This functionality is currently unused

The plotDivisions automatically creates empty parcels filled with smaller plots. It's convinient for dividing up a big plot.
It looks like this:

{
	"sizeX": 96,
	"sizeY": 192,
	"sizeZ": 32,
	"divisionsX": [32],
	"divisionsY": [32],
	"divisionsZ": [32],
	"connections": {
		"connectionNames": [
			"8ground",
			"8ground",
			"8ground",
			"8ground",
			"sky",
			"ground",
		]
	}
}

The divisions input defines how the parcel is divided into plots. Here it is divided into plots of 32 by 32 by 32. You can also make a full list of divisions