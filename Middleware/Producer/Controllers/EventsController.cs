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
            var organiserResource = await _umService.GetResourceQueryString(umHttpClient, "organiser", eventDto.Owner);
            if (organiserResource is null) {
                var organiserResourceToCreate = new ResourceDto(Source.FrontEnd, "organiser", eventDto.Owner, 1);
                var createdOrganiserResource = await _umService.CreateResource(umHttpClient, organiserResourceToCreate);
                if(createdOrganiserResource is not null) {
                    _logger.LogInformation($"{createdOrganiserResource.EntityType} with Uuid {createdOrganiserResource.Uuid} was added to UuidMasterApi db.");
                } else {
                    _logger.LogError($"Producer failed to insert the resource into the database. {organiserResourceToCreate.EntityType} with SourceEntityId {organiserResourceToCreate.SourceEntityId} was not added to UuidMasterApi db.");
                    return Problem();
                }
            } else {
                _logger.LogInformation($"{organiserResource.EntityType} with Uuid {organiserResource.Uuid} already exists in UuidMasterApi db. No action taken.");
                return NoContent();
            }

            var eventResourceToCreate =  new ResourceDto(Source.FrontEnd, "event", eventDto.Id, 1);
            var createdEventResource = await _umService.CreateResource(umHttpClient, eventResourceToCreate);
            
            // prepare and send rabbitmq message relating to new event.
            if (createdEventResource is not null) {
                _logger.LogInformation($"{createdEventResource.EntityType} with Uuid {createdEventResource.Uuid} was added to UuidMasterApi db.");
                var message = await _xmlService.PreparePayload(createdEventResource, eventDto, CrudMethod.CREATE);
                if (message is not null) {
                    return NoContent();

                } else {
                    _logger.LogError($"Producer failed to serialize the message. {createdEventResource.EntityType} with Uuid {createdEventResource.Uuid} was not sent to the queue.");
                    return Problem();
                }
            } else {
                _logger.LogError($"Producer failed to insert the resource into the database. {eventResourceToCreate.EntityType} with SourceEntityId {eventResourceToCreate.SourceEntityId} was not added to UuidMasterApi db.");
                return Problem();
            }
        }

        // Patch action
        [HttpPatch("{sourceEntityId}")]
        public async Task<ActionResult> PatchEvent(int sourceEntityId, [FromBody] dynamic attributes)
        {
            JObject rawJson = JObject.Parse(attributes.ToString());
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");
            
            var eventResource = await _umService.GetResourceQueryString(umHttpClient, "event", sourceEntityId);
            // Difficult to retrieve owner id at this stage.
            if (eventResource is not null) {
                var response = await _umService.PatchResource(umHttpClient, eventResource.Uuid, eventResource.EntityVersion);
                if (response) {
                    _logger.LogInformation($"Event with Uuid {eventResource.Uuid} was updated in UuidMasterApi db.");                    
                    // Send Rabbitmq message relating to updated event.
                    
                    return NoContent();
                } else {
                    _logger.LogError($"Producer failed to update the resource into the database. {eventResource.EntityType} with SourceEntityId {eventResource.SourceEntityId} was not updated in UuidMasterApi db.");
                    return Problem();
                }
            } else {
                _logger.LogError($"Producer failed to update the resource into the database. {EntityType.Event} with SourceEntityId {sourceEntityId} does not exist in UuidMasterApi db.");
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