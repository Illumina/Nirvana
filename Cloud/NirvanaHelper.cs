using System.IO;
using System.Linq;
using Genome;

namespace Cloud
{
    public static class NirvanaHelper
    {
        public const int AnnotationTimeOut = 295_000;
        public const string S3Url = "https://illumina-annotation.s3.amazonaws.com/";
        public const string ProjectName = "RiverRun";
        public const string S3CacheFoler = S3Url + "Cache/26/";
        public const string DefaultCacheSource = "Both";
        public const string S3RefPrefix = S3Url + "References/5/Homo_sapiens.";

        public const string RefSuffix = ".Nirvana.dat";
        public const string JsonSuffix = ".json.gz";
        public const string JsonIndexSuffix = ".jsi";
        public const string TabixSuffix = ".tbi";


        public static string GetS3RefLocation(GenomeAssembly genomeAssembly) => S3RefPrefix + genomeAssembly + RefSuffix;

        public static void CleanOutput(string directory)
        {
            Directory.GetFiles(directory, "*" + JsonSuffix).ToList().ForEach(File.Delete);
            Directory.GetFiles(directory, "*" + JsonIndexSuffix).ToList().ForEach(File.Delete);
        }
    }
}