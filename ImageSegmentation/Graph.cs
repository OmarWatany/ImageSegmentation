using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageTemplate
{
    public struct Link
    {
        public (int x, int y) index;
        public RGBPixel weight;

        public void CalcWeight(RGBPixel pixel1, RGBPixel pixel2)
        {
            weight.red = (byte)Math.Abs(pixel1.red - pixel2.red);
            weight.blue = (byte)Math.Abs(pixel1.blue - pixel2.blue);
            weight.green = (byte)Math.Abs(pixel1.green - pixel2.green);
        }
    }

    public struct Node
    {
        private byte size;

        public (int x, int y) index;
        public Link[] neighbors;
        public void Init((int x, int y) index)
        {
            neighbors = new Link[8];
            this.index = index;
            for (int i = 0; i < neighbors.Length; i++)
                neighbors[i].index = (-1, -1);
        }
    }

    public class PixelGraph
    {
        private Node[] nodes { get; }

        public PixelGraph(RGBPixel[,] picture)
        {
            nodes = new Node[picture.Length];

            for (int y = 0; y < picture.GetLength(0); y++)
            {
                for (int x = 0; x < picture.GetLength(1); x++)
                {
                    // convert 2D Array index to 1D
                    nodes[y * picture.GetLength(1) + x].Init((x, y));

                    int linkIdx = 0;
                    for (int r = -1; r <= 1; r++)
                    {
                        for (int c = -1; c <= 1; c++)
                        {
                            if (r == 0 && c == 0) continue;
                            if (y + r < 0 || y + r >= picture.GetLength(0)) continue;
                            if (x + c < 0 || x + c >= picture.GetLength(1)) continue;
                            nodes[y * picture.GetLength(1) + x].neighbors[linkIdx].index = (x + c, y + r);
                            nodes[y * picture.GetLength(1) + x].neighbors[linkIdx].CalcWeight(
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
