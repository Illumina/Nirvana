using System;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
    public sealed class ClinVarItem : ISupplementaryDataItem, IComparable<ClinVarItem>
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        public int Stop { get; }
        public string VariantType { get; }
        public readonly string[][] Values;
        private readonly SaJsonSchema _jsonSchema;
		
        public ClinVarItem(IChromosome chromosome,
            int position,
            int stop,
            string refAllele,
            string altAllele,
            string variantType,
            string[][] values,
            SaJsonSchema jsonSchema)
        {
            Chromosome  = chromosome;
            Position    = position;
            Stop        = stop;
            RefAllele   = refAllele;
            AltAllele   = altAllele;
            VariantType = variantType;
            Values      = values;
            _jsonSchema = jsonSchema;
        }

        public string GetJsonString()
        {
            var allValues = Values.Take(3).ToList();
            allValues.Add(new[] { SingleNToNull(string.IsNullOrEmpty(RefAllele) ? "-" : RefAllele)});
            allValues.Add(new[] { SingleNToNull(SaUtilsCommon.ReverseSaReducedAllele(string.IsNullOrEmpty(AltAllele) ? "-" : AltAllele))}); 
            allValues.AddRange(Values.Skip(3));
            return _jsonSchema.GetJsonString(allValues);          
        }

        public string Id => Values[ClinVarCommon.IdIndex][0];

        public string ReviewStatus => Values[ClinVarCommon.ReviewStatusIndex][0];

        public string[] AlleleOrigins => Values[ClinVarCommon.AlleleOriginsIndex];

        public string[] Phenotypes => Values[ClinVarCommon.PhenotypesIndex];

        public string[] MedGenIds => Values[ClinVarCommon.MedGenIdsIndex];

        public string[] OmimIds => Values[ClinVarCommon.OmimIdsIndex];

        public string[] OrphanetIds => Values[ClinVarCommon.OrphanetIdsIndex];

        public string[] Significance => Values[ClinVarCommon.SignificanceIndex];

        public string LastUpdateDate => Values[ClinVarCommon.LastUpdateDateIndex][0];

        public string[] PubMedIds => Values[ClinVarCommon.PubMedIdsIndex];

        public int CompareTo(ClinVarItem other)
        {
            return Chromosome.Index != other.Chromosome.Index
                ? Chromosome.Index.CompareTo(other.Chromosome.Index)
                : Position.CompareTo(other.Position);
        }

        private static string SingleNToNull(string allele) => allele == "N" ? null : allele;
    }
}
