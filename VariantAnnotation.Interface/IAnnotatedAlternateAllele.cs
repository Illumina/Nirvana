using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VariantAnnotation.Interface
{
    public interface IAnnotatedAlternateAllele
    {
        // permanent records
        string VariantId { get; }
        string VariantType { get; }

        string ReferenceName { get; }
        int? ReferenceBegin { get; }
        int? ReferenceEnd { get; }
        string RefAllele { get; }
        string AltAllele { get; }

        bool IsReferenceMinor { get; }
        bool IsIntergenic { get; }
        bool IsReference { get; }
        bool IsReferenceNoCall { get; }
        int GenotypeIndex { get; }
        string SaAltAllele { get; }
        bool IsRecomposedVariant { get; }

        // extensible records

        string PhylopScore { get; } // 11.612

        string AncestralAllele { get; set; }




        // -----------
        // transcripts
        // -----------

		IList<IAnnotatedTranscript> RefSeqTranscripts { get; set; }
        IList<IAnnotatedTranscript> EnsemblTranscripts { get; set; }
	    ISet<IRegulatoryRegion> RegulatoryRegions { get; }

        // -----------------
        // overlapping genes
        // -----------------
        ISet<string> OverlappingGenes { get; }

		// -------------------------
		// Supplementary annotations
		// -------------------------
		IList<IAnnotatedSA> SuppAnnotations { get; set; }


        //-------------------------------
        // Overlapping transcripts for SV
        //-------------------------------
        IList<IOverlapTranscript> SvOverlappingTranscripts { get; }
    }


    public interface ISupplementaryInterval
    {
        int Start { get; }
        int End { get; }
        string ReferenceName { get; }
        string AlternateAllele { get; }
        VariantType VariantType { get; }
        string Source { get; }
        IReadOnlyDictionary<string, string> StringValues { get; }

        IReadOnlyDictionary<string, int> IntValues { get; }
        IEnumerable<string> BoolValues { get; }

        IReadOnlyDictionary<string, double> DoubleValues { get; }
        IReadOnlyDictionary<string, double> PopulationFrequencies { get; }
        IReadOnlyDictionary<string, IEnumerable<string>> StringLists { get; }

        string GetJsonString();
        double OverlapFraction(int begin, int end);
    }

	public interface IInterimInterval:IInterval
	{
		string KeyName { get; }
		string ReferenceName { get; }
		string JsonString { get; }
		ReportFor ReportingFor { get; }
		void Write(BinaryWriter writer);


	}

	public enum ReportFor
	{
		None,
		AllVariants,
		SmallVariants,
		StructuralVariants

	}



	public interface IAnnotatedTranscript
    {
        string AminoAcids { get; }
        string CdsPosition { get; }
        string Codons { get; }
        string ComplementaryDnaPosition { get; }
        IEnumerable<string> Consequence { get; }
        string Exons { get; }
        string Introns { get; }
        string Gene { get; }
        string Hgnc { get; }
        string HgvsCodingSequenceName { get; }
        string HgvsProteinSequenceName { get; }
        string GeneFusion { get; set; }
        string IsCanonical { get; }
        string PolyPhenPrediction { get; }
        string PolyPhenScore { get; }
        string ProteinID { get; }
        string ProteinPosition { get; }
        string SiftPrediction { get; }
        string SiftScore { get; }
        string TranscriptID { get; }
		string BioType { get; }
	    Dictionary<string, string> AdditionalInfo { get; set; }

    }

    public interface IRegulatoryRegion : IJsonSerializer
    {
        string ID { get; }
        string Type { get; }
        IEnumerable<string> Consequence { get; }
    }

    public interface IOverlapTranscript : IJsonSerializer, IEquatable<IOverlapTranscript>
    {
        string TranscriptID { get; }
        string IsCanonical { get; }
        string Hgnc { get; }
        string IsPartialOverlap { get; }
    }

    public interface IJsonSerializer
    {
        void SerializeJson(StringBuilder sb);
    }

	public interface IAnnotatedSA
	{
		bool? IsAlleleSpecific { get; }

		string KeyName { get; }
		string VcfKeyName { get; }
		IList<string> GetStrings(string format);
		bool IsArray { get; }

	}
}
