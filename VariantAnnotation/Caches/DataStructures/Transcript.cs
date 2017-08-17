using System.Collections.Generic;
using System.IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Transcript : ITranscript
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public ICompactId Id { get; }
        public byte Version { get; }
        public BioType BioType { get; }
        public bool IsCanonical { get; }
        public Source Source { get; }
        public IGene Gene { get; }
        public IInterval[] Introns { get; }
        public ICdnaCoordinateMap[] CdnaMaps { get; }
        public int TotalExonLength { get; }
        public byte StartExonPhase { get; }
        public int SiftIndex { get; }
        public int PolyPhenIndex { get; }
        public ITranslation Translation { get; }
        public IInterval[] MicroRnas { get; }

        /// <summary>
        /// constructor
        /// </summary>
        internal Transcript(IChromosome chromosome, int start, int end, CompactId id, byte version,
            ITranslation translation, BioType bioType, IGene gene, int totalExonLength, byte startExonPhase,
            bool isCanonical, IInterval[] introns, IInterval[] microRnas, ICdnaCoordinateMap[] cdnaMaps,
            int siftIndex, int polyPhenIndex, Source transcriptSource)
        {


            Start      = start;
            End        = end;
            Chromosome = chromosome;

            Id              = id;
            Version         = version;
            Translation     = translation;
            BioType         = bioType;
            Gene            = gene;
            TotalExonLength = totalExonLength;
            StartExonPhase  = startExonPhase;
            IsCanonical     = isCanonical;
            Introns         = introns;
            MicroRnas       = microRnas;
            CdnaMaps        = cdnaMaps;
            SiftIndex       = siftIndex;
            PolyPhenIndex   = polyPhenIndex;
            Source          = transcriptSource;
        }

        public static ITranscript Read(ExtendedBinaryReader reader,
            IDictionary<ushort, IChromosome> chromosomeIndexDictionary, IGene[] cacheGenes, IInterval[] cacheIntrons,
            IInterval[] cacheMirnas, string[] cachePeptideSeqs)
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
            var translation = encoded.HasTranslation ? DataStructures.Translation.Read(reader, cachePeptideSeqs) : null;

            // attributes
            var mirnas = encoded.HasMirnas ? ReadIndices(reader, cacheMirnas) : null;

            return new Transcript(chromosomeIndexDictionary[referenceIndex], start, end, id, encoded.Version,
                translation, encoded.BioType, gene, ExonUtilities.GetTotalExonLength(cdnaMaps), encoded.StartExonPhase,
                encoded.IsCanonical, introns, mirnas, cdnaMaps, siftIndex, polyphenIndex, encoded.TranscriptSource);
        }

        /// <summary>
        /// writes the transcript to the binary writer
        /// </summary>
        public void Write(IExtendedBinaryWriter writer, Dictionary<IGene, int> geneIndices,
            Dictionary<IInterval, int> intronIndices, Dictionary<IInterval, int> microRnaIndices,
            Dictionary<string, int> peptideIndices)
        {
            // transcript
            writer.Write(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            Id.Write(writer);

            // gene
            writer.WriteOpt(GetIndex(Gene, geneIndices));

            // encoded data
            var encoded = new EncodedTranscriptData(BioType, Version, Source, IsCanonical, SiftIndex != -1,
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

        private static ICdnaCoordinateMap[] ReadCdnaMaps(ExtendedBinaryReader reader)
        {
            int numItems = reader.ReadOptInt32();
            var items = new ICdnaCoordinateMap[numItems];

            for (int i = 0; i < numItems; i++) items[i] = CdnaCoordinateMap.Read(reader);
            return items;
        }

        private void WriteCdnaMaps(IExtendedBinaryWriter writer)
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

        private static void WriteIndices<T>(IExtendedBinaryWriter writer, T[] items, Dictionary<T, int> indices)
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
                throw new InvalidDataException($"Unable to locate the {typeof(T)} in the indices: {item}");
            }
            return index;
        }
    }
}