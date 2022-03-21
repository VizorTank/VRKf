using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
//using VRKf_WMS_Prototype.Models;
using VRKf_WMS_Prototype.Models;

namespace VRKf_WMS_Prototype.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync(float? la, float? lo)
        {
            // Politechnika kierunek informatyka
            // latitude: 53.11675
            // longtitude: 23.1466

            // 23.064779f, 53.061270f, 23.248423f, 53.177203f
            var ew = "https://wms.gisbialystok.pl/arcgis/services/Ewidencja/MapServer/WMSServer?";
            var ort = "https://wms.gisbialystok.pl/arcgis/services/MSIP_orto2019/MapServer/WMSServer?";
            int layer = 8;
            //int[] size = { 2000, 4000 };
            int[] size = { 2000, 2000 };
            float[] pos = { 23.064779f, 53.061270f };
            float[] realSize = { 0.183644f, 0.115933f };
            float[] radius = { realSize[0] / 2, realSize[1] / 2 };
            float[] centerPos = { pos[0] + radius[0], pos[1] + radius[1] };

            //float[] zoom = { 50f, 50f };
            float[] zoom = { 100f, 100f };


            radius[0] /= zoom[0];
            radius[1] /= zoom[1];

            //size[0] = (int)(size[0] * zoom[0]);
            //size[1] = (int)(size[1] * zoom[1]);


            string tmpImagePath = "wwwroot/Data/a.png";
            float[] searchPos;
            //realSize[0] /= 2;
            //realSize[1] /= 2;
            if (lo == null || la == null)
            {
                string data = await GetPos("http://api.positionstack.com/v1/forward", "Wiejska 45, Białystok, Poland");

                DataList position = System.Text.Json.JsonSerializer.Deserialize<DataList>(data);
                searchPos = new float[] { position.data.First().longitude, position.data.First().latitude };
                if (position != null)
                    ViewData["featureinfo"] = "" + position.data.First().latitude + " " + position.data.First().longitude;
            }
            else
            {
                searchPos = new float[] { (float)lo, (float)la };
                ViewData["featureinfo"] = "" + searchPos[0] + " " + searchPos[1];
            }
            


            //byte[] response = await GetByteMap(ew, layer, size, new float[] { pos[0], pos[1], pos[0] + realSize[0], pos[1] + realSize[1] });
            byte[] response = await GetByteMap(ew, layer, size, new float[] { searchPos[0] - radius[0], searchPos[1] - radius[1], searchPos[0] + radius[0], searchPos[1] + radius[1] });

            Bitmap bitmap = PreProcess(GetBitmap(response));
            bitmap.Save("wwwroot/Data/output2.jpg", ImageFormat.Jpeg);

            using (System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(response)))
            {
                image.Save("wwwroot/Data/output.jpg", ImageFormat.Jpeg);  // Or Png
            }
            byte[] response2 = await GetByteMap(ort, 0, size, new float[] { searchPos[0] - radius[0], searchPos[1] - radius[1], searchPos[0] + radius[0], searchPos[1] + radius[1] });

            ViewData["image"] = Convert.ToBase64String(response2);
            
            ViewData["image2"] = Convert.ToBase64String(response);
            //ViewData["image2"] = ImageToString(GetImage(tmpImagePath));

            ImageProcessing(bitmap);
            //ImageProcessing(tmpImagePath);
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
            List<Blob> blobs1 = blobs.ToList();
            List<Blob> toRemove = new List<Blob>();
            foreach (var item in blobs1)
            {
                if (item.Area < 1000)
                    toRemove.Add(item);
            }
            foreach (var item in toRemove)
            {
                blobs1.Remove(item);
            }
            blobs1.Count();
            List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[0]);
            List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
            //ViewData["featureinfo"] = drawQuadrilateralCorners(corners);
        }

        public string drawQuadrilateralCorners(List<IntPoint> corners)
        {
            string result = "";
            for (int i = 0; i < corners.Count; i++)
            {
                result += (i + 1) + ": " + drawPoint(corners[0]) + " ";
            }

            return result;
        }
        public string drawPoint(IntPoint point)
        {
            return "" + point.X + " " + point.Y;
        }
        public Bitmap GetBitmap(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return new Bitmap(ms);
            }
        }

        public Bitmap PreProcess(Bitmap bmp)
        {
            //Those are AForge filters "using Aforge.Imaging.Filters;"
            //Grayscale gfilter = new Grayscale(0.2125, 0.7154, 0.0721);
            /*
            Grayscale gfilter = new Grayscale(0.5, 0.5, 0.8);
            Invert ifilter = new Invert();
            BradleyLocalThresholding thfilter = new BradleyLocalThresholding();
            bmp = gfilter.Apply(bmp);
            thfilter.ApplyInPlace(bmp);
            ifilter.ApplyInPlace(bmp);
            */
            Grayscale gfilter = new Grayscale(1, 1, 1);
            Invert ifilter = new Invert();
            BradleyLocalThresholding thfilter = new BradleyLocalThresholding();
            bmp = gfilter.Apply(bmp);
            thfilter.ApplyInPlace(bmp);
            ifilter.ApplyInPlace(bmp);
            return bmp;
        }
    }

    public class DataList
    {
        public List<PositionData> data { get; set; }
    }
}
