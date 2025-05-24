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
        bool GaussianFilterApllied = false;
        string folderPath;
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
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {

            if (!GaussianFilterApllied && checkBox2.Checked)
            {
                ImageMatrix = ImageTemplate.ImageOperations.GaussianFilter1D(ImageMatrix, 5, 0.8);//O(N^2)
                GaussianFilterApllied = true;
            }

            int k = int.Parse(textBox1.Text);
            Stopwatch timer = Stopwatch.StartNew();

            RedGraph = new PixelGraph(this.ImageMatrix, x => x.red);
            RedGraph.Segments.SegmentChannel(RedGraph, k);//O(E*logE + E*N), E: number of Edges collected, N: number of pixels in smaller segment
            RedGraph.Edges = null;

            BlueGraph = new PixelGraph(this.ImageMatrix, x => x.blue);
            BlueGraph.Segments.SegmentChannel(BlueGraph, k);
            BlueGraph.Edges = null;

            GreenGraph = new PixelGraph(this.ImageMatrix, x => x.green);
            GreenGraph.Segments.SegmentChannel(GreenGraph, k);
            GreenGraph.Edges = null;

            final = new Segments(RedGraph);

            var NewImage = final.Combine(RedGraph, BlueGraph, GreenGraph);

            timer.Stop();

            long time = timer.ElapsedMilliseconds;

            MessageBox.Show("time: " + time + "ms\n" + "Segments Count: " + final.segments.Count);
            ImageOperations.DisplayImage(NewImage, pictureBox2);


            string textReportPath = Path.Combine(folderPath, "SegmentReport.txt");
            File.WriteAllText(textReportPath, final.GetSegmentsInfo());

            string imagePath = Path.Combine(folderPath, "myOutput.bmp");
            pictureBox2.Image.Save(imagePath, ImageFormat.Bmp);
        }

        private RGBPixel[,] CreateImage()
        {
            int s = 150;
            var NewImage = new RGBPixel[2 * s, 2 * s];
            for (var i = 0; i < s; i++)
                for (var j = 0; j < s; j++)
                    NewImage[i, j] = new RGBPixel();

            for (var i = 0; i < s; i++)
                for (var j = 0; j < s; j++)
                    NewImage[i + s, j] = new RGBPixel { red = 250 };

            for (var i = 0; i < s; i++)
                for (var j = 0; j < s; j++)
                    NewImage[i, j + s] = new RGBPixel { blue = 250 };

            bool o = true;
            for (var kk = 0; kk < 5; kk++)
            {
                for (var i = 0; i < (s/5); i++)
                {
                    for (var j = 1; j <= s; j++)
                    {
                        NewImage[i+(s/5*kk) + s, j - 1 + s] = o ?  new RGBPixel { red = 150 , blue = 150} : new RGBPixel { red = 250 , blue = 250};
                        if (j % (s /5) == 0) o = !o;
                    }
                    o = !o;
                }
                o = !o;
            }
            return NewImage;
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

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}