using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNet.Common
{
    public static class AttributeUtil
    {
        public enum Test
        {
            [EnumDescription("hhhhhh")]
            None = 0,
            [EnumDescription("xxxxxx")]
            Done = 1
        }
        private static IEnumerable<string> GetEnumDescriptions(this Enum e)
        {
            IEnumerable<string> result = null;
            var type = e.GetType();
            var fieldInfo = type.GetField(e.ToString());
            var attr = fieldInfo?.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
            if (attr?.Length > 0)
            {
                result = attr.Cast<EnumDescriptionAttribute>().Select(x => x.Description);
            }
            return result ?? Enumerable.Empty<string>();
        }
    }


    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public EnumDescriptionAttribute(string description)
        {
            Description = description;
        }
    }



    public class Person
    {
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime CreateTime { get; set; }
    }
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return DateTime.MinValue;

            if (DateTime.TryParse(reader.Value.ToString(), out DateTime result))
                return result;

            return DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }


}
