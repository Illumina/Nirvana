using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public class CosmicItem: SupplementaryDataItem, ICosmic,IJsonSerializer, IEquatable<CosmicItem>
	{
		#region members
		public string ID { get; }
	    private string RefAllele { get; }
		public string AltAllele { get; }
		public string SaAltAllele { get; }
		public string Gene { get; }

	    public string IsAlleleSpecific { get; set; }
		IEnumerable<ICosmicStudy> ICosmic.Studies => Studies;

		public HashSet<CosmicStudy> Studies { get; internal set; }

		#endregion
		//constructors
		public CosmicItem(ExtendedBinaryReader reader)
		{
			if (reader == null) return;

			ID          = reader.ReadAsciiString();
			SaAltAllele = reader.ReadAsciiString();
			AltAllele   = SupplementaryAnnotation.ReverseSaReducedAllele(SaAltAllele);
			RefAllele   = reader.ReadAsciiString();
			Gene        = reader.ReadAsciiString();

			int countStudy = reader.ReadInt();
			if (countStudy>0) Studies=new HashSet<CosmicStudy>();

			for (int i = 0; i < countStudy; i++)
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
			HashSet<CosmicStudy> studies,
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

		}

		public class CosmicStudy : ICosmicStudy, IEquatable<CosmicStudy>,IJsonSerializer
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
				return ID.Equals(other.ID) &&
					   Histology.Equals(other.Histology) &&
					   PrimarySite.Equals(other.PrimarySite);
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
				writer.WriteAsciiString(ID);
				writer.WriteAsciiString(Histology);
				writer.WriteAsciiString(PrimarySite);
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

		public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryAnnotation sa, string refBases = null)
		{
			// check if the ref allele matches the refBases as a prefix
			if (!SupplementaryAnnotation.ValidateRefAllele(RefAllele, refBases))
			{
				return null; //the ref allele for this entry did not match the reference bases.
			}

			int newStart = Start;
			var newAlleles = SupplementaryAnnotation.GetReducedAlleles(RefAllele, AltAllele, ref newStart);

			var newRefAllele = newAlleles.Item1;
			var newAltAllele = newAlleles.Item2;

			if (newRefAllele != RefAllele)
			{
				return new CosmicItem(Chromosome, newStart, ID,newRefAllele,newAltAllele,Gene, Studies);
			}
			
			var newRefAlleleParsed = string.IsNullOrEmpty(newRefAllele) ? "-" : newRefAllele;

			sa.AddCosmic(new CosmicItem(Chromosome, Start, ID, newRefAlleleParsed, newAltAllele,Gene, Studies));

			return null;
		}

		public override SupplementaryInterval GetSupplementaryInterval()
		{
			throw new NotImplementedException();
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
			writer.WriteAsciiString(ID);
			writer.WriteAsciiString(SaAltAllele);
			writer.WriteAsciiString(RefAllele);
			writer.WriteAsciiString(Gene);

			if (Studies == null)
			{
				writer.WriteInt(0);
				return;
			}

			writer.WriteInt(Studies.Count);

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
			jsonObject.AddObjectValues("studies", Studies);
			sb.Append(JsonObject.CloseBrace);
		}

	}
}
