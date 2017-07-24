using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace FrisyLUL
{
    class OCR<T> where T : IOCRservice, new()
    {
        public void grabText(Screenshot screenshot) {
            var service = new T();
            service.Scan(screenshot);
        }
    }

    interface IOCRservice
    {
        string Scan(Screenshot screenshot);
    }

    class FreeOCR : IOCRservice
    {
        [DataContract]
        private class ResponseRoot
        {
            [DataMember]
            public int OCRExitCode;

            [DataMember]
            public ResponseResult[] ParsedResults;
        }

        [DataContract]
        private class ResponseResult
        {
            [DataMember]
            public string ParsedText;
        }

        private string apiKey = "4b18865d5488957";

        public string Scan(Screenshot screenshot)
        {
            HttpClient httpClient = new HttpClient();

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(this.apiKey), "apikey");
            content.Add(new StringContent("data:image/png;base64," + 
                screenshot.ToBase64(ImageFormat.Png)), "base64Image");

            var result = httpClient.PostAsync("https://api.ocr.space/parse/image", content).Result;

            var json = result.Content.ReadAsStreamAsync().Result;

            DataContractJsonSerializer ser = new
                DataContractJsonSerializer(typeof(ResponseRoot));

            ResponseRoot root = ser.ReadObject(json) as ResponseRoot;

            if (root.OCRExitCode != 1) return string.Empty;
            
            Console.WriteLine(root.ParsedResults.First().ParsedText);

            return "";
        }
    }
}
