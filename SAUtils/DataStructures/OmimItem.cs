using System.Collections.Generic;
using System.Text;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class OmimItem:ISuppGeneItem
    {
        public string GeneSymbol { get; }
        private readonly string _description;
        private readonly int _mimNumber;
        private readonly List<Phenotype> _phenotypes;

        public OmimItem(string geneSymbol, string description, int mimNumber, List<Phenotype> phenotypes)
        {
            GeneSymbol    = geneSymbol;
            _description  = description;
            _mimNumber     = mimNumber;
            _phenotypes   = phenotypes;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddIntValue("mimNumber", _mimNumber);
            jsonObject.AddStringValue("description", _description?.Replace(@"\'", @"'"));
            if (_phenotypes.Count > 0) jsonObject.AddObjectValues("phenotypes", _phenotypes);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        
        public sealed class Phenotype : IJsonSerializer
        {
            private readonly int _mimNumber;
            private readonly string _phenotype;
            private readonly Mapping _mapping;
            private readonly Comments _comments;
            private readonly HashSet<string> _inheritance;

            public Phenotype(int mimNumber, string phenotype, Mapping mapping, Comments comments, HashSet<string> inheritance)
            {
                _mimNumber = mimNumber;
                _phenotype = phenotype;
                _mapping = mapping;
                _comments = comments;
                _inheritance = inheritance;
            }

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);

                if (_mimNumber >= 100000) jsonObject.AddIntValue("mimNumber", _mimNumber);
                jsonObject.AddStringValue("phenotype", _phenotype);
                if (_mapping != Mapping.unknown) jsonObject.AddStringValue("mapping", _mapping.ToString().Replace("_", " "));
                if (_inheritance != null && _inheritance.Count > 0) jsonObject.AddStringValues("inheritances", _inheritance);
                if (_comments != Comments.unknown) jsonObject.AddStringValue("comments", _comments.ToString().Replace("_", " "));

                sb.Append(JsonObject.CloseBrace);
            }
            
        }

        public enum Mapping : byte
        {
            // ReSharper disable InconsistentNaming
            unknown,
            mapping_of_the_wildtype_gene,
            disease_phenotype_itself_was_mapped,
            molecular_basis_of_the_disorder_is_known,
            chromosome_deletion_or_duplication_syndrome
            // ReSharper restore InconsistentNaming
        }

        public enum Comments : byte
        {
            // ReSharper disable InconsistentNaming
            unknown,
            nondiseases,
            contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection,
            unconfirmed_or_possibly_spurious_mapping
            // ReSharper restore InconsistentNaming
        }

        public bool IsEmpty()
        {
            return (_phenotypes == null || _phenotypes.Count==0) && string.IsNullOrEmpty(_description);
        }
    }
}