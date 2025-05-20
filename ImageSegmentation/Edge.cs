using System;
using System.Collections.Generic;

namespace ImageTemplate
{
    public class Edge : IComparable<Edge>
    {
        public int weight;
        public Node n1,n2;
        public Edge(Node n1, Node n2, int weight)
        {
            this.n1 = n1;
            this.n2 = n2;
            this.weight = weight;
        }
        public Edge(int weight)
        {
            this.weight = weight;
            this.n1 = null;
            this.n2 = null;
        }
        public static Edge getEdge(Node n1, Node n2,List<Edge> Edges)
        {
            return Edges.Find(e => (e.n1 == n1 && e.n2 == n2) || ((e.n2 == n1 && e.n1 == n2))) ?? new Edge(-1);
        }
        public int CompareTo(Edge other)
        {
            return this.weight.CompareTo(other.weight);
        }

    }
}