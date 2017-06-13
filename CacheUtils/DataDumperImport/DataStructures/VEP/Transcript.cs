using System;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.Transcript;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class Transcript : SortableCoordinate, IEquatable<Transcript>
    {
        #region members

        // use null values to detect downstream annotation problems
        public BioType BioType;
        public readonly SimpleInterval[] MicroRnas;
        public Exon[] TransExons;
        public Gene Gene;               // null
        public Translation Translation; // null
        public VariantEffectFeatureCache VariantEffectCache;
        public Slice Slice;             // null

        public readonly bool OnReverseStrand;   // set
        public bool IsCanonical;       // set

        public int CompDnaCodingStart; // set
        public int CompDnaCodingEnd;   // set

        public readonly byte Version;

        public readonly string CcdsId;          // set // null
        public readonly string DatabaseId;      // set // null
        public string ProteinId;       // set // null
        public readonly string RefSeqId;        // set // null
        public readonly string GeneStableId;    // set // null
        public readonly string StableId;        // set // null

        public string GeneSymbol;
        public readonly GeneSymbolSource GeneSymbolSource;
        public readonly int HgncId;

        // ====================
        // conversion variables
        // ====================

        public readonly VariantAnnotation.DataStructures.Gene FinalGene;
        public int GeneIndex;

        public LegacyExon[] FinalExons;
        public int[] ExonIndices;

        public SimpleInterval[] FinalIntrons;
        public int[] IntronIndices;

        public SimpleInterval[] FinalMicroRnas;
        public int[] MicroRnaIndices;

        public CdnaCoordinateMap[] FinalCdnaMaps;

        public int SiftIndex       = -1;
        public int PolyPhenIndex   = -1;
        public int CdnaSeqIndex    = -1;
        public int PeptideSeqIndex = -1;

        private int _hashCode;

        #endregion

        public Transcript(BioType biotype, Exon[] transExons, Gene gene, Translation translation, VariantEffectFeatureCache cache, Slice slice,
            bool onReverseStrand, bool isCanonical, int cdnaCodingStart, int cdnaCodingEnd, ushort referenceIndex, int start, int end, 
            string ccdsId, string databaseId, string proteinId, string refSeqId, string geneStableId, string stableId, string geneSymbol, 
            GeneSymbolSource geneSymbolSource, int hgncId, byte version, SimpleInterval[] microRnas)
            : base(referenceIndex, start, end)
        {
            BioType            = biotype;
            CcdsId             = ccdsId;
            CompDnaCodingEnd   = cdnaCodingEnd;
            CompDnaCodingStart = cdnaCodingStart;
            DatabaseId         = databaseId;
            Gene               = gene;
            GeneStableId       = geneStableId;
            GeneSymbol         = geneSymbol;
            GeneSymbolSource   = geneSymbolSource;
            HgncId             = hgncId;
            IsCanonical        = isCanonical;
            MicroRnas          = microRnas;
            OnReverseStrand    = onReverseStrand;
            ProteinId          = proteinId;
            RefSeqId           = refSeqId;
            Slice              = slice;
            StableId           = stableId;
            TransExons         = transExons;
            Translation        = translation;
            VariantEffectCache = cache;
            Version            = version;

            var entrezId = ImportDataStore.TranscriptSource == TranscriptDataSource.Ensembl
                ? CompactId.Empty
                : CompactId.Convert(geneStableId);

            var ensemblId = ImportDataStore.TranscriptSource == TranscriptDataSource.Ensembl
                ? CompactId.Convert(geneStableId)
                : CompactId.Empty;

            FinalGene = new VariantAnnotation.DataStructures.Gene(referenceIndex, start, end, onReverseStrand,
                geneSymbol, hgncId, entrezId, ensemblId, -1);

            GenerateHashCode();
        }

        /// <summary>
        /// generates the hash code
        /// </summary>
        private void GenerateHashCode()
        {
            _hashCode = BioType.GetHashCode()            ^
                        CompDnaCodingEnd.GetHashCode()   ^
                        CompDnaCodingStart.GetHashCode() ^
                        End.GetHashCode()                ^
                        IsCanonical.GetHashCode()        ^
                        OnReverseStrand.GetHashCode()    ^
                        ReferenceIndex.GetHashCode()     ^
                        Start.GetHashCode();

            if (CcdsId       != null) _hashCode ^= CcdsId.GetHashCode();
            if (DatabaseId   != null) _hashCode ^= DatabaseId.GetHashCode();
            if (ProteinId    != null) _hashCode ^= ProteinId.GetHashCode();
            if (RefSeqId     != null) _hashCode ^= RefSeqId.GetHashCode();
            if (GeneStableId != null) _hashCode ^= GeneStableId.GetHashCode();
            if (StableId     != null) _hashCode ^= StableId.GetHashCode();

            if (Gene  != null) _hashCode ^= Gene.GetHashCode();
            if (Slice != null) _hashCode ^= Slice.GetHashCode();
        }

        #region Equality Overrides

        // ReSharper disable once NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Transcript return false:
            var other = obj as Transcript;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Transcript>.Equals(Transcript other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Transcript transcript)
        {
            return this == transcript;
        }

        public static bool operator ==(Transcript a, Transcript b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            return a.BioType            == b.BioType            &&
                   a.CcdsId             == b.CcdsId             &&
                   a.CompDnaCodingEnd   == b.CompDnaCodingEnd   &&
                   a.CompDnaCodingStart == b.CompDnaCodingStart &&
                   a.DatabaseId         == b.DatabaseId         &&
                   a.End                == b.End                &&
                   a.Gene               == b.Gene               &&
                   a.GeneStableId       == b.GeneStableId       &&
                   a.IsCanonical        == b.IsCanonical        &&
                   a.OnReverseStrand    == b.OnReverseStrand    &&
                   a.ProteinId          == b.ProteinId          &&
                   a.ReferenceIndex     == b.ReferenceIndex     &&
                   a.RefSeqId           == b.RefSeqId           &&
                   a.Slice              == b.Slice              &&
                   a.StableId           == b.StableId           &&
                   a.Start              == b.Start;
        }

        public static bool operator !=(Transcript a, Transcript b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        private int GetCodingRegionStart()
        {
            // sanity check: make sure that translation is not null
            if (Translation == null) return -1;

            return Translation.StartExon.OnReverseStrand
                ? Translation.EndExon.End     - Translation.End   + 1
                : Translation.StartExon.Start + Translation.Start - 1;
        }

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        private int GetCodingRegionEnd()
        {
            // sanity check: make sure that translation is not null
            if (Translation == null) return -1;

            return Translation.StartExon.OnReverseStrand
                ? Translation.StartExon.End - Translation.Start + 1
                : Translation.EndExon.Start + Translation.End - 1;
        }

        /// <summary>
        /// returns the sum of the exon lengths
        /// </summary>
        private int GetTotalExonLength()
        {
            return TransExons.Sum(exon => exon.End - exon.Start + 1);
        }
        
        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            byte proteinVersion = Translation?.Version ?? 1;

            // write the IDs
            sb.AppendLine($"Transcript\t{StableId}\t{Version}\t{ProteinId}\t{proteinVersion}\t{Gene.StableId}\t{(byte)BioType}");

            // write the transcript info
            var canonical = IsCanonical ? 'Y' : 'N';
            var startExonPhase = Translation?.StartExon.Phase.ToString();
            sb.AppendLine($"{ReferenceIndex}\t{Start}\t{End}\t{GetCodingRegionStart()}\t{GetCodingRegionEnd()}\t{CompDnaCodingStart}\t{CompDnaCodingEnd}\t{GetTotalExonLength()}\t{canonical}\t{startExonPhase}\t{GeneIndex}");

            // write the internal indices
            sb.AppendLine($"{CdnaSeqIndex}\t{PeptideSeqIndex}\t{SiftIndex}\t{PolyPhenIndex}");

            DumpExons(sb);
            DumpIntrons(sb);
            DumpCdnaMaps(sb);
            DumpMicroRnas(sb);

            return sb.ToString();
        }

        private void DumpCdnaMaps(StringBuilder sb)
        {
            sb.AppendLine($"cDNA maps\t{FinalCdnaMaps.Length}");
            foreach (var cdnaMap in FinalCdnaMaps) sb.AppendLine(cdnaMap.ToString());
        }

        private void DumpExons(StringBuilder sb)
        {
            sb.AppendLine($"Exons\t{ExonIndices.Length}");
            foreach (var index in ExonIndices) sb.AppendLine(index.ToString());
        }

        private void DumpIntrons(StringBuilder sb)
        {
            if (IntronIndices == null)
            {
                sb.AppendLine("Introns\t0");
                return;
            }

            sb.AppendLine($"Introns\t{IntronIndices.Length}");
            foreach (var index in IntronIndices) sb.AppendLine(index.ToString());
        }

        private void DumpMicroRnas(StringBuilder sb)
        {
            if (MicroRnaIndices == null)
            {
                sb.AppendLine("miRNAs\t0");
                return;
            }

            sb.AppendLine($"miRNAs\t{MicroRnaIndices.Length}");
            foreach (var index in MicroRnaIndices) sb.AppendLine(index.ToString());
        }
    }
}
