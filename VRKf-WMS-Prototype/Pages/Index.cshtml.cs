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
            if (la == null)
            {
                la = 53.11675f;
                lo = 23.14655f;
            }

            // 23.064779f, 53.061270f, 23.248423f, 53.177203f
            @ViewData["ImageHeight"] = DataCollector.ImageSize[1] / 2;
            float[] searchPos;
            if (lo == null || la == null)
            {
                DataList position = await DataCollector.GetPositionsFromAddress(
                    "http://api.positionstack.com/v1/forward", 
                    "Wiejska 45, Białystok, Poland");
                searchPos = new float[] { position.data.First().longitude, position.data.First().latitude };
                if (position != null)
                    ViewData["featureinfo"] = "" + position.data.First().latitude + " " + position.data.First().longitude;
            }
            else
            {
                searchPos = new float[] { (float)lo, (float)la };
                ViewData["featureinfo"] = "" + searchPos[0] + " " + searchPos[1];
            }

            double imageRealSize = DataCollector.GetRealSizeOfImage(searchPos[0], searchPos[1]);
            ViewData["ImageSize"] = imageRealSize;

            byte[] response = await DataCollector.GetBytePremiterMap(searchPos[0], searchPos[1]);
            byte[] response2 = await DataCollector.GetByteOrthoMap(searchPos[0], searchPos[1]);

            Bitmap rawImage = BuildingRecognition.GetBitmap(response);
            Bitmap bitmap = BuildingRecognition.PreProcessing(rawImage);
            rawImage.Save("wwwroot/Data/output.jpg", ImageFormat.Jpeg);
            bitmap.Save("wwwroot/Data/output2.jpg", ImageFormat.Jpeg);

            ViewData["image"] = Convert.ToBase64String(response2);
            ViewData["image2"] = Convert.ToBase64String(response);

            List<Blob> buildings = BuildingRecognition.ImageProcessing(bitmap);
            List<float> buildingsSizes = new List<float>();
            string buildingsSizesInString = "Building Sizes: ";
            float sum = 0;
            foreach (Blob item in buildings)
            {
                buildingsSizes.Add(item.Area);
                float a = (float)BuildingRecognition.GetBuildingSize(item, imageRealSize);
                buildingsSizesInString += a + " m2, ";
                sum += item.Area;
            }

            ViewData["BuildingsSizes"] = buildingsSizesInString;
            ViewData["BuildingsSizesSum"] = "Building sizes sum: " + sum;
            ViewData["ImageLocalSize"] = bitmap.Width * bitmap.Height;
        }
    }
}
