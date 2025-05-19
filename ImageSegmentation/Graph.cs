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
        public Dictionary<(Node, Node), int> Edges;

        public Node Node((int y, int x) index)
        {
            return this.Nodes[index.y, index.x];
        }

        public static (Node, Node) MakeEdgeKey(Node a, Node b)
        {
            // if a.y > b.y (b,a) to preserve order and make sure a.y is always < b.y
            // if a.x > b.x (b,a)
            if (a.index.y == b.index.y)
                return (a.index.x < b.index.x) ? (a, b) : (b, a);
            else if (a.index.y > b.index.y) 
                return (b, a);
            return (a, b);
        }

        public int getEdge(Node n1,Node n2)
        {
            return (n1==n2)?int.MaxValue :this.Edges[MakeEdgeKey(n1, n2)];
        }
        public void CalcWeight(Node n1, Node n2,int pixel1, int pixel2)
        {
            Edges[MakeEdgeKey(n1, n2)] = pixel1 > pixel2 ? (pixel1 - pixel2) : (pixel2 - pixel1);
        }

        public PixelGraph(RGBPixel[,] picture, Func<RGBPixel, byte> GetColor) //O(N) , N: number of pixels
        {
            this.Picture = picture;
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);
            this.Nodes = new Node[picture.GetLength(0), picture.GetLength(1)];
            this.Segments = new Segments();
            this.GetColor = GetColor; // Maybe we could use it 
            this.Edges = new Dictionary<(Node, Node), int>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Nodes[y, x] = new Node();

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
                            Nodes[y, x].index = (y, x);
                            Nodes[y, x].neighbors.Add(Nodes[y + r, x + c]);
                            CalcWeight(Nodes[y,x], Nodes[y + r, x + c],
                                 GetColor(picture[y, x]), GetColor(picture[y + r, x + c])
                            );
                        }
                    }
                }
            }
        }
    }
}
