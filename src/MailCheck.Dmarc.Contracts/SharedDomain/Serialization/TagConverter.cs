using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.Dmarc.Contracts.SharedDomain.Serialization
{
    public class TagConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            switch (jo["tagType"].Value<string>().ToLower())
            {
                case "adkim":
                    return JsonConvert.DeserializeObject<Adkim>(jo.ToString());
                case "aspf":
                    return JsonConvert.DeserializeObject<Aspf>(jo.ToString());
                case "failureoption":
                    return JsonConvert.DeserializeObject<FailureOption>(jo.ToString());
                case "percent":
                    return JsonConvert.DeserializeObject<Percent>(jo.ToString());
                case "policy":
                    return JsonConvert.DeserializeObject<Policy>(jo.ToString());
                case "reportformat":
                    return JsonConvert.DeserializeObject<ReportFormat>(jo.ToString());
                case "reportinterval":
                    return JsonConvert.DeserializeObject<ReportInterval>(jo.ToString());
                case "reporturiaggregate":
                    return JsonConvert.DeserializeObject<ReportUriAggregate>(jo.ToString());
                case "reporturiforensic":
                    return JsonConvert.DeserializeObject<ReportUriForensic>(jo.ToString());
                case "subdomainpolicy":
                    return JsonConvert.DeserializeObject<SubDomainPolicy>(jo.ToString());
                case "version":
                    return JsonConvert.DeserializeObject<Version>(jo.ToString());
                case "unknown":
                    return JsonConvert.DeserializeObject<UnknownTag>(jo.ToString());
                default:
                    throw new InvalidOperationException($"Failed to convert type of {jo["tagType"].Value<string>()}.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Tag);
        }
    }
}
