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
        private float Longtitude;
        private float Latitude;

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

        public async Task<byte[]> GetOrtoMap()
        {
            if (OrtoMap == null)
            {
                OrtoMapResponse = await DataCollector.GetByteOrthoMap(Longtitude, Latitude);

                OrtoMap = BuildingRecognition.GetBitmap(OrtoMapResponse);
            }
            return OrtoMapResponse;
        }

        public async Task<byte[]> GetPremiterMap()
        {
            if (PremiterMap == null)
            {
                PremiterMapResponse = await DataCollector.GetBytePremiterMap(Longtitude, Latitude);

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
