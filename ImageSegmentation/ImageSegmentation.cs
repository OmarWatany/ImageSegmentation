using System;
using System.Collections.Generic;
using System.Linq;

//Main problem right now: every pixel is a segment on its own
namespace ImageTemplate
{
    public class Segment
    {
        public int count
        {
            get => nodes.Count;
        }

        public int ID;
        public List<Node> nodes;
        public Dictionary<(Node, Node), int> Edges; //contains all edges of the segment
        public Segment()
        {
            this.nodes = new List<Node>();
            this.Edges = new Dictionary<(Node, Node), int>();
        }

        public void Add(Node node)
        {
            node.segment = this;
            this.nodes.Add(node);
        }

        public void Add(PixelGraph graph, Node node)
        {
            node.segment = this;
            this.nodes.Add(node);
            foreach (Node neighbor in node.neighbors)
            {
                if(neighbor.segment == this)
                {
                    this.Edges[PixelGraph.MakeEdgeKey(node,neighbor)] = graph.getEdge(node,neighbor);
                }
            }
        }
        public int getEdge(Node n1, Node n2)
        {
            return this.Edges[PixelGraph.MakeEdgeKey(n1, n2)];
        }

        public Dictionary<(Node, Node), int> mst()
        {
            var mst = new Dictionary<(Node, Node), int>();

            var edges = this.Edges.ToList();

            // Sort edges by weight (ascending for Kruskal's)
            edges.Sort((a, b) => a.Value.CompareTo(b.Value)); // O(N^2) || O(NLOGN) ?
            // if O(N^2) replace with priority queue

            //var pq = new PriorityQueue<KeyValuePair<(Node, Node), int>>(
            //    (a,b) => a.Value.CompareTo(b.Value)
            //);

            //foreach (var e in Edges) pq.Enqueue(e);

            // union-find
            var parent = new Dictionary<Node, Node>();
            this.nodes.ForEach(n => parent[n] = n);

            //while (mst.Count < this.nodes.Count && pq.Count >= 1)
            //{
            //    var edge = pq.Dequeue();
            //    var root1 = Find(edge.Key.Item1);
            //    var root2 = Find(edge.Key.Item2);
            //    if (root1 == root2) continue;

            //    mst.Add(edge.Key, edge.Value);
            //    parent[root2] = root1; // Union
            //}

            foreach (var edge in edges) // O(E), E: number of edges
            {
                if (mst.Count >= this.nodes.Count - 1)
                    break;

                var root1 = Find(edge.Key.Item1);
                var root2 = Find(edge.Key.Item2);
                if (root1 == root2) continue;

                mst.Add(edge.Key, edge.Value);
                parent[root2] = root1; // Union
            }

            Node Find(Node node)
            {
                if (parent[node] != node)
                    parent[node] = Find(parent[node]); // Path compression
                return parent[node];
            }

            return mst;
        }
        public int InternalDifference()
        {
            // If the segment has 1 or less than 1, return 0
            if (this.count <= 1) return 0;
            var mst = this.mst();
            // Return the maximum edge weight in the segment
            return mst.Max(e => e.Value);
        }

        public int SegmentsDifference(PixelGraph graph, Segment s2) //O(N) , N: number of nodes in the smaller segment
        {
            Segment smallSegment = (this.count < s2.count) ? this : s2;
            Segment bigSegment = (this.count < s2.count) ? s2 : this;

            int minEdge = smallSegment.nodes.Select(n =>
                n.neighbors.Select(
                    ni => (ni.segment == bigSegment) ? graph.getEdge(n, ni) : int.MaxValue
                ).Min<int>() // O(1) //returns the min edge connecting from a node to the other segment if found
            ).Min<int>(); // O(N), N: number of nodes in the smaller segment //returns the min edge connecting to the other segment from all minimums

            return (minEdge < int.MaxValue) ? minEdge : -1; //return -1 if no connecting edges
        }

