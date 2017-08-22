using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class TranscriptTests
    {
        [Fact]
        public void Transcript_EndToEnd()
        {
            IChromosome expectedChromosome        = new Chromosome("chrBob", "Bob", 1);
            int expectedStart                     = int.MaxValue;
            int expectedEnd                       = int.MinValue;
            var expectedId                        = "ENST00000540021";
            byte expectedVersion                  = 7;
            var expectedBioType                   = BioType.IG_J_pseudogene;
            bool expectedCanonical                = true;
            var expectedSource                    = Source.BothRefSeqAndEnsembl;

            IInterval[] expectedIntrons           = GetIntrons();
            ICdnaCoordinateMap[] expectedCdnaMaps = GetCdnaMaps();
            int expectedTotalExonLength           = 300;
            byte expectedStartExonPhase           = 3;
            int expectedSiftIndex                 = 11;
            int expectedPolyPhenIndex             = 13;

            IInterval[] expectedMicroRnas         = GetMicroRnas();

            ITranslation expectedTranslation =
                new Translation(expectedCdnaMaps[0], CompactId.Convert("ENSP00000446475"), 17, "VEIDSD");

            IGene expectedGene = new Gene(expectedChromosome, 100, 200, true, "TP53", 300, CompactId.Convert("7157"),
                CompactId.Convert("ENSG00000141510"), 500);

            var genes = new IGene[1];
            genes[0] = expectedGene;

            var peptideSeqs = new string[1];
            peptideSeqs[0] = expectedTranslation.PeptideSeq;

            var geneIndices     = CreateIndices(genes);
            var intronIndices   = CreateIndices(expectedIntrons);
            var microRnaIndices = CreateIndices(expectedMicroRnas);
            var peptideIndices  = CreateIndices(peptideSeqs);

            var indexToChromosome = new Dictionary<ushort, IChromosome>
            {
                [expectedChromosome.Index] = expectedChromosome
            };

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var transcript = new Transcript(expectedChromosome, expectedStart, expectedEnd,
                CompactId.Convert(expectedId), expectedVersion, expectedTranslation, expectedBioType, expectedGene,
                expectedTotalExonLength, expectedStartExonPhase, expectedCanonical, expectedIntrons, expectedMicroRnas,
                expectedCdnaMaps, expectedSiftIndex, expectedPolyPhenIndex, expectedSource);
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            ITranscript observedTranscript;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    transcript.Write(writer, geneIndices, intronIndices, microRnaIndices, peptideIndices);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedTranscript = Transcript.Read(reader, indexToChromosome, genes, expectedIntrons, expectedMicroRnas, peptideSeqs);
                }
            }

            Assert.NotNull(observedTranscript);
            Assert.Equal(expectedStart,           observedTranscript.Start);
            Assert.Equal(expectedEnd,             observedTranscript.End);
            Assert.Equal(expectedId,              observedTranscript.Id.ToString());
            Assert.Equal(expectedVersion,         observedTranscript.Version);
            Assert.Equal(expectedBioType,         observedTranscript.BioType);
            Assert.Equal(expectedCanonical,       observedTranscript.IsCanonical);
            Assert.Equal(expectedSource,          observedTranscript.Source);
            Assert.Equal(expectedTotalExonLength, observedTranscript.TotalExonLength);
            Assert.Equal(expectedStartExonPhase,  observedTranscript.StartExonPhase);
            Assert.Equal(expectedSiftIndex,       observedTranscript.SiftIndex);
            Assert.Equal(expectedPolyPhenIndex,   observedTranscript.PolyPhenIndex);

            Assert.Equal(expectedChromosome.Index,       observedTranscript.Chromosome.Index);
            Assert.Equal(expectedGene.Symbol,            observedTranscript.Gene.Symbol);
            Assert.Equal(expectedTranslation.PeptideSeq, observedTranscript.Translation.PeptideSeq);
            Assert.Equal(expectedIntrons.Length,         observedTranscript.Introns.Length);
            Assert.Equal(expectedCdnaMaps.Length,        observedTranscript.CdnaMaps.Length);
            Assert.Equal(expectedMicroRnas.Length,       observedTranscript.MicroRnas.Length);
        }

        private Dictionary<T, int> CreateIndices<T>(T[] objects)
        {
            var indexDict = new Dictionary<T, int>();
            for (int i = 0; i < objects.Length; i++) indexDict[objects[i]] = i;
            return indexDict;
        }

        private static ICdnaCoordinateMap[] GetCdnaMaps()
        {
            var cdnaMaps = new ICdnaCoordinateMap[3];
            cdnaMaps[0] = new CdnaCoordinateMap(100, 199, 300, 399);
            cdnaMaps[1] = cdnaMaps[0];
            cdnaMaps[2] = cdnaMaps[0];
            return cdnaMaps;
        }

        private static IInterval[] GetIntrons()
        {
            var introns = new IInterval[2];
            introns[0] = new Interval(100, 200);
            introns[1] = introns[0];
            return introns;
        }

        private static IInterval[] GetMicroRnas()
        {
            var introns = new IInterval[1];
            introns[0] = new Interval(100, 200);
            return introns;
        }
    }
}
