using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OptimizedCore;
using SAUtils.Schema;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures;

public sealed class OmimItem : ISuppGeneItem
{
    public           string          GeneSymbol { get; }
    private readonly string          _geneName;
    private readonly string          _description;
    private readonly int             _mimNumber;
    public readonly  List<Phenotype> Phenotypes;
    public           SaJsonSchema    JsonSchema { get; }

    public OmimItem(string geneSymbol, string geneName, string description, int mimNumber, List<Phenotype> phenotypes, SaJsonSchema jsonSchema)
    {
        GeneSymbol   = geneSymbol;
        _geneName    = geneName;
        _description = description;
        _mimNumber   = mimNumber;
        Phenotypes   = phenotypes;
        JsonSchema   = jsonSchema;
    }

    public string GetJsonString()
    {
        var sb         = StringBuilderPool.Get();
        var jsonObject = new JsonObject(sb);

        sb.Append(JsonObject.OpenBrace);
        JsonSchema.TotalItems++;
        JsonSchema.CountKeyIfAdded(jsonObject.AddIntValue("mimNumber", _mimNumber), "mimNumber");
        JsonSchema.CountKeyIfAdded(
            jsonObject.AddStringValue("geneName", string.IsNullOrEmpty(_geneName) ? null : JsonConvert.SerializeObject(_geneName), false),
            "geneName");
        //Serialized string has the double quote at the beginning and the end
        JsonSchema.CountKeyIfAdded(
            jsonObject.AddStringValue("description", string.IsNullOrEmpty(_description) ? null : JsonConvert.SerializeObject(_description),
                false), "description");
        if (Phenotypes.Count > 0)
            JsonSchema.CountKeyIfAdded(jsonObject.AddObjectValues("phenotypes", Phenotypes), "phenotypes");
        sb.Append(JsonObject.CloseBrace);

        return StringBuilderPool.GetStringAndReturn(sb);
    }

    public sealed class Phenotype : IJsonSerializer
    {
        private readonly int             _mimNumber;
        public readonly  string          _phenotype;
        private readonly string          _description;
        public readonly  Mapping         Mapping;
        private readonly Comment[]       _comments;
        public readonly  HashSet<string> Inheritance;
        private readonly SaJsonSchema    _jsonSchema;

        public Phenotype(int mimNumber, string phenotype, string description, Mapping mapping, Comment[] comments, HashSet<string> inheritance,
            SaJsonSchema schema)
        {
            _mimNumber   = mimNumber;
            _phenotype   = phenotype;
            _description = description;
            Mapping      = mapping;
            _comments    = comments;
            Inheritance  = inheritance;
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
            _jsonSchema.CountKeyIfAdded(
                jsonObject.AddStringValue("description", string.IsNullOrEmpty(_description) ? null : JsonConvert.SerializeObject(_description),
                    false), "description");
            if (Mapping != Mapping.unknown)
                _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValue("mapping", Mapping.ToString().Replace("_", " ")), "mapping");
            if (Inheritance != null && Inheritance.Count > 0)
                _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValues("inheritances", Inheritance), "inheritances");
            if (_comments.Length > 0)
                _jsonSchema.CountKeyIfAdded(jsonObject.AddStringValues("comments", _comments.Select(x => x.ToString().Replace("_", " "))),
                    "comments");

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

    public enum Comment : byte
    {
        // ReSharper disable InconsistentNaming
        unknown,
        unconfirmed_or_possibly_spurious_mapping,
        nondiseases,

        contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection
        // ReSharper restore InconsistentNaming
    }
}