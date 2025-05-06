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
        public int segmentID;
        public Edge[] neighbors;
        public void Init((int x, int y) index) //O(1)
        {
            neighbors = new Edge[8];
            this.index = index;
            for (int i = 0; i < 8; i++) //O(1)
                neighbors[i].index = (-1, -1);
        }
    }
    
    public class PixelGraph
    {
        public Node[,] nodes { get; }
        public RGBPixel[,] picture;
        public int noOfSegments = 0;
        public int width, height;
        public void ColorPixel(int y,int x, int r,int g,int b)
        {
            this.picture[y,x].red = (byte)r;
            this.picture[y, x].green = (byte)g;
            this.picture[y, x].blue = (byte)b;
        }

        public PixelGraph(RGBPixel[,] picture) //O(N^2)
        {
            this.picture = ImageTemplate.ImageOperations.GaussianFilter1D(picture, 9, 0.8); //what filter size to use? //O(N^2)
            this.height = picture.GetLength(0);
            this.width = picture.GetLength(1);
            nodes = new Node[picture.GetLength(0),picture.GetLength(1)];

            for (int y = 0; y < height; y++) //O(N^2)
            {
                for (int x = 0; x < width; x++)
                {
                    
                    nodes[y, x].Init((x, y));
                    if (y < height / 2)
                        nodes[y, x].segmentID = (x < width / 2) ? 1 : 2;
                    else
                        nodes[y, x].segmentID = (x < width / 2) ? 3 : 4;
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
