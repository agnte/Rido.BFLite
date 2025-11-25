using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Rest.Serialization
{
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
}
