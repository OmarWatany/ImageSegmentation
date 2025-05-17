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

        public List<((Node, Node), int)> mst_prim()
        {
            var mst = new List<((Node, Node), int)>(this.count);
            var pq = new PriorityQueue<((Node, Node), int)>(
                (a, b) => a.Item2.CompareTo(b.Item2), this.Edges.Count
            );

            var firstNode = this.Edges.ElementAt(0).Key.Item1;

            foreach (var n in firstNode.neighbors)
            {
                if (pq.Count >= this.Edges.Count) break;
                var e = PixelGraph.MakeEdgeKey(firstNode, n);
                if(this.Edges.ContainsKey(e))
                    pq.Enqueue((e,this.Edges[e]));
            }

            var visit = new HashSet<Node>(this.count + 1);
            visit.Add(pq.heap[0].Item1.Item1);

            while (mst.Count < this.count && pq.Count >= 1)
            {
                var edge = pq.Dequeue();
                if (visit.Contains(edge.Item1.Item2)) continue;
                mst.Add(edge);
                visit.Add(edge.Item1.Item2);
                foreach (var n in edge.Item1.Item2.neighbors)
                {
                    if (pq.Count >= this.Edges.Count) break;
                    var e = PixelGraph.MakeEdgeKey(firstNode, n);
                    if(this.Edges.ContainsKey(e))
                        pq.Enqueue((e,this.Edges[e]));
                }
            }

            return mst;
        }
        public int InternalDifference()
        {
            // If the segment has 1 or less than 1, return 0
            if (this.count <= 1) return 0;
            if(this==Segments.EmptySegment1) { Console.WriteLine("1"); }
            else if(this==Segments.EmptySegment2) { Console.WriteLine("2"); }
            var mst = this.mst_prim();
            // Return the maximum edge weight in the segment
            return mst.Max(e => e.Item2);
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

        public int segmentIdIncrement = 1;
        public int NewSegmentId => ++segmentIdIncrement;
        //we have to use that because segments.count is variable , so two segments can have the same id

        public List<Segment> segments;
        public int Count
        {
            get => segments.Count;
        }
        public static Segment EmptySegment1 = new Segment { ID = 0 }, EmptySegment2 = new Segment{ ID = 1 };
        //public static Segment EmptySegment1 = new Segment(), EmptySegment2 = new Segment();
        public Segments()
        {
            segments = new List<Segment>(30);
        }

        public int Add(Segment seg)
        {
            segments.Add(seg);
            return segments.Count - 1;
        }
        public void MergeSegments(PixelGraph graph, Segment s1, Segment s2)
        {
            if (s1==EmptySegment1 && s2==EmptySegment2)
            {
                Node n1=EmptySegment1.nodes[0];
                Node n2=EmptySegment2.nodes[0];
                EmptySegment1.nodes.RemoveAt(0);
                EmptySegment2.nodes.RemoveAt(0);
                CreateSegment(n1);
                n1.segment.Add(graph ,n2);
                return;
            }
            else if (s1==EmptySegment1)
            {
                Node n1 = EmptySegment1.nodes[0];
                EmptySegment1.nodes.RemoveAt(0);
                s2.Add(graph, n1);
                return;
            }
            else if (s2 == EmptySegment2)
            {
                Node n2 = EmptySegment2.nodes[0];
                EmptySegment2.nodes.RemoveAt(0);
                s1.Add(graph, n2);
                return;
            }

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
        public void SegmentChannel(PixelGraph channelGraph, int k)
        {
            Node neighbor, myNode;
            for (int i = 0; i < channelGraph.height; i++)
            {
                for (int j = 0; j < channelGraph.width; j++)
                {
                    myNode = channelGraph.Nodes[i, j];

                    //create a segment just for this node so we can use segments comparison function
                    if (IsUnsegmented(myNode)) { 
                        myNode.segment = EmptySegment1;
                        EmptySegment1.nodes.Add(myNode);
                    }

                    for (int l = 0; l < myNode.neighbors.Count; l++)
                    {
                        neighbor = myNode.neighbors[l];

                        if (AreSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            neighbor.segment=EmptySegment2;
                            EmptySegment2.nodes.Add(neighbor);
                        }

                        //if the function returns true, then no merging and continue, else : merge
                        // we should stop getting segments using it's ID because its different from it's place #important
                        if (!neighbor.segment 
                            .SegmentsComparison(channelGraph,
                            myNode.segment,
                            k))
                        {
                            MergeSegments(channelGraph,
                                myNode.segment,
                                neighbor.segment
                            );
                        }
                        else
                        {
                            EmptySegment2.nodes.ForEach(n => n.segment = Segment.EmptySegment);
                            EmptySegment2.nodes.Clear();
                        }
                    }
                    EmptySegment1.nodes.ForEach(n => n.segment = Segment.EmptySegment);
                    EmptySegment1.nodes.Clear();
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
                    Node n = graph.Nodes[i, j];
                    Segment s = n.segment;
                    int index=graph.Segments.segments.IndexOf(s);
                    RGBPixel c= colors[index];
                    graph.Picture[i, j] = c;
                }
            }
        }

        public void Combine(PixelGraph RedGraph,PixelGraph BlueGraph,PixelGraph GreenGraph)
        {
            Node RedNode,BlueNode, GreenNode;
            Node RedNeighbor, BlueNeighbor, GreenNeighbor;
            for (int r = 0; r < RedGraph.height; r++) {
                for (int c = 0; c < RedGraph.width; c++)
                {

                    RedNode = RedGraph.Nodes[r, c];
                    BlueNode = BlueGraph.Nodes[r, c];
                    GreenNode = GreenGraph.Nodes[r, c];
                    for (int n = 0; n < RedNode.neighbors.Count; n++)
                    {
                        RedNeighbor = RedNode.neighbors[n];
                        BlueNeighbor = BlueNode.neighbors[n];
                        GreenNeighbor = GreenNode.neighbors[n];

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
        public static bool AreSameSegment(Node a, Node b) => a.segment == b.segment;
        private bool IsUnsegmented(Node node) => node.segment.ID == -1;
    }
}
