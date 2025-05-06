namespace ImageTemplate
{
    public class SegmentOperations
    {
        static public void ColorSegments(PixelGraph graph)
        {
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    switch (graph.nodes[i, j].segmentID)
                    {
                        case 1:
                            graph.ColorPixel(i, j, 2, 3, 140);
                            break;
                        case 2:
                            graph.ColorPixel(i, j, 10, 200, 4);
                            break;
                        case 3:
                            graph.ColorPixel(i, j, 0, 0, 0);
                            break;
                        case 4:
                            graph.ColorPixel(i, j, 190, 200, 0);
                            break;
                    }
                }
            }
        }
    }

}