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
        List<((Index n, Index ni), int w)> Mst_m = new List<((Index, Index), int)>();
        Segment s;
        Dictionary<(Index, Index), int> Edges => this.s.Edges;
        int count => s.count;

        public int Max;

        public MSTree(Segment s)
        {
            this.s = s;
        }

        public void Add(Node node)
        {
            ((Index n, Index ni), int w) edge = ((node.index, node.index), int.MaxValue);
            Node min_ni = new Node(0);
            var mst = this.Mst_m;

            foreach (var ni in node.neighbors)
            {
                var e = PixelGraph.MakeEdgeKey(node, s.graph.At(ni));
                if (this.Edges.ContainsKey(e) && this.Edges[e] < edge.w)
                {
                    edge = (e, this.Edges[e]);
                    min_ni = s.graph.At(ni);
                }
            }

            //node.neighbors.ForEach(ni =>
            //{
            //    var e = PixelGraph.MakeEdgeKey(node, s.graph.At(ni));
            //    if (this.Edges.ContainsKey(e) && this.Edges[e] < edge.w)
            //    {
            //        edge = (e, this.Edges[e]);
            //        min_ni = s.graph.At(ni);
            //    }
            //});

            mst.Add(edge);
            if (edge.w >= this.Max) this.Max = 0;
        }
    }

    public class Segment
    {
        public int count
        {
            get => nodes.Count;
        }

        public int ID;
        public List<Index> nodes;
        public Dictionary<(Index, Index), int> Edges; //contains all edges of the segment
        MSTree mst;
        public PixelGraph graph;


        public Segment()
        {
            this.nodes = new List<Index>();
            this.Edges = new Dictionary<(Index, Index), int>();
            this.mst = new MSTree(this);
        }

        public Segment(PixelGraph graph)
        {
            this.nodes = new List<Index>();
            this.Edges = new Dictionary<(Index, Index), int>();
            this.mst = new MSTree(this);
            this.graph = graph;
        }

        public void Add(PixelGraph graph, Index i)
        {
            graph.Nodes[i.y, i.x].segment = this;
            this.nodes.Add(i);
            Node node = graph.At(i);

            mst.Add(node);
            foreach (var neighbor in node.neighbors)
            {
                if(graph.At(neighbor).segment == this)
                {
                    this.Edges[PixelGraph.MakeEdgeKey(node, graph.At(neighbor))] = graph.getEdge(node, graph.At(neighbor));
                    mst.Add(graph.At(neighbor));
                }
            }
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
                graph.At(n).neighbors.Select(
                    ni => (graph.At(ni).segment == bigSegment) ? graph.getEdge(n, ni) : int.MaxValue
                ).Min<int>() // O(E) //returns the min edge connecting from a node to the other segment if found
            ).First(); // O(N), N: number of nodes in the smaller segment //returns the min edge connecting to the other segment from all minimums

            return (minEdge < int.MaxValue) ? minEdge : -1; //return -1 if no connecting edges
        }

        public bool SegmentsComparison(PixelGraph graph, Segment s2, int k)
        {
            double internalDiff1 = this.InternalDifference();
            double internalDiff2 = s2.InternalDifference();
            double segmentsDifference = this.SegmentsDifference(graph, s2);
            if (segmentsDifference == -1) return true; //it returns -1 when no edges are common , so we should return true, which means don't merge
            double tao1 = (double)k / this.count;
            double tao2 = (double)k / s2.count;
            double MInt = Math.Ceiling(Math.Min(internalDiff1 + tao1, internalDiff2 + tao2));//DEBATEBLE
            return (segmentsDifference > MInt);
        }

        public static Segment EmptySegment = new Segment()
        {
            ID = -1
        };
    }
}
