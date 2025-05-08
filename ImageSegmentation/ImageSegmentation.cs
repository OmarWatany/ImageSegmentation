using System.Collections.Generic;
using System.Net.Security;

namespace ImageTemplate
{
    public struct Segment
    {
        public int ID, internalDifference, count;
        public List<Node> nodes;
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
        // TODO: Find Intersection between red, blue and green

        public static void CreateSegment(PixelGraph graph, Node n1, Node n2)
        {
            Segment newSegment = new Segment
            {
                ID = ++graph.noOfSegments,
                count = 2,
                nodes = new List<Node> { n1, n2 } 
            };
            n1.segment = n2.segment = newSegment;
        }

        public static void AddToSegment(Node node, Segment segment)
        {
            node.segment = segment;
            segment.count++;
            segment.nodes.Add(node);
        }

        private static bool PixelsMatch(Node a, Node b)
        {
            // TODO: Implement your actual pixel matching logic
            return true;
        }

        public static void SegmentChannel(PixelGraph channelGraph)
        {
            Node neighbor,myNode;
            for (int i = 0; i < channelGraph.height; i++)
            {
                for (int j = 0; j < channelGraph.width; j++)
                {
                    myNode = channelGraph.Nodes[i, j];
                    foreach (Edge edge in myNode.neighbors)
                    {
                        neighbor = channelGraph.Nodes[edge.index.y, edge.index.x];
                        if (IsSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            HandleUnsegmentedNeighbor(channelGraph, myNode, neighbor);
                        }
                        else
                        {
                            HandleSegmentedNeighbor(myNode, neighbor);
                        }
                    }
                }
            }
        }

        public static void ColorSegments(PixelGraph graph) //O(N) , N: number of pixels
        {
            RGBPixel[] colors = CreateRandomColors(graph.noOfSegments);
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    graph.Picture[i, j] = colors[graph.Nodes[i, j].segment.ID];
                }
            }
        }

        //Helper functions

        private static bool IsSameSegment(Node a, Node b)
        {
            return a.segment.ID == b.segment.ID;
        }

        private static bool IsUnsegmented(Node node)
        {
            return node.segment.ID == -1;
        }
        private static void HandleUnsegmentedNeighbor(PixelGraph graph, Node myNode, Node neighbor)
        {
            if (!PixelsMatch(myNode, neighbor))
                return;

            if (IsUnsegmented(myNode))
            {
                CreateSegment(graph, myNode, neighbor);
            }
            else
            {
                AddToSegment(neighbor, myNode.segment);
            }
        }

        private static void HandleSegmentedNeighbor(Node myNode, Node neighbor)
        {
            if (IsUnsegmented(myNode))
            {
                if (PixelsMatch(myNode, neighbor))
                {
                    AddToSegment(myNode, neighbor.segment);
                }
            }
            else
            {
                // TODO: Apply segment comparison/merging logic
            }
        }
    }
}