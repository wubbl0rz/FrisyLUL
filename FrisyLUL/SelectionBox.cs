using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Device = SharpDX.Direct3D11.Device;

namespace FrisyLUL
{
    class SelectionBox : Control
    {
        private Device device;
        private SwapChain swapChain;
        private Texture2D backBuffer;
        private RenderTargetView targetView;

        private Surface1 surface;
        private Bitmap1 bitmap;
        private SharpDX.Direct2D1.DeviceContext painter;

        Task renderTask;

        private FpsCounter fpsCounter = new FpsCounter();

        public IntPtr TargetHandle { get; set; }
        public Size CanvasSize => new Size(this.swapChain.Description.ModeDescription.Width, this.swapChain.Description.ModeDescription.Height);

        private enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
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

        public SelectionBox(IntPtr targetHandle)
        {
            this.TargetHandle = targetHandle;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);

            this.CreateDevice();
            this.SetupResources();
            this.StartRendering();
        }

        private void ResizeSwapChain(int width, int height) {
            this.CleanResources();
            this.swapChain.ResizeBuffers(2, width, height, Format.B8G8R8A8_UNorm, SwapChainFlags.GdiCompatible);
            this.SetupResources();
        }

        private void SetupResources()
        {
            this.backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            this.targetView = new RenderTargetView(this.device, this.backBuffer);

            this.surface = this.backBuffer.QueryInterface<Surface1>();
            this.painter = new SharpDX.Direct2D1.DeviceContext(this.surface);
            this.bitmap = new Bitmap1(this.painter, this.surface);
            this.painter.Target = this.bitmap;

            device.ImmediateContext.OutputMerger.SetRenderTargets(targetView);
        }

        private void CleanResources() {
            this.backBuffer.Dispose();
            this.targetView.Dispose();
            this.surface.Dispose();
            this.painter.Dispose();
            this.bitmap.Dispose();
        }

        private void CreateDevice()
        {
            var desc = new SwapChainDescription()
            {
                BufferCount = 2,
                IsWindowed = true,
                Flags = SwapChainFlags.GdiCompatible,
                ModeDescription =
                    new ModeDescription(640, 480,
                    new Rational(60, 1),
                    Format.B8G8R8A8_UNorm),
                OutputHandle = this.Handle,
                SampleDescription = new SampleDescription() { Count = 1, Quality = 0 },
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.BgraSupport, desc, out device, out swapChain);
        }

        private bool stopRendering = false;

        private void StartRendering() {
            this.renderTask = Task.Run(() => {
                var colorBackground = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1);
                var colorText = new SharpDX.Mathematics.Interop.RawColor4(255, 255, 255, 1);

                SharpDX.DirectWrite.Factory factory = new SharpDX.DirectWrite.Factory();

                var textFormat = new TextFormat(factory, "Arial", 100);

                while (!this.stopRendering)
                {
                    if (this.CanvasSize != this.ClientRectangle.Size)
                    {
                        this.ResizeSwapChain(this.Width, this.Height);
                    }

                    SolidColorBrush brush = new SolidColorBrush(painter, colorText);

                    device.ImmediateContext.ClearRenderTargetView(this.targetView, colorBackground);

                    var g = System.Drawing.Graphics.FromHwnd(this.TargetHandle);
                    var dc = this.surface.GetDC(new SharpDX.Mathematics.Interop.RawBool(true));

                    var size = g.VisibleClipBounds.Size.ToSize();

                    BitBlt(dc, 0, 0, size.Width, size.Height, g.GetHdc(), 0, 0, TernaryRasterOperations.SRCCOPY);

                    g.ReleaseHdc();
                    g.Dispose();

                    this.surface.ReleaseDC();

                    this.painter.BeginDraw();

                    this.painter.DrawText(this.fpsCounter.GetFps().ToString(),
                        textFormat, new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, 500, 500),
                        brush);

                    this.painter.EndDraw();

                    brush.Dispose();

                    this.fpsCounter.Update();

                    this.swapChain.Present(0, PresentFlags.None);
                }

                textFormat.Dispose();
                factory.Dispose();
            });
        }

        private void Draw() {

        }

        protected override void Dispose(bool disposing)
        {
            this.stopRendering = true;
            this.renderTask.Wait();
            this.CleanResources();
            this.device.Dispose();
            this.swapChain.Dispose();
            base.Dispose(disposing);
        }
    }
}
