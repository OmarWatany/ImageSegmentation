using System;
using System.Collections.Generic;
using System.Threading;

namespace ImageTemplate
{
    public struct Segment
    {
        public int ID, internalDifference, count;
        public List<Node> nodes;
        //TODO: CalculateInternalDifference
    };

    public class Segments
    {
        public List<Segment> segments;
        public int Count
        {
            get  => segments.Count;
        }

        public Segments()
        {
            segments = new List<Segment>();
        }

        // return new segment's ID
        public int New()
        {
            segments.Add(new Segment { ID = segments.Count });
            return segments.Count - 1;
        }

        public int Add(Segment seg)
        {
            segments.Add(seg);
            return segments.Count - 1;
        }

        public Segment At(int segmentID)
        {
            if (segmentID < 0) return new Segment { ID = -1 }; // Empty Segment
            return  segments[segmentID];
        }

        public Segment Last() => segments[this.Count];
    }

    public static class SegmentOperations
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

        public static void CreateSegment(PixelGraph graph, (int y, int x) index)
        {

            Segment newSegment = new Segment
            {
                ID = graph.Segments.Count,
                count = 1,
                nodes = new List<Node> { graph.Nodes[index.y,index.x] }
            };
            graph.Nodes[index.y, index.x].segmentID = newSegment.ID;

            graph.Segments.Add(newSegment);
        }

        public static void AddToSegment(Segment segment, Node node)
        {
            node.segmentID = segment.ID;
            segment.count++;
            segment.nodes.Add(node);
        }

        public static int InternalDifference(PixelGraph graph, Segment segment)
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
                    if (edge.node.segmentID == segment.ID)
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
                Node root2 = FindRoot(graph.Segments.segments[edges[i].node.segmentID].nodes[0], parent);
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
                    if ((myNode.neighbors[j].node.segmentID == bigSegment.ID) && (myNode.neighbors[j].weight < minEdge))
                    {
                        minEdge = myNode.neighbors[j].weight;
                    }
                }
            }
            return (minEdge < int.MaxValue) ? minEdge : -1; //return -1 if no connecting edges
        }

        public static bool SegmentsComparison(PixelGraph graph, Segment s1, Segment s2, int k)
        {
            int internalDiff1 = InternalDifference(graph, s1);
            int internalDiff2 = InternalDifference(graph, s2);
            int segmentsDifference = SegmentsDifference(s1, s2);
            if (segmentsDifference == -1) return true; //it returns -1 when no edges are common , so we should return true, which means don't merge
            int tao1 = k / s1.count;
            int tao2 = k / s2.count;
            int MInt = Math.Min(internalDiff1 + tao1, internalDiff2 + tao2);
            return (segmentsDifference > MInt);
        }

        public static void MergeSegments(PixelGraph graph, Segment s1, Segment s2)
        {
            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            Node temp;
            for (int i = 0; i < smallSegment.count; i++)
            {
                bigSegment.count++;
                temp = smallSegment.nodes[i];

                temp.segmentID = bigSegment.ID;
                bigSegment.nodes.Add(temp);

                //should be unnecessary , just for debugging , and to make sure
                smallSegment.nodes.RemoveAt(i);
                smallSegment.count--;
            }
            graph.Segments.segments.Remove(smallSegment);
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
                    if (IsUnsegmented(myNode)) CreateSegment(channelGraph, myNode.index);

                    for (int l = 0; l < myNode.neighborsCount; l++)
                    {
                        neighbor = myNode.neighbors[l].node;

                        if (AreSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            CreateSegment(channelGraph, neighbor.index);
                        }

                        //if the function returns true, then no merging and continue, else : merge
                        if (!SegmentsComparison(channelGraph,
                            channelGraph.Segments.At(neighbor.segmentID),
                            channelGraph.Segments.At(myNode.segmentID),
                            k))
                        {
                            MergeSegments(channelGraph, 
                                channelGraph.Segments.At(neighbor.segmentID),
                                channelGraph.Segments.At(myNode.segmentID)
                            );
                        }
                    }
                }
            }
        }

        public static void ColorSegments(PixelGraph graph) //O(N) , N: number of pixels
        {
            RGBPixel[] colors = CreateRandomColors(graph.Segments.Count);
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    int id = graph.Nodes[i, j].segmentID;
                    RGBPixel c= colors[id];
                    graph.Picture[i, j] = c;
                }
            }
        }

        //Helper functions

        private static bool AreSameSegment(Node a, Node b) => a.segmentID == b.segmentID;

        private static Node FindRoot(Node node, Dictionary<Node, Node> parent)
        {
            if (!parent[node].Equals(node))
            {
                parent[node] = FindRoot(parent[node], parent);
            }
            return parent[node];
        }

        private static bool IsUnsegmented(Node node) => node.segmentID == -1;
    }
}