using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelMapConverter
{
    class RGBColor
    {
        public byte red { get; set; }
        public byte green { get; set; }
        public byte blue { get; set; }

        public RGBColor(byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public RGBColor(int red, int green, int blue)
        {
            this.red = Convert.ToByte(red);
            this.green = Convert.ToByte(green);
            this.blue = Convert.ToByte(blue);
        }

        public RGBColor(string input)
        {
            List<string> split = new List<string>(input.Split(' '));
            if(split.Count == 3)
            {
                this.red = Convert.ToByte(split[0]);
                this.green = Convert.ToByte(split[1]);
                this.blue = Convert.ToByte(split[2]);
                return;
            }

            switch (input.ToLower())
            {
                case "red":
                    this.red = 255;
                    this.green = 0;
                    this.blue = 0;
                    break;
                case "green":
                    this.red = 0;
                    this.green = 128;
                    this.blue = 0;
                    break;
                case "blue":
                    this.red = 0;
                    this.green = 0;
                    this.blue = 255;
                    break;
                case "black":
                    this.red = 0;
                    this.green = 0;
                    this.blue = 0;
                    break;
                case "white":
                    this.red = 255;
                    this.green = 255;
                    this.blue = 255;
                    break;
                case "dirt":
                    this.red = 47;
                    this.green = 30;
                    this.blue = 15;
                    break;
                case "grey":
                    this.red = 128;
                    this.green = 128;
                    this.blue = 128;
                    break;
                case "silver":
                    this.red = 192;
                    this.green = 192;
                    this.blue = 192;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

    }
}
