using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ImageTemplate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        PixelGraph RedGraph;
        PixelGraph BlueGraph;
        PixelGraph GreenGraph;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                //ImageMatrix = ImageTemplate.ImageOperations.GaussianFilter1D(ImageMatrix, 5, 0.8); //what filter size to use? //O(N^2)
                RedGraph = new PixelGraph(this.ImageMatrix,x => x.blue);
                BlueGraph = new PixelGraph(this.ImageMatrix, x => x.blue);
                GreenGraph = new PixelGraph(this.ImageMatrix, x => x.green);
            }
            textBox1.Text = "1";
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            int k = int.Parse(textBox1.Text);
            Stopwatch timer = Stopwatch.StartNew();
            RedGraph.Segments.SegmentChannel(RedGraph, k);
            timer.Stop();

            long time = timer.ElapsedMilliseconds;
            Console.WriteLine("number of segments:" + RedGraph.Segments.segments.Count);
            Console.WriteLine("Milliseconds taken to segment the image:" + time);
            Console.WriteLine("Seconds taken to segment the image:" + time/1000);

            BlueGraph.Segments.SegmentChannel(BlueGraph, k);
            GreenGraph.Segments.SegmentChannel(GreenGraph, k);

            Segments final = new Segments();
            final.Combine(RedGraph, BlueGraph, GreenGraph);

            var colors = final.CreateRandomColors(final.Count + 1);
            final.ColorSegments(colors, RedGraph);

            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}