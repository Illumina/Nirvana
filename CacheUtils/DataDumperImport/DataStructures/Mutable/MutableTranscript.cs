using System;
using Genome;
using Intervals;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.DataDumperImport.DataStructures.Mutable
{
    public sealed class MutableTranscript : IEquatable<MutableTranscript>
    {
        public readonly IChromosome Chromosome;
        public readonly int Start;
        public readonly int End;
        public readonly string Id;
        public readonly byte Version;
        public readonly string CcdsId;
        public readonly string RefSeqId;
        public readonly Source Source;
        public readonly MutableGene Gene;
        public readonly IInterval[] MicroRnas;
        public readonly bool CdsStartNotFound;
        public readonly bool CdsEndNotFound;
        public readonly int[] SelenocysteinePositions;
        public readonly int StartExonPhase;
        public readonly IRnaEdit[] RnaEdits;
        
        public readonly string ProteinId;
        public readonly byte ProteinVersion;
        public readonly string PeptideSequence;
        public readonly MutableExon[] Exons;
        public readonly int TotalExonLength;
        public readonly IInterval[] Introns;
        public readonly string TranslateableSequence;
        public readonly MutableTranscriptRegion[] CdnaMaps;
        public readonly string BamEditStatus;

        // mutable
        public BioType BioType;
        public bool IsCanonical;
        public Gene UpdatedGene;

        public int CdsLength;
        public ITranscriptRegion[] TranscriptRegions;
        public byte NewStartExonPhase;
        public ICodingRegion CodingRegion;

        public readonly string SiftData;
        public readonly string PolyphenData;
        public int SiftIndex     = -1;
        public int PolyPhenIndex = -1;

        public MutableTranscript(IChromosome chromosome, int start, int end, string id, byte version, string ccdsId,
            string refSeqId, BioType bioType, bool isCanonical, ICodingRegion codingRegion, string proteinId,
            byte proteinVersion, string peptideSequence, Source source, MutableGene gene, MutableExon[] exons,
            int startExonPhase, int totalExonLength, IInterval[] introns, MutableTranscriptRegion[] cdnaMaps,
            string siftData, string polyphenData, string translateableSequence, IInterval[] microRnas,
            bool cdsStartNotFound, bool cdsEndNotFound, int[] selenocysteinePositions, IRnaEdit[] rnaEdits,
            string bamEditStatus)
        {
            Chromosome              = chromosome;
            Start                   = start;
            End                     = end;
            Id                      = id;
            Version                 = version;
            CcdsId                  = ccdsId;
            RefSeqId                = refSeqId;
            BioType                 = bioType;
            IsCanonical             = isCanonical;
            CodingRegion            = codingRegion;
            ProteinId               = proteinId;
            ProteinVersion          = proteinVersion;
            PeptideSequence         = peptideSequence;
            Source                  = source;
            Gene                    = gene;
            Exons                   = exons;
            StartExonPhase          = startExonPhase;
            TotalExonLength         = totalExonLength;
            Introns                 = introns;
            CdnaMaps                = cdnaMaps;
            SiftData                = siftData;
            PolyphenData            = polyphenData;
            TranslateableSequence   = translateableSequence;
            MicroRnas               = microRnas;
            CdsStartNotFound        = cdsStartNotFound;
            CdsEndNotFound          = cdsEndNotFound;
            SelenocysteinePositions = selenocysteinePositions;
            RnaEdits                = rnaEdits;
            BamEditStatus           = bamEditStatus;
        }

        public bool Equals(MutableTranscript other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Chromosome.Index == other.Chromosome.Index &&
                   Start            == other.Start            &&
                   End              == other.End              &&
                   Id               == other.Id               &&
                   Version          == other.Version          &&
                   BioType          == other.BioType          &&
                   Source           == other.Source;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                int hashCode = Chromosome.Index.GetHashCode();
                hashCode = (hashCode * 397) ^ Start;
                hashCode = (hashCode * 397) ^ End;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ Version.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) BioType;
                hashCode = (hashCode * 397) ^ (int) Source;
                return hashCode;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }
    }
}
