using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VRKf_WMS_Prototype.Models
{
    public class DataCollector
    {
        public static readonly int[] ImageSize = { 2000, 2000 };
        public static readonly float[] realSize = { 0.183644f, 0.115933f };
        public static readonly float[] zoom = { 100f, 100f };
        public static readonly float[] radius = { realSize[0] / (2 * zoom[0]), realSize[1] / (2 * zoom[1]) };
        private static readonly string LandAndBuildingRecordsServer =
            "https://wms.gisbialystok.pl/arcgis/services/Ewidencja/MapServer/WMSServer?";
        private static readonly string OrthophotomapServer =
            "https://wms.gisbialystok.pl/arcgis/services/MSIP_orto2021/MapServer/WMSServer?";

        private static readonly string PositionServer = "http://api.positionstack.com/v1/forward";
        // TODO: Get new key
        private static readonly string AccessKey = "3fae4edf360680a55977be834e713664";

        public static async Task<byte[]> GetByteOrthoMap(float longitude, float latitude)
        {
            return await GetByteOrthoMap(longitude, latitude, ImageSize[0], ImageSize[1]);
        }

        public static async Task<byte[]> GetByteOrthoMap(float longitude, float latitude, int width, int height)
        {
            return await GetByteMap(OrthophotomapServer, 0, 
                new float[] { longitude, latitude },
                new int[] { width, height });
        }

        public static async Task<byte[]> GetBytePremiterMap(float longitude, float latitude)
        {
            return await GetBytePremiterMap(longitude, latitude, ImageSize[0], ImageSize[1]);
        }

        public static async Task<byte[]> GetBytePremiterMap(float longitude, float latitude, int width, int height)
        {
            return await GetByteMap(LandAndBuildingRecordsServer, 8,
                new float[] { longitude, latitude },
                new int[] { width, height });
        }

        public static async Task<byte[]> GetByteMap(string server, int layer, float[] searchPos)
        {
            return await GetByteMap(server, layer, searchPos, ImageSize);
        }

        public static async Task<byte[]> GetByteMap(string server, int layer, float[] searchPos, int[] imageSize)
        {
            //radius[0] /= zoom[0];
            //radius[1] /= zoom[1];
            var rectangle = new float[] {
                searchPos[0] - radius[0],
                searchPos[1] - radius[1],
                searchPos[0] + radius[0],
                searchPos[1] + radius[1]
            };
            return await GetByteMap(server, layer, imageSize, rectangle);
        }

        public static async Task<byte[]> GetByteMap(string server, int layer, int[] size, float[] position2Corners)
        {
            using (var client = new HttpClient())
            {
                var url = server +
                "request=GetMap&service=WMS&version=1.1.1" +
                "&layers=" + layer +
                "&styles=default" +
                "&srs=EPSG:4326" +
                "&bbox=" + position2Corners[0].ToString().Replace(",", ".") +
                "," + position2Corners[1].ToString().Replace(",", ".") +
                "," + position2Corners[2].ToString().Replace(",", ".") +
                "," + position2Corners[3].ToString().Replace(",", ".") +
                "&width=" + size[0] +
                "&height=" + size[1] +
                "&format=image/png32";
                var uri = new Uri(url);

                var response = await client.GetAsync(uri);
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        public static async Task<DataList> GetPositionsFromAddress(string address)
        {
            return await GetPositionsFromAddress(PositionServer, address);
        }

        public static async Task<DataList> GetPositionsFromAddress(string server, string address)
        {
            using (var client = new HttpClient())
            {
                var url = server + "?access_key=" + AccessKey + "&output=json&query=" + address;
                var uri = new Uri(url);

                var response = await client.GetAsync(uri);
                var data = await response.Content.ReadAsStringAsync();
                DataList position = System.Text.Json.JsonSerializer.Deserialize<DataList>(data);
                return position;
            }
        }

        public static double GetRealSizeOfImage(float longitude, float latitude)
        {
            var searchPos = new float[] { longitude, latitude };
            var rectangle = new float[] {
                searchPos[0] - radius[0],
                searchPos[1] - radius[1],
                searchPos[0] + radius[0],
                searchPos[1] + radius[1]
            };
            return ConvertGlobalPosToMeters(rectangle[0], rectangle[1], rectangle[0], rectangle[3]) *
                    ConvertGlobalPosToMeters(rectangle[0], rectangle[1], rectangle[2], rectangle[1]);
        }

        public static double ConvertGlobalPosToMeters(float lat1, float lon1, float lat2, float lon2)
        {  // generally used geo measurement function
            var R = 6378.137; // Radius of earth in KM
            var dLat = lat2 * Math.PI / 180 - lat1 * Math.PI / 180;
            var dLon = lon2 * Math.PI / 180 - lon1 * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return d * 1000; // meters
        }
    }
}
