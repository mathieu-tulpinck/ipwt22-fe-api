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
        
        public async Task<string?> PreparePayload(Resource resource, dynamic dto, CrudMethod crudMethod)
        {
            var umHttpClient = _httpClientFactory.CreateClient("UuidMasterApi");

            switch (resource.EntityType)
            {
                // attendee
                case EntityType.Event:
                    var organiserResource = await _umService.GetResourceQueryString(umHttpClient, "organiser", dto.Owner);
                    var message = new EventMessage(resource, dto, crudMethod, organiserResource.Uuid);
                    
                    // XML should be validated programmatically based on xsd. TODO.
               
                    var xmlSerializer =  new XmlSerializer(message.GetType());
                    var xmlMessage = SerializeToXML<EventMessage>(message);

                    return xmlMessage;
                default:
                    return null;
            }
        }


        // From https://stackoverflow.com/questions/721537/streaming-xml-serialization-in-net.
        public String SerializeToXML<T>(T message)
        {
            StringBuilder mutableString = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

            using (XmlWriter xmlWriter = XmlWriter.Create(mutableString, settings))
            {
                if (xmlWriter != null)
                {
                    new XmlSerializer(typeof(T)).Serialize(xmlWriter, message);
                }
            }

            return mutableString.ToString();
        }

        public void DeserializeFromXML<T>(string xmlString, out T eventMessage) where T : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof (T));

            using (MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xmlString)))
            {
                eventMessage = xmlSerializer.Deserialize(memoryStream) as T;
            }
        }

        public Byte[] StringToUTF8ByteArray(String xmlString)
        {
            return new UTF8Encoding().GetBytes(xmlString);
        }
    }
}