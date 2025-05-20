using System;
using System.Collections.Generic;
namespace ImageTemplate
{
    public class Node
    {
        //public Segment segment;
        public List<Node> neighbors;
        public (int y, int x) index;
        public Segment segment;
        public Segment finalsegment;
        public Node()
        {
            neighbors = new List<Node>(8);
            segment = Segment.EmptySegment;
            finalsegment = Segment.EmptySegment;
        }
    }

    public class PixelGraph
    {
        public Node[,] Nodes;
        public RGBPixel[,] Picture;
        public Segments Segments;
        public int width, height;
        public Func<RGBPixel, byte> GetColor;
        public List<Edge> Edges;
        public byte CalcWeight(byte pixel1, byte pixel2)
        {
             return pixel1 > pixel2 ? (byte)(pixel1 - pixel2) : (byte)(pixel2 - pixel1);
        }

        public PixelGraph(RGBPixel[,] picture, Func<RGBPixel, byte> GetColor) //O(N) , N: number of pixels
        {
            this.Picture = picture;
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);
            this.Nodes = new Node[picture.GetLength(0), picture.GetLength(1)];
            this.Segments = new Segments();
            this.GetColor = GetColor; // Maybe we could use it 
            this.Edges = new List<Edge>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Nodes[y, x] = new Node();

            for (int y = 0; y < height; y++) // O(W*H)
            {
                for (int x = 0; x < width; x++) // O(N)
                {
                    Nodes[y, x].index = (y, x);
                    if (x + 1 != width)
                    {
                        Nodes[y, x].neighbors.Add(Nodes[y, x + 1]);
                        Edges.Add(new Edge(Nodes[y, x], Nodes[y, x + 1], CalcWeight(GetColor(picture[y, x]), GetColor(picture[y, x + 1]))));
                        if (y + 1 != height)
                        {
                            Nodes[y, x].neighbors.Add(Nodes[y + 1, x + 1]);
                            Edges.Add(new Edge(Nodes[y, x], Nodes[y + 1, x + 1], CalcWeight(GetColor(picture[y, x]), GetColor(picture[y + 1, x + 1]))));
                        }
                    }
                    if (y + 1 != height)
                    {
                        Nodes[y, x].neighbors.Add(Nodes[y + 1, x]);
                        Edges.Add(new Edge(Nodes[y, x], Nodes[y + 1, x], CalcWeight(GetColor(picture[y, x]), GetColor(picture[y + 1, x]))));
                    }
                }
            }
        }
    }
}
