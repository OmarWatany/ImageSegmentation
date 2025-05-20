using System;
using System.Diagnostics;
using System.IO;
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
        Segments final;
        RGBPixel[] colors;

        int current = 0;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                ImageMatrix = ImageTemplate.ImageOperations.GaussianFilter1D(ImageMatrix, 5, 0.8); //what filter size to use? //O(N^2)
                RedGraph = new PixelGraph(this.ImageMatrix,x => x.red);
                BlueGraph = new PixelGraph(this.ImageMatrix, x => x.blue);
                GreenGraph = new PixelGraph(this.ImageMatrix, x => x.green);
                final = new Segments();
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
            BlueGraph.Segments.SegmentChannel(BlueGraph, k);
            GreenGraph.Segments.SegmentChannel(GreenGraph, k);
            timer.Stop();

            final.Combine(RedGraph, BlueGraph, GreenGraph);

            colors = final.CreateRandomColors(final.Count + 1);
            final.ColorSegments(colors, RedGraph);

            long time = timer.ElapsedMilliseconds;
            Console.WriteLine("number of segments:" + final.segments.Count);
            Console.WriteLine("Milliseconds taken to segment the image:" + time);
            Console.WriteLine("Seconds taken to segment the image:" + time/1000);

            ImageOperations.DisplayImage(RedGraph.Picture, pictureBox2);

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Segment Report";
                saveFileDialog.FileName = "SegmentReport.txt";

                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    sw.WriteLine(final.GetSegmentsInfo());
                }
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        sw.WriteLine(final.GetSegmentsInfo());
                    }
                }
            }

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
        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = null;
            if(current==-1)
                final.ColorSegments(colors, RedGraph);
            if (current==0)
                RedGraph.Segments.ColorSegments(RedGraph);
            if (current == 1)
                GreenGraph.Segments.ColorSegments(GreenGraph);
            else if (current == 2)
                BlueGraph.Segments.ColorSegments(BlueGraph);

            ImageOperations.DisplayImage(GreenGraph.Picture, pictureBox2);
            current++;
            if (current == 3)
            {
                current = -1;
            }
        }
    }
}