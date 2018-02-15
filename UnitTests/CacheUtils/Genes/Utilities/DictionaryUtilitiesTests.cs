using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.Utilities;
using Xunit;

namespace UnitTests.CacheUtils.Genes.Utilities
{
    public sealed class DictionaryUtilitiesTests
    {
        [Fact]
        public void GetSingleValueDict_OneKey_OneValue()
        {
            var uga1 = new UgaGene(null, null, null, true, "102466751", null, "MIR6859-1", 50039);
            var uga2 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var genes = new List<UgaGene> { uga1, uga2 };

            var observedResult = genes.GetSingleValueDict(x => x.EnsemblId);
            Assert.NotNull(observedResult);
            Assert.Single(observedResult);
            Assert.True(observedResult.ContainsKey("ENSG00000278267"));
        }

        [Fact]
        public void GetSingleValueDict_ThrowException_IfMultipleValuesShareKey()
        {
            var uga1 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var uga2 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var genes = new List<UgaGene> { uga1, uga2 };

            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var observedResult = genes.GetSingleValueDict(x => x.EnsemblId);
            });
        }

        [Fact]
        public void GetMultiValueDict_OneKey_WithTwoValues()
        {
            var uga1 = new UgaGene(null, null, null, true, "102466751", null, "MIR6859-1", 50039);
            var uga2 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var uga3 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var genes = new List<UgaGene> { uga1, uga2, uga3 };

            var observedResult = genes.GetMultiValueDict(x => x.EnsemblId);
            Assert.NotNull(observedResult);
            Assert.Single(observedResult);

            var firstEntry = observedResult["ENSG00000278267"];
            Assert.NotNull(firstEntry);
            Assert.Equal(2, firstEntry.Count);
        }

        [Fact]
        public void GetKeyValueDict_OneKey_OneValue()
        {
            var uga1 = new UgaGene(null, null, null, true, "102466751", null, "MIR6859-1", 50039);
            var uga2 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var uga3 = new UgaGene(null, null, null, true, null, "ENSG00000278267", "MIR6859-1", 50039);
            var genes = new List<UgaGene> { uga1, uga2, uga3 };

            var observedResult = genes.GetKeyValueDict(x => x.EnsemblId, x => x.HgncId);
            Assert.NotNull(observedResult);
            Assert.Single(observedResult);

            var hgncId = observedResult["ENSG00000278267"];
            Assert.Equal(50039, hgncId);
        }

        [Fact]
        public void CreateIndex_ThreeValues()
        {
            const string a = "tom";
            const string b = "jane";
            const string c = "sally";
            var genes = new List<string> { a, b, c };

            var observedResult = genes.CreateIndex();
            Assert.NotNull(observedResult);
            Assert.Equal(3, observedResult.Count);

            Assert.Equal(0, observedResult[a]);
            Assert.Equal(1, observedResult[b]);
            Assert.Equal(2, observedResult[c]);
        }
    }
}
