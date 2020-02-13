using System.Collections.Generic;
using System.IO;
using SAUtils.ExtractCosmicSvs;
using UnitTests.TestUtilities;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.AnnotationItems
{
    public sealed class CosmicCnvItemTests
    {
        [Fact]
        public void Merge_add_new_items()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                }, 1);

            var item2 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology3", 1},
                    {"histology4", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue3", 2},
                    { "tissue4", 1}
                },2);

            item1.Merge(item2);

            Assert.Equal(4, item1.CancerTypeCount);
            Assert.Equal(4, item1.TissueTypeCount);
        }

        [Fact]
        public void GetJsonString()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },2);

            
            Assert.Equal("\"id\":1,\"variantType\":\"copy_number_gain\",\"copyNumber\":3,\"cancerTypes\":[{\"histology1\":1},{\"histology2\":2}],\"tissueTypes\":[{\"tissue1\":2},{\"tissue2\":1}]", item1.GetJsonString());
        }

        [Fact]
        public void GetJsonString_unspecified_copy_number()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, -1,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                }, 2);


            Assert.Equal("\"id\":1,\"variantType\":\"copy_number_gain\",\"cancerTypes\":[{\"histology1\":1},{\"histology2\":2}],\"tissueTypes\":[{\"tissue1\":2},{\"tissue2\":1}]", item1.GetJsonString());
        }


        [Fact]
        public void Merge_same_histology_site()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },1);

            var item2 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },2);

            item1.Merge(item2);

            Assert.Equal(2, item1.CancerTypeCount);
            Assert.Equal(2, item1.TissueTypeCount);
        }

        [Fact]
        public void Merge_avoid_double_counting()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                }, 1);

            var item2 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                }, 1);

            item1.Merge(item2);

            Assert.Equal("\"id\":1,\"variantType\":\"copy_number_gain\",\"copyNumber\":3,\"cancerTypes\":[{\"histology1\":1},{\"histology2\":2}],\"tissueTypes\":[{\"tissue1\":2},{\"tissue2\":1}]", item1.GetJsonString());
        }

        [Fact]
        public void Merge_check_adjust_counts()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },1);

            var item2 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },2);

            item1.Merge(item2);

            Assert.Equal("\"id\":1,\"variantType\":\"copy_number_gain\",\"copyNumber\":3,\"cancerTypes\":[{\"histology1\":2},{\"histology2\":4}],\"tissueTypes\":[{\"tissue1\":4},{\"tissue2\":2}]", item1.GetJsonString());
        }

        [Fact]
        public void Merge_throws_exception_if_cnvs_differ()
        {
            var item1 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_loss, 0,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },1);

            var item2 = new CosmicCnvItem(1, ChromosomeUtilities.Chr1, 100, 1000, VariantType.copy_number_gain, 3,
                new Dictionary<string, int>
                {
                    {"histology1", 1},
                    {"histology2", 2}
                }, new Dictionary<string, int>
                {
                    { "tissue1", 2},
                    { "tissue2", 1}
                },1);


            Assert.Throws<InvalidDataException>(()=>item1.Merge(item2));
        }
    }
}