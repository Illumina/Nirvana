using System.IO;
using System.Text;
using Amazon.Lambda.Serialization.Json;

namespace Cloud.Utilities
{
    public static class JsonUtilities
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        public static string Stringify(object obj) => Encoding.UTF8.GetString(Serialize(obj).ToArray());

        public static MemoryStream Serialize(object obj)
        {
            var memoryStream = new MemoryStream();
            JsonSerializer.Serialize(obj, memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public static T Deserialize<T>(MemoryStream memoryStream) => JsonSerializer.Deserialize<T>(memoryStream);
    }
}
