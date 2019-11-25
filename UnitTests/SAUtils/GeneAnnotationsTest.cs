using System;
using System.Collections.Generic;
using System.IO;
using SAUtils;
using SAUtils.DataStructures;
using SAUtils.Omim;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class GeneAnnotationsTest
    {
        private static Dictionary<string, List<ISuppGeneItem>> GetGeneAnnotations()
        {
            var omimJsonSchema = OmimSchema.Get();
            return new Dictionary<string, List<ISuppGeneItem>>
            {
                { "gene1", new List<ISuppGeneItem>
                    {
                        new OmimItem("gene1", "gene name 1 (\'minibrain\', Drosophila, homolog of)", "describing gene 1\n\"some citation\"", 123,
                            new List<OmimItem.Phenotype>
                            {
                                new OmimItem.Phenotype(1, "disease 1", OmimItem.Mapping.mapping_of_the_wildtype_gene, new [] {OmimItem.Comment.unconfirmed_or_possibly_spurious_mapping}, new HashSet<string> {"autosomal recessive"}, omimJsonSchema.GetSubSchema("phenotypes"))
                            }, omimJsonSchema) 
                    }
                },
                {
                    "gene2", new List<ISuppGeneItem>
                    {
                        new OmimItem("gene2", "gene name 2","", 124,
                            new List<OmimItem.Phenotype>
                            {
                                new OmimItem.Phenotype( 2, "disease 2", OmimItem.Mapping.chromosome_deletion_or_duplication_syndrome, new [] {OmimItem.Comment.nondiseases}, new HashSet<string> {"whatever", "never-ever"}, omimJsonSchema.GetSubSchema("phenotypes"))
                            }, omimJsonSchema)
                    }
                }

            };
        }

        [Fact]
        public void ReadBackGeneAnnotations()
        {
            NgaReader reader;
            var version          = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks);
            const string jsonKey = "mimo";
            const bool isArray   = true;

            using (var ms = new MemoryStream())
            {
                using (var writer = new NgaWriter(ms, version, jsonKey, SaCommon.SchemaVersion, isArray, true))
                {
                    writer.Write(GetGeneAnnotations());
                }

                ms.Position = 0;
                reader = NgaReader.Read(ms);
            }

            Assert.NotNull(reader);
            Assert.Null(reader.GetAnnotation("gene3"));
            Assert.Equal("[{\"mimNumber\":123,\"geneName\":\"gene name 1 ('minibrain', Drosophila, homolog of)\",\"description\":\"describing gene 1\\n\\\"some citation\\\"\",\"phenotypes\":[{\"phenotype\":\"disease 1\",\"mapping\":\"mapping of the wildtype gene\",\"inheritances\":[\"autosomal recessive\"],\"comments\":[\"unconfirmed or possibly spurious mapping\"]}]}]", reader.GetAnnotation("gene1"));
            Assert.Equal("[{\"mimNumber\":124,\"geneName\":\"gene name 2\",\"phenotypes\":[{\"phenotype\":\"disease 2\",\"mapping\":\"chromosome deletion or duplication syndrome\",\"inheritances\":[\"whatever\",\"never-ever\"],\"comments\":[\"nondiseases\"]}]}]", reader.GetAnnotation("gene2"));
        }
    }
}