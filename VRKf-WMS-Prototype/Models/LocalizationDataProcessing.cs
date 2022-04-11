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

        private Bitmap OrtoMap;
        private Bitmap RealMap;

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

        public async Task<Bitmap> GetOrtoMap()
        {
            if (OrtoMap == null)
            {
                byte[] response = await DataCollector.GetByteOrthoMap(Longtitude, Latitude);

                OrtoMap = BuildingRecognition.GetBitmap(response);
            }
            return OrtoMap;
        }

        public async Task<Bitmap> GetRealMap()
        {
            if (RealMap == null)
            {
                byte[] response = await DataCollector.GetBytePremiterMap(Longtitude, Latitude);

                RealMap = BuildingRecognition.GetBitmap(response);
            }
            return RealMap;
        }

        public List<Blob> GetBuildings()
        {
            if (Buildings != null)
                Buildings = BuildingRecognition.ImageProcessing(OrtoMap);

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

        public void ClearRealMap()
        {
            RealMap = null;
        }

        public void ClearBuildings()
        {
            Buildings = null;
        }
    }
}
