using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

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

        public void MergeSegments(PixelGraph graph, Segment s1, Segment s2,Edge edge)
        {
            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            int max = Math.Max(bigSegment.internalDifference, Math.Max(smallSegment.internalDifference, edge.weight));
            for (int i = 0; i < smallSegment.count; i++)
            {
                bigSegment.Add(smallSegment.nodes[i],max);
            }
            this.segments.Remove(smallSegment);
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
            };
            newSegment.Add(node,0);
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
                        n2.segment,
                        edge
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

        public void ColorSegments(PixelGraph graph) //O(N) , N: number of pixels
        {
            RGBPixel[] colors = CreateRandomColors(graph.Segments.Count);
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    graph.Picture[i, j] = colors[graph.Segments.segments.IndexOf(graph.Nodes[i, j].segment)];
                }
            }
        }
        // In the Combine method, you want to create a "final segment" for each group of nodes that belong to the same segment in all three color channels (Red, Green, Blue).
        // The logic is: For each segment in the Red channel, check if the corresponding nodes in the Green and Blue channels belong to the same segment as the first node in the Red segment. 
        // If so, add them to the same final segment. Otherwise, create a new final segment for the node.

        public void Combine(PixelGraph RedGraph, PixelGraph BlueGraph, PixelGraph GreenGraph)
        {
            foreach (Segment redSegment in RedGraph.Segments.segments)
            {
                // Use a dictionary to group nodes by their (green segment, blue segment) pair
                var groupMap = new Dictionary<(int greenId, int blueId), List<Node>>();

                foreach (Node node in redSegment.nodes)
                {
                    var greenSeg = GreenGraph.Nodes[node.index.y, node.index.x].segment;
                    var blueSeg = BlueGraph.Nodes[node.index.y, node.index.x].segment;
                    var key = (greenSeg.ID, blueSeg.ID);

                    if (!groupMap.ContainsKey(key))
                        groupMap[key] = new List<Node>();
                    groupMap[key].Add(node);
                }

                // For each group, create a final segment and assign it to all nodes in the group
                foreach (var group in groupMap.Values)
                {
                    if (group.Count == 0) continue;
                    CreateFinalSegment(group[0]);
                    var finalSeg = group[0].finalsegment;
                    for (int i = 1; i < group.Count; i++)
                    {
                        finalSeg.nodes.Add(group[i]);
                        group[i].finalsegment = finalSeg;
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
