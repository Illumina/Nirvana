using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ScoreJsonEncoder
    {
        public readonly  string JsonKey;
        private readonly string _jsonSubKey;

        public string JsonRepresentation<T>(T data)
        {
            if (_jsonSubKey != null)
                return $"\"{_jsonSubKey}\":{data}";
            
            return data.ToString();
        }

        public ScoreJsonEncoder(string jsonKey, string jsonSubKey)
        {
            JsonKey     = jsonKey;
            _jsonSubKey = jsonSubKey;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(JsonKey);
            writer.WriteOptAscii(_jsonSubKey);
        }

        public static ScoreJsonEncoder Read(ExtendedBinaryReader reader)
        {
            return new ScoreJsonEncoder(
                reader.ReadAsciiString(),
                reader.ReadAsciiString()
            );
        }
    }
}