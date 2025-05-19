using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
            mst.Add(node);
        }

        public void Add(PixelGraph graph, Node node)
        {
            node.segment = this;
            this.nodes.Add(node);
            foreach (Node neighbor in node.neighbors)
            {
                if(neighbor.segment == this)
                {
                    this.Edges[PixelGraph.MakeEdgeKey(node, neighbor)] = graph.getEdge(node, neighbor);
                    mst.Add(node);
                    mst.Add(neighbor);
                }
            }
            mst.Add(node);
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


            int minEdge = smallSegment.nodes
                .Select(n => n.neighbors
                .Where(ni => ni.segment == bigSegment)
                .DefaultIfEmpty(n)
                .Min(ni => graph.getEdge(n, ni))).Min();

            //int minEdge = smallSegment.nodes.Select(n =>
            //    n.neighbors.Select(
            //        ni => (ni.segment == bigSegment) ? graph.getEdge(n, ni) : int.MaxValue
            //    ).Min<int>() // O(E) //returns the min edge connecting from a node to the other segment if found
            //).First(); // O(N), N: number of nodes in the smaller segment //returns the min edge connecting to the other segment from all minimums

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
            double MInt = Math.Min(internalDiff1 + tao1, internalDiff2 + tao2);//DEBATEBLE
            return (segmentsDifference > MInt);
        }

        public static Segment EmptySegment = new Segment
        {
            ID = -1
        };
    }
}
