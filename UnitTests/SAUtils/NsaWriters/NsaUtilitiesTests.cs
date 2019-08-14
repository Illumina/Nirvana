using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.TOPMed;
using SAUtils.SpliceAi;
using UnitTests.SAUtils.InputFileParsers;
using VariantAnnotation.Interface.SA;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class NsaUtilitiesTests
    {
        private static readonly IChromosome Chrom5 = new Chromosome("chr5", "5", 4);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "chr5", Chrom5}
        };

        private static Stream GetDupItemsStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##TopMED");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("chr5\t70220313\trs377439976;rs372466088\tTGCC\tT\t155\tSVM;DISCVRT=2;NS=62784;AN=125568;AC=43904;AF=0.349643;Het=12194;Hom=15855\tNA:FRQ 125568:0.349643");
            writer.WriteLine("chr5\t70220313\trs377439976;rs372466088\tTGCC\tT\t155\tSVM;DISCVRT=2;NS=62784;AN=125568;AC=43904;AF=0.349643;Het=12194;Hom=15855\tNA:FRQ 125568:0.349643");
            
            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        [Fact]
        public void RemoveConflictingAlleles_does_not_remove_duplicates()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(70220313, "TGCC", 'A', _chromDict);
            var topMedReader = new TopMedReader(new StreamReader(GetDupItemsStream()), seqProvider);

            var items = topMedReader.GetItems().ToList();
            var saItems = new List<ISupplementaryDataItem>(items);
            saItems = SuppDataUtilities.RemoveConflictingAlleles(saItems, false);
            Assert.Single(saItems);

        }
        
    }
}