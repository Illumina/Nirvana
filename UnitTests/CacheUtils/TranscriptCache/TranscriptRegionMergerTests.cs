using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.TranscriptCache;
using CacheUtils.TranscriptCache.Comparers;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.CacheUtils.TranscriptCache
{
    public sealed class TranscriptRegionMergerTests
    {
        private readonly TranscriptRegionComparer _comparer = new TranscriptRegionComparer();

        [Fact]
        public void GetTranscriptRegions_OneExon()
        {
            var chromosome = new Chromosome("chr5", "5", 4);

            var cdnaMaps = new[]
            {
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 64571756, 64572037, 2569, 2850)
            };

            var exons = new[]
            {
                new MutableExon(chromosome, 64571756, 64572037, 0)
            };

            var expectedRegions = new ITranscriptRegion[]
            {
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 1, 64571756, 64572037, 2569, 2850)
            };

            var observedRegions = TranscriptRegionMerger.GetTranscriptRegions(cdnaMaps, exons, null, false);
            Assert.Single(observedRegions);
            Assert.Equal(expectedRegions, observedRegions, _comparer);
        }

        [Fact]
        public void GetTranscriptRegions_WithGap_Forward()
        {
            var chromosome = new Chromosome("chr5", "5", 4);

            var exons = new[]
            {
                new MutableExon(chromosome, 89623195, 89624305, 0),
                new MutableExon(chromosome, 89653782, 89653866, 0),
                new MutableExon(chromosome, 89690803, 89690846, 0),
                new MutableExon(chromosome, 89692770, 89693008, 0),
                new MutableExon(chromosome, 89702368, 89702526, 0),
                new MutableExon(chromosome, 89711875, 89712016, 0),
                new MutableExon(chromosome, 89717610, 89717776, 0),
                new MutableExon(chromosome, 89720651, 89720875, 0),
                new MutableExon(chromosome, 89725044, 89731687, 0)
            };

            var introns = new IInterval[]
            {
                new Interval(89624306, 89653781),
                new Interval(89653867, 89690802),
                new Interval(89690847, 89692769),
                new Interval(89693009, 89702367),
                new Interval(89702527, 89711874),
                new Interval(89712017, 89717609),
                new Interval(89717777, 89720650),
                new Interval(89720876, 89725043)
            };

            var cdnaMaps = new[]
            {
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89623195, 89623860, 1, 666),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89623862, 89624305, 667, 1110),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89653782, 89653866, 1111, 1195),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89690803, 89690846, 1196, 1239),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89692770, 89693008, 1240, 1478),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89702368, 89702526, 1479, 1637),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89711875, 89712016, 1638, 1779),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89717610, 89717776, 1780, 1946),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89720651, 89720875, 1947, 2171),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 89725044, 89731687, 2172, 8815)
            };

            var expectedRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 89623195, 89623860, 1, 666),
                new TranscriptRegion(TranscriptRegionType.Gap, 1, 89623861, 89623861, 666, 667),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 89623862, 89624305, 667, 1110),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 89624306, 89653781, 1110, 1111),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 89653782, 89653866, 1111, 1195),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 89653867, 89690802, 1195, 1196),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 89690803, 89690846, 1196, 1239),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 89690847, 89692769, 1239, 1240),
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 89692770, 89693008, 1240, 1478),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 89693009, 89702367, 1478, 1479),
                new TranscriptRegion(TranscriptRegionType.Exon, 5, 89702368, 89702526, 1479, 1637),
                new TranscriptRegion(TranscriptRegionType.Intron, 5, 89702527, 89711874, 1637, 1638),
                new TranscriptRegion(TranscriptRegionType.Exon, 6, 89711875, 89712016, 1638, 1779),
                new TranscriptRegion(TranscriptRegionType.Intron, 6, 89712017, 89717609, 1779, 1780),
                new TranscriptRegion(TranscriptRegionType.Exon, 7, 89717610, 89717776, 1780, 1946),
                new TranscriptRegion(TranscriptRegionType.Intron, 7, 89717777, 89720650, 1946, 1947),
                new TranscriptRegion(TranscriptRegionType.Exon, 8, 89720651, 89720875, 1947, 2171),
                new TranscriptRegion(TranscriptRegionType.Intron, 8, 89720876, 89725043, 2171, 2172),
                new TranscriptRegion(TranscriptRegionType.Exon, 9, 89725044, 89731687, 2172, 8815)
            };

            var observedRegions = TranscriptRegionMerger.GetTranscriptRegions(cdnaMaps, exons, introns, false);
            Assert.Equal(19, observedRegions.Length);
            Assert.Equal(expectedRegions, observedRegions, _comparer);
        }

        [Fact]
        public void GetTranscriptRegions_WithGap_Reverse()
        {
            var chromosome = new Chromosome("chr5", "5", 4);

            var exons = new[]
            {
                new MutableExon(chromosome, 64571756, 64574228, 2),
                new MutableExon(chromosome, 64575621, 64575829, 0),
                new MutableExon(chromosome, 64578301, 64578407, 0),
                new MutableExon(chromosome, 64578866, 64578927, 0)
            };

            var introns = new IInterval[]
            {
                new Interval(64574229, 64575620),
                new Interval(64575830, 64578300),
                new Interval(64578408, 64578865)
            };

            var cdnaMaps = new[]
            {
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 64571756, 64572037, 2569, 2850),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 64572039, 64574228, 379, 2568),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 64575621, 64575829, 170, 378),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 64578301, 64578407, 63, 169),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 64578866, 64578927, 1, 62)
            };

            var expectedRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 64571756, 64572037, 2569, 2850),
                new TranscriptRegion(TranscriptRegionType.Gap, 4, 64572038, 64572038, 2568, 2569),
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 64572039, 64574228, 379, 2568),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 64574229, 64575620, 378, 379),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 64575621, 64575829, 170, 378),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 64575830, 64578300, 169, 170),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 64578301, 64578407, 63, 169),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 64578408, 64578865, 62, 63),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 64578866, 64578927, 1, 62)
            };

            var observedRegions = TranscriptRegionMerger.GetTranscriptRegions(cdnaMaps, exons, introns, true);
            Assert.Equal(9, observedRegions.Length);
            Assert.Equal(expectedRegions, observedRegions, _comparer);
        }

        [Fact]
        public void GetTranscriptRegions_Reverse()
        {
            var chromosome = new Chromosome("chr1", "1", 0);

            var exons = new[]
            {
                new MutableExon(chromosome, 20977055, 20977207, 1),
                new MutableExon(chromosome, 20976856, 20977050, 1)
            };

            var introns = new IInterval[]
            {
                new Interval(20977051, 20977054)
            };

            var cdnaMaps = new[]
            {
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 20977055, 20977207, 1, 153),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 20976856, 20977050, 154, 348)
            };

            var expectedRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 20976856, 20977050, 154, 348),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 20977051, 20977054, 153, 154),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 20977055, 20977207, 1, 153)
            };

            var observedRegions = TranscriptRegionMerger.GetTranscriptRegions(cdnaMaps, exons, introns, true);
            Assert.Equal(3, observedRegions.Length);
            Assert.Equal(expectedRegions, observedRegions, _comparer);
        }

        [Fact]
        public void GetTranscriptRegions_TwoExonsNoGap_Forward()
        {
            var chromosome = new Chromosome("chr12", "12", 11);

            var exons = new[]
            {
                new MutableExon(chromosome, 7079944, 7080253, 1),
                new MutableExon(chromosome, 7083501, 7083602, 2),
                new MutableExon(chromosome, 7083714, 7083855, 2),
                new MutableExon(chromosome, 7084252, 7084310, 1),
                new MutableExon(chromosome, 7084391, 7084540, 2),
                new MutableExon(chromosome, 7084858, 7085165, 2)
            };

            var introns = new IInterval[]
            {
                new Interval(7080254, 7083500),
                new Interval(7083603, 7083713),
                new Interval(7083856, 7084251),
                new Interval(7084311, 7084390),
                new Interval(7084541, 7084857)
            };

            var cdnaMaps = new[]
            {
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7079944, 7080212, 1, 269),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7080213, 7080253, 271, 311),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7083501, 7083602, 312, 413),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7083714, 7083855, 414, 555),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7084252, 7084310, 556, 614),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7084391, 7084540, 615, 764),
                new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, 7084858, 7085165, 765, 1072)
            };

            var expectedRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 7079944, 7080212, 1, 269), 
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 7080213, 7080253, 271, 311),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 7080254, 7083500, 311, 312),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 7083501, 7083602, 312, 413),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 7083603, 7083713, 413, 414),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 7083714, 7083855, 414, 555),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 7083856, 7084251, 555, 556),
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 7084252, 7084310, 556, 614),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 7084311, 7084390, 614, 615),
                new TranscriptRegion(TranscriptRegionType.Exon, 5, 7084391, 7084540, 615, 764),
                new TranscriptRegion(TranscriptRegionType.Intron, 5, 7084541, 7084857, 764, 765),
                new TranscriptRegion(TranscriptRegionType.Exon, 6, 7084858, 7085165, 765, 1072)
            };

            var observedRegions = TranscriptRegionMerger.GetTranscriptRegions(cdnaMaps, exons, introns, false);
            Assert.Equal(12, observedRegions.Length);
            Assert.Equal(expectedRegions, observedRegions, _comparer);
        }
    }
}
