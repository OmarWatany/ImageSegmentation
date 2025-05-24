using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ImageTemplate
{
    public struct Edge
    {
        public Node n1, n2;
        public Edge(Node n1, Node n2)
        {
            this.n1 = n1;
            this.n2 = n2;
        }

        //public int CompareTo(Edge other)
        //{
        //    return this.weight.CompareTo(other.weight);
        //}
    }
}