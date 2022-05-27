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
        private readonly RabbitMQService _rbmqService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UuidMasterApiService _umService;
        private readonly XMLService _xmlService;

        public EventsController(ILogger<EventsController> logger, RabbitMQService rbmqService, IHttpClientFactory httpClientFactory, UuidMasterApiService umService, XMLService xmlService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rbmqService = rbmqService ?? throw new ArgumentNullException(nameof(rbmqService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _umService = umService ?? throw new ArgumentNullException(nameof(umService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        }

        [HttpPost]
        public async Task<ActionResult> CreateEvent(EventDto eventDto)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");
            Resource? createdOrganiserResource = null;
            // If event owner is not known in UuidMasterApi, create it. No uuid check given that creation is not caused by incoming message.
            var organiserResource = await _umService.GetResourceQueryString(umHttpClient, Source.FRONTEND, EntityType.ORGANISER, eventDto.Owner);
            if (organiserResource is null) {
                var organiserResourceToCreate = new ResourceDto(Source.FRONTEND, EntityType.ORGANISER, eventDto.Owner, 1);
                createdOrganiserResource = await _umService.CreateResource(umHttpClient, organiserResourceToCreate);
                if(createdOrganiserResource is not null) {
                    _logger.LogInformation($"{createdOrganiserResource.EntityType} with Uuid {createdOrganiserResource.Uuid} was added to UuidMasterApi db.");
                } else {
                    _logger.LogError($"Producer failed to insert the resource into the database. {organiserResourceToCreate.EntityType} with SourceEntityId {organiserResourceToCreate.SourceEntityId} was not added to UuidMasterApi db.");
                    return Problem();
                }
            } else {
                _logger.LogInformation($"{organiserResource.EntityType} with Uuid {organiserResource.Uuid} already exists in UuidMasterApi db. No action taken.");
                createdOrganiserResource = organiserResource;
            }

            // No uuid check given that creation is not caused by incoming message.
            var eventResourceToCreate =  new ResourceDto(Source.FRONTEND, EntityType.SESSION, eventDto.Id, 1);
            var createdEventResource = await _umService.CreateResource(umHttpClient, eventResourceToCreate);
            
            // prepare and send rabbitmq messages relating to new event.
            if (createdEventResource is not null) {
                _logger.LogInformation($"{createdEventResource.EntityType} with Uuid {createdEventResource.Uuid} was added to UuidMasterApi db.");
                if(createdOrganiserResource is not null) {
                    var eventMessage = _xmlService.PreparePayload(createdEventResource, eventDto, CrudMethod.CREATE, organiserUuid: createdOrganiserResource.Uuid);
                    var organiserMessage = _xmlService.PreparePayload(createdOrganiserResource, eventDto, CrudMethod.CREATE);
                    if (eventMessage is not null && organiserMessage is not null) {
                        _logger.LogInformation("Producer successfully serialized the messages.");
                        var sessionBindings = new Dictionary<QueueName, RoutingKey>() {
                            { QueueName.CrmSession, RoutingKey.CrmSession },
                            { QueueName.PlanningSession, RoutingKey.PlanningSession }
                        };
                        _rbmqService.ConfigureBroker(ExchangeName.FrontSession, sessionBindings);
                        _rbmqService.PublishMessage(ExchangeName.FrontSession, sessionBindings.Values, eventMessage);
                        
                        var attendeeBindings = new Dictionary<QueueName, RoutingKey>() {
                            { QueueName.CrmAttendee, RoutingKey.CrmAttendee },
                            { QueueName.PlanningAttendee, RoutingKey.PlanningAttendee }
                        };
                        _rbmqService.ConfigureBroker(ExchangeName.FrontAttendee, attendeeBindings);
                        _rbmqService.PublishMessage(ExchangeName.FrontAttendee, attendeeBindings.Values, organiserMessage);
                        return NoContent();   
                    }  else {
                        _logger.LogError($"Producer failed to serialize the messages.");
                        return Problem();
                    }
                } else {
                    return Problem();
                }
            } else {
                _logger.LogError($"Producer failed to insert the resource into the database. {eventResourceToCreate.EntityType} with SourceEntityId {eventResourceToCreate.SourceEntityId} was not added to UuidMasterApi db.");
                return Problem();
            }
        }

        // Patch action. Would be preferable to replace by Put action due to lack of easily accessible wordpress api on events.
        [HttpPatch("{sourceEntityId}")]
        public async Task<ActionResult> PatchEvent(int sourceEntityId, [FromBody] EventUpdateDto eventUpdateDto)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");
    

            // JObject rawJson = JObject.Parse(attributes.ToString());
            // var organiserId = (int)rawJson["event_owner"]!;
            // foreach(var field in rawJson) {

            //     _logger.LogInformation(field.Key);
            //     _logger.LogInformation((string) field.Value);
            // }
            
            var eventResource = await _umService.GetResourceQueryString(umHttpClient, Source.FRONTEND, EntityType.SESSION, sourceEntityId);
            if (eventResource is not null) {
                var organiserResource = await _umService.GetResourceQueryString(umHttpClient, Source.FRONTEND, EntityType.ORGANISER, eventUpdateDto.Owner);
                if (organiserResource is not null) {
                    var response = await _umService.PatchResource(umHttpClient, eventResource.Uuid, eventResource.EntityVersion);
                    if (response) {
                        _logger.LogInformation($"Event with Uuid {eventResource.Uuid} was updated in UuidMasterApi db.");                    
                        // Send Rabbitmq message relating to updated event.
                        var message = _xmlService.PreparePayload(eventResource, eventUpdateDto, CrudMethod.UPDATE, organiserUuid: organiserResource.Uuid);
                        return NoContent();
                    } else {
                        _logger.LogError($"Producer failed to update the resource into the database. {eventResource.EntityType} with SourceEntityId {eventResource.SourceEntityId} was not updated in UuidMasterApi db.");
                        return Problem();
                    }
                } else {
                    _logger.LogError($"Producer failed to update the resource into the database. {EntityType.ORGANISER} with SourceEntityId {eventUpdateDto.Owner} does not exist in UuidMasterApi db. ");
                    return Problem();
                }
            } else {
                _logger.LogError($"Producer failed to update the resource into the database. {EntityType.SESSION} with SourceEntityId {sourceEntityId} does not exist in UuidMasterApi db. ");
                return Problem();
            }
        }

        // Delete action not yet implemented.
    }
}