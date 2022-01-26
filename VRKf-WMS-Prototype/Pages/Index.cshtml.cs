using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace VRKf_WMS_Prototype.Pages
{
    public class IndexModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string AddName { get; set; }
        private readonly ILogger<IndexModel> _logger;

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
            int[] size = { 1000, 500 };
            float[] pos = { 23.064779f, 53.061270f };
            float[] realSize = { 0.183644f, 0.115933f };

            var response = await GetByteMap(ew, layer, size, new float[] { pos[0], pos[1], pos[0] + realSize[0], pos[1] + realSize[1] });
            ViewData["image"] = Convert.ToBase64String(response);
            ViewData["featureinfo"] = await GetPos(ew);
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
        public async Task<string> GetPos(string server)
        {
            using (var client = new HttpClient())
            {
                //AddName = "Mieszka I 4, Białystok, Poland";
                var url = "http://api.positionstack.com/v1/forward?access_key=3fae4edf360680a55977be834e713664&query= " +" " +AddName + " "  /*Mieszka I 4, Białystok, Poland*/ +"& output = json";
                var uri = new Uri(url);

                var response = await client.GetAsync(uri);
                //JsonSerializer.Deserialize(await response.Content.ReadAsByteArrayAsync());
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
