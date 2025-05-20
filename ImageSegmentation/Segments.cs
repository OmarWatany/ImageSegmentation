using System;
using System.Collections.Generic;
using System.Text;

namespace ImageTemplate
{
    public class Segments
    {
        public int segmentIdIncrement = 1;
        public int NewSegmentId => ++segmentIdIncrement;
        //we have to use that because segments.count is variable , so two segments can have the same id

        public List<Segment> segments;
        public int Count
        {
            get => segments.Count;
        }
        public static Segment EmptySegment1 = new Segment { ID = 0 }, EmptySegment2 = new Segment{ ID = 1 };


        public Segments()
        {
            segments = new List<Segment>(30);
        }

        // return new segment's ID
        public int Add(Segment seg)
        {
            segments.Add(seg);
            return segments.Count - 1;
        }

        public void MergeSegments(PixelGraph graph, Segment s1, Segment s2)
        {
            if (s1==EmptySegment1 && s2==EmptySegment2)
            {
                Node n1=EmptySegment1.nodes[0];
                Node n2=EmptySegment2.nodes[0];
                EmptySegment1.nodes.RemoveAt(0);
                EmptySegment2.nodes.RemoveAt(0);
                CreateSegment(n1);
                n1.segment.Add(n2);
                return;
            }
            else if (s1==EmptySegment1 || s2==EmptySegment1)
            {
                if (EmptySegment1.count < 1) return;
                Node n1 = EmptySegment1.nodes[0];
                EmptySegment1.nodes.RemoveAt(0);
                Segment s = (s1 == EmptySegment1) ? s2 : s1;
                s.Add(n1);
                return;
            }
            else if (s2 == EmptySegment2 || s1 == EmptySegment2)
            {
                if (EmptySegment2.count < 1) return;
                Node n2 = EmptySegment2.nodes[0];
                EmptySegment2.nodes.RemoveAt(0);
                Segment s = (s1 == EmptySegment2) ? s2 : s1;
                s.Add(n2);
                return;
            }

            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            for (int i = 0; i < smallSegment.count; i++)
            {
                bigSegment.Add(smallSegment.nodes[i]);
            }
            this.segments.Remove(smallSegment); // O(n) operation
        }

        // NOTE: Probably class Segments' operation
        public RGBPixel[] CreateRandomColors(int m) //O(M) , M: number of segments
        {
            RGBPixel[] colors = new RGBPixel[m];
            for (int i = 0; i < m; i++)
            {
                colors[i].red = (byte)((i * 100) % 255);
                colors[i].green = (byte)((i * 20) % 255);
                colors[i].blue = (byte)((i * 30) % 255);
            }
            return colors;
        }

        public void CreateFinalSegment(Node node)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
                nodes = new List<Node> { node }
            };
            node.finalsegment = newSegment;
            this.Add(newSegment);
        }

        public void CreateSegment(Node node)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
                //nodes = new List<Node> { node }
            };
            //node.segment = newSegment;
            newSegment.Add(node);
            this.Add(newSegment);
        }

        public void SegmentChannel(PixelGraph channelGraph, int k)
        {
            channelGraph.Edges.Sort();
            Node n1,n2;
            foreach (var edge in channelGraph.Edges)
            {
                n1 = edge.n1;
                n2 = edge.n2;
                if (IsUnsegmented(n1)) CreateSegment(n1);
                if (IsUnsegmented(n2)) CreateSegment(n2);
                if (AreSameSegment(n1, n2))
                    continue;
                if (!n1.segment
                    .SegmentsComparison(channelGraph,
                    n2.segment,edge.weight,
                    k)) 
                {
                    MergeSegments(channelGraph,
                        n1.segment,
                        n2.segment
                    );
                }

            }
        }

        public void ColorSegments(RGBPixel[] colors, PixelGraph graph) //O(N) , N: number of pixels
        {
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    RGBPixel c;
                    if (graph.Nodes[i, j].finalsegment.ID == -1)
                        c = new RGBPixel ();
                    else
                    {
                        int id = this.segments.IndexOf(graph.Nodes[i, j].finalsegment);
                        c = colors[id];
                    }
                    graph.Picture[i, j] = c;
                }
            }
        }
        public void Combine(PixelGraph RedGraph,PixelGraph BlueGraph,PixelGraph GreenGraph)
        {
            for (int r = 0; r < RedGraph.height; r++) {
                for (int c = 0; c < RedGraph.width; c++)
                {

                    var RedNode = RedGraph.Nodes[r, c];
                    var BlueNode = BlueGraph.Nodes[r, c];
                    var GreenNode = GreenGraph.Nodes[r, c];
                    for (int n = 0; n < RedNode.neighbors.Count; n++)
                    {
                        var RedNeighbor = RedNode.neighbors[n];
                        var BlueNeighbor = BlueNode.neighbors[n];
                        var GreenNeighbor = GreenNode.neighbors[n];

                        if (RedNeighbor.segment == RedNode.segment
                        && BlueNeighbor.segment == BlueNode.segment
                        && GreenNeighbor.segment == GreenNode.segment)
                        {
                            // the same segment
                            if (RedNode.finalsegment.ID == -1) CreateFinalSegment(RedNode);
                            RedNode.finalsegment.nodes.Add(RedNeighbor);
                            RedNeighbor.finalsegment = RedNode.finalsegment;
                        }

                    }

                }
            }
        }
        public int CountUnSegmented()
        {
            int count = 0;
            Node node;
            this.segments.ForEach(s =>
            {
                for (int i = 0; i < s.count; i++)
                {
                    node = s.nodes[i];
                    if (node.segment.ID < 2)
                        count++;
                }
            });
            return count;
        }
        public string GetSegmentsInfo()
        {
            int numSegments = segments.Count;

            List<int> sizes = new List<int>();
            foreach (var segment in segments)
            {
                sizes.Add(segment.count);
            }

            sizes.Sort((a, b) => b.CompareTo(a));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{numSegments}");
            foreach (int size in sizes)
            {
                sb.AppendLine(size.ToString());
            }

            return sb.ToString();
        }

        //Helper functions
        // Should we compare using pointer value ? I don't think so #important
        public static bool AreSameSegment(Node a, Node b) => a.segment.ID == b.segment.ID;
        private bool IsUnsegmented(Node node) => node.segment.ID == -1;
    }
}
