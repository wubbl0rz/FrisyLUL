using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FrisyLUL
{
    public class Screenshot : IDisposable
    {
        public Bitmap Bitmap { get; private set; }

        private enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            CAPTUREBLT = 0x40000000
        }
        
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        static extern bool BitBlt(IntPtr hdc,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            TernaryRasterOperations dwRop);

        public bool IsEmpty { get; private set; }
        public int Height => this.IsEmpty ? 0 : this.Bitmap.Height;
        public int Width => this.IsEmpty ? 0 : this.Bitmap.Width;

        public Rectangle Bounds { get => new Rectangle(Point.Empty, new Size(this.Width, this.Height)); }

        private Screenshot() : this(new Bitmap(1,1))
        {
            this.IsEmpty = true;
        }

        private Screenshot(Bitmap bitmap) {
            this.Bitmap = bitmap;
        }

        public void Save(string filename) {
            if(this.IsEmpty) return;

            this.Bitmap.Save(filename);
        }

        public string ToBase64(ImageFormat format)
        {
            if (this.IsEmpty) return string.Empty;
            
            using (MemoryStream memoryStream = new MemoryStream())
            {
                this.Bitmap.Save(memoryStream, format);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public Screenshot Extract(int x, int y, int width, int height) {
            Rectangle rect = new Rectangle(x, y, width, height);

            Rectangle scRect = this.Bounds;
            scRect.Intersect(rect);
            
            return scRect.IsEmpty ? new Screenshot() : new Screenshot(this.Bitmap.Clone(scRect, this.Bitmap.PixelFormat));
        }

        /// <summary>
        /// Takes a <see cref="Screenshot"/> of the first window with the given case insensitive <see cref="String"/> title
        /// </summary>
        /// <param name="windowTitle">Title of window. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">When window title is null.</exception>  
        public static Screenshot TakeScreenshot(string windowTitle) {
            windowTitle = windowTitle?.ToLower() ?? throw new ArgumentNullException(nameof(windowTitle));

            //var handle = Process.GetProcesses()
            //    .Where(proc => proc.MainWindowHandle != IntPtr.Zero && proc.MainWindowTitle.ToLower().Contains(windowTitle))
            //    .Select(proc => proc.MainWindowHandle)
            //    .FirstOrDefault();

            var handle = new IntPtr(67246);

            if (handle == IntPtr.Zero) return new Screenshot();
            
            Graphics source = Graphics.FromHwnd(handle);

            if (source.IsVisibleClipEmpty) return new Screenshot();

            Rectangle sourceWindowSize = Rectangle.Round(source.VisibleClipBounds);

            Bitmap targetBitmap = new Bitmap(sourceWindowSize.Width, sourceWindowSize.Height);
            Graphics target = Graphics.FromImage(targetBitmap);

            IntPtr sourceDC = source.GetHdc();
            IntPtr targetDC = target.GetHdc();

            BitBlt(targetDC,
                0,
                0,
                targetBitmap.Width,
                targetBitmap.Height,
                sourceDC,
                0,
                0,
                TernaryRasterOperations.SRCCOPY);

            source.ReleaseHdc();
            target.ReleaseHdc();
            source.Dispose();
            target.Dispose();

            return new Screenshot(targetBitmap) { Handle = handle };
        }

        public IntPtr Handle { get; private set; }

        public void Dispose()
        {
            this.Bitmap.Dispose();
        }
    }
}
