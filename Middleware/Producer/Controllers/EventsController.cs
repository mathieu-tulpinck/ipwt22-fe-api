using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Mvc;
using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using System.Text;
using System.Text.Json;

namespace Middleware.Producer.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<ActionResult> CreateEvent(EventDto eventCreateDto)
        {
            // check if entity exists in uuidmasterapi
            //    checked if owner exists
            //        if not post it
            var httpClient = _httpClientFactory.CreateClient("UuidMasterApi");

            if (SearchResource(httpClient, "user", eventCreateDto.Owner) is null) {
                var resourceCreateDto = new ResourceCreateDto(SourceType.FrontEnd, "user", eventCreateDto.Owner, 1);
                var resourceDto = await CreateResource(httpClient, resourceCreateDto);
                // prepare and send message

                return CreatedAtAction("CreateEvent", resourceDto);
            }


            // if yes update. In principle, incoming updates are true updates.
            
            return NoContent();
        }

        [NonAction]
        public async Task<ResourceDto> SearchResource(HttpClient httpClient, string entityType, int sourceEntityId)
        {
            var resourceDto = await httpClient.GetFromJsonAsync<ResourceDto>($"resources/search?source=FrontEnd&entityType={entityType}&sourceEntityId={sourceEntityId}");

            return resourceDto;
        }

        [NonAction]
        public async Task<ResourceDto> CreateResource(HttpClient httpClient, ResourceCreateDto resourceCreateDto)
        {
                var response = await httpClient.PostAsJsonAsync<ResourceCreateDto>("resources", resourceCreateDto);
                ResourceDto resourceDto = await response.Content.ReadFromJsonAsync<ResourceDto>();
                return resourceDto;
        }

    }
}