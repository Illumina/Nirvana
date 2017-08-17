using VariantAnnotation.Interface.GeneAnnotation;

namespace VariantAnnotation.GeneAnnotation
{
    public class OmimAnnotation:IGeneAnnotation
    {
        public string DataSource => "omim";
        public string[] JsonStrings { get; }
        public bool IsArray => true;


        public OmimAnnotation(string[] jsonStrings)
        {
            JsonStrings = jsonStrings;
        }
    }
}