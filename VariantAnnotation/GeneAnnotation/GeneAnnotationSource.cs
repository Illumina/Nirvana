using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotationSource:IGeneAnnotationSource
    {
        public string DataSource { get; }
        public string[] JsonStrings { get; }
        public bool IsArray { get; }


        public GeneAnnotationSource(string dataSource, string[] jsonStrings, bool isArray)
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

        public static IGeneAnnotationSource Read(IExtendedBinaryReader reader)
        {
            var dataSource = reader.ReadAsciiString();
            var isArray = reader.ReadBoolean();
            var jsonStringLength = reader.ReadOptInt32();
            var jsonStrings = new string[jsonStringLength];
            for (int i=0; i<jsonStringLength; i++)
            {
                jsonStrings[i] = reader.ReadString();
            }

            return new GeneAnnotationSource(dataSource, jsonStrings, isArray);
        }
    }
}