using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageTemplate
{
    public struct Edge
    {
        public (int x, int y) index;
        public RGBPixel weight;

        public void CalcWeight(RGBPixel pixel1, RGBPixel pixel2)
        {
            weight.red = pixel1.red > pixel2.red ? (byte)(pixel1.red - pixel2.red) : (byte)(pixel2.red - pixel1.red);
            weight.blue = pixel1.blue > pixel2.blue ? (byte)(pixel1.blue - pixel2.blue) : (byte)(pixel2.blue - pixel1.blue);
            weight.green = pixel1.green > pixel2.green ? (byte)(pixel1.green - pixel2.green) : (byte)(pixel2.green - pixel1.green);
        }
    }

    public struct Node
    {
        private byte size;

        public (int x, int y) index;
        public Edge[] neighbors;
        public void Init((int x, int y) index)
        {
            neighbors = new Edge[8];
            this.index = index;
            for (int i = 0; i < neighbors.Length; i++)
                neighbors[i].index = (-1, -1);
        }
    }
    
    public class PixelGraph
    {
        private Node[,] nodes { get; }
        private RGBPixel[,] picture;

        public PixelGraph(RGBPixel[,] picture)
        {
            this.picture = ImageTemplate.ImageOperations.GaussianFilter1D(picture, 9, 0.8); //what filter size to use?
            nodes = new Node[picture.GetLength(0),picture.GetLength(1)];

            for (int y = 0; y < picture.GetLength(0); y++)
            {
                for (int x = 0; x < picture.GetLength(1); x++)
                {
                    nodes[y, x].Init((x, y));

                    int linkIdx = 0;
                    for (int r = -1; r <= 1; r++)
                    {
                        for (int c = -1; c <= 1; c++)
                        {
                            // traverse the surrounding cells
                            if (r == 0 && c == 0) continue;
                            if (y + r < 0 || y + r >= picture.GetLength(0)) continue;
                            if (x + c < 0 || x + c >= picture.GetLength(1)) continue;
                            nodes[y, x].neighbors[linkIdx].index = (x + c, y + r);
                            nodes[y, x].neighbors[linkIdx].CalcWeight(
                                pixel1: picture[y, x], pixel2: picture[y + r, x + c]
                            );
                            linkIdx++;
                        }
                    }
                }
            }
        }

    }
}
