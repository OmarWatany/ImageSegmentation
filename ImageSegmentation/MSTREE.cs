using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTemplate
{
    public class MSTree {
        //List<Node> Mst_m = new List<Node>();
        HashSet<Node> Mst_m = new HashSet<Node>();
        Segment s;
        Dictionary<(Node, Node), int> Edges => this.s.Edges;
        int count => s.count;

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
                var e = PixelGraph.MakeEdgeKey(node, ni);
                return (mst.Contains(ni) && s.Edges.ContainsKey(e)) ? s.Edges[e] : int.MaxValue;
            }).Min();

            mst.Add(node);
            if(min != int.MaxValue && min > this.Max)
                this.Max = min;
        }
    }

    public class MSTREE
    {
        private SortedDictionary<int, List<((Node, Node), int)>> edgeQueue;
        private HashSet<Node> includedNodes;
        public int Max { get; private set; }
        public Segment segment { get; private set; }
    
        public MSTREE(Segment segment)
        {
            edgeQueue = new SortedDictionary<int, List<((Node, Node), int)>>();
            includedNodes = new HashSet<Node>();
            Max = 0;
            this.segment = segment;
            
            // Initialize with first node
            if (segment.count > 0)
            {
                Add(segment.nodes[0]);
            }
        }
    
        public void Add(Node node)
        {
            includedNodes.Add(node);
            
            // Add all adjacent edges to priority queue
            foreach (var neighbor in node.neighbors)
            {
                if (!includedNodes.Contains(neighbor) && segment.Edges.TryGetValue(
                    PixelGraph.MakeEdgeKey(node, neighbor), out int weight))
                {
                    if (!edgeQueue.TryGetValue(weight, out var list))
                    {
                        list = new List<((Node, Node), int)>();
                        edgeQueue.Add(weight, list);
                    }
                    list.Add(((node, neighbor), weight));
                }
            }
        }
    }
}
