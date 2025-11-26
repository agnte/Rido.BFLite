using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rido.BFLite.Compat.Rest.ClientRuntime
{
    public class Iso8601TimeSpanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            string value2 = XmlConvert.ToString((TimeSpan)value);
            serializer.Serialize(writer, value2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            return XmlConvert.ToTimeSpan(serializer.Deserialize<string>(reader));
        }

        public override bool CanConvert(Type objectType)
        {
            if (!(objectType == typeof(TimeSpan)))
            {
                return objectType == typeof(TimeSpan?);
            }

            return true;
        }
    }
}
