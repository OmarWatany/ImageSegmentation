using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace ImageTemplate
{
    public class Node
    {
        //public Segment segment;
        public (int y, int x) index;
        public Segment segment;

        public Node(int y, int x)
        {
            index = (y, x);
            segment = Segment.EmptySegment;
        }
    }

    public class PixelGraph
    {
        public Node[,] Nodes;
        public RGBPixel[,] Picture;
        public Segments Segments;
        public int width, height;
        public Func<RGBPixel, byte> GetColor;
        public List<Edge>[] Edges = new List<Edge>[256];
        public byte CalcWeight(byte pixel1, byte pixel2) => 
             pixel1 > pixel2 ? (byte)(pixel1 - pixel2) : (byte)(pixel2 - pixel1);

        public PixelGraph(RGBPixel[,] picture, Func<RGBPixel, byte> GetColor) //O(N) , N: number of pixels
        {
            this.Picture = picture;
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);
            this.Nodes = new Node[picture.GetLength(0), picture.GetLength(1)];
            this.Segments = new Segments(this);
            this.GetColor = GetColor; // Maybe we could use it 

            for(int i = 0; i < 256; i++)
                this.Edges[i] = new List<Edge>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Nodes[y, x] = new Node(y, x);

            for (int y = 0; y < height; y++) // O(W*H)
            {
                for (int x = 0; x < width; x++) // O(N)
                {
                    for (int r = -1; r <= 1; r++) 
                    {
                        if (r + y == -1 || r + y >= this.height) continue;
                        for(int c = 0; c <= 1; c++)
                        {
                            if (r == 0 && c == 0) continue;
                            if (c + x == -1 || c + x >= this.width) continue;
                            byte w = CalcWeight(GetColor(picture[y, x]), GetColor(picture[r + y, c + x]));
                            Edges[w].Add(new Edge(Nodes[y, x], Nodes[r + y, c + x]));
                        }
                    }
                }
            }
        }

        public Node this[int y, int x]
        {
            get => this.Nodes[y, x];
            set => this.Nodes[y, x] = value;
        }

        public Node this[(int y, int x) idx]
        {
            get => this.Nodes[idx.y, idx.x];
            set => this.Nodes[idx.y, idx.x] = value;
        }

    }
}
