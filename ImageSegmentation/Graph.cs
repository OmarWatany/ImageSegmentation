using System;
using System.Collections.Generic;
using System.ComponentModel;
namespace ImageTemplate
{
    public struct NeighborList
    {
        Node[] List_;
        public int Count;
        public NeighborList(int count = 8)
        {
            Count = 0;
            List_ = new Node[count];
        }

        public Node this[int index]
        {
            get
            {
                return List_[index];
            }
            set
            {
                List_[index] = value;
                Count++;
            }
        }

        public void Add(Node node)
        {
            List_[Count++] = node;
        }
    }

    public class Node
    {
        //public Segment segment;
        public NeighborList neighbors;
        public (int y, int x) index;
        public Segment segment;

        public Node(int y, int x)
        {
            index = (y, x);
            neighbors = new NeighborList(8);
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
            this.Segments = new Segments(this);
            this.GetColor = GetColor; // Maybe we could use it 
            this.Edges = new List<Edge>();

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
                        for(int c = -1; c <= 1; c++)
                        {
                            if (r == 0 && c == 0) continue;
                            if (c + x == -1 || c + x >= this.width) continue;
                            Nodes[y, x].neighbors.Add(Nodes[r + y, c + x]);
                            Edges.Add(new Edge(Nodes[y, x], Nodes[r + y, c + x], CalcWeight(GetColor(picture[y, x]), GetColor(picture[r + y, c + x]))));
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

    }
}
