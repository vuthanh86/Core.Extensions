using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Core.Extensions
{
    public static class XmlExtensions
    {
        private static ConcurrentDictionary<Type, XmlSerializer> xmlSerializers = new ConcurrentDictionary<Type, XmlSerializer>();

        private static XmlSerializer GetOrAdd<T>()
        {
            var type = typeof(T);
            return GetOrAdd(type);
        }

        private static XmlSerializer GetOrAdd(Type type)
        {
            return xmlSerializers.GetOrAdd(type, t => { return new XmlSerializer(t); });
        }

        public static string XmlSerialize<T>(this T obj)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                GetOrAdd<T>().Serialize(sw, obj);
            }
            return sb.ToString();
        }

        public static T XmlDeserialize<T>(this string xml)
        {
            using (var sr = new StringReader(xml))
            {
                return (T)GetOrAdd<T>().Deserialize(sr);
            }
        }

        public static object XmlDeserialize(this string xml, Type type)
        {
            using (var sr = new StringReader(xml))
            {
                return GetOrAdd(type).Deserialize(sr);
            }
        }
    }
}
