using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;

//Main problem right now: every pixel is a segment on its own
namespace ImageTemplate
{
    public class Segment
    {
        public int count
        {
            get => nodes.Count;
        }

        public int ID, internalDifference;
        public List<Node> nodes;
        public Dictionary<(Node, Node), int> Edges; //contains all edges of the segment
        bool[] visited;
        public Segment()
        {
            this.nodes = new List<Node>();
            this.Edges = new Dictionary<(Node, Node), int>();
        }
        public void Add(PixelGraph graph, Node node)
        {
            node.segment = this;
            this.nodes.Add(node);
            foreach (Node neighbor in node.neighbors)
            {
                if(neighbor.segment ==this)
                {
                    this.Edges[PixelGraph.MakeEdgeKey(node,neighbor)]=graph.getEdge(node,neighbor);
                }
            }
        }
        public int getEdge(Node n1, Node n2)
        {
            return this.Edges[PixelGraph.MakeEdgeKey(n1, n2)];
        }

        public int InternalDifference(PixelGraph graph)
        {
            // If the segment has 1 or less than 1, return 0
            if (this.count <= 1) return 0;

            int maxEdgeWeight = -1;
            foreach (var edge in Edges)
            {
                if (DisConnected(edge.Key.Item1, edge.Key.Item2) && edge.Value > maxEdgeWeight)
                    maxEdgeWeight = edge.Value;
            }

            // Return the maximum edge weight in the segment
            return maxEdgeWeight;
        }
        public int SegmentsDifference(PixelGraph graph, Segment s2) //O(N) , N: number of nodes in the smaller segment
        {
            Segment smallSegment = (this.count < s2.count) ? this : s2;
            Segment bigSegment = (this.count < s2.count) ? s2 : this;
            Node myNode;
            int minEdge = int.MaxValue;
            for (int i = 0; i < smallSegment.count; i++)
            {
                myNode = smallSegment.nodes[i];
                //for every node in the smaller segment:
                for (int j = 0; j < myNode.neighbors.Count; j++)
                {
                    //check if any of its neighbors are from the other segment, and if their edge is  smaller than the current minimum
                    if ((myNode.neighbors[j].segment.ID == bigSegment.ID) && (graph.getEdge(myNode,myNode.neighbors[j]) < minEdge))
                    {
                        minEdge = graph.getEdge(myNode, myNode.neighbors[j]);
                    }
                }
            }
            return (minEdge < int.MaxValue) ? minEdge : -1; //return -1 if no connecting edges
        }

        public bool SegmentsComparison(PixelGraph graph, Segment s2, int k)
        {
            int internalDiff1 = this.InternalDifference(graph);
            int internalDiff2 = s2.InternalDifference(graph);
            int segmentsDifference = this.SegmentsDifference(graph, s2);
            if (segmentsDifference == -1) return true; //it returns -1 when no edges are common , so we should return true, which means don't merge
            int tao1 = k / this.count;
            int tao2 = k / s2.count;
            int MInt = Math.Min(internalDiff1 + tao1, internalDiff2 + tao2);
            return (segmentsDifference > MInt);
        }

        public static Segment EmptySegment = new Segment
        {
            ID = -1
        };
        public bool DisConnected(Node n1, Node n2)
        {
            visited= new bool[nodes.Count];
            DFS(0, visited,n1,n2);
            return visited.Any(x => !x);
        }
        //i: current vertex index  // j: current edge to pass
        //This implementation of DFS checks for connectivity whe removing a certain edge
        private void DFS(int i,bool[] visited,Node n1,Node n2)
        {
            visited[i] = true;
            foreach (Node neighbor in nodes[i].neighbors)
            {
                if ((nodes[i] == n1 && neighbor == n2) || (nodes[i] == n2 && neighbor == n1)) continue; //Do not iterate on this edge

                if ((neighbor.segment == this) && !visited[nodes.IndexOf(neighbor)])
                    DFS(nodes.IndexOf(neighbor), visited,n1,n2);
            }
        }
    }

    public class Segments
    {

        public List<Segment> segments;
        public int Count
        {
            get => segments.Count;
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
            return segments[segmentID];
        }

        public Segment Last() => segments[this.Count];



        public void MergeSegments(PixelGraph graph, Segment s1, Segment s2)
        {
            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            for (int i = 0; i < smallSegment.count; i++)
            {
                bigSegment.Add(graph, smallSegment.nodes[i]);
            }
            this.segments.Remove(smallSegment); // O(n) operation
            // When smallSegment get's removed the segments after it get changed so the id
        }

        // NOTE: Probably class Segments' operation
        public RGBPixel[] CreateRandomColors(int m) //O(M) , M: number of segments
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

        public void CreateSegment(PixelGraph graph, (int y, int x) index)
        {

            Segment newSegment = new Segment
            {
                ID = ++graph.segmentIdIncrement,
                nodes = new List<Node> { graph.Nodes[index.y, index.x] }
            };
            graph.Nodes[index.y, index.x].segment = newSegment;

            graph.Segments.Add(newSegment);
        }

        public void AddToSegment(PixelGraph graph, Segment segment, Node node) => segment.Add(graph,node);

        public void SegmentChannel(PixelGraph channelGraph, int k)
        {
            Node neighbor, myNode;
            for (int i = 0; i < channelGraph.height; i++)
            {
                for (int j = 0; j < channelGraph.width; j++)
                {
                    myNode = channelGraph.Nodes[i, j];

                    //create a segment just for this node so we can use segments comparison function
                    if (IsUnsegmented(myNode)) CreateSegment(channelGraph, myNode.index);

                    for (int l = 0; l < myNode.neighbors.Count; l++)
                    {
                        neighbor = myNode.neighbors[l];

                        if (AreSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            CreateSegment(channelGraph, neighbor.index);
                        }

                        //if the function returns true, then no merging and continue, else : merge
                        // we should stop getting segments using it's ID because its different from it's place #important
                        if (!neighbor.segment 
                            .SegmentsComparison(channelGraph,
                            myNode.segment,
                            k))
                        {
                            MergeSegments(channelGraph,
                                neighbor.segment,
                                myNode.segment
                            );
                        }
                    }
                }
            }
        }

        public void ColorSegments(PixelGraph graph) //O(N) , N: number of pixels
        {
            RGBPixel[] colors = CreateRandomColors(graph.Segments.Count);
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    int id = graph.Segments.segments.IndexOf(graph.Nodes[i, j].segment);
                    RGBPixel c = colors[id];
                    graph.Picture[i, j] = c;
                }
            }
        }

        //Helper functions
        // Should we compare using pointer value ? I don't think so #important
        public static bool AreSameSegment(Node a, Node b) => a.segment.ID == b.segment.ID;
        private bool IsUnsegmented(Node node) => node.segment.ID == -1;
    }
}
