using System;
using System.Diagnostics;
using System.Drawing.Imaging;
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
        string folderPath;
        int current = 0;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                folderPath = Path.GetDirectoryName(OpenedFilePath);
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                pictureBox2.Image = null;
                //it must be checked before opening the image
                if (checkBox2.Checked)
                    ImageMatrix = ImageTemplate.ImageOperations.GaussianFilter1D(ImageMatrix, 5, 0.8);//O(N^2)
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            int k = int.Parse(textBox1.Text);
            Stopwatch timer = Stopwatch.StartNew();

            RedGraph = new PixelGraph(this.ImageMatrix,x => x.red);
            RedGraph.Segments.SegmentChannel(RedGraph, k);//O(E*logE + E*N), E: number of edges collected, N: number of pixels in smaller segment
            RedGraph.Edges = null;

            BlueGraph = new PixelGraph(this.ImageMatrix, x => x.blue);
            BlueGraph.Segments.SegmentChannel(BlueGraph, k);
            BlueGraph.Edges = null;


            GreenGraph = new PixelGraph(this.ImageMatrix, x => x.green);
            GreenGraph.Segments.SegmentChannel(GreenGraph, k);
            GreenGraph.Edges = null;


            final = new Segments();

            var NewImage = final.Combine(RedGraph, BlueGraph, GreenGraph);

            timer.Stop();

            long time = timer.ElapsedMilliseconds;
            Console.WriteLine("number of segments:" + final.segments.Count);
            Console.WriteLine("Milliseconds taken to segment the image:" + time);
            Console.WriteLine("Seconds taken to segment the image:" + time/1000);

            ImageOperations.DisplayImage(NewImage, pictureBox2);
            RedGraph.Segments.CreateRandomColors();
            BlueGraph.Segments.CreateRandomColors();
            GreenGraph.Segments.CreateRandomColors();


            string textReportPath = Path.Combine(folderPath, "SegmentReport.txt");
            string imagePath = Path.Combine(folderPath, "myOutput.bmp");

            File.WriteAllText(textReportPath, final.GetSegmentsInfo());
            pictureBox2.Image.Save(imagePath, ImageFormat.Bmp);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //textBox1.Text = "1";
            textBox1.Text = "30000";
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            //if(current==-1)
            //    final.ColorFinalSegments(RedGraph);
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

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}