using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VRKf_WMS_Prototype.Models
{
    public static class BuildingRecognition
    {
        public static Bitmap GetBitmap(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;
                return new Bitmap(ms);
            }
        }

        public static Bitmap PreProcessing(byte[] bytes)
        {
            return PreProcessing(GetBitmap(bytes));
        }

        public static Bitmap PreProcessing(Bitmap bmp)
        {
            Grayscale gfilter = new Grayscale(1, 1, 1);
            Invert ifilter = new Invert();
            BradleyLocalThresholding thfilter = new BradleyLocalThresholding();
            bmp = gfilter.Apply(bmp);
            thfilter.ApplyInPlace(bmp);
            ifilter.ApplyInPlace(bmp);
            return bmp;
        }

        public static List<Blob> ImageProcessing(Bitmap image, int minArea = 1000)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(image);
            Blob[] totalBlobs = blobCounter.GetObjectsInformation();
            List<Blob> resultBlobs = totalBlobs.ToList();
            List<Blob> toRemove = new List<Blob>();
            foreach (var item in resultBlobs)
            {
                if (item.Area < minArea)
                    toRemove.Add(item);
            }
            foreach (var item in toRemove)
            {
                resultBlobs.Remove(item);
            }

            return resultBlobs;
        }

        public static double GetBuildingSize(Blob blob, double imageRealSize)
        {
            return blob.Area * imageRealSize / (DataCollector.ImageSize[0] * DataCollector.ImageSize[1]) * 6.259967280017101;
        }
    }
}
