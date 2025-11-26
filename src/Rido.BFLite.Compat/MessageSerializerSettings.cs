// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Connector;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Rido.BFLite.Compat.Rest.ClientRuntime;

namespace Microsoft.Bot.Builder.Integration
{
    /// <summary>
    /// A class containing serializer settings for Microsoft.Bot.Connector.
    /// </summary>
    public static class MessageSerializerSettings
    {
        /// <summary>
        /// Creates a new ConnectorClient deserialization settings object.
        /// </summary>
        /// <returns>A <see cref="JsonSerializerSettings"/> object.</returns>
        public static JsonSerializerSettings Create()
        {
            //using (var connector = new ConnectorClient(new Uri("http://localhost/")))
            //{
            //    return connector.DeserializationSettings;
            //}

            return new JsonSerializerSettings
            {
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                    {
                        new Iso8601TimeSpanConverter()
                    },
                MaxDepth = null
            };
        }
    }
}
