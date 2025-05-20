using System;
using System.Collections.Generic;

namespace ImageTemplate
{
    public struct Edge : IComparable<Edge>
    {
        public byte weight;
        public Node n1,n2;
        public Edge(Node n1, Node n2, byte weight)
        {
            this.n1 = n1;
            this.n2 = n2;
            this.weight = weight;
        }
        public Edge(byte weight)
        {
            this.weight = weight;
            this.n1 = null;
            this.n2 = null;
        }
        public int CompareTo(Edge other)
        {
            return this.weight.CompareTo(other.weight);
        }

    }
}