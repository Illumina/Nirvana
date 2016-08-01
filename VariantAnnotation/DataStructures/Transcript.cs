using System;
using System.Collections.Generic;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures
{
    public class Transcript : AnnotationInterval, IEquatable<Transcript>
    {
        #region members

        public readonly Exon[] Exons;
        public readonly Exon StartExon;
        public readonly int TotalExonLength;

        public readonly Intron[] Introns;
        public readonly MicroRna[] MicroRnas;

        public readonly CdnaCoordinateMap[] CdnaCoordinateMaps;

        public readonly TranscriptDataSource TranscriptDataSource;
        public readonly BioType BioType;
        public readonly byte CodingVersion;
        public readonly byte ProteinVersion;

        public readonly string Peptide;
        public readonly string TranslateableSeq;

        public readonly bool OnReverseStrand;
        public readonly bool IsCanonical;

        public readonly bool HasTranslation; // not serialized
 
        public readonly int CodingRegionStart;
        public readonly int CodingRegionEnd;

        // the start and end position of the coding region of the exon in cDNA coordinates. (contains UTRs)
        public readonly int CompDnaCodingStart;
        public readonly int CompDnaCodingEnd;

        private readonly string _ccdsId;
        public readonly string ProteinId;
        public readonly string GeneStableId;
        public readonly string StableId;

        public readonly string GeneSymbol;
        public readonly GeneSymbolSource GeneSymbolSource;
        private readonly string _hgncId;
	    public readonly int GeneStart;
	    public readonly int GeneEnd;

        public readonly Sift Sift;
        public readonly PolyPhen PolyPhen;
        
        private readonly int _hashCode;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        private Transcript(Exon[] exons, Exon startExon, int totalExonLength, Intron[] introns,
            CdnaCoordinateMap[] cdnaMaps, string peptide, string translateableSeq, bool onReverseStrand,
			bool isCanonical, int codingRegionStart, int codingRegionEnd, int compDnaCodingStart,
			int compDnaCodingEnd, int start, int end, string ccdsId, string proteinId, string geneStableId,
			string stableId, string geneSymbol, GeneSymbolSource geneSymbolSource, string hgncId,
			byte codingVersion, byte proteinVersion, BioType biotype, TranscriptDataSource transcriptDataSource, MicroRna[] microRnas, Sift sift,
			PolyPhen polyPhen, int geneStart, int geneEnd)
			: base(start, end)
        {
            BioType              = biotype;
            _ccdsId               = ccdsId;
            CdnaCoordinateMaps   = cdnaMaps;
            CodingRegionEnd      = codingRegionEnd;
            CodingRegionStart    = codingRegionStart;
            CodingVersion        = codingVersion;
            CompDnaCodingEnd     = compDnaCodingEnd;
            CompDnaCodingStart   = compDnaCodingStart;
            Exons                = exons;
            GeneStableId         = geneStableId;
            GeneSymbol           = geneSymbol;
            GeneSymbolSource     = geneSymbolSource;
            _hgncId               = hgncId;
            Introns              = introns;
            IsCanonical          = isCanonical;
            MicroRnas            = microRnas;
            OnReverseStrand      = onReverseStrand;
            Peptide              = peptide;
            ProteinId            = proteinId;
            ProteinVersion       = proteinVersion;
            StableId             = stableId;
            StartExon            = startExon;
            TotalExonLength      = totalExonLength;
            TranslateableSeq     = translateableSeq;
            Sift                 = sift;
            PolyPhen             = polyPhen;
            TranscriptDataSource = transcriptDataSource;
	        GeneStart            = geneStart;
	        GeneEnd              = geneEnd;

            HasTranslation = StartExon != null;

            _hashCode = Start.GetHashCode()              ^
                        End.GetHashCode()                ^
                        CodingRegionStart.GetHashCode()  ^
                        CodingRegionEnd.GetHashCode()    ^
                        CompDnaCodingStart.GetHashCode() ^
                        CompDnaCodingEnd.GetHashCode()   ^
                        IsCanonical.GetHashCode()        ^
                        OnReverseStrand.GetHashCode();

            if (Peptide          != null) _hashCode ^= Peptide.GetHashCode();
            if (TranslateableSeq != null) _hashCode ^= TranslateableSeq.GetHashCode();
            if (_ccdsId           != null) _hashCode ^= _ccdsId.GetHashCode();
            if (ProteinId        != null) _hashCode ^= ProteinId.GetHashCode();
            if (GeneStableId     != null) _hashCode ^= GeneStableId.GetHashCode();
            if (StableId         != null) _hashCode ^= StableId.GetHashCode();
            if (_hgncId           != null) _hashCode ^= _hgncId.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Exon return false:
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
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.Start              == b.Start)              &&
                   (a.End                == b.End)                &&
                   (a.CodingRegionStart  == b.CodingRegionStart)  &&
                   (a.CodingRegionEnd    == b.CodingRegionEnd)    &&
                   (a.CompDnaCodingStart == b.CompDnaCodingStart) &&
                   (a.CompDnaCodingEnd   == b.CompDnaCodingEnd)   &&
                   (a.OnReverseStrand    == b.OnReverseStrand)    &&
                   (a.StableId           == b.StableId);
        }

        public static bool operator !=(Transcript a, Transcript b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// calculates the cDNA coordinates given the specified genomic coordinates [Transcript.pm:927 cdna_coding_start]
        /// genomic2pep [TransciptMapper:482]
        /// </summary>
        public void GetCodingDnaEndpoints(int genomicBegin, int genomicEnd, out int cdnaBegin, out int cdnaEnd)
        {
            // find an overlapping mapper pair
            CdnaCoordinateMap coordinateMap = null;
            bool foundOverlap = false;

            // replace this with interval tree logic
            foreach (CdnaCoordinateMap cdnaMap in CdnaCoordinateMaps)
            {
                coordinateMap = cdnaMap;

                if ((genomicEnd >= coordinateMap.Genomic.Start) &&
                    (genomicBegin <= coordinateMap.Genomic.End))
                {
                    foundOverlap = true;
                    break;
                }
            }

            if (!foundOverlap)
            {
                throw new GeneralException($"Unable to find an overlapping mapping pair for these genomic coordinates: ({genomicBegin}, {genomicEnd})");
            }

            // calculate the cDNA position
            cdnaBegin = coordinateMap.CodingDna.End - (genomicEnd - coordinateMap.Genomic.Start);
            cdnaEnd   = coordinateMap.CodingDna.End - (genomicBegin - coordinateMap.Genomic.Start);
        }

        /// <summary>
        /// sets both the exon and intron number strings according to which were affected by the variant [BaseTranscriptVariation.pm:474 _exon_intron_number]
        /// </summary>
        public void ExonIntronNumber(TranscriptAnnotation ta, out string exonNumber, out string intronNumber)
        {
            int exonCount = 0;

            var altAllele = ta.AlternateAllele;
            var variantInterval = new AnnotationInterval(altAllele.ReferenceBegin, altAllele.ReferenceEnd);

            var overlappedExons = new List<int>();
            var overlappedIntrons = new List<int>();

            Exon prevExon = null;

            foreach (var exon in Exons)
            {
                exonCount++;

                if (variantInterval.Overlaps(exon.Start, exon.End)) overlappedExons.Add(exonCount);

                if (prevExon != null)
                {
                    int intronStart = prevExon.End + 1;
                    int intronEnd = exon.Start - 1;

                    if (variantInterval.Overlaps(intronStart, intronEnd)) overlappedIntrons.Add(exonCount - 1);
                }

                prevExon = exon;
            }

            exonNumber = GetExonIntronNumber(overlappedExons, Exons.Length);
            intronNumber = Introns != null ? GetExonIntronNumber(overlappedIntrons, Introns.Length): null;

            if (overlappedExons.Count > 0) ta.HasExonOverlap = true;
        }

        private string GetExonIntronNumber(List<int> overlappedItems, int totalItems)
        {
            // sanity check: make sure we have some overlapped items
            if (overlappedItems.Count == 0) return null;

            int firstItem = overlappedItems[0];
            if (OnReverseStrand) firstItem = totalItems - firstItem + 1;

            // handle one item
            if (overlappedItems.Count == 1) return firstItem + "/" + totalItems;

            // handle multiple items
            int lastItem = overlappedItems[overlappedItems.Count - 1];

            if (OnReverseStrand)
            {
                lastItem = totalItems - lastItem + 1;
                Swap.Int(ref firstItem, ref lastItem);
            }

            return firstItem + "-" + lastItem + "/" + totalItems;
        }

        /// <summary>
        /// Retrieves all Exon sequences and concats them together. 
        /// This includes 5' UTR + cDNA + 3' UTR [Transcript.pm:862 spliced_seq]
        /// </summary>
        private string GetSplicedSequence()
        {
            var sb = new StringBuilder();
            var compressedSequence = AnnotationLoader.Instance.CompressedSequence;

            foreach (var exon in Exons)
            {
                var exonLength = exon.End - exon.Start + 1;

                // sanity check: handle the situation where no reference has been provided
                if (compressedSequence == null)
                {
                    sb.Append(new string('N', exonLength));
                    continue;
                }

                sb.Append(compressedSequence.Substring(exon.Start - 1, exonLength));
            }

            return OnReverseStrand ? SequenceUtilities.GetReverseComplement(sb.ToString()) : sb.ToString();
        }

        /// <summary>
        /// returns the alternate CDS given the reference sequence, the cds coordinates, and the alternate allele.
        /// </summary>
        public string GetAlternateCds(int cdsBegin, int cdsEnd, string alternateAllele)
        {
            var splicedSeq     = GetSplicedSequence();
            int numPaddedBases = StartExon.Phase > 0 ? StartExon.Phase : 0;

            int shift            = CompDnaCodingStart - 1;
            string upstreamSeq   = splicedSeq.Substring(shift, cdsBegin - numPaddedBases - 1);
            string downstreamSeq = splicedSeq.Substring(cdsEnd - numPaddedBases + shift);

            if (alternateAllele == null) alternateAllele = string.Empty;
            var paddedBases = numPaddedBases > 0 ? new string('N', numPaddedBases) : "";

            return paddedBases + upstreamSeq + alternateAllele + downstreamSeq;
        }

        /// <summary>
        /// reads the transcript from the binary reader
        /// </summary>
        public static Transcript Read(ExtendedBinaryReader reader, List<CdnaCoordinateMap> coordinateMapCache,
            List<Exon> exonCache, List<Intron> intronCache, List<MicroRna> microRnaCache,
            List<Sift> siftCache, List<PolyPhen> polyPhenCache)
        {
            CdnaCoordinateMap[] cdnaMaps;
            Exon[] exons;
            Intron[] introns;
            MicroRna[] microRnas;

            ReadIndices(reader, out cdnaMaps, coordinateMapCache);
            ReadIndices(reader, out exons, exonCache);
            ReadIndices(reader, out introns, intronCache);
            ReadIndices(reader, out microRnas, microRnaCache);

            // read the start exon index
            var startExonIndex = reader.ReadInt();
            var startExon      = startExonIndex >= 0 ? exonCache[startExonIndex] : null;

            // read the Sift index
            var siftIndex = reader.ReadInt();
            var sift      = siftIndex >= 0 ? siftCache[siftIndex] : null;

            // read the PolyPhen index
            var polyPhenIndex = reader.ReadInt();
            var polyPhen      = polyPhenIndex >= 0 ? polyPhenCache[polyPhenIndex] : null;

            // read the total exon length
            var totalExonLength = reader.ReadInt();

            int start = reader.ReadInt();
            int end   = reader.ReadInt();

            // read the peptide and sequence strings
            string peptide          = reader.ReadAsciiString();
            string translateableSeq = reader.ReadAsciiString();

            bool onReverseStrand = reader.ReadBoolean();
            bool isCanonical     = reader.ReadBoolean();

            int codingRegionStart = reader.ReadInt();
            int codingRegionEnd   = reader.ReadInt();

            var compDnaCodingStart = reader.ReadInt();
            var compDnaCodingEnd   = reader.ReadInt();

			int geneStart = reader.ReadInt();
			int geneEnd = reader.ReadInt();

			byte codingVersion       = reader.ReadByte();
            byte proteinVersion      = reader.ReadByte();
            var biotype              = (BioType)reader.ReadByte();
            var transcriptDataSource = (TranscriptDataSource)reader.ReadByte();

            string ccdsId       = reader.ReadAsciiString();
            string proteinId    = reader.ReadAsciiString();
            string geneStableId = reader.ReadAsciiString();
            string stableId     = reader.ReadAsciiString();

            string geneSymbol    = reader.ReadAsciiString();
            string hgncId        = reader.ReadAsciiString();
            var geneSymbolSource = (GeneSymbolSource)reader.ReadByte();


            return new Transcript(exons, startExon, totalExonLength, introns, cdnaMaps, peptide,
                translateableSeq, onReverseStrand, isCanonical, codingRegionStart, codingRegionEnd, 
                compDnaCodingStart, compDnaCodingEnd, start, end, ccdsId, proteinId, geneStableId,
                stableId, geneSymbol, geneSymbolSource, hgncId, codingVersion, proteinVersion, 
                biotype, transcriptDataSource, microRnas, sift, polyPhen,geneStart,geneEnd);
        }

        /// <summary>
        /// given an array of items, this method looks up each item in an index dictionary and then outputs the index for each item
        /// </summary>
        private static void ReadIndices<T>(ExtendedBinaryReader reader, out T[] items, List<T> itemCache)
        {
            int numItems = reader.ReadInt();

            if (numItems != 0)
            {
                items = new T[numItems];

                for (int i = 0; i < numItems; i++)
                {
                    int itemIndex = reader.ReadInt();
                    items[i] = itemCache[itemIndex];
                }
            }
            else
            {
                items = null;
            }
        }

        /// <summary>
        /// returns a string representation of our transcript
        /// </summary>
        public override string ToString()
        {
            return $"transcript: {Start} - {End}. {StableId} ({(OnReverseStrand ? "R" : "F")})";
        }

        /// <summary>
        /// writes the transcript to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer, Dictionary<CdnaCoordinateMap, int> cdnaMapIndices, Dictionary<Exon, int> exonIndices,
            Dictionary<Intron, int> intronIndices, Dictionary<MicroRna, int> microRnaIndices,
            Dictionary<Sift, int> siftIndices, Dictionary<PolyPhen, int> polyPhenIndices)
        {
            WriteIndices(writer, CdnaCoordinateMaps, cdnaMapIndices, "cDNA map");
            WriteIndices(writer, Exons, exonIndices, "exon");
            WriteIndices(writer, Introns, intronIndices, "intron");
            WriteIndices(writer, MicroRnas, microRnaIndices, "miRNA");

            // write the start exon index
            bool hasStartExon  = StartExon != null;
            int startExonIndex = -1;

            if (hasStartExon)
            {
                if (!exonIndices.TryGetValue(StartExon, out startExonIndex))
                {
                    throw new GeneralException($"Unable to locate the start exon in the exon indices: {StartExon}");
                }
            }

            writer.WriteInt(startExonIndex);

            // write the Sift index
            bool hasSift = Sift != null;
            int siftIndex = -1;

            if (hasSift)
            {
                if (!siftIndices.TryGetValue(Sift, out siftIndex))
                {
                    throw new GeneralException($"Unable to locate the Sift object in the Sift indices: {Sift}");
                }
            }

            writer.WriteInt(siftIndex);

            // write the PolyPhen index
            bool hasPolyPhen = PolyPhen != null;
            int polyPhenIndex = -1;

            if (hasPolyPhen)
            {
                if (!polyPhenIndices.TryGetValue(PolyPhen, out polyPhenIndex))
                {
                    throw new GeneralException(
                        $"Unable to locate the PolyPhen object in the PolyPhen indices: {PolyPhen}");
                }
            }

            writer.WriteInt(polyPhenIndex);

            // write the total exon length 
            writer.WriteInt(TotalExonLength);
            writer.WriteInt(Start);
            writer.WriteInt(End);

            // write the peptide and sequence strings
            writer.WriteAsciiString(Peptide);
            writer.WriteAsciiString(TranslateableSeq);

            writer.WriteBoolean(OnReverseStrand);
            writer.WriteBoolean(IsCanonical);

            writer.WriteInt(CodingRegionStart);
            writer.WriteInt(CodingRegionEnd);

            writer.WriteInt(CompDnaCodingStart);
            writer.WriteInt(CompDnaCodingEnd);

			writer.WriteInt(GeneStart);
			writer.WriteInt(GeneEnd);

            writer.WriteByte(CodingVersion);
            writer.WriteByte(ProteinVersion);
            writer.WriteByte((byte)BioType);
            writer.WriteByte((byte)TranscriptDataSource);

            writer.WriteAsciiString(_ccdsId);
            writer.WriteAsciiString(ProteinId);
            writer.WriteAsciiString(GeneStableId);
            writer.WriteAsciiString(StableId);

            writer.WriteAsciiString(GeneSymbol);
            writer.WriteAsciiString(_hgncId);
            writer.WriteByte((byte)GeneSymbolSource);

        }

        /// <summary>
        /// given an array of items, this method looks up each item in an index dictionary and then outputs the index for each item
        /// </summary>
        private static void WriteIndices<T>(ExtendedBinaryWriter writer, T[] items, Dictionary<T, int> indices, string description)
        {
            if (items != null)
            {
                writer.WriteInt(items.Length);

                foreach (var item in items)
                {
                    int itemIndex;
                    if (!indices.TryGetValue(item, out itemIndex))
                    {
                        throw new GeneralException(string.Format("Unable to locate the {0} in the {0} indices: {1}", description, item));
                    }

                    writer.WriteInt(itemIndex);
                }
            }
            else
            {
                writer.WriteInt(0);
            }
        }
    }
}
