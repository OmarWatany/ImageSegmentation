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
                colors[i].red = (byte)((i * 100) % 255);
                colors[i].green = (byte)((i * 20) % 255);
                colors[i].blue = (byte)((i * 30) % 255);
            }
            return colors;
        }
        // TODO: Find Intersection between red, blue and green

        public static void CreateSegment(PixelGraph graph, Node n)
        {
            Segment newSegment = new Segment
            {
                ID = ++graph.noOfSegments,
                count = 2,
                nodes = new List<Node> { n }
            };
            n.segment = newSegment;
        }

        public static void AddToSegment(Node node, Segment segment)
        {
            node.segment = segment;
            segment.count++;
            segment.nodes.Add(node);
        }

        public static int InternalDifference(Segment segment)
        {
            // If the segment has 1 or less than 1, return 0
            if (segment.count <= 1) return 0;

            List<Edge> edges = new List<Edge>();
            // Collect all edges where both nodes are in the same segment
            for (int i = 0; i < segment.nodes.Count; i++)
            {
                Node node = segment.nodes[i];
                for (int j = 0; j < node.neighbors.Length; j++)
                {
                    Edge edge = node.neighbors[j];
                    if (edge.node.segment.ID == segment.ID)
                    {
                        edges.Add(edge);
                    }
                }
            }

            // Sort edges by weight  (ascending)
            edges.Sort((a, b) => a.weight.CompareTo(b.weight));

            // Initialize union-find structure for Kruskal's algorithm
            Dictionary<Node, Node> parent = new Dictionary<Node, Node>();
            for (int i = 0; i < segment.nodes.Count; i++)
            {
                parent[segment.nodes[i]] = segment.nodes[i];
            }

            int maxEdgeWeight = 0;
            // build the maximum spanning tree within the segment
            for (int i = 0; i < edges.Count; i++)
            {
                Node root1 = FindRoot(edges[i].node, parent);
                Node root2 = FindRoot(edges[i].node.segment.nodes[0], parent);
                if (!root1.Equals(root2))
                {
                    parent[root1] = root2;
                    if (edges[i].weight > maxEdgeWeight)
                    {
                        maxEdgeWeight = edges[i].weight;
                    }
                }
            }

            // Return the maximum edge weight in the segment
            return maxEdgeWeight;
        }

        public static int SegmentsDifference(Segment s1, Segment s2) //O(N) , N: number of nodes in the smaller segment
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

        public static bool SegmentsComparison(Segment s1, Segment s2, int k)
        {
            int internalDiff1 = InternalDifference(s1);
            int internalDiff2 = InternalDifference(s2);
            int segmentsDifference = SegmentsDifference(s1, s2);
            if (segmentsDifference == -1) return true; //it returns -1 when no edges are common , so we should return true, which means don't merge
            int tao1 = k / s1.count;
            int tao2 = k / s2.count;
            int MInt = Math.Min(internalDiff1 + tao1, internalDiff2 + tao2);
            return (segmentsDifference > MInt);
        }

        public static void MergeSegments(PixelGraph graph, Segment s1, Segment s2)
        {
            graph.noOfSegments--;
            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            Node temp;
            for (int i = 0; i < smallSegment.count; i++)
            {
                bigSegment.count++;
                temp = smallSegment.nodes[i];

                temp.segment = bigSegment;
                bigSegment.nodes.Add(temp);

                //should be unnecessary , just for debugging , and to make sure
                smallSegment.nodes.RemoveAt(i);
                smallSegment.count--;
            }
        }
        public static void SegmentChannel(PixelGraph channelGraph, int k)
        {
            Node neighbor, myNode;
            for (int i = 0; i < channelGraph.height; i++)
            {
                for (int j = 0; j < channelGraph.width; j++)
                {
                    myNode = channelGraph.Nodes[i, j];

                    //create a segment just for this node so we can use segments comparison function
                    if (IsUnsegmented(myNode)) CreateSegment(channelGraph, myNode);

                    foreach (Edge edge in myNode.neighbors)
                    {
                        neighbor = channelGraph.Nodes[edge.node.index.y, edge.node.index.x];

                        if (AreSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            CreateSegment(channelGraph, neighbor);
                        }

                        //if the function returns true, then no merging and continue, else : merge
                        if (!SegmentsComparison(neighbor.segment, myNode.segment, k))
                        {
                            MergeSegments(channelGraph, myNode.segment, neighbor.segment);
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

        private static bool AreSameSegment(Node a, Node b)
        {
            return a.segment.ID == b.segment.ID;
        }

        private static bool IsUnsegmented(Node node)
        {
            return node.segment.ID == -1;
        }
        private static Node FindRoot(Node node, Dictionary<Node, Node> parent)
        {
            if (!parent[node].Equals(node))
            {
                parent[node] = FindRoot(parent[node], parent);
            }
            return parent[node];
        }

    }
}