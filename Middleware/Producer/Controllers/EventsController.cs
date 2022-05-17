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
        private readonly UuidMasterApiRepository _repository;

        public EventsController(ILogger<EventsController> logger, IHttpClientFactory httpClientFactory, UuidMasterApiRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpPost]
        public async Task<ActionResult> CreateEvent(EventCreateDto eventCreateDto)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");

            // If event owner is not known in UuidMasterApi, create it.
            var userDto = await _repository.GetResourceQueryString(umHttpClient, "user", eventCreateDto.Owner);
            if (userDto is null) {
                var userResourceCreateDto = new ResourceCreateDto(SourceType.FrontEnd, "user", eventCreateDto.Owner, 1);
                var userResourceDto = await _repository.CreateResource(umHttpClient, userResourceCreateDto);
                // prepare and send rabbitmq message relating to new user.

            } else {
                _logger.LogInformation($"User {eventCreateDto.Owner} already exists in UuidMasterApi db. No action taken.");
            }

            var eventResourceCreateDto =  new ResourceCreateDto(SourceType.FrontEnd, "event", eventCreateDto.Id, 1);
            var response = await _repository.CreateResource(umHttpClient, eventResourceCreateDto);
            // prepare and send rabbitmq message relating to new event.
            if (response is not null) {
                return NoContent();
            } else {
                _logger.LogError($"Event {eventCreateDto.Id} was not added to UuidMasterApi db.");
                return Problem();
            }
        }

        // Patch action
        [HttpPatch("{sourceEntityId}")]
        public async Task<ActionResult> PatchEvent(int sourceEntityId, [FromBody] dynamic attributes)
        {
            JObject rawJson = JObject.Parse(attributes.ToString());
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");

            var eventDto = await _repository.GetResourceQueryString(umHttpClient, "event", sourceEntityId);
            if (eventDto is not null) {
                var response = await _repository.PatchResource(umHttpClient, eventDto.Uuid, eventDto.EntityVersion);
                if (response) {
                    // Send Rabbitmq message.
                    return NoContent();
                } else {
                    _logger.LogError($"The update of Event {sourceEntityId} in UuidMasterApi db failed.");
                    return Problem();
                }
            } else {
                _logger.LogError($"User {sourceEntityId} does not exist in UuidMasterApi db.");
            }

            // Relevant for rabbitmq
            foreach(var field in rawJson) {

                _logger.LogInformation(field.Key);
                _logger.LogInformation((string) field.Value);
            }

            return NoContent();
        }

        // Delete action not yet implemented.
    }
}