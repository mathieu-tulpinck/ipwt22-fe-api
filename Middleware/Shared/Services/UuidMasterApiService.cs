using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

using Middleware.Shared.Models;

namespace Middleware.Shared.Services
{
    public class UuidMasterApiService
    {
        public async Task<Resource?> GetResourceQueryString(HttpClient umHttpClient, string entityType, int sourceEntityId)
        {
            var response = await umHttpClient.GetAsync($"resources/search?source=FrontEnd&entityType={entityType}&sourceEntityId={sourceEntityId}");
            if(response.StatusCode == HttpStatusCode.NotFound) {
                return null;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<Resource>(content);
            
            return resource;
        }

        public async Task<Resource?> CreateResource(HttpClient umHttpClient, ResourceDto resourceDto)
        {
            var json = JsonConvert.SerializeObject(resourceDto);
            var body = new StringContent(json, Encoding.UTF8, Application.Json);
            var response = await umHttpClient.PostAsync("resources", body);
            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                var resource = JsonConvert.DeserializeObject<Resource>(content);
                return resource;
            } else {
                return null;
            }
        }

        public async Task<bool> PatchResource(HttpClient umHttpClient, Guid uuid, int entityVersion)
        {
            var patchDoc = new JsonPatchDocument<ResourceDto>();
            patchDoc.Replace(r => r.EntityVersion, entityVersion++);
            var json = JsonConvert.SerializeObject(patchDoc);
            var body = new StringContent(json, Encoding.UTF8, Application.Json);
            var response = await umHttpClient.PatchAsync($"resources/{uuid}", body);
            if (response.IsSuccessStatusCode) {
                return true;
            } else {
                return false;
            }
        }
    }
}