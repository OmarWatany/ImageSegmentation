using System.Collections.Generic;
using System.Linq;

namespace ImageTemplate
{
    public class MSTree {
        HashSet<Node> Mst_m = new HashSet<Node>();
        Segment s;
        List<Edge> Edges => this.s.Edges;

        public int Max = 0;


        public MSTree(Segment s)
        {
            this.s = s;
        }

        public void Add(Node node)
        {
            var mst = this.Mst_m;
            if (mst.Contains(node)) return;

            var min = node.neighbors.Select(ni =>
            {
                var e = Edge.getEdge(node, ni,Edges);
                return (mst.Contains(ni) && e.weight>-1) ? e.weight : int.MaxValue;
            }).DefaultIfEmpty(int.MaxValue).Min();

            mst.Add(node);
            if(min != int.MaxValue && min > this.Max)
                this.Max = min;
        }
    }
}
