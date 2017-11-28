using System;
using CacheUtils.DataDumperImport.FauxRegex;
using CacheUtils.DataDumperImport.IO;
using Xunit;

namespace UnitTests.CacheUtils.DataDumperImport.FauxRegex
{
    public sealed class RegexDecisionTreeTests
    {
        [Fact]
        public void GetEntryType_RootObjectKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("$VAR1 = {");
            Assert.Equal(EntryType.RootObjectKeyValue, results.Type);
            Assert.Equal("$VAR1", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_ListObjectKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("          '1' => [");
            Assert.Equal(EntryType.ListObjectKeyValue, results.Type);
            Assert.Equal("1", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_OpenBraces()
        {
            var results = RegexDecisionTree.GetEntryType("                   bless( {");
            Assert.Equal(EntryType.OpenBraces, results.Type);
            Assert.Null(results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_StringKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                            '_ccds' => 'CCDS44137.1',");
            Assert.Equal(EntryType.StringKeyValue, results.Type);
            Assert.Equal("_ccds", results.Key);
            Assert.Equal("CCDS44137.1", results.Value);
        }

        [Fact]
        public void GetEntryType_DigitKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                                                              'phase' => -1,");
            Assert.Equal(EntryType.DigitKeyValue, results.Type);
            Assert.Equal("phase", results.Key);
            Assert.Equal("-1", results.Value);
        }

        [Fact]
        public void GetEntryType_EndBracesWithDataType()
        {
            var results = RegexDecisionTree.GetEntryType("                                                            }, 'Bio::EnsEMBL::Exon' ),");
            Assert.Equal(EntryType.EndBracesWithDataType, results.Type);
            Assert.Equal("Bio::EnsEMBL::Exon", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_EndBraces()
        {
            var results = RegexDecisionTree.GetEntryType("                                                                },");
            Assert.Equal(EntryType.EndBraces, results.Type);
            Assert.Null(results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_ObjectKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                                                                                           'next' => bless( {");
            Assert.Equal(EntryType.ObjectKeyValue, results.Type);
            Assert.Equal("next", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_UndefKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                                                                                           'adaptor' => undef,");
            Assert.Equal(EntryType.UndefKeyValue, results.Type);
            Assert.Equal("adaptor", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_EmptyListKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                                                                    'seq_edits' => [],");
            Assert.Equal(EntryType.EmptyListKeyValue, results.Type);
            Assert.Equal("seq_edits", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_EmptyValueKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                                                'cell_types' => {},");
            Assert.Equal(EntryType.EmptyValueKeyValue, results.Type);
            Assert.Equal("cell_types", results.Key);
            Assert.Null(results.Value);
        }

        [Fact]
        public void GetEntryType_ReferenceStringKeyValue()
        {
            var results = RegexDecisionTree.GetEntryType("                                                       'transcript' => $VAR1->{'22'}[0],");
            Assert.Equal(EntryType.ReferenceStringKeyValue, results.Type);
            Assert.Equal("transcript", results.Key);
            Assert.Equal("$VAR1->{'22'}[0]", results.Value);
        }

        [Fact]
        public void GetEntryType_DigitKey()
        {
            var results = RegexDecisionTree.GetEntryType("                                                                            0,");
            Assert.Equal(EntryType.DigitKey, results.Type);
            Assert.Equal("0", results.Key);
            Assert.Null(results.Value);
        }

        [Theory]
        [InlineData("'next' => bless( [")]
        [InlineData("A.B,")]
        [InlineData("$VAR1 = [")]
        public void GetEntryType_ThrowsNotImplementedException(string s)
        {
            Assert.Throws<NotImplementedException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var results = RegexDecisionTree.GetEntryType(s);
            });
        }

        [Theory]
        [InlineData("123", true)]
        [InlineData("-123", true)]
        [InlineData("12A", false)]
        public void OnlyDigits(string s, bool expectedResult)
        {
            var observedResult = RegexDecisionTree.OnlyDigits(s);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
