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
                    if (i<graph.height/2)
                    {
                        if (j<graph.width/2)
                            graph.ColorPixel(i, j, 2, 3, 140);
                        else
                            graph.ColorPixel(i, j, 10, 200, 4);
                    }
                    else
                    {
                        if (j < graph.width / 2)
                            graph.ColorPixel(i, j, 0, 0, 0);
                        else
                            graph.ColorPixel(i, j, 190, 200, 0);
                    }
                }
            }
        }
    }

}