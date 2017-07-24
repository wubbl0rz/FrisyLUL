using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace FrisyLUL
{
    class ObservableStringContent : StringContent
    {
        private IProgress<double> progress;

        public ObservableStringContent(string content, IProgress<double> progress = null) : base(content)
        {
            this.progress = progress;
        }

        // eigene progress class init with size

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            this.TryComputeLength(out long size);
            Console.WriteLine(size);

            return Task.Run(() =>
            {
                using (var content = base.ReadAsStreamAsync().Result)
                {
                    var buffer = new byte[512];
                    
                    for(int cnt = 0, length = 0; (length = content.Read(buffer, 0, buffer.Length)) > 0; ++cnt)
                    {
                        this.progress.Report((int)((float)cnt*512/size*100));
                        stream.Write(buffer, 0, length);
                        stream.Flush();
                    }
                }
            });

            //return base.SerializeToStreamAsync(stream, context);
        }
    }

    class FreeOCR
    {
        [DataContract]
        private class ResponseRoot
        {
            [DataMember(Name = "OCRExitCode")]
            public int ExitCode { get; private set; }

            [DataMember(Name = "ParsedResults")]
            public ResponseResult[] Results { get; private set; }
        }

        [DataContract]
        private class ResponseResult
        {
            [DataMember(Name = "ParsedText")]
            public string Text { get; private set; }
        }

        private string apiKey = "4b18865d5488957";
        private string serviceUrl = "https://api.ocr.space/parse/image";

        private static HttpClient httpClient = new HttpClient();

        public async Task<string> readText(Screenshot screenshot)
        {
            var content = new MultipartFormDataContent();

            var p = new Progress<double>((y) => Console.WriteLine(y));

            content.Add(new StringContent(this.apiKey), "apikey");
            content.Add(new ObservableStringContent("data:image/bmp;base64," + 
                screenshot.ToBase64(ImageFormat.Bmp), p), "base64Image");

            using (var result = await FreeOCR.httpClient.PostAsync(this.serviceUrl, content))
            {
                if (!result.IsSuccessStatusCode) return string.Empty;

                Console.WriteLine("DASHIER");

                using (var json = await result.Content.ReadAsStreamAsync())
                {
                    var root = new DataContractJsonSerializer(typeof(ResponseRoot))
                        .ReadObject(json) as ResponseRoot;

                    if (root.ExitCode != 1) return string.Empty;

                    return root.Results.First().Text;
                }
            }
        }
    }
}
