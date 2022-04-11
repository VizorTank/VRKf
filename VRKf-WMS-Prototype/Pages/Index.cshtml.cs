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

        public async Task OnGetAsync(float? lo, float? la)
        {
            @ViewData["ImageHeight"] = DataCollector.ImageSize[1] / 2;
            if (la == null)
            {
                la = 53.11675f;
                lo = 23.14655f;
            }
            await Version2(lo, la);
        }

        public async Task Version2(float? lo, float? la)
        {
            LocalizationDataProcessing LDP;
            if (lo == null || la == null)
            {
                LDP = new LocalizationDataProcessing(
                    await DataCollector.GetPositionsFromAddress(
                    "http://api.positionstack.com/v1/forward",
                    "Wiejska 45, Białystok, Poland"));
            }
            else
            {
                LDP = new LocalizationDataProcessing((float)lo, (float)la);
            }

            ViewData["image"] = Convert.ToBase64String(await LDP.GetOrtoMap());
            ViewData["image2"] = Convert.ToBase64String(await LDP.GetPremiterMap());

            List<Blob> buildings = LDP.GetBuildings();
            List<float> buildingsSizes = new List<float>();
            string buildingsSizesInString = "Building Sizes: ";
            float sum = 0;
            foreach (Blob item in buildings)
            {
                buildingsSizes.Add(item.Area);
                // Obliczenie wielkości budynków
                float a = (float)LDP.GetBuildingSize(item);
                buildingsSizesInString += a + " m2, ";
                sum += item.Area;
            }

            ViewData["BuildingsSizes"] = buildingsSizesInString;
            ViewData["BuildingsSizesSum"] = "Building sizes sum: " + sum;
        }

        public async Task Version1(float? lo, float? la)
        {
            // Politechnika kierunek informatyka
            // latitude: 53.11675
            // longtitude: 23.1466

            //--------------------------------------------------
            //          Określenie koordynatów
            //--------------------------------------------------

            

            // 23.064779f, 53.061270f, 23.248423f, 53.177203f
            float[] searchPos;
            if (lo == null || la == null)
            {
                // Zdobycie koordynatów z położenia
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

            //--------------------------------------------------
            //          Wykrywanie budynków
            //--------------------------------------------------

            // Zdobycie rzeczywistej wielkości obrazu (potrzebne do obliczenia wielkości budynku)
            double imageRealSize = DataCollector.GetRealSizeOfImage(searchPos[0], searchPos[1]);
            ViewData["ImageSize"] = imageRealSize;

            // Zdobycie obrysowań budynków
            byte[] response = await DataCollector.GetBytePremiterMap(searchPos[0], searchPos[1]);
            // Zdobycie rzeczywistej mayp
            byte[] response2 = await DataCollector.GetByteOrthoMap(searchPos[0], searchPos[1]);

            // Zamiana odpowiedzi na Bitmap
            Bitmap rawImage = BuildingRecognition.GetBitmap(response);
            // Nałożenie filtrów na mapę obrysów budynków
            Bitmap bitmap = BuildingRecognition.PreProcessing(rawImage);
            rawImage.Save("wwwroot/Data/output.jpg", ImageFormat.Jpeg);
            bitmap.Save("wwwroot/Data/output2.jpg", ImageFormat.Jpeg);

            ViewData["image"] = Convert.ToBase64String(response2);
            ViewData["image2"] = Convert.ToBase64String(response);

            // Wykrycie budynków
            List<Blob> buildings = BuildingRecognition.ImageProcessing(bitmap);
            List<float> buildingsSizes = new List<float>();
            string buildingsSizesInString = "Building Sizes: ";
            float sum = 0;
            foreach (Blob item in buildings)
            {
                buildingsSizes.Add(item.Area);
                // Obliczenie wielkości budynków
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
