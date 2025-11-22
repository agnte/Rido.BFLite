using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Rido.BFLite.Compat.Adapter;

internal static class CompatActivity
{

    public static Microsoft.Bot.Schema.Activity ToCompatActivity(this Rido.BFLite.Core.Schema.Activity activity)
    {
        var json = activity.ToJson();
        var compatActivity = s_botMessageSerializer.Deserialize<Microsoft.Bot.Schema.Activity>(new JsonTextReader(new System.IO.StringReader(json)))!;
        return compatActivity;
    }

    public static Rido.BFLite.Core.Schema.Activity FromCompatActivity(this Microsoft.Bot.Schema.Activity activity)
    {
        var sb = new StringBuilder();
        s_botMessageSerializer.Serialize(new JsonTextWriter(new System.IO.StringWriter(sb)), activity);
        var compatActivity = Rido.BFLite.Core.Schema.Activity.FromJsonString(sb.ToString());
        return compatActivity;
    }

    private static readonly JsonSerializer s_botMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Newtonsoft.Json.Formatting.Indented,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        ContractResolver = new ReadOnlyJsonContractResolver(),
        Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() },
        MaxDepth = 128
    });

    public class ReadOnlyJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
            PropertyInfo propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                jsonProperty.ShouldSerialize = (object t) => (propertyInfo.SetMethod != null && !propertyInfo.SetMethod.IsPrivate && !propertyInfo.SetMethod.IsFamily) || (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsStatic);
            }

            return jsonProperty;
        }
    }

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
