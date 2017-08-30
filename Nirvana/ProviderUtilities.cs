using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace Nirvana
{
	public static class ProviderUtilities
	{
		public static IAnnotator GetAnnotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider, IAnnotationProvider saProviders, IAnnotationProvider conservationProvider,IGeneAnnotationProvider[] geneAnnotationProviders)
		{
			return new Annotator(taProvider, sequenceProvider, saProviders, conservationProvider,geneAnnotationProviders);
		}

		public static ISequenceProvider GetSequenceProvider(string compressedReferencePath)
		{
			return new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReferencePath));
		}

		public static IAnnotationProvider GetConservationProvider(IEnumerable<string> dirPaths)
		{
			if (dirPaths==null) return null;
		    if (dirPaths.All(x => Directory.GetFiles(x, "*.npd").Length == 0)) return null;
			return new ConservationScoreProvider(dirPaths);
		}

	    public static IAnnotationProvider GetSaProvider(List<string> supplementaryAnnotationDirectories)
	    {
	        if (supplementaryAnnotationDirectories == null || supplementaryAnnotationDirectories.Count == 0)
	            return null;
	        return new SupplementaryAnnotationProvider(supplementaryAnnotationDirectories);
	    }

		public static IAnnotationProvider GetTranscriptAnnotationProvider(string path,  ISequenceProvider sequenceProvider)
		{
			return new TranscriptAnnotationProvider(path, sequenceProvider);
		}

	    public static IRefMinorProvider GetRefMinorProvider(List<string> supplementaryAnnotationDirectories)
	    {
	        return supplementaryAnnotationDirectories==null || supplementaryAnnotationDirectories.Count==0? null: new RefMinorProvider(supplementaryAnnotationDirectories);
	    }

	    public static IGeneAnnotationProvider[] GetGeneAnnotationProviders(List<string> supplementaryAnnotationDirectories)
	    {

	        var reader = SaReaderUtils.GetGeneAnnotationDatabaseReader(supplementaryAnnotationDirectories);
	        if (reader == null) return null;
            var geneAnnotationProviders = new IGeneAnnotationProvider[1];
	        geneAnnotationProviders[0] = new OmimAnnotationProvider(reader);
	        return geneAnnotationProviders;

	    }
	}
}