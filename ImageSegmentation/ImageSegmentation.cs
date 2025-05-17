using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms.VisualStyles;

//Main problem right now: every pixel is a segment on its own
namespace ImageTemplate
{
    public class MSTree {
        List<((Node, Node), int)> Mst_m = new List<((Node, Node), int)>();
        Segment s;
        Dictionary<(Node, Node), int> Edges => this.s.Edges;
        int count => s.count;

        public int Max;


        public MSTree(Segment s)
        {
            this.s = s;
        }

        public void Add(PixelGraph graph, Node node)
        {
            var mst = this.Mst_m; 
            var pq = new PriorityQueue<((Node, Node), int)>(
                (a, b) => a.Item2.CompareTo(b.Item2), this.Edges.Count
            );

            var firstNode = node;
            foreach (var n in firstNode.neighbors)
            {
                if (pq.Count >= this.Edges.Count) break;
                var e = PixelGraph.MakeEdgeKey(firstNode, n);
                if(this.Edges.ContainsKey(e))
                    pq.Enqueue((e,this.Edges[e]));
            }

            if (pq.Count <= 0) return;
            var edge = pq.Dequeue();
            mst.Add(edge);
            if (edge.Item2 > this.Max) this.Max = 0;
        }
        
        public List<((Node, Node), int)> Build()
        {
            var mst = this.Mst_m; 
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
    }

    public class Segment
    {
        public int count
        {
            get => nodes.Count;
        }

        public int ID;
        public List<Node> nodes;
        public Dictionary<(Node, Node), int> Edges; //contains all edges of the segment
        MSTree mst;


        public Segment()
        {
            this.nodes = new List<Node>();
            this.Edges = new Dictionary<(Node, Node), int>();
            this.mst = new MSTree(this);
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
                    var k = PixelGraph.MakeEdgeKey(node, neighbor);
                    var w = graph.getEdge(node, neighbor);
                    this.Edges[k] = w;
                }
            }
            mst.Add(graph, node);
        }

        public int getEdge(Node n1, Node n2)
        {
            return this.Edges[PixelGraph.MakeEdgeKey(n1, n2)];
        }



        public int InternalDifference()
        {
            // If the segment has 1 or less than 1, return 0
            if (this.count <= 1) return 0;
            var mst = this.mst;
            // Return the maximum edge weight in the segment
            return mst.Max;
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
}
