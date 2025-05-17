using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //public static Segment EmptySegment1 = new Segment(), EmptySegment2 = new Segment();
        public Segments()
        {
            segments = new List<Segment>(30);
        }

        public Segments(int n)
        {
            segments = new List<Segment>(n);
        }

        // return new segment's ID
        public Segment New()
        {
            var s = new Segment { ID = segments.Count };
            segments.Add(s);
            return s;
        }

        public int Add(Segment seg)
        {
            segments.Add(seg);
            return segments.Count - 1;
        }

        public Segment At(int segmentID)
        {
            if (segmentID < 0) return new Segment { ID = -1 }; // Empty Segment
            return segments[segmentID];
        }

        public Segment Last() => segments[this.Count];


        public void MergeSegments(PixelGraph graph, Segment s1, Segment s2)
        {
            if (s1==EmptySegment1 && s2==EmptySegment2)
            {
                Node n1=EmptySegment1.nodes[0];
                Node n2=EmptySegment2.nodes[0];
                EmptySegment1.nodes.RemoveAt(0);
                EmptySegment2.nodes.RemoveAt(0);
                CreateSegment(n1);
                n1.segment.Add(graph ,n2);
                return;
            }
            else if (s1==EmptySegment1)
            {
                Node n1 = EmptySegment1.nodes[0];
                EmptySegment1.nodes.RemoveAt(0);
                s2.Add(graph, n1);
                return;
            }
            else if (s2 == EmptySegment2)
            {
                Node n2 = EmptySegment2.nodes[0];
                EmptySegment2.nodes.RemoveAt(0);
                s1.Add(graph, n2);
                return;
            }

            Segment smallSegment = (s1.count < s2.count) ? s1 : s2;
            Segment bigSegment = (s1.count < s2.count) ? s2 : s1;
            for (int i = 0; i < smallSegment.count; i++)
            {
                bigSegment.Add(graph, smallSegment.nodes[i]);
            }
            this.segments.Remove(smallSegment); // O(n) operation
            // When smallSegment get's removed the segments after it get changed so the id
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
        // TODO: Find Intersection between red, blue and green

        public void CreateSegment(PixelGraph graph, (int y,int x) index)
        {
            Segment newSegment = new Segment
            {
                ID = NewSegmentId,
                nodes = new List<Node> { graph.Nodes[index.y, index.x] }
            };
            graph.Nodes[index.y, index.x].segment = newSegment;

            graph.Segments.Add(newSegment);
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
                nodes = new List<Node> { node }
            };
            node.segment = newSegment;
            this.Add(newSegment);
        }

        public void AddToSegment(PixelGraph graph, Segment segment, Node node) => segment.Add(graph,node);

        public void SegmentChannel(PixelGraph channelGraph, int k)
        {
            Node neighbor, myNode;
            for (int i = 0; i < channelGraph.height; i++)
            {
                for (int j = 0; j < channelGraph.width; j++)
                {
                    myNode = channelGraph.Nodes[i, j];

                    //create a segment just for this node so we can use segments comparison function
                    if (IsUnsegmented(myNode)) { 
                        myNode.segment = EmptySegment1;
                        EmptySegment1.nodes.Add(myNode);
                    }

                    for (int l = 0; l < myNode.neighbors.Count; l++)
                    {
                        neighbor = myNode.neighbors[l];

                        if (AreSameSegment(myNode, neighbor))
                            continue;

                        if (IsUnsegmented(neighbor))
                        {
                            neighbor.segment=EmptySegment2;
                            EmptySegment2.nodes.Add(neighbor);
                        }

                        //if the function returns true, then no merging and continue, else : merge
                        // we should stop getting segments using it's ID because its different from it's place #important
                        if (!neighbor.segment 
                            .SegmentsComparison(channelGraph,
                            myNode.segment,
                            k))
                        {
                            MergeSegments(channelGraph,
                                myNode.segment,
                                neighbor.segment
                            );
                        }
                        else
                        {
                            EmptySegment2.nodes.ForEach(n => n.segment = Segment.EmptySegment);
                            EmptySegment2.nodes.Clear();
                        }
                    }
                    EmptySegment1.nodes.ForEach(n => n.segment = Segment.EmptySegment);
                    EmptySegment1.nodes.Clear();
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
            sb.AppendLine($"Number of segments: {numSegments}");
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
