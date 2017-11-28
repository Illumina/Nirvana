using System;
using System.Collections.Generic;
using System.IO;
using Compression.FileHandling;
using Compression.Utilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using Vcf;

namespace Nirvana
{
	public static class ReadWriteUtilities
	{
		public static StreamWriter GetOutputWriter(string outputPath)
		{
		    return outputPath == "-"
		        ? new StreamWriter(Console.OpenStandardOutput())
		        : new BgzipTextWriter(outputPath + ".json.gz");
		}

	    internal static IVcfReader GetVcfReader(string vcfPath, IDictionary<string, IChromosome> chromosomeDictionary,
	        IRefMinorProvider refMinorProvider, bool verboseTranscript)
	    {
	        var useStdInput = vcfPath == "-";

	        var peekStream =
	            new PeekStream(useStdInput
	                ? Console.OpenStandardInput()
	                : GZipUtilities.GetAppropriateReadStream(vcfPath));

	        return new VcfReader(peekStream, chromosomeDictionary, refMinorProvider, verboseTranscript);
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