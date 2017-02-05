using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class CustomItem : SupplementaryDataItem, ICustomAnnotation, ICustomItem
    {
		public string Id { get; }
		public string AnnotationType { get; }
        private string RefAllele { get; }
		public string AltAllele { get; }
		public string SaAltAllele { get; }
		public bool IsPositional { get; }
		public string IsAlleleSpecific { get; set; }
		public Dictionary<string, string> StringFields { get; }
		public List<string> BooleanFields { get; }

		IDictionary<string, string> ICustomAnnotation.StringFields => StringFields;
		IEnumerable<string> ICustomAnnotation.BooleanFields => BooleanFields;

        public CustomItem(string chromosome, int start, string refAllele, string altAllele, string annotationType, string id, bool isPositional, Dictionary<string, string> stringFields, List<string> boolFields, string isAlleleSpecific=null)
        {
            Chromosome     = chromosome;
            Start          = start;
            RefAllele      = refAllele;
            AltAllele      = altAllele;
	        SaAltAllele    = altAllele;
			AnnotationType = annotationType;
            Id             = id;
            IsPositional   = isPositional;
            StringFields   = stringFields;
            BooleanFields  = boolFields;
	        IsAlleleSpecific = isAlleleSpecific;
        }
		public CustomItem(ExtendedBinaryReader reader)
		{
			Id             = reader.ReadAsciiString();
			AnnotationType = reader.ReadAsciiString();
			SaAltAllele    = reader.ReadAsciiString();
			AltAllele      = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(SaAltAllele);
			IsPositional   = reader.ReadBoolean();

			var numStringFields = reader.ReadOptInt32();
			if (numStringFields > 0)
			{
				StringFields = new Dictionary<string, string>();

				for (var i = 0; i < numStringFields; i++)
				{
					var key = reader.ReadAsciiString();
					var value = reader.ReadAsciiString();
					StringFields[key] = value;
				}
			}
			else StringFields = null;

			var numBoolFields = reader.ReadOptInt32();
			if (numBoolFields > 0)
			{
				BooleanFields = new List<string>();

				for (var i = 0; i < numBoolFields; i++)
				{
					BooleanFields.Add(reader.ReadAsciiString());
				}
			}
			else BooleanFields = null;

		}


		public override bool Equals(object other)
		{
			var otherItem = other as CustomItem;
			if (otherItem == null) return false;

			return Chromosome.Equals(otherItem.Chromosome)
				   && Start.Equals(otherItem.Start)
				   && RefAllele.Equals(otherItem.RefAllele)
				   && AltAllele.Equals(otherItem.AltAllele)
				   && AnnotationType.Equals(otherItem.AnnotationType);
		}

		public override int GetHashCode()
		{
			var hashCode = Start.GetHashCode() ^ Chromosome.GetHashCode();
			hashCode = (hashCode * 397) ^ (AltAllele?.GetHashCode() ?? 0);
			hashCode = (hashCode * 397) ^ (AnnotationType?.GetHashCode() ?? 0);

			return hashCode;
		}

		public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryPositionCreator saCreator, string refBases = null)
        {

            // check if the ref allele matches the refBases as a prefix
            if (!SupplementaryAnnotationUtilities.ValidateRefAllele(RefAllele, refBases))
            {
                return null; //the ref allele for this entry did not match the reference bases.
            }

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(Start, RefAllele, AltAllele);

			var newStart = newAlleles.Item1;
			var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;
			
			if (newRefAllele != RefAllele)
            {
                return new CustomItem(Chromosome, newStart, newRefAllele, newAltAllele, AnnotationType, Id, IsPositional, StringFields, BooleanFields);
            }
			//for ins, del, indel without padding base, the altAllele will change even though the ref does not.
            saCreator.SaPosition.CustomItems.Add(new CustomItem(Chromosome, newStart, newRefAllele, newAltAllele, AnnotationType, Id, IsPositional, StringFields, BooleanFields));

            return null;
        }

		public void Write(IExtendedBinaryWriter writer)
		{
			writer.WriteOptAscii(Id);
			writer.WriteOptAscii(AnnotationType);
			writer.WriteOptAscii(SaAltAllele);
			writer.Write(IsPositional);

			if (StringFields != null)
			{
				writer.WriteOpt(StringFields.Count);
				foreach (var stringField in StringFields)
				{
					writer.WriteOptAscii(stringField.Key);
					writer.WriteOptAscii(stringField.Value);
				}
			}
			else writer.WriteOpt(0);

			if (BooleanFields != null)
			{
				writer.WriteOpt(BooleanFields.Count);
				foreach (var booleanField in BooleanFields)
				{
					writer.WriteOptAscii(booleanField);
				}
			}
			else writer.WriteOpt(0);
		}

		
		public void SerializeJson(StringBuilder sb)
		{
			var jsonObject = new JsonObject(sb);

			sb.Append(JsonObject.OpenBrace);
			jsonObject.AddStringValue("id", Id);
			jsonObject.AddStringValue("altAllele", "N" == AltAllele ? null : AltAllele);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);

			if (StringFields != null)
				foreach (var stringField in StringFields)
				{
					jsonObject.AddStringValue(stringField.Key, stringField.Value);
				}

			if (BooleanFields != null)
				foreach (var booleanField in BooleanFields)
				{
					jsonObject.AddBoolValue(booleanField, true, true, "true");
				}
			sb.Append(JsonObject.CloseBrace);
		}

		public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
        {
            throw new System.NotImplementedException();
        }
    }
}
