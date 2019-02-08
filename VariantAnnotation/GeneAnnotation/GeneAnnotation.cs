using IO;
using VariantAnnotation.Interface.GeneAnnotation;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotation:IGeneAnnotation
    {
        public string DataSource { get; }
        public string[] JsonStrings { get; }
        public bool IsArray { get; }


        public GeneAnnotation(string dataSource, string[] jsonStrings, bool isArray)
        {
            DataSource = dataSource;
            JsonStrings = jsonStrings;
            IsArray = isArray;
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write(DataSource);
            writer.Write(IsArray);
            writer.WriteOpt(JsonStrings.Length);
            foreach (var jsonString in JsonStrings)
                writer.Write(jsonString);
        }

        public static IGeneAnnotation Read(ExtendedBinaryReader reader)
        {
            var dataSource = reader.ReadAsciiString();
            var isArray = reader.ReadBoolean();
            var jsonStringLength = reader.ReadOptInt32();
            var jsonStrings = new string[jsonStringLength];
            for (int i=0; i<jsonStringLength; i++)
            {
                jsonStrings[i] = reader.ReadString();
            }

            return new GeneAnnotation(dataSource, jsonStrings, isArray);
        }
    }
}