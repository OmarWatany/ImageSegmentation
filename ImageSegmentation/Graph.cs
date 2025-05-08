using System;
namespace ImageTemplate
{
    public struct Edge
    {
        // Should we just save color value ?
        public (int x, int y) index;
        public byte weight;

        public void CalcWeight(byte pixel1, byte pixel2)
        {
            weight = pixel1 > pixel2 ? (byte)(pixel1 - pixel2) : (byte)(pixel2 - pixel1);
        }
    }

    public struct Node
    {

        public Segment segment;
        public Edge[] neighbors;
        public byte neighborsCount;
        public (int y, int x) index;
        public void Init() //O(1)
        {
            segment.ID = -1;
            neighbors = new Edge[8];
            for (int i = 0; i < 8; i++) //O(1)
                neighbors[i].index = (-1, -1);
        }
    }
    
    public class PixelGraph
    {
        public Node[,] Nodes;
        public RGBPixel[,] Picture;
        public int noOfSegments = -1; //To make it easier when indexing
        public int width, height;
        bool[,] visited;
        (int x, int y)[,] parent;
        Segment s;

        public PixelGraph(RGBPixel[,] picture, Func<RGBPixel,byte> GetColor) //O(N) , N: number of pixels
        {
            this.parent= new (int x, int y)[this.height, this.width];
            this.Picture = picture;
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);
            this.visited = new bool[this.height, this.width];
            this.noOfSegments = 1;
            this.s = new Segment
            {
                ID = 0,
            };
            Nodes = new Node[picture.GetLength(0),picture.GetLength(1)];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Nodes[y, x].Init();
                    for (int r = -1; r <= 1; r++)
                    {
                        if (y + r < 0 || y + r >= picture.GetLength(0)) continue;
                        for (int c = -1; c <= 1; c++)
                        {
                            // traverse the surrounding cells
                            if (x + c < 0 || x + c >= picture.GetLength(1)) continue;
                            if (r == 0 && c == 0) continue;
                            Nodes[y, x].index = (y, x);
                            Nodes[y, x].neighbors[Nodes[y,x].neighborsCount].index = (x + c, y + r);
                            Nodes[y, x].neighbors[Nodes[y,x].neighborsCount].CalcWeight(
                                 GetColor(picture[y, x]),  GetColor(picture[y + r, x + c])
                            );
                            Nodes[y, x].neighborsCount++;
                        }
                    }
                }
            }
            DFS();
        }

        public void DFS()
        {
            Node n;
            for (int i = 0; i < this.height; i++)
            {
                for (int j = 0; j < this.width; j++)
                {
                    n = Nodes[i, j];
                    this.visited[i,j] = true;
                    this.Nodes[i,j].segment = this.s;
                    for (int k = 0; k < n.neighborsCount; k++)
                    {
                        if (this.visited[n.index.y, n.index.x]) continue;
                        this.visited[n.neighbors[k].index.y, n.neighbors[k].index.x] = true;
                        this.Nodes[n.neighbors[k].index.y, n.neighbors[k].index.x].segment = this.s;
                    }
                    
                }
            }
        }


    }
}
