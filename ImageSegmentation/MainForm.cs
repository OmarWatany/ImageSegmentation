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
        //PixelGraph RedGraph;
        PixelGraph BlueGraph;
        PixelGraph GreenGraph;

        int current = 0;
        int g => (++current) % 3;
        PixelGraph[] graphs = new PixelGraph[3];
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
                //RedGraph = new PixelGraph(this.ImageMatrix,x => x.red);
                BlueGraph = new PixelGraph(this.ImageMatrix, x => x.blue);
                GreenGraph = new PixelGraph(this.ImageMatrix, x => x.green);
                //graphs[0] = RedGraph;
                graphs[1] = BlueGraph;
                graphs[2] = GreenGraph;
            }
            textBox1.Text = "1";
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            int k = int.Parse(textBox1.Text);
            Stopwatch timer = Stopwatch.StartNew();
            //RedGraph.Segments.SegmentChannel(RedGraph, k);
            //RedGraph.Segments.ColorSegments(RedGraph);
            //int ur = RedGraph.Segments.CountUnSegmented();
            timer.Stop();
            BlueGraph.Segments.SegmentChannel(BlueGraph, k);
            BlueGraph.Segments.ColorSegments(BlueGraph);
            //ur = BlueGraph.Segments.CountUnSegmented();
            GreenGraph.Segments.SegmentChannel(GreenGraph, k);
            GreenGraph.Segments.ColorSegments(GreenGraph);
            //ur = GreenGraph.Segments.CountUnSegmented();

            //Segments final = new Segments();
            //final.Combine(RedGraph, BlueGraph, GreenGraph);

            //var colors = final.CreateRandomColors(final.Count + 1);
            //final.ColorSegments(colors, RedGraph);
            //ur = RedGraph.Segments.CountUnSegmented();

            long time = timer.ElapsedMilliseconds;
            //Console.WriteLine("number of segments:" + RedGraph.Segments.segments.Count);
            Console.WriteLine("Milliseconds taken to segment the image:" + time);
            Console.WriteLine("Seconds taken to segment the image:" + time/1000);

            //ImageOperations.DisplayImage(RedGraph.Picture, pictureBox2);

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Segment Report";
                saveFileDialog.FileName = "SegmentReport.txt";

                using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                {
                    //sw.WriteLine(final.GetSegmentsInfo());
                }
                //if (saveFileDialog.ShowDialog() == DialogResult.OK)
                //{
                //    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                //    {
                //        sw.WriteLine(final.GetSegmentsInfo());
                //    }

                //    MessageBox.Show("Segment report has been saved to:\n" + saveFileDialog.FileName,
                //    "File Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
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
            //if (current==1)
            //    ImageOperations.DisplayImage(RedGraph.Picture, pictureBox2);
            if (current == 0)
                ImageOperations.DisplayImage(BlueGraph.Picture, pictureBox2);
            else if (current == 2)
                ImageOperations.DisplayImage(GreenGraph.Picture, pictureBox2);

            current++;
            if (current == 3)
            {
                current = 0;
            }
        }
    }
}