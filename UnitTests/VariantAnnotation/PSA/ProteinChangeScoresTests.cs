using System.IO;
using IO;
using SAUtils.Sift;
using VariantAnnotation.PSA;
using Xunit;

namespace UnitTests.VariantAnnotation.PSA
{
    public sealed class ProteinChangeScoresTests
    {
        [Fact]
        public void AddScores()
        {
            var scoreMatrix    = PsaTestUtilities.GetScoreMatrix("TRAN0001", "MIPASEAGVETPS");
            var annotationItem = new SiftItem("TRAN0001", 1, 'M', 'G', PsaUtilities.GetShortScore(0.123));

            Assert.True(scoreMatrix.AddScore(annotationItem));

            // invalid position
            annotationItem = new SiftItem("TRAN0001", 0, 'M', 'G', PsaUtilities.GetShortScore(0.123));
            Assert.False(scoreMatrix.AddScore(annotationItem));

            //invalid ref allele
            annotationItem = new SiftItem("TRAN0001", 4, 'M', 'G', PsaUtilities.GetShortScore(0.123));
            Assert.False(scoreMatrix.AddScore(annotationItem));

            //invalid alt allele
            annotationItem = new SiftItem("TRAN0001", 4, 'A', '8', PsaUtilities.GetShortScore(0.123));
            Assert.False(scoreMatrix.AddScore(annotationItem));
        }

        [Fact]
        public void ReadBackScores()
        {
            var    scoreMatrix = PsaTestUtilities.GetScoreMatrix("TRAN0001", "MIPASEAGVETPS");
            var    stream      = new MemoryStream();
            byte[] writeBuffer;
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                scoreMatrix.Write(writer);
                writer.Flush();
                writeBuffer = stream.GetBuffer();
            }

            ProteinChangeScores readMatrix;
            using (var reader = new ExtendedBinaryReader(new MemoryStream(writeBuffer)))
            {
                readMatrix = ProteinChangeScores.Read(reader);
            }

            Assert.Equal(scoreMatrix.ProteinLength, readMatrix.ProteinLength);
            for (int i = 1; i <= scoreMatrix.ProteinLength; i++)
            {
                foreach (char aa in ProteinChangeScores.AllAminoAcids)
                {
                    Assert.Equal(scoreMatrix.GetScore(i, aa), readMatrix.GetScore(i, aa));
                }
            }
        }
    }
}