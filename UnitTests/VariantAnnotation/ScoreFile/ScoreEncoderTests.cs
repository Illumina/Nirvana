using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using IO;
using VariantAnnotation.GenericScore;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile
{
    public sealed class ScoreEncoderTests
    {
        [Fact]
        public void TestEncoderDecoder()
        {
            const int    numberOfDigits = 3;
            const double maxScore       = 1.0;

            var scoreEncoder = new ZeroToOneScoreEncoder(numberOfDigits, maxScore);

            var stream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(stream, System.Text.Encoding.Default);

            scoreEncoder.Write(writer);
            stream.Position = 0;
            var reader = new ExtendedBinaryReader(stream);

            var deserializedScoreEncoder = ZeroToOneScoreEncoder.Read(reader);
            stream.Close();

            var testData = new List<(double inputNumber, double expectedResult)>
            {
                (0.246, 0.246),
                (0.2461, 0.246),
                (0.2466, 0.247),

                (0.800, 0.800),
                (0.999, 0.999),
                (0.9999, 1.000),

                (0.127, 0.127),
                (0.128, 0.128),
                (0.129, 0.129),
                
                (0.254, 0.254),
                (0.255, 0.255),
                (0.256, 0.256),
                
                (0.1271, 0.127),
                (0.1281, 0.128),
                (0.1291, 0.129),
                
                (0.2541, 0.254),
                (0.2551, 0.255),
                (0.2561, 0.256),
                
                (0.1266, 0.127),
                (0.1276, 0.128),
                (0.1286, 0.129),
                (0.1296, 0.130),
                
                (0.2536, 0.254),
                (0.2546, 0.255),
                (0.2556, 0.256),
                (0.2566, 0.257),
                
                (0.0, 0.0),
                (1.0, 1.0),
                (double.NaN, double.NaN)
            };

            // Test encoder and its deserialized version
            foreach (ZeroToOneScoreEncoder encoder in new[] {scoreEncoder, deserializedScoreEncoder})
            {
                foreach ((double inputNumber, double expectedOutput)in testData)
                {
                    Assert.Equal(expectedOutput, EncodeDecode(encoder, inputNumber));
                }

                Assert.Throws<UserErrorException>(() => encoder.EncodeToBytes(2.1));
            }
        }

        [Fact]
        public void TestByteRequired()
        {
            var testData = new List<(int numberOfDigits, double maxScore, int expectedBytesRequired)>
            {
                (2, 1.0, 1),
                (2, 10.0, 1),

                (3, 1.0, 2),
                (4, 1.0, 2),

                (5, 1.0, 3),
                (6, 1.0, 3),
                (7, 1.0, 3),
                (5, 1000, 3)
            };

            foreach ((int numberOfDigits, double maxScore, int expectedBytesRequired) in testData)
            {
                var scoreEncoder = new ZeroToOneScoreEncoder(numberOfDigits, maxScore);
                Assert.Equal(expectedBytesRequired, scoreEncoder.BytesRequired);
            }
        }

        private static double EncodeDecode(ZeroToOneScoreEncoder encoder, double number)
        {
            return encoder.DecodeFromBytes(encoder.EncodeToBytes(number));
        }
    }
}