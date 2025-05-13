using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
                ImageMatrix = ImageTemplate.ImageOperations.GaussianFilter1D(ImageMatrix, 9, 0.8); //what filter size to use? //O(N^2)
                RedGraph = new PixelGraph(this.ImageMatrix,x => x.red);
                SegmentOperations.SegmentChannel(RedGraph,1);
                SegmentOperations.ColorSegments(RedGraph);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);

                //BlueGraph = new PixelGraph(this.ImageMatrix, x => x.blue);
                //GreenGraph = new PixelGraph(this.ImageMatrix, x => x.green);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}