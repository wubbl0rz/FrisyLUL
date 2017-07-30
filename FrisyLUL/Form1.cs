using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrisyLUL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private Bitmap lastBitmap = null;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Bitmap bitmap;

            if (!this.bitmapQueue.Any())
            {
                bitmap = this.lastBitmap;
                if (this.lastBitmap == null) return;
            }
            else
            {
                bitmap = this.bitmapQueue.Dequeue();
                if (this.lastBitmap != null)  this.lastBitmap.Dispose();
                this.lastBitmap = bitmap;
            }

            var painter = e.Graphics;

            painter.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            painter.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            painter.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var bitmapRatio = (float)bitmap.Width / (float)bitmap.Height;
            var formRatio = (float)this.Width / (float)this.Height;

            int width, height;

            if (formRatio > bitmapRatio)
            {
                height = this.Height;
                width = Convert.ToInt32(height * bitmapRatio);
            }
            else
            {
                width = this.Width;
                height = Convert.ToInt32(width / bitmapRatio);
            }

            Console.WriteLine($"width: {width} height: {height}");

            //Console.WriteLine(ratio);

            painter.DrawImage(bitmap, new Rectangle(0, this.textBox1.Bottom + 10,
                width,
                height));

            sw.Stop();
            //bitmap.Dispose();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        private Queue<Bitmap> bitmapQueue = new Queue<Bitmap>();

        private void Form1_Shown(object sender, EventArgs e)
        {
            // queue max size
            Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                
                while (true)
                {
                    sw.Start();
                    if (this.bitmapQueue.Count < 20)
                    {
                        var bitmap = Screenshot.TakeScreenshot("vlc").Bitmap;

                        this.bitmapQueue.Enqueue(bitmap);
                    }

                    this.Invalidate();
                    //this.pictureBox1.Invalidate();

                    sw.Stop();
                    //Console.WriteLine(sw.ElapsedMilliseconds);
                    sw.Reset();
                }
            });

            return;

            //httpClient.PostAsync

            //var result = sc.ToBase64(ImageFormat.Jpeg);

            //Console.WriteLine(result);

            //base64
        }
    }
}
