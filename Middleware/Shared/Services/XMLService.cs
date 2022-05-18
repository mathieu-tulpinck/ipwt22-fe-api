using Middleware.Shared.Enums;
using Middleware.Shared.Messages;
using Middleware.Shared.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Middleware.Shared.Services
{
    public class XMLService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UuidMasterApiService _umService;
        
        public XMLService(UuidMasterApiService umService, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _umService = umService ?? throw new ArgumentNullException(nameof(umService));
        }
        
        public async Task<string?> PreparePayload(ResourceDto resourceDto, dynamic dynamicObject, CrudMethod crudMethod)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");

            switch (resourceDto.EntityType)
            {
                case "attendee":

                case "event":
                    var organiserResourceDto = await _umService.GetResourceQueryString(umHttpClient, "organiser", dynamicObject.Owner);
                    var message = new EventMessage(resourceDto, dynamicObject, crudMethod,organiserResourceDto);
                    
                    // XML should be validated programmatically based on xsd. TODO.
               
                    var xmlSerializer =  new XmlSerializer(message.GetType());
                    var xmlMessage = SerializeToXML(message);

                    return xmlMessage;
                default:
                    return null;
            }
        }


        // From https://stackoverflow.com/questions/721537/streaming-xml-serialization-in-net.
        public String SerializeToXML<EventMessage>(EventMessage message)
        {
            StringBuilder sb = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

            using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
            {
                if (xmlWriter != null)
                {
                    new XmlSerializer(typeof(EventMessage)).Serialize(xmlWriter, message);
                }
            }

            return sb.ToString();
        }

        public void DeserializeFromXML<EventMessage>(string xmlString, out EventMessage eventMessage) where EventMessage : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof (EventMessage));

            using (MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xmlString)))
            {
                eventMessage = xmlSerializer.Deserialize(memoryStream) as EventMessage;
            }
        }

        public Byte[] StringToUTF8ByteArray(String xmlString)
        {
            return new UTF8Encoding().GetBytes(xmlString);
        }
    }
}