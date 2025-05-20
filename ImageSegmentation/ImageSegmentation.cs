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
        public int internalDifference;


        public Segment()
        {
            this.nodes = new List<Node>();
            this.Edges = new List<Edge>();
            this.internalDifference = 0;
        }

        public void Add(Node node,int weight)
        {
            node.segment = this;
            this.nodes.Add(node);
            internalDifference = (internalDifference < weight) ? weight : internalDifference;
        }
        public bool SegmentsComparison(PixelGraph graph, Segment s2,int weight, int k)
        {
            double tao1 = k / this.count;
            double tao2 = k / s2.count;
            double MInt = Math.Min(this.internalDifference + tao1, s2.internalDifference + tao2);
            return (weight > MInt);
        }

        public static Segment EmptySegment = new Segment
        {
            ID = -1
        };
    }
}
