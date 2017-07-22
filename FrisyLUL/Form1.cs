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

        private void Form1_Shown(object sender, EventArgs e)
        {
            Screenshot sc = new Screenshot.TakeScreenshot("notepad");

            var apiKey = "4b18865d5488957";

            Console.WriteLine(sc.Width);
            Console.WriteLine(sc.Height);

            sc = sc.Extract(1600, 700, sc.Width - 1600, sc.Height - 700);

            sc.Save("blub.png");

            HttpClient httpClient = new HttpClient();

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(apiKey), "apikey");
            content.Add(new StringContent("data:image/png;base64," + sc.ToBase64(ImageFormat.Png)), "base64Image");

            var result = httpClient.PostAsync("https://api.ocr.space/parse/image", content);

            Console.WriteLine(result.Result.Content.ReadAsStringAsync().Result);

            //httpClient.PostAsync

            //var result = sc.ToBase64(ImageFormat.Jpeg);

            //Console.WriteLine(result);

            //base64
        }
    }
}
