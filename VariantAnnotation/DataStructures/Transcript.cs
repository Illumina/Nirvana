using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.DataStructures
{
    public sealed class Transcript : ReferenceAnnotationInterval
    {
        // transcript
        public readonly CompactId Id;
        public readonly byte Version;
        public readonly BioType BioType;
        public readonly bool IsCanonical;
        public readonly TranscriptDataSource TranscriptSource;

        // gene
        public readonly Gene Gene;

        // exons & introns
        public readonly SimpleInterval[] Introns;
        public readonly CdnaCoordinateMap[] CdnaMaps;
        public readonly int TotalExonLength;
        public readonly byte StartExonPhase;

        // protein function predictions
        public readonly int SiftIndex;
        public readonly int PolyPhenIndex;

        // translation
        public readonly Translation Translation;

        // attributes
        public readonly SimpleInterval[] MicroRnas;

        /// <summary>
        /// constructor
        /// </summary>
        public Transcript(ushort referenceIndex, int start, int end, CompactId id, byte version,
            Translation translation, BioType bioType, Gene gene, int totalExonLength, byte startExonPhase,
            bool isCanonical, SimpleInterval[] introns, SimpleInterval[] microRnas, CdnaCoordinateMap[] cdnaMaps,
            int siftIndex, int polyPhenIndex, TranscriptDataSource transcriptSource) : base(referenceIndex, start, end)
        {
            Id               = id;
            Version          = version;
            Translation      = translation;
            BioType          = bioType;
            Gene             = gene;
            TotalExonLength  = totalExonLength;
            StartExonPhase   = startExonPhase;
            IsCanonical      = isCanonical;
            Introns          = introns;
            MicroRnas        = microRnas;
            CdnaMaps         = cdnaMaps;
            SiftIndex        = siftIndex;
            PolyPhenIndex    = polyPhenIndex;
            TranscriptSource = transcriptSource;
            TotalExonLength  = TranscriptUtilities.GetTotalExonLength(cdnaMaps);
        }

        /// <summary>
        /// reads the transcript from the binary reader
        /// </summary>
        public static Transcript Read(ExtendedBinaryReader reader, Gene[] cacheGenes, SimpleInterval[] cacheIntrons,
            SimpleInterval[] cacheMirnas, string[] cachePeptideSeqs)
        {
            // transcript
            var referenceIndex = reader.ReadUInt16();
            var start          = reader.ReadOptInt32();
            var end            = reader.ReadOptInt32();
            var id             = CompactId.Read(reader);

            // gene
            var geneIndex = reader.ReadOptInt32();
            var gene      = cacheGenes[geneIndex];

            // encoded data
            var encoded = new EncodedTranscriptData(reader.ReadUInt16(), reader.ReadByte());

            // exons & introns
            var introns  = encoded.HasIntrons  ? ReadIndices(reader, cacheIntrons) : null;
            var cdnaMaps = encoded.HasCdnaMaps ? ReadCdnaMaps(reader)              : null;

            // protein function predictions
            int siftIndex     = encoded.HasSift     ? reader.ReadOptInt32() : -1;
            int polyphenIndex = encoded.HasPolyPhen ? reader.ReadOptInt32() : -1;

            // translation
            var translation = encoded.HasTranslation ? Translation.Read(reader, cachePeptideSeqs) : null;

            // attributes
            var mirnas = encoded.HasMirnas ? ReadIndices(reader, cacheMirnas) : null;

            return new Transcript(referenceIndex, start, end, id, encoded.Version, translation, encoded.BioType,
                gene, TranscriptUtilities.GetTotalExonLength(cdnaMaps), encoded.StartExonPhase, encoded.IsCanonical,
                introns, mirnas, cdnaMaps, siftIndex, polyphenIndex, encoded.TranscriptSource);
        }

        /// <summary>
        /// writes the transcript to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer, Dictionary<Gene, int> geneIndices,
            Dictionary<SimpleInterval, int> intronIndices, Dictionary<SimpleInterval, int> microRnaIndices,
            Dictionary<string, int> peptideIndices)
        {
            // transcript
            writer.Write(ReferenceIndex);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            Id.Write(writer);

            // gene
            writer.WriteOpt(GetIndex(Gene, geneIndices));

            // encoded data
            var encoded = new EncodedTranscriptData(BioType, Version, TranscriptSource, IsCanonical, SiftIndex != -1,
                PolyPhenIndex != -1, MicroRnas != null, Introns != null, CdnaMaps != null, Translation != null,
                StartExonPhase);

            encoded.Write(writer);

            // exons & introns
            if (encoded.HasIntrons) WriteIndices(writer, Introns, intronIndices);
            if (encoded.HasCdnaMaps) WriteCdnaMaps(writer);

            // protein function predictions
            if (encoded.HasSift) writer.WriteOpt(SiftIndex);
            if (encoded.HasPolyPhen) writer.WriteOpt(PolyPhenIndex);

            // translation
            if (encoded.HasTranslation)
            {
                // ReSharper disable once PossibleNullReferenceException
                var peptideIndex = GetIndex(Translation.PeptideSeq, peptideIndices);
                Translation.Write(writer, peptideIndex);
            }

            // attributes
            if (encoded.HasMirnas) WriteIndices(writer, MicroRnas, microRnaIndices);
        }

        private static CdnaCoordinateMap[] ReadCdnaMaps(ExtendedBinaryReader reader)
        {
            int numItems = reader.ReadOptInt32();
            var items = new CdnaCoordinateMap[numItems];

            for (int i = 0; i < numItems; i++) items[i] = CdnaCoordinateMap.Read(reader);
            return items;
        }

        private void WriteCdnaMaps(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(CdnaMaps.Length);
            foreach (var cdnaMap in CdnaMaps) cdnaMap.Write(writer);
        }

        private static T[] ReadIndices<T>(ExtendedBinaryReader reader, T[] cachedItems)
        {
            int numItems = reader.ReadOptInt32();
            var items = new T[numItems];

            for (int i = 0; i < numItems; i++)
            {
                var index = reader.ReadOptInt32();
                items[i] = cachedItems[index];
            }

            return items;
        }

        private static void WriteIndices<T>(ExtendedBinaryWriter writer, T[] items, Dictionary<T, int> indices)
        {
            writer.WriteOpt(items.Length);
            foreach (var item in items) writer.WriteOpt(GetIndex(item, indices));
        }

        /// <summary>
        /// returns the array index of the specified item
        /// </summary>
        private static int GetIndex<T>(T item, IReadOnlyDictionary<T, int> indices)
        {
            int index;
            if (!indices.TryGetValue(item, out index))
            {
                throw new GeneralException($"Unable to locate the {typeof(T)} in the indices: {item}");
            }
            return index;
        }
    }
}
