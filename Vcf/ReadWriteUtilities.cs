using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Compression.FileHandling;
using Compression.Utilities;
using Genome;
using IO;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Providers;

namespace Vcf
{
	public static class ReadWriteUtilities
	{
		public static StreamWriter GetOutputWriter(string outputPath)
		{
		    if (outputPath == "-") return new StreamWriter(Console.OpenStandardOutput());

		    var stream = new BlockGZipStream(FileUtilities.GetCreateStream(outputPath + ".json.gz"), CompressionMode.Compress);
            return new BgzipTextWriter(stream);
		}

	    public static IVcfReader GetVcfReader(string vcfPath, IDictionary<string, IChromosome> chromosomeDictionary,
	        IRefMinorProvider refMinorProvider, bool verboseTranscript, IRecomposer recomposer)
	    {
	        bool useStdInput = vcfPath == "-";
            var stream = useStdInput ? Console.OpenStandardInput() : new BlockGZipStream(StreamSourceUtils.GetStream(vcfPath), CompressionMode.Decompress);
            return VcfReader.Create(stream, chromosomeDictionary, refMinorProvider, verboseTranscript, recomposer, new NullVcfFilter());
        }
        
	    public static StreamWriter GetVcfOutputWriter(string outputPath)
	    {
	        return outputPath == "-"
	            ? new StreamWriter(Console.OpenStandardOutput())
	            : GZipUtilities.GetStreamWriter(outputPath + ".vcf.gz");

	    }

        public static StreamWriter GetGvcfOutputWriter(string outputPath)
	    {
	        return outputPath == "-"
	            ? new StreamWriter(Console.OpenStandardOutput())
	            : GZipUtilities.GetStreamWriter(outputPath + ".genome.vcf.gz");
	    }
    }
}