using System;
using System.Collections.Generic;
using System.Linq;
using IO;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.GeneFusions.IO;
using VariantAnnotation.GenericScore;
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

        public static ProteinConservationProvider GetProteinConservationProvider(AnnotationFiles files) =>
            files == null || string.IsNullOrEmpty(files.ProteinConservationFile)
                ? null
                : new ProteinConservationProvider(PersistentStreamUtils.GetReadStream(files.ProteinConservationFile));

        public static IAnnotationProvider GetConservationProvider(AnnotationFiles files)
        {
            if (files == null || files.PhylopFile == default) return null;
            return new ConservationScoreProvider()
                .AddPhylopReader(PersistentStreamUtils.GetReadStream(files.PhylopFile.Npd),
                    PersistentStreamUtils.GetReadStream(files.PhylopFile.Idx));
        }

        public static IAnnotationProvider GetLcrProvider(AnnotationFiles files) =>
            files?.LowComplexityRegionFile == null
                ? null
                : new LcrProvider(PersistentStreamUtils.GetReadStream(files.LowComplexityRegionFile));

        public static IRefMinorProvider GetRefMinorProvider(AnnotationFiles files) =>
            files == null || files.RefMinorFile == default
                ? null
                : new RefMinorProvider(PersistentStreamUtils.GetReadStream(files.RefMinorFile.Rma),
                    PersistentStreamUtils.GetReadStream(files.RefMinorFile.Idx));

        public static IGeneAnnotationProvider GetGeneAnnotationProvider(AnnotationFiles files) => files?.NsiFiles == null
            ? null
            : new GeneAnnotationProvider(PersistentStreamUtils.GetStreams(files.NgaFiles));

        public static IAnnotationProvider GetNsaProvider(AnnotationFiles files)
        {
            if (files == null) return null;

            INsaReader[]          nsaReaders    = GetNsaReaders(files.NsaFiles);
            INsiReader[]          nsiReaders    = GetNsiReaders(files.NsiFiles);
            IGeneFusionSaReader[] fusionReaders = GetGeneFusionReaders(files.GeneFusionSourceFiles, files.GeneFusionJsonFiles);

            int numReaders = nsaReaders.Length + nsiReaders.Length + fusionReaders.Length;
            return numReaders == 0 ? null : new NsaProvider(nsaReaders, nsiReaders, fusionReaders);
        }

        private static INsaReader[] GetNsaReaders(IReadOnlyCollection<(string Nsa, string Idx)> filePaths)
        {
            var readers = new List<INsaReader>(filePaths.Count);
            foreach ((string nsaPath, string idxPath) in filePaths)
                readers.Add(new NsaReader(PersistentStreamUtils.GetReadStream(nsaPath), PersistentStreamUtils.GetReadStream(idxPath)));
            return readers.SortByJsonKey();
        }

        public static IAnnotationProvider GetGsaProvider(AnnotationFiles files)
        {
            if (files?.GsaFiles == null || files.GsaFiles.Count == 0) return null;

            List<(string Gsa, string Idx)> filePaths = files.GsaFiles;

            var readers = new ScoreReader[filePaths.Count];

            var i = 0;
            foreach ((string gsaPath, string idxPath) in filePaths)
            {
                readers[i] = ScoreReader.Read(PersistentStreamUtils.GetReadStream(gsaPath), PersistentStreamUtils.GetReadStream(idxPath));
                i++;
            }

            readers = readers.SortByJsonKey();
            return new ScoreProvider(readers);
        }


        private static INsiReader[] GetNsiReaders(IReadOnlyCollection<string> filePaths)
        {
            var readers = new List<INsiReader>(filePaths.Count);
            foreach (string filePath in filePaths) readers.Add(NsiReader.Read(PersistentStreamUtils.GetReadStream(filePath)));
            return readers.SortByJsonKey();
        }

        private static IGeneFusionSaReader[] GetGeneFusionReaders(IReadOnlyCollection<string> sourceFilePaths,
            IReadOnlyCollection<string> jsonFilePaths)
        {
            var readers = new List<IGeneFusionSaReader>(jsonFilePaths.Count);
            foreach (string filePath in sourceFilePaths) readers.Add(new GeneFusionSourceReader(PersistentStreamUtils.GetReadStream(filePath)));
            foreach (string filePath in jsonFilePaths) readers.Add(new GeneFusionJsonReader(PersistentStreamUtils.GetReadStream(filePath)));
            return readers.SortByJsonKey();
        }

        private static T[] SortByJsonKey<T>(this IEnumerable<T> entries) where T : ISaMetadata =>
            entries.OrderBy(x => x.JsonKey, StringComparer.Ordinal).ToArray();

        public static ITranscriptAnnotationProvider GetTranscriptAnnotationProvider(string path, ISequenceProvider sequenceProvider,
            ProteinConservationProvider proteinConservationProvider) =>
            new TranscriptAnnotationProvider(path, sequenceProvider, proteinConservationProvider);
    }
}