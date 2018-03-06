using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CommonUtilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class CustomItem : SupplementaryDataItem, IJsonSerializer
    {
		public string Id { get; }
		public string AnnotationType { get; }
        private string IsAlleleSpecific { get; }
		public Dictionary<string, string> StringFields { get; }
        private Dictionary<string, double> NumberFields { get; }

		public List<string> BooleanFields { get; }


        public CustomItem(IChromosome chromosome, int start, string referenceAllele, string alternateAllele, string annotationType, string id, Dictionary<string, string> stringFields, Dictionary<string, double> numberFields,  List<string> boolFields, string isAlleleSpecific=null)
        {
            Chromosome       = chromosome;
            Start            = start;
            ReferenceAllele  = referenceAllele;
            AlternateAllele  = alternateAllele;
            AnnotationType   = annotationType;
            Id               = id;
            StringFields     = stringFields;
            NumberFields     = numberFields;
            BooleanFields    = boolFields;
            IsAlleleSpecific = isAlleleSpecific;
        }

        public override bool Equals(object other)
		{
		    if (!(other is CustomItem otherItem)) return false;

			return Chromosome.Equals(otherItem.Chromosome)
				   && Start.Equals(otherItem.Start)
				   && ReferenceAllele.Equals(otherItem.ReferenceAllele)
				   && AlternateAllele.Equals(otherItem.AlternateAllele)
				   && AnnotationType.Equals(otherItem.AnnotationType);
		}

		public override int GetHashCode()
		{
            // ReSharper disable NonReadonlyMemberInGetHashCode
            var hashCode = Start.GetHashCode() ^ Chromosome.GetHashCode();
            hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (AnnotationType?.GetHashCode() ?? 0);
            // ReSharper restore NonReadonlyMemberInGetHashCode

            return hashCode;
        }

        public string GetJsonString()
	    {
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

			jsonObject.AddStringValue("id", Id);
			jsonObject.AddStringValue("altAllele", "N" == AlternateAllele ? null : AlternateAllele);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);

			if (StringFields != null)
				foreach (var stringField in StringFields)
				{
					jsonObject.AddStringValue(stringField.Key, stringField.Value);
				}

			if (NumberFields != null)
				foreach (var numFields in NumberFields)
				{
					jsonObject.AddStringValue(numFields.Key, numFields.Value.ToString(CultureInfo.InvariantCulture), false);
				}

			if (BooleanFields != null)
				foreach (var booleanField in BooleanFields)
				{
					jsonObject.AddBoolValue(booleanField, true);
				}

	        return StringBuilderCache.GetStringAndRelease(sb);
	    }

		public void SerializeJson(StringBuilder sb)
		{
			var jsonObject = new JsonObject(sb);

			sb.Append(JsonObject.OpenBrace);
			jsonObject.AddStringValue("id", Id);
			jsonObject.AddStringValue("altAllele", "N" == AlternateAllele ? null : AlternateAllele);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);

			if (StringFields != null)
				foreach (var stringField in StringFields)
				{
					jsonObject.AddStringValue(stringField.Key, stringField.Value);
				}

			if (NumberFields != null)
				foreach (var numFields in NumberFields)
				{
					jsonObject.AddStringValue(numFields.Key, numFields.Value.ToString(CultureInfo.InvariantCulture),false);
				}

			if (BooleanFields != null)
				foreach (var booleanField in BooleanFields)
				{
					jsonObject.AddBoolValue(booleanField, true);
				}
			sb.Append(JsonObject.CloseBrace);
		}
		public override SupplementaryIntervalItem GetSupplementaryInterval()
		{
			throw new System.NotImplementedException();
		}

	}
}
