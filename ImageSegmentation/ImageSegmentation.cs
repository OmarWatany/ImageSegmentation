namespace ImageTemplate
{
    public class SegmentOperations
    {
        public static RGBPixel[] CreateRandomColors(int n)
        {
            RGBPixel[] colors = new RGBPixel[n];
            for (int i = 0; i < n; i++)
            {
                colors[i].red = (byte) ((i*100)%255);
                colors[i].green = (byte)((i * 20) % 255);
                colors[i].blue = (byte)((i * 30) % 255);
            }
            return colors;
        }
        public static void ColorSegments(PixelGraph graph)
        {
            RGBPixel[] colors = CreateRandomColors(graph.noOfSegments);
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    graph.picture[i, j] = colors[graph.nodes[i, j].segmentID];
                }
            }
        }
    }

}