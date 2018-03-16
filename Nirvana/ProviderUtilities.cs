using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace Nirvana
{
    public static class ProviderUtilities
    {
        public static IAnnotator GetAnnotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider, IAnnotationProvider saProviders, IAnnotationProvider conservationProvider, IGeneAnnotationProvider geneAnnotationProviders, IEnumerable<IPlugin> plugins = null)
        {
            return new Annotator(taProvider, sequenceProvider, saProviders, conservationProvider, geneAnnotationProviders, plugins);
        }

        public static ISequenceProvider GetSequenceProvider(string compressedReferencePath)
        {
            return new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReferencePath));
        }

        public static IAnnotationProvider GetConservationProvider(IEnumerable<string> dirPaths)
        {
            if (dirPaths == null) return null;
            dirPaths = dirPaths.ToList();
            return dirPaths.All(x => Directory.GetFiles(x, "*.npd").Length == 0) ? null : new ConservationScoreProvider(dirPaths);
        }

        public static IAnnotationProvider GetSaProvider(List<string> supplementaryAnnotationDirectories)
        {
            if (supplementaryAnnotationDirectories == null || supplementaryAnnotationDirectories.Count == 0)
                return null;
            return new SupplementaryAnnotationProvider(supplementaryAnnotationDirectories);
        }

        public static ITranscriptAnnotationProvider GetTranscriptAnnotationProvider(string path, ISequenceProvider sequenceProvider)
        {
            return new TranscriptAnnotationProvider(path, sequenceProvider);
        }

        public static IRefMinorProvider GetRefMinorProvider(List<string> supplementaryAnnotationDirectories)
        {
            return supplementaryAnnotationDirectories == null || supplementaryAnnotationDirectories.Count == 0 ? null : new RefMinorProvider(supplementaryAnnotationDirectories);
        }

        public static IGeneAnnotationProvider GetGeneAnnotationProvider(IEnumerable<string> supplementaryAnnotationDirectories)
        {
            var reader = SaReaderUtils.GetGeneAnnotationDatabaseReader(supplementaryAnnotationDirectories.ToList());
            if (reader == null) return null;
            var geneAnnotationProvider = new GeneAnnotationProvider(reader);
            return geneAnnotationProvider;
        }
    }
}