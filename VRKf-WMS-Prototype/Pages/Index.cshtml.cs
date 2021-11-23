using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VRKf_WMS_Prototype.Pages
{
    public class IndexModel : PageModel
    {
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
    }
}
