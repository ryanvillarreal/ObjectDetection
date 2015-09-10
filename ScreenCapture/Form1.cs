using System;
using System.Diagnostics; 
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;

namespace ScreenCapture
{
    public partial class Form1 : Form
    {
        // use this to get window sizes 
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);


        RECT Rect = new RECT();
        Rectangle[] region;
        MCvObjectDetection[] regions; 

        public Form1()
        {
            InitializeComponent();

            Application.Idle += updateFrame; 
        }

        private void GetScreenImage(Rectangle _Rectangle)
        {
            Size _Size = new Size(_Rectangle.Width, _Rectangle.Height);

            try
            {
                using (Bitmap _Bitmap = new Bitmap(_Size.Width, _Size.Height))
                {
                    using (Graphics _Graphics = Graphics.FromImage(_Bitmap))
                    {
                        _Graphics.CopyFromScreen(new Point(_Rectangle.X, _Rectangle.Y), Point.Empty, _Size);
                        Image<Bgr, Byte> myImage = new Image<Bgr, Byte>(_Bitmap);
                        imageBox1.Image = myImage;

                        objectDetection(myImage); 
                    }
                }
            }
            catch(ArgumentException except)
            {
                Console.WriteLine(except.Message);
                    return; 
            }
        }

        private void objectDetection(Image<Bgr, Byte> myImage)
        {
            using (HOGDescriptor des = new HOGDescriptor())
            {
                des.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());
                regions = des.DetectMultiScale(myImage);

            }

            foreach (MCvObjectDetection pedestrian in regions)
            {
                myImage.Draw(pedestrian.Rect, new Bgr(Color.Red), 1);
            }
        }

        private void updateFrame(object sender, EventArgs e)
        {
            // lets get windows here and look for the one we are interested in
            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (process.ProcessName == "chrome")
                    {
                        IntPtr handle = process.MainWindowHandle;

                        GetWindowRect(handle, ref Rect);
                        Rectangle rect = new Rectangle(Rect.left, Rect.top, Math.Abs(Rect.left), Rect.bottom);

                        GetScreenImage(rect);
                    }
                }
            }

        }
    }
}
