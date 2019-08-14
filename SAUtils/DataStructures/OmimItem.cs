using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using OptimizedCore;
using SAUtils.Schema;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class OmimItem:ISuppGeneItem
    {
        public string GeneSymbol { get; }
        private readonly string _geneName;
        private readonly string _description;
        private readonly int _mimNumber;
        private readonly List<Phenotype> _phenotypes;
        public SaJsonSchema JsonSchema { get; }

        public OmimItem(string geneSymbol, string geneName, string description, int mimNumber, List<Phenotype> phenotypes, SaJsonSchema jsonSchema)
        {
            GeneSymbol     = geneSymbol;
            _geneName      = geneName;
            _description   = description;
            _mimNumber     = mimNumber;
            _phenotypes    = phenotypes;
            JsonSchema     = jsonSchema;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            JsonSchema.TotalItems++;
            JsonSchema.CountKeyIfAdded(jsonObject.AddIntValue("mimNumber", _mimNumber), "mimNumber");
            JsonSchema.CountKeyIfAdded(jsonObject.AddStringValue("geneName", string.IsNullOrEmpty(_geneName) ? null : JsonConvert.SerializeObject(_geneName), false), "geneName");
            //Serialized string has the double quote at the beginning and the end
            JsonSchema.CountKeyIfAdded(jsonObject.AddStringValue("description", string.IsNullOrEmpty(_description) ? null : JsonConvert.SerializeObject(_description), false), "description");
            if (_phenotypes.Count > 0)
                JsonSchema.CountKeyIfAdded(jsonObject.AddObjectValues("phenotypes", _phenotypes), "phenotypes");
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
            private readonly SaJsonSchema _jsonSchema;

            public Phenotype(int mimNumber, string phenotype, Mapping mapping, Comments comments, HashSet<string> inheritance, SaJsonSchema schema)
            {
                _mimNumber   = mimNumber;
                _phenotype   = phenotype;
                _mapping     = mapping;
                _comments    = comments;
                _inheritance = inheritance;
                _jsonSchema  = schema;
            }

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
                _jsonSchema.TotalItems++;

                if (_mimNumber >= 100000)
                    _jsonSchema.CountKeyIfAdded(jsonObject.AddIntValue("mimNumber", _mimNumber), "mimNumber");
                _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValue("phenotype", _phenotype), "phenotype");
                if (_mapping != Mapping.unknown)
                    _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValue("mapping", _mapping.ToString().Replace("_", " ")), "mapping");
                if (_inheritance != null && _inheritance.Count > 0)
                    _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValues("inheritances", _inheritance), "inheritances");
                if (_comments != Comments.unknown)
                    _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValue("comments", _comments.ToString().Replace("_", " ")), "comments");

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
    }
}