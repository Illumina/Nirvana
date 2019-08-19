using System;
using System.Collections.Generic;
using System.IO;
using SAUtils;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.OMIM;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class GeneAnnotationsTest
    {
        private Dictionary<string, List<ISuppGeneItem>> GetGeneAnnotations()
        {
            var omimJsonSchema = OmimSchema.Get();
            return new Dictionary<string, List<ISuppGeneItem>>
            {
                { "gene1", new List<ISuppGeneItem>
                    {
                    new OmimItem("gene1", "describing gene1", 123,
                        new List<OmimItem.Phenotype>
                        {
                            new OmimItem.Phenotype(1, "disease 1", OmimItem.Mapping.mapping_of_the_wildtype_gene, OmimItem.Comments.unconfirmed_or_possibly_spurious_mapping, new HashSet<string> {"autosomal recessive"}, omimJsonSchema.GetSubSchema("phenotypes"))
                        }, omimJsonSchema) 
                    }
                },
                {
                    "gene2", new List<ISuppGeneItem>
                    {
                        new OmimItem("gene2", "gene 2 description", 124,
                            new List<OmimItem.Phenotype>
                            {
                                new OmimItem.Phenotype( 2, "disease 2", OmimItem.Mapping.chromosome_deletion_or_duplication_syndrome, OmimItem.Comments.nondiseases, new HashSet<string> {"whatever", "never-ever"}, omimJsonSchema.GetSubSchema("phenotypes"))
                            }, omimJsonSchema)

                    }
                }

            };
        }

        [Fact]
        public void ReadBackGeneAnnotations()
        {
            var writeStream = new MemoryStream();
            var version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks);
            var ngaWriter   = new NgaWriter(writeStream, version, "mimo", SaCommon.SchemaVersion, true);
            
            ngaWriter.Write(GetGeneAnnotations());

            var readStream = new MemoryStream(writeStream.ToArray());
            using (var ngaReader = new NgaReader(readStream))
            {
                Assert.Null(ngaReader.GetAnnotation("gene3"));
                Assert.Equal("[{\"mimNumber\":123,\"description\":\"describing gene1\",\"phenotypes\":[{\"phenotype\":\"disease 1\",\"mapping\":\"mapping of the wildtype gene\",\"inheritances\":[\"autosomal recessive\"],\"comments\":\"unconfirmed or possibly spurious mapping\"}]}]", ngaReader.GetAnnotation("gene1"));
            }

            ngaWriter.Dispose();
        }
    }
}