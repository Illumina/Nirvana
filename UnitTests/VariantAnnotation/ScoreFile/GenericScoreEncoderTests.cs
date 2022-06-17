using System.Collections.Generic;
using System.IO;
using IO;
using VariantAnnotation.GenericScore;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile;

public sealed class GenericScoreEncoderTests
{
    [Fact]
    public void TestEncoderDecoder()
    {
        var testData = new List<(double inputNumber, double expectedResult)>
        {
            (0.246, 0.246),
            (0.2461, 0.2461),
            (0.999, 0.999),

            (0.127, 0.127),
            (0.128, 0.128),
            (0.129, 0.129),

            // Duplicate of above 3 data points to check if the generic score only stores the codes uniquely
            (0.127, 0.127),
            (0.128, 0.128),
            (0.129, 0.129),

            (0.254, 0.254),
            (0.255, 0.255),
            (0.256, 0.256),

            (0.1271, 0.1271),
            (0.1281, 0.1281),
            (0.1291, 0.1291),

            (0.2541, 0.2541),
            (0.2551, 0.2551),
            (0.2561, 0.2561),

            (0.1266, 0.1266),
            (0.1276, 0.1276),
            (0.0, 0.0),
            (1.0, 1.0),
            (-1.0, -1.0),
            (double.NaN, double.NaN)
        };

        var scoreEncoder = new GenericScoreEncoder();

        foreach ((double input, _) in testData)
        {
            scoreEncoder.AddScore(input);
        }

        using var stream = new MemoryStream();
        using var writer = new ExtendedBinaryWriter(stream, System.Text.Encoding.Default);

        scoreEncoder.Write(writer);
        stream.Position = 0;
        var reader = new ExtendedBinaryReader(stream);

        GenericScoreEncoder deserializedScoreEncoder = GenericScoreEncoder.Read(reader);
        stream.Close();

        foreach ((double inputNumber, double expectedOutput)in testData)
        {
            Assert.Equal(expectedOutput, EncodeDecode(deserializedScoreEncoder, inputNumber));
        }
    }

    private static double EncodeDecode(GenericScoreEncoder encoder, double number)
    {
        return encoder.DecodeFromBytes(encoder.EncodeToBytes(number));
    }
}