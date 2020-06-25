using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyCNBlog.Api.Extensions
{
    public class JsonConvertSettings
    {
        public static JsonSerializerSettings CamelCasePropertyName = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
