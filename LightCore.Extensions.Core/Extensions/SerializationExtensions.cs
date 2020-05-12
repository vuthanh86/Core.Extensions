using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace NetCore.Extensions.Core.Extensions
{
    public static class SerializationExtensions
    {
        internal static JsonSerializer Serializer;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
            TypeNameHandling = TypeNameHandling.Auto
        };

        static SerializationExtensions()
        {
            Serializer = JsonSerializer.Create(JsonSerializerSettings);
        }

        public static string JsonSerialize<T>(this T obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        }

        public static string JsonSerialize<T>(this T obj, JsonSerializerSettings setting)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, setting);
        }

        public static T JsonDeserialize<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
        }

        public static object JsonDeserialize(this string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject(json, type, JsonSerializerSettings);
        }

        public static T JsonDeserialize<T>(this Stream stream)
        {
            using (var textReader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        public static string GZipCompress(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var bytes = Encoding.UTF8.GetBytes(text);

            return GZipCompress(bytes);
        }

        public static string GZipCompress(this byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStream, CompressionLevel.Fastest, true))
                {
                    gZipStream.Write(bytes, 0, bytes.Length);
                }
                memoryStream.Position = 0;
                var compressed = new byte[memoryStream.Length];
                memoryStream.Read(compressed, 0, compressed.Length);
                var gZipBuffer = new byte[compressed.Length + 4];
                Buffer.BlockCopy(compressed, 0, gZipBuffer, 4, compressed.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, gZipBuffer, 0, 4);
                return Convert.ToBase64String(gZipBuffer);
            }
        }

        public static string GZipDecompress(this string compressedText)
        {
            var gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);
                var buffer = new byte[dataLength];
                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
