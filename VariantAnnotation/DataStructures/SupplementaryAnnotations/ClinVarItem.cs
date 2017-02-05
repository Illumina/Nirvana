using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public sealed class ClinVarItem: SupplementaryDataItem, IClinVar
	{
		#region members
		public string ID { get; }
        internal string ReferenceAllele { get;  }
		public string AltAllele { get; }
		public string SaAltAllele { get; }
		private readonly List<string> _alleleOrigins;
		private readonly List<string> _phenotypes;
		private readonly List<string> _medgenIds;
		private readonly List<string> _omimIds;
		private readonly List<string> _orphanetIds;
		private readonly List<long> _pubmedIds;

		public IEnumerable<string> AlleleOrigins => _alleleOrigins;
		public IEnumerable<string> Phenotypes => _phenotypes;
		public string Significance { get;  }
		private string ReviewStatusString { get; }
		public ReviewStatusEnum ReviewStatus { get;}
		public string IsAlleleSpecific { get; set; }
		public IEnumerable<string> MedGenIDs => _medgenIds;
		public IEnumerable<string> OmimIDs => _omimIds;
		public IEnumerable<string> OrphanetIDs => _orphanetIds;

		public IEnumerable<long> PubmedIds => _pubmedIds;
		public long LastUpdatedDate { get; }

	    private readonly int _hashCode;
		
		private static readonly Dictionary<string, ReviewStatusEnum> ReviewStatusNameMapping = new Dictionary<string, ReviewStatusEnum>
		{
			["no_assertion"] = ReviewStatusEnum.no_assertion,
			["no_criteria"] = ReviewStatusEnum.no_criteria,
			["guideline"] = ReviewStatusEnum.practice_guideline,
			["single"] = ReviewStatusEnum.single_submitter,
			["mult"] = ReviewStatusEnum.multiple_submitters,
			["conf"] = ReviewStatusEnum.conflicting_interpretations,
			["exp"] = ReviewStatusEnum.expert_panel,
			//the following are the long forms found in XML
			["no assertion provided"] = ReviewStatusEnum.no_assertion,
			["no assertion criteria provided"] = ReviewStatusEnum.no_criteria,
			["practice guideline"] = ReviewStatusEnum.practice_guideline,
			["criteria provided, conflicting interpretations"] = ReviewStatusEnum.conflicting_interpretations,
			["reviewed by expert panel"] = ReviewStatusEnum.expert_panel,
			["classified by multiple submitters"] = ReviewStatusEnum.multiple_submitters,
			["criteria provided, multiple submitters, no conflicts"] = ReviewStatusEnum.multiple_submitters_no_conflict,
			["criteria provided, single submitter"] = ReviewStatusEnum.single_submitter
		};

		private static readonly Dictionary< ReviewStatusEnum, string> ReviewStatusStrings = new Dictionary<ReviewStatusEnum, string>
		{
			
			[ReviewStatusEnum.no_criteria]                     = "no assertion criteria provided",
			[ReviewStatusEnum.no_assertion]                    = "no assertion provided",
			[ReviewStatusEnum.expert_panel]                    = "reviewed by expert panel",
			[ReviewStatusEnum.single_submitter]                = "criteria provided, single submitter",
			[ReviewStatusEnum.practice_guideline]              = "practice guideline",
			[ReviewStatusEnum.multiple_submitters]             = "classified by multiple submitters",
			[ReviewStatusEnum.conflicting_interpretations]     = "criteria provided, conflicting interpretations",
			[ReviewStatusEnum.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts",
			
		};
		#endregion
		#region Equality Overrides

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override bool Equals(object o)
		{
			// If parameter cannot be cast to ClinVarItem return false:
			var other = o as ClinVarItem;
			if ((object)other == null) return false;

			// Return true if the fields match:
			return this == other;
		}

	    public static bool operator ==(ClinVarItem a, ClinVarItem b)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(a, b)) return true;

			// If one is null, but not both, return false.
			if ((object)a == null || (object)b == null) return false;


		    return a.Start == b.Start &&
		           a.Chromosome == b.Chromosome &&
		           a.ID == b.ID &&
		           a.AltAllele == b.AltAllele &&
		           a.ReferenceAllele == b.ReferenceAllele &&
		           a.Significance == b.Significance &&
		           a.LastUpdatedDate == b.LastUpdatedDate &&
		           StringSequenceEqual(a.AlleleOrigins, b.AlleleOrigins) &&
		           StringSequenceEqual(a.OmimIDs, b.OmimIDs) &&
		           StringSequenceEqual(a.OrphanetIDs, b.OrphanetIDs) &&
		           StringSequenceEqual(a.Phenotypes, b.Phenotypes) &&
		           StringSequenceEqual(a.MedGenIDs, b.MedGenIDs);

		}

		private static bool StringSequenceEqual(IEnumerable<string> a, IEnumerable<string> b)
		{
			if (a == null && b == null) return true;

			
			if (a == null || b ==null) return false;

			return a.SequenceEqual(b);

		}

		public static bool operator !=(ClinVarItem a, ClinVarItem b)
		{
			return !(a == b);
		}

		#endregion

		public ClinVarItem(string chromosome,
			int position,
			List<string> alleleOrigins,
			string altAllele,
			string id,
			string reviewStatusString,
			List<string> medGenIds,
			List<string> omimIds,
			List<string> orphanetIds,
			List<string> phenotypes,
			string referenceAllele,
			string significance,
			List<long> pubmedIds = null,
			long lastUpdatedDate = long.MinValue
			)
		{
			Chromosome         = chromosome;
			Start              = position;
			_alleleOrigins     = alleleOrigins?.Count==0? null: alleleOrigins;
			AltAllele          = altAllele;
			SaAltAllele        = altAllele;
			ID                 = id;
			ReviewStatusString = reviewStatusString;
			_medgenIds         = medGenIds?.Count == 0 ? null : medGenIds ;
			_omimIds           = omimIds?.Count == 0 ? null : omimIds;
			_orphanetIds       = orphanetIds?.Count == 0 ? null : orphanetIds;
			_phenotypes        = phenotypes?.Count == 0 ? null : phenotypes; 
			ReferenceAllele    = referenceAllele;
			Significance       = significance;
			_pubmedIds         = pubmedIds?.Count == 0 ? null : pubmedIds;
			LastUpdatedDate    = lastUpdatedDate;
			IsAlleleSpecific   = null;

			if (reviewStatusString != null)
				if (ReviewStatusNameMapping.ContainsKey(reviewStatusString))
					ReviewStatus = ReviewStatusNameMapping[reviewStatusString];

			_hashCode = CalculateHashCode();
		}

		public ClinVarItem(ExtendedBinaryReader reader)
		{
		    _alleleOrigins = reader.ReadOptArray(reader.ReadAsciiString)?.ToList();
			SaAltAllele      = reader.ReadAsciiString();
			AltAllele        = SaAltAllele != null ? SupplementaryAnnotationUtilities.ReverseSaReducedAllele(SaAltAllele) : ""; // A
			ReferenceAllele  = reader.ReadAsciiString();
			ID               = reader.ReadAsciiString();
			ReviewStatus     = (ReviewStatusEnum)reader.ReadByte();
			IsAlleleSpecific = reader.ReadAsciiString();
			_medgenIds       = reader.ReadOptArray(reader.ReadAsciiString)?.ToList();
            _omimIds         = reader.ReadOptArray(reader.ReadAsciiString)?.ToList();
            _orphanetIds     = reader.ReadOptArray(reader.ReadAsciiString)?.ToList();
            _phenotypes      = reader.ReadOptArray(reader.ReadUtf8String)?.ToList();
            Significance     = reader.ReadAsciiString();
			LastUpdatedDate  = reader.ReadOptInt64();
			_pubmedIds       = reader.ReadOptArray(reader.ReadOptInt64)?.ToList();
        }

		/// <summary>
		/// calculates the hash code for this object
		/// </summary>
		// ReSharper disable once FunctionComplexityOverflow
		private int CalculateHashCode()
		{
			var hashCode = Start.GetHashCode();
			if (Chromosome != null) hashCode ^= Chromosome.GetHashCode();
			if (ID != null) hashCode ^= ID.GetHashCode();
			if (AlleleOrigins != null) hashCode ^= AlleleOrigins.GetHashCode();
			if (AltAllele != null) hashCode ^= AltAllele.GetHashCode();
			if (MedGenIDs != null) hashCode ^= MedGenIDs.GetHashCode();
			if (OmimIDs != null) hashCode ^= OmimIDs.GetHashCode();
			if (OrphanetIDs != null) hashCode ^= OrphanetIDs.GetHashCode();
			if (Phenotypes != null) hashCode ^= Phenotypes.GetHashCode();
			if (ReferenceAllele != null) hashCode ^= ReferenceAllele.GetHashCode();
			if (Significance != null) hashCode ^= Significance.GetHashCode();
			return hashCode;
		}

		/// <summary>
		/// Adds the ClinVar items in this object to the supplementary annotation object
		/// </summary>
		public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryPositionCreator saCreator, string refBases = null)
		{
			// check if the ref allele matches the refBases as a prefix
			if (!SupplementaryAnnotationUtilities.ValidateRefAllele(ReferenceAllele, refBases))
			{
				return null; //the ref allele for this entry did not match the reference bases.
			}

			// for insertions and deletions, the alternate allele has to be modified to conform with VEP convension
			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(Start, ReferenceAllele, AltAllele);

			var newStart = newAlleles.Item1;
			var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;

			if (newRefAllele != ReferenceAllele )
			{
				var additionalItem = new ClinVarItem(Chromosome, newStart, _alleleOrigins, newAltAllele, ID, ReviewStatusString, _medgenIds, _omimIds, _orphanetIds, _phenotypes, newRefAllele, Significance, _pubmedIds, LastUpdatedDate);

				return additionalItem;
			}

			saCreator.SaPosition.ClinVarItems.Add(new ClinVarItem(Chromosome, newStart, _alleleOrigins, newAltAllele, ID, ReviewStatusString, _medgenIds, _omimIds, _orphanetIds, _phenotypes, newRefAllele, Significance, _pubmedIds, LastUpdatedDate));

			return null;
		}

		public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// returns a string representation of this object
		/// </summary>
		public override string ToString()
		{
			return string.Join("\t",
				Chromosome,
				Start,
				ID,
				ReviewStatusString,
				ReferenceAllele,
				AltAllele,
				_alleleOrigins,
				_medgenIds,
				OmimIDs,
				OrphanetIDs,
				Phenotypes,
				Significance
				);
		}

		public void Write(ExtendedBinaryWriter writer)
		{
			writer.WriteOptArray(_alleleOrigins?.Distinct().ToArray(), writer.WriteOptAscii);
			writer.WriteOptAscii(SaAltAllele);
			writer.WriteOptAscii(ReferenceAllele);
			writer.WriteOptAscii(ID);
			writer.Write((byte)ReviewStatus);
			writer.WriteOptAscii(IsAlleleSpecific);
			writer.WriteOptArray(_medgenIds?.Distinct().ToArray(), writer.WriteOptAscii);
			writer.WriteOptArray(_omimIds?.Distinct().ToArray(), writer.WriteOptAscii);
			writer.WriteOptArray(_orphanetIds?.Distinct().ToArray(), writer.WriteOptAscii);

			writer.WriteOptArray(SupplementaryAnnotationUtilities.ConvertMixedFormatStrings(_phenotypes)?.Distinct().ToArray(), writer.WriteOptUtf8);
			writer.WriteOptAscii(Significance);

			writer.WriteOpt(LastUpdatedDate);
			writer.WriteOptArray(_pubmedIds.ToArray(), writer.WriteOpt);
		}

	    public void SerializeJson(StringBuilder sb)
		{
			var jsonObject = new JsonObject(sb);

			//converting empty alleles to '-'
			var refAllele = string.IsNullOrEmpty(ReferenceAllele)? "-":ReferenceAllele;
			var altAllele = string.IsNullOrEmpty(AltAllele) ? "-" : AltAllele;
			

			sb.Append(JsonObject.OpenBrace);
			jsonObject.AddStringValue("id", ID);
			jsonObject.AddStringValue("reviewStatus", ReviewStatusStrings[ReviewStatus]);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);
			jsonObject.AddStringValues("alleleOrigins", _alleleOrigins);
			jsonObject.AddStringValue("refAllele", "N" == refAllele ? null : refAllele);
			jsonObject.AddStringValue("altAllele", "N" == altAllele ? null : altAllele);
			jsonObject.AddStringValues("phenotypes", Phenotypes);
			jsonObject.AddStringValues("medGenIDs", _medgenIds);
			jsonObject.AddStringValues("omimIDs", OmimIDs);
			jsonObject.AddStringValues("orphanetIDs", OrphanetIDs);
			jsonObject.AddStringValue("significance", Significance);

			if (LastUpdatedDate != long.MinValue)
				jsonObject.AddStringValue("lastUpdatedDate", new DateTime(LastUpdatedDate).ToString("yyyy-MM-dd"));

			jsonObject.AddStringValues("pubMedIds", PubmedIds?.Select(id => id.ToString()));
			sb.Append(JsonObject.CloseBrace);
		}
	}
}
