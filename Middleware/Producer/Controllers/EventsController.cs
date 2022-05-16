using Microsoft.AspNetCore.Mvc;
using Middleware.Shared.Models;

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
        public /*async Task<*/ActionResult/*<EventDto>>*/ CreateEvent(EventCreateDto eventCreateDto)
        {
            // check if entity exists in uuidmasterapi
            //    checked if owner exists
            //        if not post it

            // if not post it

            // if yes update. In principle, incoming updates are true updates.
            _logger.LogInformation(eventCreateDto.ToString());
            return NoContent();
        }

    }
}