using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class OmimEntry
    {
        public readonly string GeneSymbol;
        private readonly string _description;
        public readonly int MimNumber;
        private readonly List<Phenotype> _phenotypes;

        public OmimEntry(string geneSymbol, string description, int mimNumber, List<Phenotype> phenotypes)
        {
            GeneSymbol   = geneSymbol;
            _description = description;
            MimNumber    = mimNumber;
            _phenotypes  = phenotypes;
        }

        public override string ToString()
        {
            var sb         = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddIntValue("mimNumber", MimNumber);
            jsonObject.AddStringValue("description", _description?.Replace(@"\'", @"'"));
            if (_phenotypes.Count > 0) jsonObject.AddObjectValues("phenotypes", _phenotypes);
            sb.Append(JsonObject.CloseBrace.ToString());

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(GeneSymbol);
            writer.WriteOptAscii(_description);
            writer.WriteOpt(MimNumber);
            writer.WriteOpt(_phenotypes.Count);
            foreach (var phenotype in _phenotypes) phenotype.Write(writer);
        }

        public static OmimEntry Read(ExtendedBinaryReader reader)
        {
            var geneSymbol     = reader.ReadAsciiString();
            var description    = reader.ReadAsciiString();
            var mimNumber      = reader.ReadOptInt32();
            var phenotypeCount = reader.ReadOptInt32();
            var phenotypes     = new List<Phenotype>();

            for (var i = 0; i < phenotypeCount; i++)
            {
                phenotypes.Add(Phenotype.ReadPhenotype(reader));
            }

            return new OmimEntry(geneSymbol, description, mimNumber, phenotypes);
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
                _mimNumber   = mimNumber;
                _phenotype   = phenotype;
                _mapping     = mapping;
                _comments    = comments;
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

            public static Phenotype ReadPhenotype(ExtendedBinaryReader reader)
            {
                var mimNumber    = reader.ReadOptInt32();
                var phenotype    = reader.ReadAsciiString();
                var mapping      = (Mapping)reader.ReadByte();
                var comments     = (Comments)reader.ReadByte();
                var inheritance  = reader.ReadOptArray(reader.ReadAsciiString);
                var inheritances = inheritance == null ? null : new HashSet<string>(inheritance);

                return new Phenotype(mimNumber, phenotype, mapping, comments, inheritances);
            }

            public void Write(ExtendedBinaryWriter writer)
            {
                writer.WriteOpt(_mimNumber);
                writer.WriteOptAscii(_phenotype);
                writer.Write((byte)_mapping);
                writer.Write((byte)_comments);
                writer.WriteOptArray(_inheritance.ToArray(), writer.WriteOptAscii);
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