        public bool SegmentsComparison(PixelGraph graph, Segment s2, int k)
        {
            int internalDiff1 = this.InternalDifference();
            int internalDiff2 = s2.InternalDifference();
            int segmentsDifference = this.SegmentsDifference(graph, s2);
            if (segmentsDifference == -1) return true; //it returns -1 when no edges are common , so we should return true, which means don't merge
            double tao1 = (double)k / this.count;
            double tao2 = (double)k / s2.count;
            double MInt = Math.Ceiling(Math.Min(internalDiff1 + tao1, internalDiff2 + tao2));//DEBATEBLE
            return (segmentsDifference > MInt);
        }

        public static Segment EmptySegment = new Segment
        {
            ID = -1
        };
    }

    public class Segments
    {

        public int segmentIdIncrement = -1;
        public int NewSegmentId => ++segmentIdIncrement;
        //we have to use that because segments.count is variable , so two segments can have the same id

        public List<Segment> segments;
        public int Count
        {
            get => segments.Count;
        }

        public Segments()
        {
            segments = new List<Segment>(30);
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

        public void CreateSegment(PixelGraph graph, (int y,int x) index)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
                nodes = new List<Node> { graph.Nodes[index.y, index.x] }
            };
            graph.Nodes[index.y, index.x].segment = newSegment;

            graph.Segments.Add(newSegment);
        }
        public void CreateFinalSegment(Node node)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
                nodes = new List<Node> { node }
            };
            node.finalsegment = newSegment;
            this.Add(newSegment);
        }

        public void CreateSegment(Node node)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
                nodes = new List<Node> { node }
            };
            node.segment = newSegment;
            this.Add(newSegment);
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
                    if (IsUnsegmented(myNode)) CreateSegment(myNode);

                    for (int l = 0; l < myNode.neighbors.Count; l++)
                    {
                        neighbor = myNode.neighbors[l];

                        if (AreSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            CreateSegment(neighbor);
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

        public void ColorSegments(RGBPixel[] colors, PixelGraph graph) //O(N) , N: number of pixels
        {
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    RGBPixel c;
                    if (graph.Nodes[i, j].finalsegment.ID == -1)
                        c = new RGBPixel ();
                    else
                    {
                        int id = this.segments.IndexOf(graph.Nodes[i, j].finalsegment);
                        c = colors[id];
                    }
                    graph.Picture[i, j] = c;
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

        public void Combine(PixelGraph RedGraph,PixelGraph BlueGraph,PixelGraph GreenGraph)
        {
            for (int r = 0; r < RedGraph.height; r++) {
                for (int c = 0; c < RedGraph.width; c++)
                {

                    var RedNode = RedGraph.Nodes[r, c];
                    var BlueNode = BlueGraph.Nodes[r, c];
                    var GreenNode = GreenGraph.Nodes[r, c];
                    for (int n = 0; n < RedNode.neighbors.Count; n++)
                    {
                        var RedNeighbor = RedNode.neighbors[n];
                        var BlueNeighbor = BlueNode.neighbors[n];
                        var GreenNeighbor = GreenNode.neighbors[n];

                        if (RedNeighbor.segment == RedNode.segment
                        && BlueNeighbor.segment == BlueNode.segment
                        && GreenNeighbor.segment == GreenNode.segment)
                        {
                            // the same segment
                            if (RedNode.finalsegment.ID == -1) CreateFinalSegment(RedNode);
                            RedNode.finalsegment.nodes.Add(RedNeighbor);
                            RedNeighbor.finalsegment = RedNode.finalsegment;
                        }

                    }

                }
            }
        }

        //Helper functions
        // Should we compare using pointer value ? I don't think so #important
        public static bool AreSameSegment(Node a, Node b) => a.segment.ID == b.segment.ID;
        private bool IsUnsegmented(Node node) => node.segment.ID == -1;
    }
}
