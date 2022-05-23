using Microsoft.AspNetCore.Mvc;
using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using Middleware.Shared.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Middleware.Producer.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly ILogger<BookingsController> _logger;
        private readonly RabbitMQService _rbmqService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UuidMasterApiService _umService;
        private readonly XMLService _xmlService;

        public BookingsController(ILogger<BookingsController> logger, RabbitMQService rbmqService, IHttpClientFactory httpClientFactory, UuidMasterApiService umService, XMLService xmlService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rbmqService = rbmqService ?? throw new ArgumentNullException(nameof(rbmqService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _umService = umService ?? throw new ArgumentNullException(nameof(umService));
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        }

        [HttpPost]
        public async Task<ActionResult> CreateBooking(BookingDto bookingDto)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");
            bool attendeeMessageSent;
            var attendeeResourceToCreate = new ResourceDto(Source.FRONTEND, EntityType.ATTENDEE, bookingDto.PersonId, 1);
            var createdAttendeeResource = await _umService.CreateResource(umHttpClient, attendeeResourceToCreate);
            if (createdAttendeeResource is not null) {
                _logger.LogInformation($"{createdAttendeeResource.EntityType} with Uuid {createdAttendeeResource.Uuid} was added to UuidMasterApi db.");
                var message = _xmlService.PreparePayload(createdAttendeeResource, bookingDto, CrudMethod.CREATE);
                if (message is not null) {
                    _logger.LogInformation("Producer successfully serialized the message.");
                    var bindings = new Dictionary<QueueName, RoutingKey>() {
                        { QueueName.CrmAttendee, RoutingKey.CrmAttendee },
                        { QueueName.PlanningAttendee, RoutingKey.PlanningAttendee }
                    };
                    _rbmqService.ConfigureBroker(ExchangeName.FrontAttendee, bindings);
                    _rbmqService.PublishMessage(ExchangeName.FrontAttendee, bindings.Values, message);
                                
                    attendeeMessageSent = true;
                } else {
                    _logger.LogError($"Producer failed to serialize the message. {createdAttendeeResource.EntityType} with Uuid {createdAttendeeResource.Uuid} was not sent to the queue.");

                    return Problem();
                }                
            } else {
                _logger.LogError($"Producer failed to insert the resource into the database. {attendeeResourceToCreate.EntityType} with SourceEntityId {attendeeResourceToCreate.SourceEntityId} was not added to UuidMasterApi db.");

                return Problem();
            }

            if (attendeeMessageSent) {
                var attendeeSessionResourceToCreate = new ResourceDto(Source.FRONTEND, EntityType.ATTENDEESESSION, bookingDto.Id, 1);
                var createdAttendeeSessionResource = await _umService.CreateResource(umHttpClient, attendeeSessionResourceToCreate);
                var eventResource = await _umService.GetResourceQueryString(umHttpClient, Source.FRONTEND, EntityType.SESSION, bookingDto.EventId);
                if (createdAttendeeSessionResource is not null) {
                    if (eventResource is not null) {
                        var message = _xmlService.PreparePayload(createdAttendeeSessionResource, bookingDto, CrudMethod.CREATE, eventUuid: eventResource.Uuid, attendeeUuid: createdAttendeeResource.Uuid);
                        if (message is not null) {
                            _logger.LogInformation("Producer successfully serialized the message.");
                            var bindings = new Dictionary<QueueName, RoutingKey>() {
                                { QueueName.CrmAttendeeSession, RoutingKey.CrmAttendeeSession },
                                { QueueName.PlanningAttendeeSession, RoutingKey.PlanningAttendeeSession }
                            };
                            _rbmqService.ConfigureBroker(ExchangeName.FrontAttendeeSession, bindings);
                            _rbmqService.PublishMessage(ExchangeName.FrontAttendeeSession, bindings.Values, message);
                                        
                            return NoContent();
                        } else {
                            _logger.LogError($"Producer failed to serialize the message. {createdAttendeeSessionResource.EntityType} with Uuid {createdAttendeeSessionResource.Uuid} was not sent to the queue.");

                            return Problem();
                        }                
                    } else {
                        _logger.LogError($"Producer failed to serialize the message. {EntityType.SESSION} with SourceEntityId {bookingDto.EventId} does not exist in UuidMasterApi db.");

                        return Problem();
                    }
                } else {
                    _logger.LogError($"Producer failed to insert the resource into the database. {attendeeSessionResourceToCreate.EntityType} with SourceEntityId {attendeeSessionResourceToCreate.SourceEntityId} was not added to UuidMasterApi db.");

                    return Problem();
                }
            } else {
                return Problem();
            }
        }
    }
}