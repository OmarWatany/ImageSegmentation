using System;
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

        public static int InternalDifference(Segment segment)
        {
            return -1;
        }

        public static int ComponentsDifference(Segment s1,Segment s2) //O(N) , N: number of nodes in the smaller segment
        {
            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            Node myNode;
            int minEdge = int.MaxValue;
            for (int i = 0; i < smallSegment.count; i++) 
            {
                myNode = smallSegment.nodes[i];
                //for every node in the smaller segment:
                for (int j = 0; j < myNode.neighborsCount; j++)
                {
                    //check if any of its neighbors are from the other segment, and if their edge is  smaller than the current minimum
                    if ((myNode.neighbors[j].node.segment.ID == bigSegment.ID) && (myNode.neighbors[j].weight < minEdge))
                    {
                            minEdge = myNode.neighbors[j].weight;
                    }
                }
            }
            return (minEdge < int.MaxValue) ? minEdge : -1; //return -1 if no connecting edges
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
                        neighbor = channelGraph.Nodes[edge.node.index.y, edge.node.index.x];
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
            //if (!PixelsMatch(myNode, neighbor))
                //return;

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
                //if (PixelsMatch(myNode, neighbor))
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