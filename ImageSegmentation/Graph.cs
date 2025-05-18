using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace ImageTemplate
{
    public struct Index
    {
        public int y;
        public int x;
        public Index(int y,int x)
        {
            this.y = y;
            this.x = x;
        }

        public Index((int y,int x) i)
        {
            this.y = i.y;
            this.x = i.x;
        }
    }
    public struct Node
    {
        //public Segment segment;
        public List<Index> neighbors;
        public Index index;
        public Segment segment;
        public Segment finalsegment;
        public Node(int x = 0)
        {
            this.index = new Index(-1, -1);
            neighbors = new List<Index>(8);
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
        public Dictionary<(Index,Index), int> Edges;

        public Node At(Index index) {
            return this.Nodes[index.y, index.x];
        }

        public static (Index,Index) MakeEdgeKey(Node a, Node b)
        {
            // if a.y > b.y (b,a) to preserve order and make sure a.y is always < b.y
            // if a.x > b.x (b,a)
            if (a.index.y == b.index.y)
                return (a.index.x < b.index.x) ? (a.index, b.index) : (b.index, a.index);
            else if (a.index.y > b.index.y) 
                return (b.index, a.index);
            return (a.index, b.index);
        }

        public int getEdge(Node n1, Index n2) => getEdge(n1,At(n2));

        public int getEdge(Index n1, Index n2) => getEdge(At(n1),At(n2));

        public int getEdge(Node n1,Node n2)
        {
            return this.Edges[MakeEdgeKey(n1, n2)];
        }
        public void CalcWeight(Node n1, Node n2,byte pixel1, byte pixel2)
        {
            Edges[MakeEdgeKey(n1, n2)] = pixel1 > pixel2 ? (byte)(pixel1 - pixel2) : (byte)(pixel2 - pixel1);
        }

        public PixelGraph(RGBPixel[,] picture, Func<RGBPixel, byte> GetColor) //O(N) , N: number of pixels
        {
            this.Picture = picture;
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);
            this.Nodes = new Node[picture.GetLength(0), picture.GetLength(1)];
            this.Segments = new Segments();
            this.GetColor = GetColor; // Maybe we could use it 
            this.Edges = new Dictionary<(Index,Index),int>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Nodes[y, x] = new Node(0)
                    {
                        index = new Index(y, x)
                    };
                }

            for (int y = 0; y < height; y++) // O(W*H)
            {
                for (int x = 0; x < width; x++) // O(N)
                {
                    for (int r = -1; r <= 1; r++) // O(1)
                    {
                        if (y + r < 0 || y + r >= height) continue;
                        for (int c = -1; c <= 1; c++) // O(1)
                        {
                            // traverse the surrounding cells
                            if (x + c < 0 || x + c >= width) continue;
                            if (r == 0 && c == 0) continue;
                            Nodes[y, x].neighbors.Add(Nodes[y + r, x + c].index);
                            CalcWeight(Nodes[y,x], Nodes[y + r, x + c],
                                 GetColor(picture[y, x]), GetColor(picture[y + r, x + c])
                            );
                        }
                    }
                }
            }
        }

        public int CountUnSegmented()
        {
            int count = 0;
            Node node;
            for(var y=0;y < this.height; y++)
            {
                for(var x=0;x < this.height; x++)
                {
                    node = this.Nodes[y, x];
                    if ( node.segment.ID == -1
                      || node.segment.ID == 0
                      || node.segment.ID == 1) count++;
                }
            }
            return count;
        }
    }
}
