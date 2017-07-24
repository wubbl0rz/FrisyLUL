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

        private async void Form1_Shown(object sender, EventArgs e)
        {
            Screenshot sc = Screenshot.TakeScreenshot("notepad");

            FreeOCR ocr = new FreeOCR();
            
            Console.WriteLine(await ocr.readText(sc));

            //OCR<FreeOCR> ocr = new OCR<FreeOCR>();

            //ocr.grabText(sc);

            return;

            //httpClient.PostAsync

            //var result = sc.ToBase64(ImageFormat.Jpeg);

            //Console.WriteLine(result);

            //base64
        }
    }
}
