using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageTemplate
{
    public class Segments
    {
        public int segmentIdIncrement = 1;
        public int NewSegmentId => ++segmentIdIncrement; //O(1)
        //we have to use that because segments.count is variable , so two segments can have the same id

        public List<Segment> segments;
        public int Count
        {
            get => segments.Count;
        }
        public static Segment EmptySegment1 = new Segment { ID = 0 }, EmptySegment2 = new Segment{ ID = 1 };


        public Segments() //O(1)
        {
            segments = new List<Segment>(30);
        }

        // return new segment's ID
        public int Add(Segment seg)// O(1)
        {
            segments.Add(seg);
            return segments.Count - 1;
        }

        public void MergeSegments(PixelGraph graph, Segment s1, Segment s2,Edge edge)//O(N), N: # of pixels of smaller segment
        {
            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            int max = Math.Max(bigSegment.internalDifference, Math.Max(smallSegment.internalDifference, edge.weight));
            for (int i = 0; i < smallSegment.count; i++)//O(N)
            {
                bigSegment.Add(smallSegment.nodes[i],(byte)max);// O(1)
            }
            this.segments.Remove(smallSegment);
        }

        // NOTE: Probably class Segments' operation
        public RGBPixel[] CreateRandomColors(int m) //O(N) , N: number of segments
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

        public int CreateFinalSegment()//O(1)
        {
            return NewSegmentId;
            //Segment newSegment = new Segment
            //{
            //    nodes = new List<Node> { node }
            //};
            //node.finalsegment = newSegment;
            //this.Add(newSegment);
            //return newSegment;
        }

        public void CreateSegment(Node node)//O(1)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
            };
            newSegment.Add(node,0);
            this.Add(newSegment);
        }

        public void SegmentChannel(PixelGraph channelGraph, int k)//O(E*logE + E*N), E: number of edges collected, N: number of pixels in smaller segment
        {
            channelGraph.Edges.Sort();//O(E*logE), E: number of edges collected
            Node n1,n2;
            foreach (var edge in channelGraph.Edges)//O(E),E: number of edges collected
            {
                n1 = edge.n1;
                n2 = edge.n2;
                if (IsUnsegmented(n1))//O(1)
                    CreateSegment(n1);//O(1)
                if (IsUnsegmented(n2)) CreateSegment(n2);
                if (AreSameSegment(n1, n2))//O(1)
                    continue;
                if (!n1.segment
                    .SegmentsComparison(channelGraph,
                    n2.segment,edge.weight,
                    k)) //O(1)
                {
                    MergeSegments(channelGraph,
                        n1.segment,
                        n2.segment,
                        edge
                    );//O(N), N: # of pixels of smaller segment
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
        //TODO: complete the analysis
        public RGBPixel[,] Combine(PixelGraph RedGraph, PixelGraph BlueGraph, PixelGraph GreenGraph)
        {
            int[,] finalId = new int[RedGraph.height, RedGraph.width];
            Dictionary<int, RGBPixel> Ids = new Dictionary<int, RGBPixel>();

            foreach (Segment redSegment in RedGraph.Segments.segments)
            {
                // Use a dictionary to group nodes by their (green segment, blue segment) pair
                var groupMap = new Dictionary<(int greenId, int blueId), List<Node>>();

                foreach (Node node in redSegment.nodes)
                {
                    var greenSeg = GreenGraph.Nodes[node.index.y, node.index.x].segment;
                    var blueSeg = BlueGraph.Nodes[node.index.y, node.index.x].segment;
                    var key = (greenSeg.ID, blueSeg.ID);

                    if (!groupMap.ContainsKey(key))//O(1), because its a dictionary 
                        groupMap[key] = new List<Node>();
                    groupMap[key].Add(node);//O(1)
                }

                // For each group, create a final segment and assign it to all nodes in the group
                foreach (var group in groupMap.Values)
                {
                    if (group.Count == 0) continue;
                    var fid = CreateFinalSegment();//O(1)
                    Ids[fid] = new RGBPixel();
                    finalId[group[0].index.y, group[0].index.x] = fid;
                    for (int i = 1; i < group.Count; i++)
                    {
                        finalId[group[i].index.y, group[i].index.x] = fid;
                    }
                }
            }

            return ColorSegments(RedGraph);
            RGBPixel[,] ColorSegments(PixelGraph graph) //O(N*P) , N: number of Segments, P: number of pixels
            {
                var Nodes = graph.Nodes;
                int height = graph.height;
                int width = graph.width;


                var newImage = new RGBPixel[height,width];

                var colors = this.CreateRandomColors(Ids.Count + 1); //O(N) , N: number of segments

                for (int i = 0; i < Ids.Count; i++)
                {
                    var item = Ids.ElementAt(i);
                    Ids[item.Key] = colors[i];
                }

                (int y,int x) NodeIdx;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        NodeIdx = Nodes[i, j].index;
                        newImage[i, j] = Ids[finalId[NodeIdx.y,NodeIdx.x]];//O(N) , N: number of segments
                    }
                }

                return newImage;
            }
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

        public static bool AreSameSegment(Node a, Node b) => a.segment.ID == b.segment.ID;
        private bool IsUnsegmented(Node node) => node.segment.ID == -1;
    }
}
