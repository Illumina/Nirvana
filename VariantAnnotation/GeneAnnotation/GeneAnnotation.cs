using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.GeneAnnotation
{
    public class GeneAnnotation:IGeneAnnotation
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
    }
}