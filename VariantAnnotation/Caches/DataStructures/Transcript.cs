﻿using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Transcript : ITranscript
    {
        public Chromosome         Chromosome        { get; }
        public int                 Start             { get; }
        public int                 End               { get; }
        public ICompactId          Id                { get; }
        public BioType             BioType           { get; }
        public bool                IsCanonical       { get; }
        public Source              Source            { get; }
        public IGene               Gene              { get; }
        public ITranscriptRegion[] TranscriptRegions { get; }
        public ushort              NumExons          { get; }
        public int                 TotalExonLength   { get; }
        public byte                StartExonPhase    { get; }
        public int                 SiftIndex         { get; }
        public int                 PolyPhenIndex     { get; }
        public ITranslation        Translation       { get; }
        public IInterval[]         MicroRnas         { get; }
        public int[]               Selenocysteines   { get; }
        public IRnaEdit[]          RnaEdits          { get; }
        public bool                CdsStartNotFound  { get; }
        public bool                CdsEndNotFound    { get; }
        public ISequence           CodingSequence    { get; set; }
        public ISequence           CdnaSequence    { get; set; }

        public Transcript(Chromosome chromosome, int start, int end, ICompactId id, ITranslation translation,
            BioType bioType, IGene gene, int totalExonLength, byte startExonPhase, bool isCanonical,
            ITranscriptRegion[] transcriptRegions, ushort numExons, IInterval[] microRnas, int siftIndex,
            int polyPhenIndex, Source source, bool cdsStartNotFound, bool cdsEndNotFound, int[] selenocysteines,
            IRnaEdit[] rnaEdits)
        {
            Chromosome        = chromosome;
            Start             = start;
            End               = end;
            Id                = id;
            Translation       = translation;
            BioType           = bioType;
            Gene              = gene;
            TotalExonLength   = totalExonLength;
            StartExonPhase    = startExonPhase;
            IsCanonical       = isCanonical;
            TranscriptRegions = transcriptRegions;
            NumExons          = numExons;
            MicroRnas         = microRnas;
            SiftIndex         = siftIndex;
            PolyPhenIndex     = polyPhenIndex;
            Source            = source;
            CdsStartNotFound  = cdsStartNotFound;
            CdsEndNotFound    = cdsEndNotFound;
            Selenocysteines   = selenocysteines;
            RnaEdits          = rnaEdits;
        }

        public static ITranscript Read(BufferedBinaryReader reader,
            Dictionary<ushort, Chromosome> chromosomeIndexDictionary, IGene[] cacheGenes,
            ITranscriptRegion[] cacheTranscriptRegions, IInterval[] cacheMirnas, string[] cachePeptideSeqs)
        {
            // transcript
            ushort referenceIndex = reader.ReadOptUInt16();
            int start             = reader.ReadOptInt32();
            int end               = reader.ReadOptInt32();
            var id                = CompactId.Read(reader);

            // gene
            int geneIndex = reader.ReadOptInt32();
            var gene      = cacheGenes[geneIndex];

            // encoded data
            var encoded = EncodedTranscriptData.Read(reader);

            // transcript regions
            ITranscriptRegion[] transcriptRegions = encoded.HasTranscriptRegions ? ReadIndices(reader, cacheTranscriptRegions) : null;
            ushort numExons       = reader.ReadOptUInt16();

            // protein function predictions
            int siftIndex     = encoded.HasSift     ? reader.ReadOptInt32() : -1;
            int polyphenIndex = encoded.HasPolyPhen ? reader.ReadOptInt32() : -1;

            // translation
            var translation = encoded.HasTranslation ? DataStructures.Translation.Read(reader, cachePeptideSeqs) : null;
            
            // attributes
            IInterval[] mirnas    = encoded.HasMirnas          ? ReadIndices(reader, cacheMirnas)         : null;
            IRnaEdit[] rnaEdits   = encoded.HasRnaEdits        ? ReadItems(reader, RnaEdit.Read)          : null;
            int[] selenocysteines = encoded.HasSelenocysteines ? ReadItems(reader, x => x.ReadOptInt32()) : null;

            return new Transcript(chromosomeIndexDictionary[referenceIndex], start, end, id, translation,
                encoded.BioType, gene, ExonUtilities.GetTotalExonLength(transcriptRegions), encoded.StartExonPhase,
                encoded.IsCanonical, transcriptRegions, numExons, mirnas, siftIndex, polyphenIndex,
                encoded.TranscriptSource, encoded.CdsStartNotFound, encoded.CdsEndNotFound, selenocysteines, rnaEdits);
        }

        /// <summary>
        /// writes the transcript to the binary writer
        /// </summary>
        public void Write(IExtendedBinaryWriter writer, Dictionary<IGene, int> geneIndices,
            Dictionary<ITranscriptRegion, int> transcriptRegionIndices, Dictionary<IInterval, int> microRnaIndices,
            Dictionary<string, int> peptideIndices)
        {
            // transcript
            writer.WriteOpt(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            Id.Write(writer);

            // gene
            writer.WriteOpt(GetIndex(Gene, geneIndices));

            // encoded data
            var encoded = EncodedTranscriptData.GetEncodedTranscriptData(BioType, CdsStartNotFound, CdsEndNotFound,
                Source, IsCanonical, SiftIndex != -1, PolyPhenIndex != -1, MicroRnas != null, RnaEdits != null,
                Selenocysteines != null, TranscriptRegions != null, Translation != null, StartExonPhase);
            encoded.Write(writer);

            // transcript regions
            if (encoded.HasTranscriptRegions) WriteIndices(writer, TranscriptRegions, transcriptRegionIndices);
            writer.WriteOpt(NumExons);

            // protein function predictions
            if (encoded.HasSift) writer.WriteOpt(SiftIndex);
            if (encoded.HasPolyPhen) writer.WriteOpt(PolyPhenIndex);

            // translation
            if (encoded.HasTranslation)
            {
                // ReSharper disable once PossibleNullReferenceException
                int peptideIndex = GetIndex(Translation.PeptideSeq, peptideIndices);
                Translation.Write(writer, peptideIndex);
            }

            // attributes
            if (encoded.HasMirnas)          WriteIndices(writer, MicroRnas, microRnaIndices);
            if (encoded.HasRnaEdits)        WriteItems(writer, RnaEdits, (x, y) => x.Write(y));
            if (encoded.HasSelenocysteines) WriteItems(writer, Selenocysteines, (x, y) => y.WriteOpt(x));
        }

        private static T[] ReadItems<T>(BufferedBinaryReader reader, Func<BufferedBinaryReader, T> readFunc)
        {
            int numItems = reader.ReadOptInt32();
            var items    = new T[numItems];
            for (var i = 0; i < numItems; i++) items[i] = readFunc(reader);
            return items;
        }

        private static void WriteItems<T>(IExtendedBinaryWriter writer, T[] items, Action<T, IExtendedBinaryWriter> writeAction)
        {
            writer.WriteOpt(items.Length);
            foreach (var item in items) writeAction(item, writer);
        }

        private static T[] ReadIndices<T>(IBufferedBinaryReader reader, T[] cachedItems)
        {
            int numItems = reader.ReadOptInt32();
            var items = new T[numItems];

            for (var i = 0; i < numItems; i++)
            {
                int index = reader.ReadOptInt32();
                items[i] = cachedItems[index];
            }

            return items;
        }

        private static void WriteIndices<T>(IExtendedBinaryWriter writer, T[] items, IReadOnlyDictionary<T, int> indices)
        {
            writer.WriteOpt(items.Length);
            foreach (var item in items) writer.WriteOpt(GetIndex(item, indices));
        }

        private static int GetIndex<T>(T item, IReadOnlyDictionary<T, int> indices)
        {
            if (item == null) return -1;

            if (!indices.TryGetValue(item, out int index))
            {
                throw new InvalidDataException($"Unable to locate the {typeof(T)} in the indices: {item}");
            }

            return index;
        }
    }
}