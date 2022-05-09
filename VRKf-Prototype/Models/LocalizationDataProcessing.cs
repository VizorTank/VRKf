using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace VRKf_WMS_Prototype.Models
{
    public class LocalizationDataProcessing
    {
        public float Longtitude;
        public float Latitude;

        public int MapHeight = 500;
        public int MapWidth = 500;

        private byte[] OrtoMapResponse;
        private Bitmap OrtoMap;
        private byte[] PremiterMapResponse;
        private Bitmap PremiterMap;

        private List<Blob> Buildings;
        public LocalizationDataProcessing(DataList address)
        {
            Longtitude = address.data.First().longitude;
            Latitude = address.data.First().latitude;
        }

        public LocalizationDataProcessing(float lo, float la)
        {
            Longtitude = lo;
            Latitude = la;
        }

        public LocalizationDataProcessing()
        {
            Longtitude = 23.14655f;
            Latitude = 53.11675f;
        }

        public async void SetAddress(string address)
        {
            OrtoMap = null;
            PremiterMap = null;
            DataList dataList = await DataCollector.GetPositionsFromAddress(
                    address);
            Longtitude = dataList.data.First().longitude;
            Latitude = dataList.data.First().latitude;
        }

        public void SetCoords(float lo, float la)
        {
            OrtoMap = null;
            PremiterMap = null;
            Longtitude = lo;
            Latitude = la;
        }

        public void SetMapSize(int width, int height)
        {
            OrtoMap = null;
            PremiterMap = null;
            MapWidth = width;
            MapHeight = height;
        }

        public async Task<byte[]> GetOrtoMap()
        {
            if (OrtoMap == null)
            {
                OrtoMapResponse = await DataCollector.GetByteOrthoMap(Longtitude, Latitude, MapWidth, MapHeight);

                OrtoMap = BuildingRecognition.GetBitmap(OrtoMapResponse);
            }
            return OrtoMapResponse;
        }

        public async Task<byte[]> GetPremiterMap()
        {
            if (PremiterMap == null)
            {
                PremiterMapResponse = await DataCollector.GetBytePremiterMap(Longtitude, Latitude, MapWidth, MapHeight);

                PremiterMap = BuildingRecognition.GetBitmap(PremiterMapResponse);
            }
            return PremiterMapResponse;
        }

        public List<Blob> GetBuildings()
        {
            if (PremiterMap == null)
                throw new Exception("No Orto map. Please use GetPremiterMap before using this metod.");
            if (Buildings == null)
            {
                Bitmap postProcessed = BuildingRecognition.PreProcessing(PremiterMap);
                Buildings = BuildingRecognition.ImageProcessing(postProcessed);
            }

            return Buildings;
        }

        public double GetBuildingSize(Blob building)
        {
            double imageRealSize = DataCollector.GetRealSizeOfImage(Longtitude, Latitude);

            return BuildingRecognition.GetBuildingSize(building, imageRealSize);
        }

        public void ClearOrtoMap()
        {
            OrtoMap = null;
        }

        public void ClearPremiterMapMap()
        {
            PremiterMap = null;
        }

        public void ClearBuildings()
        {
            Buildings = null;
        }
    }
}
