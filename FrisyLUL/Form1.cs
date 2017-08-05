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
        }

        protected override void OnShown(EventArgs e)
        {
            var vlcHandle = System.Diagnostics.Process.GetProcesses()
                .Where(proc => proc.MainWindowHandle != IntPtr.Zero && proc.MainWindowTitle.ToLower().Contains("vlc"))
                .Select(proc => proc.MainWindowHandle)
                .FirstOrDefault();

            var sel = new SelectionBox(vlcHandle);
            sel.Dock = DockStyle.Fill;
            this.Controls.Add(sel);
            base.OnShown(e);
        }
    }
}
