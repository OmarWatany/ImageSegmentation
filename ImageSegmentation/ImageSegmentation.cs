using System;
using System.Collections.Generic;

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
        public List<Edge> Edges; //contains all edges of the segment
        MSTree mst;


        public Segment()
        {
            this.nodes = new List<Node>();
            this.Edges = new List<Edge>();
            this.mst = new MSTree(this);
        }

        public void Add(Node node)
        {
            node.segment = this;
            this.nodes.Add(node);
            mst.Add(node);
        }
        public int InternalDifference()
        {
            // If the segment has 1 or less than 1, return 0
            if (this.count <= 1) return 0;
            var mst = this.mst;
            // Return the maximum edge weight in the segment
            return mst.Max;
        }
        public bool SegmentsComparison(PixelGraph graph, Segment s2,int weight, int k)
        {
            double internalDiff1 = this.InternalDifference();
            double internalDiff2 = s2.InternalDifference();
            double segmentsDifference = weight;
            double tao1 = (double)k / this.count;
            double tao2 = (double)k / s2.count;
            double MInt = Math.Min(internalDiff1 + tao1, internalDiff2 + tao2);
            return (segmentsDifference > MInt);
        }

        public static Segment EmptySegment = new Segment
        {
            ID = -1
        };
    }
}
