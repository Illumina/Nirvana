using System.Collections.Generic;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ITranscript : IChromosomeInterval
    {
        ICompactId Id { get; }
        BioType BioType { get; }
        bool IsCanonical { get; }
        Source Source { get; }

        IGene Gene { get; }
        ITranscriptRegion[] TranscriptRegions { get; }
        ushort NumExons { get; }
        int TotalExonLength { get; }
        byte StartExonPhase { get; }
        int SiftIndex { get; }
        int PolyPhenIndex { get; }

        ITranslation Translation { get; }
        IInterval[] MicroRnas { get; }
        int[] Selenocysteines { get; }
        IRnaEdit[] RnaEdits { get; }

        bool CdsStartNotFound { get; }
        bool CdsEndNotFound { get; }

        void Write(IExtendedBinaryWriter writer, Dictionary<IGene, int> geneIndices,
            Dictionary<ITranscriptRegion, int> transcriptRegionIndices, Dictionary<IInterval, int> microRnaIndices,
            Dictionary<string, int> peptideIndices);
    }

    public enum Source : byte
    {
        None,
        RefSeq,
        Ensembl,
        BothRefSeqAndEnsembl
    }
}