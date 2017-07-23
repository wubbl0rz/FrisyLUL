﻿using System;
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
    class Screenshot : IDisposable
    {
        private Bitmap bitmap;

        enum TernaryRasterOperations : uint
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
        public int Height => this.IsEmpty ? 0 : this.bitmap.Height;
        public int Width => this.IsEmpty ? 0 : this.bitmap.Width;

        public Rectangle Bounds { get => new Rectangle(Point.Empty, new Size(this.Width, this.Height)); }

        private Screenshot() : this(new Bitmap(1,1))
        {
            this.IsEmpty = true;
        }

        private Screenshot(Bitmap bitmap) {
            this.bitmap = bitmap;
        }

        public void Save(string filename) {
            this.bitmap.Save(filename);
        }

        public string ToBase64(ImageFormat format)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                this.bitmap.Save(memoryStream, format);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public Screenshot Extract(int x, int y, int width, int height) {
            Rectangle rect = new Rectangle(x, y, width, height);

            Rectangle scRect = this.Bounds;
            scRect.Intersect(rect);
            
            return scRect.IsEmpty ? new Screenshot() : new Screenshot(this.bitmap.Clone(scRect, this.bitmap.PixelFormat));
        }

        /// <summary>
        /// Takes a <see cref="Screenshot"/> of the first window with the given case insensitive <see cref="String"/> title
        /// </summary>
        /// <param name="windowTitle">Title of window. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">When window title is null.</exception>  
        public static Screenshot TakeScreenshot(string windowTitle) {
            windowTitle = windowTitle?.ToLower() ?? throw new ArgumentNullException(nameof(windowTitle));

            var process = Process.GetProcesses()
                .FirstOrDefault(proc => proc.MainWindowHandle != IntPtr.Zero && proc.MainWindowTitle.ToLower().Contains(windowTitle));

            if(process == null) return new Screenshot();

            var handle = process.MainWindowHandle;
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
                TernaryRasterOperations.SRCCOPY | TernaryRasterOperations.CAPTUREBLT);

            source.ReleaseHdc();
            target.ReleaseHdc();
            source.Dispose();
            target.Dispose();

            return new Screenshot(targetBitmap);
        }

        public void Dispose()
        {
            this.bitmap.Dispose();
        }
    }
}
