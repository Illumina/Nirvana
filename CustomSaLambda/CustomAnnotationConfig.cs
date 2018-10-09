// ReSharper disable InconsistentNaming
using Cloud;

namespace CustomSaLambda
{
    public class CustomAnnotationConfig
    {
        public string id;
        public string genomeAssembly;
        public S3Path inputTsv;
        public S3Path outputDir;
    }
}