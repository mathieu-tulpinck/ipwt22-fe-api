using Microsoft.AspNetCore.Mvc;
using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using Middleware.Shared.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Middleware.Producer.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UuidMasterApiService _umService;
        private readonly XMLService _xmlService;

        public EventsController(ILogger<EventsController> logger, IHttpClientFactory httpClientFactory, UuidMasterApiService umService, XMLService xmlService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _umService = umService ?? throw new ArgumentNullException(nameof(umService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        }

        [HttpPost]
        public async Task<ActionResult> CreateEvent(EventDto eventDto)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");

            // If event owner is not known in UuidMasterApi, create it.
            var organiserDto = await _umService.GetResourceQueryString(umHttpClient, "organiser", eventDto.Owner);
            if (organiserDto is null) {
                var organiserResourceCreateDto = new ResourceCreateDto(SourceType.FrontEnd, "organiser", eventDto.Owner, 1);
                var organiserResourceDto = await _umService.CreateResource(umHttpClient, organiserResourceCreateDto);
                if(organiserResourceDto is not null) {
                    _logger.LogInformation($"Organiser with Uuid {organiserResourceDto.Uuid} was added to UuidMasterApi db.");
                } else {
                    _logger.LogError($"The resource creation process failed. User with SourceEntityId {organiserResourceCreateDto.SourceEntityId} was not added to UuidMasterApi db.");
                    return Problem();
                }
            } else {
                _logger.LogInformation($"Organiser with Uuid {organiserDto.Uuid} already exists in UuidMasterApi db. No action taken.");
                return NoContent();
            }

            var eventResourceCreateDto =  new ResourceCreateDto(SourceType.FrontEnd, "event", eventDto.Id, 1);
            var eventResourceDto = await _umService.CreateResource(umHttpClient, eventResourceCreateDto);
            
            // prepare and send rabbitmq message relating to new event.
            if (eventResourceDto is not null) {
                _logger.LogInformation($"Event with Uuid {eventResourceDto.Uuid} was added to UuidMasterApi db.");
                _xmlService.PreparePayload(eventResourceDto, eventDto, CrudMethod.CREATE);
                return NoContent();
            } else {
                _logger.LogError($"The resource creation process failed. Event with SourceEntityId {eventResourceCreateDto.SourceEntityId} was not added to UuidMasterApi db.");
                return Problem();
            }
        }

        // Patch action
        [HttpPatch("{sourceEntityId}")]
        public async Task<ActionResult> PatchEvent(int sourceEntityId, [FromBody] dynamic attributes)
        {
            JObject rawJson = JObject.Parse(attributes.ToString());
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");
            
            var eventDto = await _umService.GetResourceQueryString(umHttpClient, "event", sourceEntityId);
            // Difficult to retrieve owner id at this stage.
            if (eventDto is not null) {
                var response = await _umService.PatchResource(umHttpClient, eventDto.Uuid, eventDto.EntityVersion);
                if (response) {
                    _logger.LogInformation($"Event with Uuid {eventDto.Uuid} was updated in UuidMasterApi db.");                    
                    // Send Rabbitmq message relating to updated event.
                    
                    return NoContent();
                } else {
                    _logger.LogError($"The resource update process failed. Event with SourceEntityId {sourceEntityId} was not updated in UuidMasterApi db.");
                    return Problem();
                }
            } else {
                _logger.LogError($"The resource update process failed. Event with SourceEntityId {sourceEntityId} does not exist in UuidMasterApi db.");
                return Problem();
            }

            // Relevant for rabbitmq
            // foreach(var field in rawJson) {

            //     _logger.LogInformation(field.Key);
            //     _logger.LogInformation((string) field.Value);
            // }
        }

        // Delete action not yet implemented.
    }
}