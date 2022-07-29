using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.Util;
using DirectShowLib;
using System.Net;

namespace Praktika_
{

    public partial class Form1 : Form
    {
        private VideoCapture capture = null;

        private double frames;

        private double framesCounter;

        private double fps;

        private bool play = false;

        private DsDevice[] webCams = null;

        private int selectedCamera = 0;

        private int selectedCameraId;
        public Form1()
        {
            InitializeComponent();
         
        }
        private Image<Bgr, byte> Find(Image<Bgr, byte> image)
        {

            MCvObjectDetection[] regions;

            using (HOGDescriptor descriptor = new HOGDescriptor())
            {
                descriptor.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

                regions = descriptor.DetectMultiScale(image);
            }

            foreach (MCvObjectDetection pesh in regions)
            {
                image.Draw(pesh.Rect, new Bgr(Color.Red), 3);

            }

            return image;
        }


        private async void ReadFrames()
        {
            Mat m = new Mat();

            while (play && framesCounter < frames)
            {
                framesCounter += 1;

                capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, framesCounter);

                capture.Read(m);

                pictureBox1.Image = m.Bitmap;

                pictureBox1.Image = Find(m.ToImage<Bgr, byte>()).Bitmap;

                await Task.Delay(1000 / Convert.ToInt16(fps));
            }
            
        }



        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult res = openFileDialog1.ShowDialog();

                if (res == DialogResult.OK)
                {
                    capture = new VideoCapture(openFileDialog1.FileName);

                    Mat m = new Mat();

                    capture.Read(m);

                    pictureBox1.Image = m.Bitmap;

                    fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);

                    frames = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);

                    framesCounter = 1;
                }
                else
                {
                    MessageBox.Show("Видео не выбрано!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            try
            {
                if (capture == null)
                    throw new Exception("Видео не выбрано!");

                play = true;

                ReadFrames();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // this.WindowState = FormWindowState.Maximized;

            webCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < webCams.Length; i++)
            {
                comboBox1.Items.Add(webCams[i].Name);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCamera = comboBox1.SelectedIndex;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (webCams.Length == 0)
                {
                    throw new Exception("Нет доступных камер");
                }
                else if (comboBox1.SelectedItem == null)
                {
                    throw new Exception("Необходимо выбрать камеру!");
                }
                else if (capture != null)
                {
                    capture.Start();

                }
                else
                {
                    capture = new VideoCapture(selectedCameraId);

                    capture.ImageGrabbed += Capture_ImageGrabbed;

                    capture.Start();

                  
                    String host = Dns.GetHostName();
                    IPHostEntry ip = Dns.GetHostEntry(host);
                    textBox1.Text = ip.AddressList[0].ToString();
                    textBox2.Text = host.ToString();
                    textBox3.Text = host.ToString();
                    textBox3.PasswordChar = '*';
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            

        }
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();

                capture.Retrieve(m);

                pictureBox1.Image = m.ToImage<Bgr, byte>().Flip(Emgu.CV.CvEnum.FlipType.Horizontal).Bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null)
                { 
                    capture.Pause();

                    capture.Dispose();

                    capture = null;

                    pictureBox1.Image.Dispose();

                    pictureBox1.Image = null;

                    selectedCameraId = 0;

                    textBox1.Clear();
                
                    textBox2.Clear();
              
                    textBox3.Clear();
                 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                play = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
