using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class VcfInfoFieldTests
    {
        /// <summary>
        /// tests the behavior with an empty dbSNP ID field
        /// </summary>
        [Fact]
        public void EmptyDbSnp()
        {
            var dbSnpField = new VcfField();

            var expected = ".";
            var observed = dbSnpField.GetString("");

            Assert.Equal(expected, observed);
        }

        /// <summary>
        /// tests the behavior with multiple info field entries, some being null
        /// </summary>
        [Fact]
        public void MultipleEntries()
        {
            var infoField = new VcfInfoKeyValue("AF1000G");
            infoField.Add("0.182308");
            infoField.Add(null);
            infoField.Add("0.282308");
            infoField.Add(null);

            var expected = "AF1000G=0.182308,.,0.282308,.";
            var observed = infoField.GetString();

            Assert.Equal(expected, observed);
        }

        /// <summary>
        /// tests the behavior with multiple null info field entries
        /// </summary>
        [Fact]
        public void MultipleNullEntries()
        {
            var infoField = new VcfInfoKeyValue("AF1000G");
            infoField.Add(null);
            infoField.Add(null);
            infoField.Add(null);
            infoField.Add(null);

            var observed = infoField.GetString();

            Assert.Null(observed);
        }

        /// <summary>
        /// tests the behavior with a single info field entry
        /// </summary>
        [Fact]
        public void SingleEntry()
        {
            var infoField = new VcfInfoKeyValue("AF1000G");
            infoField.Add("0.182308");

            var expected = "AF1000G=0.182308";
            var observed = infoField.GetString();

            Assert.Equal(expected, observed);
        }
    }
}