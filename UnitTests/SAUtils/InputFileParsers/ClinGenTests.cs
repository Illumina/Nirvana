using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.ClinGen;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class ClinGenTests
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 1);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "1", Chrom1}
        };

        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("nsv530705\t1\t564405\t8597804\t0\t1\tcopy_number_loss\tpathogenic\tFalse\tDevelopmental delay AND/OR other significant developmental or morphological phenotypes\t");
            writer.WriteLine("nsv530706\t1\t564424\t3262790\t0\t1\tcopy_number_loss\tpathogenic\tFalse\tAbnormal facial shape,Abnormality of cardiac morphology,Global developmental delay,Muscular hypotonia\tHP:0001252,HP:0001263,HP:0001627,HP:0001999,MedGen:CN001147,MedGen:CN001157,MedGen:CN001482,MedGen:CN001810");
            writer.WriteLine("nsv530300\t1\t728138\t5066371\t1\t0\tcopy_number_gain\tpathogenic\tFalse\tAbnormality of cardiac morphology,Cleft palate,Global developmental delay\tHP:0000175,HP:0001263,HP:0001627,MedGen:C2240378,MedGen:CN001157,MedGen:CN001482");
            writer.WriteLine("nsv530780\t1\t807685\t2574042\t1\t1\tcopy_number_variation\tpathogenic\tFalse\tDevelopmental delay AND/OR other significant developmental or morphological phenotypes,Global developmental delay,Hirsutism,Obesity,Seizure,Short stature\tHP:0001007,HP:0001250,HP:0001263,HP:0001513,HP:0004322,MedGen:C0019572,MedGen:C0349588,MedGen:C1959629,MedGen:C1963185,MedGen:CN001157");
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems()
        {
            using (var reader = new ClinGenReader(new StreamReader(GetStream()), _chromDict))
            {
                var items = reader.GetItems().ToList();

                Assert.Equal(4, items.Count);
                Assert.Equal("\"chromosome\":\"1\",\"begin\":564405,\"end\":8597804,\"variantType\":\"copy_number_loss\",\"id\":\"nsv530705\",\"clinicalInterpretation\":\"pathogenic\",\"phenotypes\":[\"Developmental delay AND/OR other significant developmental or morphological phenotypes\"],\"observedLosses\":1", items[0].GetJsonString());
            }

        }

    }
}