using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public sealed class CosmicItem: SupplementaryDataItem, ICosmic, IEquatable<CosmicItem>
	{
		#region members
		public string ID { get; }
	    private string RefAllele { get; }
		public string AltAllele { get; }
		public string SaAltAllele { get; }
		public string Gene { get; }
	    private int? SampleCount { get; }
	

	    public string IsAlleleSpecific { get; set; }
		IEnumerable<ICosmicStudy> ICosmic.Studies => Studies;

		public HashSet<CosmicStudy> Studies { get; private set; }

		#endregion
		//constructors
		public CosmicItem(ExtendedBinaryReader reader)
		{
			if (reader == null) return;

			ID          = reader.ReadAsciiString();
			SaAltAllele = reader.ReadAsciiString();
			AltAllele   = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(SaAltAllele);
			RefAllele   = reader.ReadAsciiString();
			Gene        = reader.ReadAsciiString();
			SampleCount = reader.ReadOptNullableInt32();

			var countStudy = reader.ReadOptInt32();
			if (countStudy>0) Studies=new HashSet<CosmicStudy>();

			for (var i = 0; i < countStudy; i++)
			{
				Studies.Add(new CosmicStudy(reader));
			}
		}

		public CosmicItem(
			string chromosome,
			int start,
			string id,
			string refAllele,
			string altAllele,
			string gene,
			HashSet<CosmicStudy> studies, int? sampleCount,
			string saAltAllele=null)
		{
			Chromosome  = chromosome;
			Start       = start;
			ID          = id;
			RefAllele   = refAllele;
			AltAllele   = altAllele;
			SaAltAllele = saAltAllele ?? altAllele;
			Gene        = gene;

			Studies = studies;
			SampleCount = sampleCount;

		}

		public sealed class CosmicStudy : ICosmicStudy, IEquatable<CosmicStudy>
		{
			#region members

			public string ID { get; }
			public string Histology { get; }
			public string PrimarySite { get; }
			#endregion

			public CosmicStudy(string studyId, string histology, string primarySite)
			{
				ID          = studyId;
				Histology   = histology;
				PrimarySite = primarySite;
			}

			public CosmicStudy(ExtendedBinaryReader reader)
			{
				ID          = reader.ReadAsciiString();
				Histology   = reader.ReadAsciiString();
				PrimarySite = reader.ReadAsciiString();
			}

			public bool Equals(CosmicStudy other)
			{
				return ID.Equals(other?.ID) &&
					   Histology.Equals(other?.Histology) &&
					   PrimarySite.Equals(other?.PrimarySite);
			}

		    public override int GetHashCode()
			{
				var hashCode= ID?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (Histology?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (PrimarySite?.GetHashCode() ?? 0);
				return hashCode;
			}

			public void Write(ExtendedBinaryWriter writer)
			{
				writer.WriteOptAscii(ID);
				writer.WriteOptAscii(Histology);
				writer.WriteOptAscii(PrimarySite);
			}

			public void SerializeJson(StringBuilder sb)
			{
				var jsonObject = new JsonObject(sb);

				sb.Append(JsonObject.OpenBrace);
				if (!string.IsNullOrEmpty(ID)) jsonObject.AddStringValue("id", ID, false);
				jsonObject.AddStringValue("histology", Histology?.Replace('_', ' '));
				jsonObject.AddStringValue("primarySite", PrimarySite?.Replace('_', ' '));
				sb.Append(JsonObject.CloseBrace);
			}
		}

		public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryPositionCreator saCreator, string refBases = null)
		{
			// check if the ref allele matches the refBases as a prefix
			if (!SupplementaryAnnotationUtilities.ValidateRefAllele(RefAllele, refBases))
			{
				return null; //the ref allele for this entry did not match the reference bases.
			}

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(Start, RefAllele, AltAllele);

			var newStart     = newAlleles.Item1;
			var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;

			if (newRefAllele != RefAllele)
			{
				return new CosmicItem(Chromosome, newStart, ID,newRefAllele,newAltAllele,Gene, Studies, SampleCount);
			}
			
			var newRefAlleleParsed = string.IsNullOrEmpty(newRefAllele) ? "-" : newRefAllele;
			var newItem = new CosmicItem(Chromosome, Start, ID, newRefAlleleParsed, newAltAllele, Gene, Studies,SampleCount);
			newItem.AddCosmicToSa(saCreator);

			return null;
		}

		public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
		{
			throw new NotImplementedException();
		}

		public void AddCosmicToSa(SupplementaryPositionCreator saCreator)
		{
			var sa = saCreator.SaPosition;
			var index = sa.CosmicItems.IndexOf(this);

			if (index == -1)
			{
				sa.CosmicItems.Add(this);
				return;
			}

			//get the existing cosmic item in dictionary
			if (sa.CosmicItems[index].Studies == null)
			{
				sa.CosmicItems[index].Studies = Studies;
				return;
			}

			foreach (var study in Studies)
			{
				sa.CosmicItems[index].Studies.Add(study);
			}

		}

		public bool Equals(CosmicItem otherItem)
		{
			// If parameter is null return false.
			if (otherItem == null) return false;

			// Return true if the fields match:
			return string.Equals(Chromosome, otherItem.Chromosome) &&
			       Start == otherItem.Start &&
			       string.Equals(ID, otherItem.ID) &&
			       string.Equals(RefAllele, otherItem.RefAllele) &&
			       string.Equals(AltAllele, otherItem.AltAllele) &&
			       string.Equals(Gene, otherItem.Gene) ;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Chromosome?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ Start;
				hashCode = (hashCode * 397) ^ (ID?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (RefAllele?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (AltAllele?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Gene?.GetHashCode() ?? 0);
				
				return hashCode;
			}
		}

	    public void Write(ExtendedBinaryWriter writer)
		{
			writer.WriteOptAscii(ID);
			writer.WriteOptAscii(SaAltAllele);
			writer.WriteOptAscii(RefAllele);
			writer.WriteOptAscii(Gene);
			writer.WriteOpt(SampleCount);

			if (Studies == null)
			{
				writer.WriteOpt(0);
				return;
			}

			writer.WriteOpt(Studies.Count);

			foreach (var study in Studies)
			{
				study.Write(writer);
			}
		}

		public void SerializeJson(StringBuilder sb)
		{
			var jsonObject = new JsonObject(sb);

			sb.Append(JsonObject.OpenBrace);
			jsonObject.AddStringValue("id", ID);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);
			jsonObject.AddStringValue("refAllele", RefAllele);
			jsonObject.AddStringValue("altAllele", AltAllele);
			jsonObject.AddStringValue("gene", Gene);
			jsonObject.AddIntValue("sampleCount",SampleCount);
			jsonObject.AddObjectValues("studies", Studies);
			sb.Append(JsonObject.CloseBrace);
		}

	}
}
