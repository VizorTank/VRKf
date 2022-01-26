using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Drawing.Imaging;
using AForge.Imaging;
//using VRKf_WMS_Prototype.Models;
using Newtonsoft.Json;
using VRKf_WMS_Prototype.Models;

namespace VRKf_WMS_Prototype.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;


        [BindProperty(SupportsGet = true)]
        public string Address { get; set; }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            
            // 23.064779f, 53.061270f, 23.248423f, 53.177203f
            var ew = "https://wms.gisbialystok.pl/arcgis/services/Ewidencja/MapServer/WMSServer?";
            var ort = "https://wms.gisbialystok.pl/arcgis/services/MSIP_orto2019/MapServer/WMSServer?";
            int layer = 0;
            int[] size = { 2000, 1000 };
            float[] pos = { 23.064779f, 53.061270f };
            float[] realSize = { 0.183644f, 0.115933f };
            float[] scaleSize = { 0.0183644f*4, 0.0115933f*4 };
            string tmpImagePath = "wwwroot/Data/a.png";
            byte[] response;
            //realSize[0] /= 2;
            //realSize[1] /= 2;
            string res ="";
            string data = await GetPos("http://api.positionstack.com/v1/forward", Address);
            DataList position = System.Text.Json.JsonSerializer.Deserialize<DataList>(data);

            
            if (position.data == null)
            {
                response = await GetByteMap(ew, layer, size, new float[] { pos[0], pos[1], pos[0] + realSize[0], pos[1] + realSize[1] });
            }
            else
            {
                res = "" + position.data.First().latitude + " " + position.data.First().longitude;
                response = await GetByteMap(ew, layer, size, new float[] { position.data.First().longitude - scaleSize[0], position.data.First().latitude - scaleSize[1] , position.data.First().longitude + scaleSize[0], position.data.First().latitude + scaleSize[1] });
            }
            ViewData["image"] = Convert.ToBase64String(response);
            ViewData["featureinfo"] = res;
            ViewData["image2"] = ImageToString(GetImage(tmpImagePath));

            ImageProcessing(GetBitmap(response));
            //ImageProcessing(tmpImagePath);
            //
        }
        public async Task<byte[]> GetByteMap(string server, int layer, int[] size, float[] pos)
        {
            using (var client = new HttpClient())
            {
                var url = server +
                "request=GetMap&service=WMS&version=1.1.1" +
                "&layers=" + layer +
                "&styles=default" +
                "&srs=EPSG:4326" +
                "&bbox=" + pos[0].ToString().Replace(",", ".") +
                "," + pos[1].ToString().Replace(",", ".") +
                "," + pos[2].ToString().Replace(",", ".") +
                "," + pos[3].ToString().Replace(",", ".") +
                "&width=" + size[0] +
                "&height=" + size[1] +
                "&format=image/png32";
                var uri = new Uri(url);

                var response = await client.GetAsync(uri);
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        public async Task<string> GetFeatureInfo(string server)
        {
            using (var client = new HttpClient())
            {
                // https://wms.gisbialystok.pl/arcgis/services/Ewidencja/MapServer/WMSServer?
                // request=GetMap&service=WMS&version=1.1.1
                // &layers=0
                // &styles=default
                // &srs=EPSG%3A4326
                // &bbox=23.064779,53.061270,23.248423,53.177203
                // &&width=780
                // &height=330
                // &format=image%2Fpng
                var url = "https://wms.gisbialystok.pl/arcgis/services/Ewidencja/MapServer/WMSServer?request=GetFeatureInfo&service=WMS&version=1.1.1&layers=0&styles=default&srs=EPSG%3A4326&bbox=23.064779,53.061270,23.248423,53.177203&&width=780&height=330&format=image%2Fpngwms.gisbialystok.pl";
                var uri = new Uri(url);

                var response = await client.GetAsync(uri);
                return await response.Content.ReadAsStringAsync();
            }
        }
        public async Task<string> GetPos(string server, string address)
        {
            using (var client = new HttpClient())
            {
                string accessKey = "3fae4edf360680a55977be834e713664";
                var url = server + "?access_key=" + accessKey + "&output=json&query=" + address;
                var uri = new Uri(url);

                var response = await client.GetAsync(uri);
                //JsonSerializer.Deserialize(await response.Content.ReadAsByteArrayAsync());
                return await response.Content.ReadAsStringAsync();
            }
        }

        public Bitmap GetImage(string imagePath)
        {
            Bitmap bitmap = new Bitmap(imagePath);
            return bitmap;
        }
        public string ImageToString(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return Convert.ToBase64String((byte[])converter.ConvertTo(img, typeof(byte[])));
        }
        public void ImageProcessing(string imagePath)
        {
            string tmpImagePath = "wwwroot/Data/a.png";
            Bitmap image = GetImage(tmpImagePath);
            ImageProcessing(image);
        }
        public void ImageProcessing(Bitmap image)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(image);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            List<AForge.IntPoint> points = blobCounter.GetBlobsEdgePoints(blobs[0]);
            //ViewData["featureinfo"] = blobs[0].Area.ToString();
        }

        public Bitmap GetBitmap(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return new Bitmap(ms);
            }
        }
        public class DataList
        {
            public List<PositionData> data { get; set; }
        }

    }
}
