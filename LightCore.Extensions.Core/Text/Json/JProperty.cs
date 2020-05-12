using System;
using System.Globalization;
using Newtonsoft.Json;

namespace NetCore.Extensions.Core.Text.Json
{
    public class DateTimeConverter : JsonConverter
    {
        private readonly string format;

        public DateTimeConverter(string format)
        {
            this.format = format;
        }

        #region Overrides of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(format));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            return DateTime.ParseExact(s, format, CultureInfo.CurrentCulture);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        #endregion
    }

    /// <summary>
    /// "yyyy-MM-dd"
    /// </summary>
    public class YmdConverter : DateTimeConverter
    {
        public YmdConverter() : base("yyyy-MM-dd")
        {
        }
    }
    /// <summary>
    /// "yyyy-MM-dd HH:mm:ss"
    /// </summary>
    public class UniversalConverter : DateTimeConverter
    {
        public UniversalConverter() : base("yyyy-MM-dd HH:mm:ss")
        {
        }
    }
}
