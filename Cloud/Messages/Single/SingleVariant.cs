using System.Collections.Generic;
using ErrorHandling.Exceptions;

namespace Cloud.Messages.Single
{
    public class SingleVariant
    {
        // ReSharper disable InconsistentNaming
        public string chromosome;
        public int? position;
        public string refAllele;
        public string[] altAlleles;
        public double? quality;
        public string[] filters;
        public string infoField;
        public string formatField;
        public string[] sampleFields;
        public string[] sampleNames;
        // ReSharper restore InconsistentNaming

        public const string VcfMissingValue = ".";

        public void Validate()
        {
            if (string.IsNullOrEmpty(chromosome)) throw new UserErrorException("Please provide the chromosome.");
            if (position == null) throw new UserErrorException("Please provide the position.");
            if (string.IsNullOrEmpty(refAllele)) throw new UserErrorException("Please provide the reference allele.");
            if (altAlleles == null || altAlleles.Length == 0) throw new UserErrorException("Please provide the alternate alleles.");

            if (!string.IsNullOrEmpty(formatField) || sampleFields != null || sampleNames != null)
            {
                if (string.IsNullOrEmpty(formatField)) throw new UserErrorException("Please provide a format field when supplying sample fields or sample names.");

                int numSampleFields = sampleFields?.Length ?? 0;
                if (numSampleFields == 0) throw new UserErrorException("Please provide sample fields when supplying sample names and the format field.");

                int numSampleNames  = sampleNames?.Length ?? 0;
                if (numSampleNames == 0) throw new UserErrorException("Please provide sample names when supplying sample fields and the format field.");

                if (sampleFields?.Length != sampleNames?.Length) throw new UserErrorException("Please provide the same number of sample fields as sample names.");
            }
        }

        public string[] GetVcfFields()
        {
            string altAlleleField = GetStringFromNullableCollection(altAlleles, ',');
            string filterField    = GetStringFromNullableCollection(filters, ';');

            var vcfFields = new List<string>
            {
                chromosome,
                position.ToString(),
                VcfMissingValue,
                refAllele,
                altAlleleField,
                quality?.ToString() ?? VcfMissingValue,
                filterField,
                infoField ?? VcfMissingValue
            };

            if (sampleFields != null)
            {
                vcfFields.Add(formatField ?? VcfMissingValue);
                vcfFields.AddRange(sampleFields);
            }

            return vcfFields.ToArray();
        }

        private static string GetStringFromNullableCollection(string[] values, char separator) =>
            values == null || values.Length == 0 
                ? VcfMissingValue 
                : string.Join(separator, values);
    }
}