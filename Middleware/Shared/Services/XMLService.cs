using Microsoft.Extensions.Logging;
using Middleware.Shared.Enums;
using Middleware.Shared.Helpers;
using Middleware.Shared.Messages;
using Middleware.Shared.Models;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Middleware.Shared.Services
{
    public class XMLService
    {
        private readonly ILogger<XMLService> _logger;

        public XMLService(ILogger<XMLService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        // dynamic dto is used to accomodate updateDto from patch action.
        public String? PreparePayload(Resource resource, dynamic dto, CrudMethod crudMethod, Guid organiserUuid)
        {
            switch (resource.EntityType)
            {
                // attendee

                case EntityType.EVENT:
                    var message = new SessionEventMessage(resource, dto, crudMethod, organiserUuid);
               
                    var xmlSerializer =  new XmlSerializer(message.GetType());
                    var xmlMessage = SerializeToXML<SessionEventMessage>(message);
                    _logger.LogInformation(xmlMessage);
                    if (ValidateXml(xmlMessage)) {
                    return xmlMessage;
                    } else {
                        return null;
                    }                 
                default:
                    return null;
            }
        }

        private bool ValidateXml(string xmlMessage)
        {
            bool isValid = true;
            var path = new Uri(AppContext.BaseDirectory + "XMLSchemas/SessionEvent_w.xsd");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlMessage);
            xml.Schemas.Add(null, path.ToString());
            string logMessage = String.Empty;
            xml.Validate((o, e) => {
                logMessage += e.Message;
                isValid = false;
            });
            if (!String.IsNullOrEmpty(logMessage)) {
                _logger.LogError(logMessage);
            }

            return isValid;
        }


        // From https://stackoverflow.com/questions/721537/streaming-xml-serialization-in-net.
        public String SerializeToXML<T>(T message)
        {
            StringBuilder mutableString = new StringBuilder();
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(mutableString, Encoding.UTF8);

            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            {
                if (xmlWriter is not null)
                {
                    new XmlSerializer(typeof(T)).Serialize(xmlWriter, message);
                }
            }

            return mutableString.ToString();
        }

        public void DeserializeFromXML<T>(string xmlString, out T sessionEventMessage) where T : class
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof (T));

            using (MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(xmlString)))
            {
                sessionEventMessage = xmlSerializer.Deserialize(memoryStream) as T;
            }
        }

        public Byte[] StringToUTF8ByteArray(String xmlString)
        {
            return new UTF8Encoding().GetBytes(xmlString);
        }
    }
}