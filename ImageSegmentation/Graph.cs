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

        public PixelGraph(RGBPixel[,] picture, Func<RGBPixel,byte> GetColor) //O(N) , N: number of pixels
        {
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);

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
                            Nodes[y, x].neighbors[Nodes[y,x].neighborsCount].index = (x + c, y + r);
                            Nodes[y, x].neighbors[Nodes[y,x].neighborsCount].CalcWeight(
                                 GetColor(picture[y, x]),  GetColor(picture[y + r, x + c])
                            );
                            Nodes[y, x].neighborsCount++;
                        }
                    }
                }
            }
        }

    }
}
