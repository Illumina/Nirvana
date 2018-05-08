using System.Collections.Generic;
using System.IO;
using System.Text;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class TranscriptTests
    {
        [Fact]
        public void Transcript_EndToEnd()
        {
            IChromosome expectedChromosome      = new Chromosome("chrBob", "Bob", 1);
            const int expectedStart             = int.MaxValue;
            const int expectedEnd               = int.MinValue;
            const string expectedId             = "ENST00000540021";
            const byte expectedVersion          = 7;
            const BioType expectedBioType       = BioType.IG_J_pseudogene;
            const bool expectedCanonical        = true;
            const Source expectedSource         = Source.BothRefSeqAndEnsembl;
            const bool expectedCdsStartNotFound = true;
            const bool expectedCdsEndNotFound   = true;

            var expectedIdAndVersion = expectedId + "." + expectedVersion;

            ICodingRegion expectedCodingRegion = new CodingRegion(10001, 10200, 1, 200, 200);
            ITranscriptRegion[] expectedTranscriptRegions = GetTranscriptRegions();
            const byte expectedNumExons = 3;

            const int expectedTotalExonLength             = 300;
            const byte expectedStartExonPhase             = 3;
            const int expectedSiftIndex                   = 11;
            const int expectedPolyPhenIndex               = 13;

            IInterval[] expectedMicroRnas = GetMicroRnas();

            ITranslation expectedTranslation = new Translation(expectedCodingRegion, CompactId.Convert("ENSP00000446475", 17), "VEIDSD");

            IGene expectedGene = new Gene(expectedChromosome, 100, 200, true, "TP53", 300, CompactId.Convert("7157"),
                CompactId.Convert("ENSG00000141510"));

            var genes = new IGene[1];
            genes[0] = expectedGene;

            var peptideSeqs = new string[1];
            peptideSeqs[0] = expectedTranslation.PeptideSeq;

            var geneIndices             = CreateIndices(genes);
            var transcriptRegionIndices = CreateIndices(expectedTranscriptRegions);
            var microRnaIndices         = CreateIndices(expectedMicroRnas);
            var peptideIndices          = CreateIndices(peptideSeqs);

            var indexToChromosome = new Dictionary<ushort, IChromosome>
            {
                [expectedChromosome.Index] = expectedChromosome
            };

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var transcript = new Transcript(expectedChromosome, expectedStart, expectedEnd,
                CompactId.Convert(expectedId, expectedVersion), expectedTranslation, expectedBioType, expectedGene,
                expectedTotalExonLength, expectedStartExonPhase, expectedCanonical, expectedTranscriptRegions,
                expectedNumExons, expectedMicroRnas, expectedSiftIndex, expectedPolyPhenIndex,
                expectedSource, expectedCdsStartNotFound, expectedCdsEndNotFound, null, null);
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            ITranscript observedTranscript;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    transcript.Write(writer, geneIndices, transcriptRegionIndices, microRnaIndices, peptideIndices);
                }

                ms.Position = 0;

                using (var reader = new BufferedBinaryReader(ms))
                {
                    observedTranscript = Transcript.Read(reader, indexToChromosome, genes, expectedTranscriptRegions, expectedMicroRnas, peptideSeqs);
                }
            }

            Assert.NotNull(observedTranscript);
            Assert.Equal(expectedStart,           observedTranscript.Start);
            Assert.Equal(expectedEnd,             observedTranscript.End);
            Assert.Equal(expectedIdAndVersion,    observedTranscript.Id.WithVersion);
            Assert.Equal(expectedBioType,         observedTranscript.BioType);
            Assert.Equal(expectedCanonical,       observedTranscript.IsCanonical);
            Assert.Equal(expectedSource,          observedTranscript.Source);
            Assert.Equal(expectedTotalExonLength, observedTranscript.TotalExonLength);
            Assert.Equal(expectedStartExonPhase,  observedTranscript.StartExonPhase);
            Assert.Equal(expectedSiftIndex,       observedTranscript.SiftIndex);
            Assert.Equal(expectedPolyPhenIndex,   observedTranscript.PolyPhenIndex);

            Assert.Equal(expectedChromosome.Index,         observedTranscript.Chromosome.Index);
            Assert.Equal(expectedGene.Symbol,              observedTranscript.Gene.Symbol);
            Assert.Equal(expectedTranslation.PeptideSeq,   observedTranscript.Translation.PeptideSeq);
            Assert.Equal(expectedTranscriptRegions.Length, observedTranscript.TranscriptRegions.Length);
            Assert.Equal(expectedMicroRnas.Length,         observedTranscript.MicroRnas.Length);
        }

        private static Dictionary<T, int> CreateIndices<T>(T[] objects)
        {
            var indexDict = new Dictionary<T, int>();
            for (int i = 0; i < objects.Length; i++) indexDict[objects[i]] = i;
            return indexDict;
        }

        private static ITranscriptRegion[] GetTranscriptRegions()
        {
            var regions = new ITranscriptRegion[5];
            regions[0] = new TranscriptRegion(TranscriptRegionType.Exon, 1, 100, 199, 300, 399);
            regions[1] = new TranscriptRegion(TranscriptRegionType.Intron, 1, 200, 299, 400, 499);
            regions[2] = new TranscriptRegion(TranscriptRegionType.Exon, 2, 300, 399, 500, 599);
            regions[3] = new TranscriptRegion(TranscriptRegionType.Intron, 2, 400, 499, 600, 699);
            regions[4] = new TranscriptRegion(TranscriptRegionType.Exon, 3, 500, 599, 700, 799);
            return regions;
        }

        private static IInterval[] GetMicroRnas()
        {
            var introns = new IInterval[1];
            introns[0] = new Interval(100, 200);
            return introns;
        }
    }
}
