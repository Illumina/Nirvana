using System.Collections.Generic;
using VariantAnnotation.DataStructures.Variants;

namespace Piano
{
	public sealed class PianoAllele
	{
		#region  members

		public string VariantId { get; }
		public string VariantType { get; }
		public string ReferenceName { get; }
		public int? ReferenceBegin { get; }
		public int? ReferenceEnd { get; }
		public string RefAllele { get; }
		public string AltAllele { get; }
		public readonly string SaAltAllele;

		public List<Transcript> Transcripts { get; }

		#endregion

		public PianoAllele(VariantAlternateAllele altAllele)
		{
			VariantId      = altAllele.VariantId;
			VariantType    = altAllele.NirvanaVariantType.ToString();
			ReferenceBegin = altAllele.Start;
			ReferenceEnd   = altAllele.End;
			RefAllele      = altAllele.ReferenceAllele;
			AltAllele      = altAllele.AlternateAllele;
			SaAltAllele    = altAllele.SuppAltAllele;
			Transcripts    = new List<Transcript>();
		}

		public class Transcript { 
			#region members

			public string AminoAcids { get; set; }               // A/ASA
			public string CdsPosition { get; set; }              // 1504-1505
			public string ComplementaryDnaPosition { get; set; } // 1601-1602
			public string Gene { get; set; }                     // ENSESTG00000032903
			public string Hgnc { get; set; }                     // OR4F5
			public string IsCanonical { get; set; }              // true
			public string ProteinID { get; set; }                // NP_000029.2
			public string ProteinPosition { get; set; }          // 502
			public string TranscriptID { get; set; }             // ENSESTT00000083143
			public string BioType { get; set; }                 // proteinCoding	
			public IEnumerable<string> Consequence { get; set; }

			#endregion

			#region piano specific field

			public string UpStreamPeptides;
			public string DownStreamPeptides;

			#endregion


		}
	}
}