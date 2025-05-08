using System.Collections.Generic;

namespace ImageTemplate
{
    public struct Segment
    {
        public int ID, internalDifference, count;
        //TODO: CalculateInternalDifference
    };

    public class SegmentOperations
    {
        public static RGBPixel[] CreateRandomColors(int m) //O(M) , M: number of segments
        {
            RGBPixel[] colors = new RGBPixel[m];
            for (int i = 0; i < m; i++)
            {
                colors[i].red = (byte) ((i*100)%255);
                colors[i].green = (byte)((i * 20) % 255);
                colors[i].blue = (byte)((i * 30) % 255);
            }
            return colors;
        }
        public static void ColorSegments(PixelGraph graph) //O(N) , N: number of pixels
        {
            RGBPixel[] colors = CreateRandomColors(graph.noOfSegments);
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    // TODO: Find Intersection between red, blue and green
                    graph.Picture[i, j] = colors[graph.Nodes[i, j].segment.ID];
                }
            }
        }

        public static void CreateSegment(PixelGraph graph, Node n1, Node n2)
        {
            Segment newSegment = new Segment
            {
                ID = ++graph.noOfSegments,
                count = 2 
            };
            n1.segment = n2.segment = newSegment;
        }
        public static void SegmentChannel(PixelGraph channelGraph)
        {
            Queue<Node> q;
            for (int i = 0; i < channelGraph.height; i++)
            {
                for (int j = 0; j < channelGraph.width; j++)
                {
                        //compare pixel with its neighbors
                        //for pixel in neighbors:
                            //if neighbor isnt in a segment:
                                //if they don't match each other
                                    // enqueue this node in the q

                                //else if they match:
                                    //if my pixel is already in a segment:
                                            //set its segment id

                                    //else if my pixel isn't in a segment:
                                        //create a new segment with each other
                                        //increment noOfSegments  , and give them a segment id       

                            //if neighbor is already in a segment:
                                //apply segments comparison function to decide whether to merge these regions or not
                }
            }
        }
    }
}