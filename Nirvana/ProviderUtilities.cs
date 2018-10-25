using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IO;
using VariantAnnotation;
using CommandLine.Utilities;
using IO.StreamSource;
using IO.StreamSourceCollection;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace Nirvana
{
    public static class ProviderUtilities
    {
        public static IAnnotator GetAnnotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider,
            IAnnotationProvider saProviders, IAnnotationProvider conservationProvider,
            IGeneAnnotationProvider geneAnnotationProviders, IEnumerable<IPlugin> plugins = null)
        {
            return new Annotator(taProvider, sequenceProvider, saProviders, conservationProvider,
                geneAnnotationProviders, plugins);
        }

        public static ISequenceProvider GetSequenceProvider(string compressedReferencePath)
        {
             return new ReferenceSequenceProvider(StreamSourceUtils.GetStream(compressedReferencePath));
        }

        public static IAnnotationProvider GetConservationProvider(IStreamSourceCollection[] annotationStreamSourceCollections)
        {
            return annotationStreamSourceCollections == null ? null : ConservationScoreProvider.GetConservationScoreProvider(annotationStreamSourceCollections);
        }

        
        public static IAnnotationProvider GetNsaProvider(IStreamSourceCollection[] annotationStreamSourceCollections)
        {
            if (annotationStreamSourceCollections == null || annotationStreamSourceCollections.Length == 0)
                return null;
            var nsaReaders = new List<NsaReader>();
            var nsiReaders = new List<INsiReader>();
            foreach (IStreamSourceCollection collection in annotationStreamSourceCollections)
            {
                nsaReaders.AddRange(collection.GetStreamSources(SaCommon.SaFileSuffix).Select(GetNsaReader));
                nsiReaders.AddRange(collection.GetStreamSources(SaCommon.SiFileSuffix).Select(GetNsiReader));
            }

            if (nsaReaders.Count > 0 || nsiReaders.Count > 0)
                return new NsaProvider(nsaReaders.ToArray(), nsiReaders.ToArray());
            return null;
        }

        public static ITranscriptAnnotationProvider GetTranscriptAnnotationProvider(string path,
            ISequenceProvider sequenceProvider)
         {
            var benchmark = new Benchmark();
            var provider = new TranscriptAnnotationProvider(path, sequenceProvider);
            var wallTimeSpan = benchmark.GetElapsedTime();
            Console.WriteLine("Cache Time: {0} ms", wallTimeSpan.TotalMilliseconds);
            return provider;
        }

        public static IRefMinorProvider GetRefMinorProvider(IStreamSourceCollection[] annotationStreamSourceCollections)
        {
            return annotationStreamSourceCollections == null || annotationStreamSourceCollections.Length == 0
                ? null
                : new RefMinorProvider(annotationStreamSourceCollections);
        }

        public static IGeneAnnotationProvider GetGeneAnnotationProvider(
            IStreamSourceCollection[] annotationStreamSourceCollections)
        {
            return annotationStreamSourceCollections == null || annotationStreamSourceCollections.Length == 0
                ? null
                : new GeneAnnotationProvider(annotationStreamSourceCollections);
            
        }

        private static NsaReader GetNsaReader(IStreamSource streamSource) =>
            new NsaReader(new ExtendedBinaryReader(new SeekableStream(streamSource, 0)), streamSource.GetAssociatedStreamSource(".idx").GetStream());

        private static NsiReader GetNsiReader(IStreamSource streamSource) => new NsiReader(streamSource.GetStream());
    }
}