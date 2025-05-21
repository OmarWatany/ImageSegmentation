using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ImageTemplate
{
    public struct SegmentsIds
    {
        public int[,] ids;
        public SegmentsIds(PixelGraph graph)
        {
            ids = new int[graph.height, graph.width];
            for (int i = 0; i < graph.height; i++)
                for (int j = 0; j < graph.width; j++)
                    ids[i, j] = -1;
        }

        public int this[Node node]
        {
            get {  return ids[node.index.y, node.index.x]; }
            set {  ids[node.index.y, node.index.x] = value; }
        }

        public int this[int y, int x]
        {
            get {  return ids[y, x]; }
            set {  ids[y, x] = value; }
        }
    }

    public class Segments
    {
        public int segmentIdIncrement = 1;
        public int NewSegmentId => ++segmentIdIncrement; //O(1)
                                                         //we have to use that because segments.count is variable , so two segments can have the same id
        RGBPixel[] colors;
        public List<Segment> segments;
        SegmentsIds ids;


        int _count = 0;
        public int Count
        {
            get => (segments.Count != 0) ? segments.Count : _count;
            set => _count = value;
        }
        public static Segment EmptySegment1 = new Segment (), EmptySegment2 = new Segment();


        public Segments(PixelGraph graph) //O(1)
        {
            segments = new List<Segment>(30);
            ids = new SegmentsIds(graph);
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

            var f = bigSegment.nodeIndeces.First();
            var bigSegmentId = ids[f.y, f.x];
            int max = Math.Max(bigSegment.internalDifference, Math.Max(smallSegment.internalDifference, edge.weight));
            for (int i = 0; i < smallSegment.count; i++)//O(N)
            {
                var nidx = smallSegment.nodeIndeces[i];
                bigSegment.Add(graph[nidx.y,nidx.x], (byte)max); // O(1)
                ids[graph[nidx.y,nidx.x]] = bigSegmentId;
            }
        }

        public void CreateSegment(Node node)//O(1)
        {
            Segment newSegment = new Segment();
            newSegment.Add(node,0);
            ids[node] = NewSegmentId;
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
            this.segments.Clear();// O(N)
        }

        struct SegmentInfo
        {
            public RGBPixel color;
            public int count;

            public SegmentInfo(RGBPixel color,int count)
            {
                this.color = color;
                this.count = count;
            }

            public SegmentInfo Inc() => new SegmentInfo(this.color, this.count+1);
        }

        Dictionary<int, SegmentInfo> Ids = new Dictionary<int, SegmentInfo>();

        //TODO: complete the analysis
        public RGBPixel[,] Combine(PixelGraph RedGraph, PixelGraph BlueGraph, PixelGraph GreenGraph)
        {
            int[,] finalId = ids.ids;

            var groupMap = new Dictionary<(int rid,int gid, int bid), List<Node>>();
            for (var y = 0;y < BlueGraph.height; y++) // O(WH) width * height
            {
                for(var x= 0;x < BlueGraph.width; x++)
                {
                    Node node = BlueGraph.Nodes[y, x];

                    var redid = RedGraph.Segments.ids[node];
                    var greenid = GreenGraph.Segments.ids[node];
                    var blueid = BlueGraph.Segments.ids[node];

                    var key = (redid,greenid, blueid);

                    if (!groupMap.ContainsKey(key)) //O(1), because its a dictionary 
                        groupMap[key] = new List<Node>();
                    groupMap[key].Add(node); //O(1)
                }
            }

            // For each group, create a final segment and assign it to all nodeIndeces in the group
            foreach (var group in groupMap.Values) // O(G) G: number of combined segments
            {
                if (group.Count == 0) continue;
                var fid = NewSegmentId;//O(1)
                this.Count++;
                Ids[fid] = new SegmentInfo().Inc();
                finalId[group[0].index.y, group[0].index.x] = fid;
                for (int i = 1; i < group.Count; i++) // O(N) N: number of nodeIndeces in subsegment
                {
                    finalId[group[i].index.y, group[i].index.x] = fid;
                    Ids[fid] = Ids[fid].Inc();
                }
            }

            return ColorSegments(RedGraph);
            RGBPixel[,] ColorSegments(PixelGraph graph) //O(N*P) , N: number of Segments, P: number of pixels
            {
                var Nodes = graph.Nodes;
                int height = graph.height;
                int width = graph.width;

                var newImage = new RGBPixel[height,width];

                var colors = this.CreateRandomColors(Ids.Count); //O(N) , N: number of segments

                for (int i = 0; i < Ids.Count; i++)
                {
                    var item = Ids.ElementAt(i);
                    Ids[item.Key] = new SegmentInfo(colors[i],Ids[item.Key].count);
                }

                (int y,int x) NodeIdx;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        NodeIdx = Nodes[i, j].index;
                        newImage[i, j] = Ids[finalId[NodeIdx.y,NodeIdx.x]].color;//O(N) , N: number of segments
                    }
                }

                return newImage;
            }
        }

        public string GetSegmentsInfo()
        {
            int numSegments = this.Count;

            List<int> sizes = new List<int>();
            foreach (var seg in Ids) // O(S) S: number of final segments
            {
                sizes.Add(seg.Value.count);
            }

            sizes.Sort((a, b) => b.CompareTo(a)); // O(N LOG N) N: number of sizes

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{numSegments}"); // O(S) S: size of string
            foreach (int size in sizes) //O(NS) N: number of sizes, S: size of string
            {
                sb.AppendLine(size.ToString());
            }

            return sb.ToString();
        }

        public bool AreSameSegment(Node a, Node b) => ids[a] == ids[b];
        private bool IsUnsegmented(Node node) => ids[node] == -1;

        #region Coloring
        public void CreateRandomColors()
        {
            this.colors = this.CreateRandomColors(this.Count);
        }

        public RGBPixel[] CreateRandomColors(int count)//O(N) , N: number of segments
        {
            var colors = new RGBPixel[count+1];
            HashSet<int> usedColors = new HashSet<int>();
            Random rand = new Random();

            for (int i = 0; i < count+1; i++)
            {
                int rgb;
                do
                {
                    byte r = (byte)rand.Next(256);//O(1)
                    byte g = (byte)rand.Next(256);
                    byte b = (byte)rand.Next(256);
                    rgb = (r << 16) | (g << 8) | b; // encode RGB into 24 bits

                    if (!usedColors.Contains(rgb))
                    {
                        colors[i].red = r;
                        colors[i].green = g;
                        colors[i].blue = b;
                        usedColors.Add(rgb);
                        break;
                    }

                } while (true); // repeat until a new color is found//O(1)
            }
            return colors;
        }

        public void ColorSegments(PixelGraph graph) //O(N*P) , N: number of Segments, P: number of pixels
        {
            for (int i = 0; i < graph.height; i++)
            {
                for (int j = 0; j < graph.width; j++)
                {
                    graph.Picture[i, j] = colors[this.segments.IndexOf(graph.Nodes[i, j].segment)];//O(N) , N: number of segments
                }
            }
        }
        #endregion
    }
}
