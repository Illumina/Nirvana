using System;
using System.Linq;
using IO;
using CommandLine.Utilities;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;

namespace Nirvana
{
    public static class ProviderUtilities
    {
        public static ISequenceProvider GetSequenceProvider(string compressedReferencePath)
        {
             return new ReferenceSequenceProvider(PersistentStreamUtils.GetReadStream(compressedReferencePath));
        }

        public static IAnnotationProvider GetConservationProvider(AnnotationFiles files) =>
            files == null || files.ConservationFile == default
                ? null
                : new ConservationScoreProvider(PersistentStreamUtils.GetReadStream(files.ConservationFile.Npd),
                    PersistentStreamUtils.GetReadStream(files.ConservationFile.Idx));

        public static IRefMinorProvider GetRefMinorProvider(AnnotationFiles files) =>
            files == null || files.RefMinorFile == default ? null : 
                new RefMinorProvider(PersistentStreamUtils.GetReadStream(files.RefMinorFile.Rma), 
                    PersistentStreamUtils.GetReadStream(files.RefMinorFile.Idx));

        public static IGeneAnnotationProvider GetGeneAnnotationProvider(AnnotationFiles files) => files?.NsiFiles == null ? null : new GeneAnnotationProvider(PersistentStreamUtils.GetStreams(files.NgaFiles));

        public static IAnnotationProvider GetNsaProvider(AnnotationFiles files)
        {
            if (files == null) return null;
            //todo: use using block to release nsa streams
            var nsaReaders = files.NsaFiles?.Select(x => new NsaReader(PersistentStreamUtils.GetReadStream(x.Nsa), PersistentStreamUtils.GetReadStream(x.Idx)))
                                            .OrderBy(x => x.JsonKey, StringComparer.Ordinal).ToArray() ?? new INsaReader[] { };
            //todo: use using block to release nsi streams
            var nsiReaders = files.NsiFiles?.Select(x => NsiReader.Read(PersistentStreamUtils.GetReadStream(x)))
                                            .OrderBy(x => x.JsonKey, StringComparer.Ordinal).ToArray() ?? new INsiReader[] { };

            if (nsaReaders.Length == 0 && nsiReaders.Length == 0) return null;
            
            return new NsaProvider(nsaReaders, nsiReaders);
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
    }
}