using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Serialization
{
    public class IgnorePropertiesContractResolver : DefaultContractResolver
    {
        private readonly List<string> ignoreProperties = new List<string>();

        public IgnorePropertiesContractResolver(params string[] properties)
        {
            if (properties != null && properties.Length > 0)
            {
                ignoreProperties.AddRange(properties);
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (ignoreProperties.Contains(member.Name))
            {
                property.Ignored = true;
            }

            return property;
        }
    }
}
