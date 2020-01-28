using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.MitoMap;
using UnitTests.TestDataStructures;
using Xunit;

namespace UnitTests.SAUtils.MitoMap
{
    public class MitoMapSvReaderTests
    {
        private static readonly Chromosome Chromosome = new Chromosome("chrM", "MT", 0);
        private static readonly string RawSequence = "ABC" + new string('N', 200); 
        private static readonly ISequence Sequence = new SimpleSequence(RawSequence);
        private readonly SimpleSequenceProvider _sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh37, Sequence, 
            new Dictionary<string, IChromosome> { { "chrM", Chromosome} });
       
        
        [Theory]
        [InlineData("[\"5:105\",\"-101\",\"1837-1840/5447-5451\",\"D, 4/4\",\"<a href='/cgi-bin/print_ref_list?refs=253&title=mtDNA+Deletion%3A+1836%3A5447+-3610+D%2C+4%2F4+1837-1840%2F5447-5451+8' target='_blank'>1</a>\"],", 
             "DeletionsSingle", "\"chromosome\":\"MT\",\"begin\":4,\"end\":104,\"variantType\":\"deletion\"")]
        [InlineData("[\"2:122\",\"-121\",\"7439/13476\",\"D, 1/1\",\"<a href='/cgi-bin/print_ref_list?refs=149&title=mtDNA+Deletion%3A+7438%3A13476+-6037+D%2C+1%2F1+7439%2F13476+1' target='_blank'>1</a>\"],", 
             "DeletionsSingle", "\"chromosome\":\"MT\",\"begin\":3,\"end\":123,\"variantType\":\"deletion\"")]
        [InlineData("[\"Complete (16.5 kb)\",\"+266\",\"7-27 D-Loop region\",\"573 D-Loop region\",\"D, 7/7\",\"25\",\"<a href='/cgi-bin/print_ref_list?refs=39,556,945,952&title=Simple+Insertion+%2B266+308-573+D-Loop+region+573+D-Loop+region+D%2C+7%2F7+25' target='_blank'>4</a>\"],", 
            "InsertionsSimple", "\"chromosome\":\"MT\",\"begin\":16030,\"end\":16050,\"variantType\":\"duplication\"")]
        public void ParseLine_AsExpected(string line, string fileName, string expectedJsonString)
        {
            var  reader = new MitoMapSvReader(new FileInfo(fileName), _sequenceProvider);
            var jsonString = reader.ParseLine(line).FirstOrDefault().GetJsonString();
            Assert.Equal(expectedJsonString, jsonString);
        }
    }
}