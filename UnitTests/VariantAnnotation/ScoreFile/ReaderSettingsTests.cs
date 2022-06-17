using System;
using System.IO;
using IO;
using VariantAnnotation.GenericScore;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile;

public sealed class ReaderSettingsTests
{
    [Fact]
    public void TestReadWriteZeroToOne()
    {
        var stream = new MemoryStream();
        var writer = new ExtendedBinaryWriter(stream, System.Text.Encoding.Default);

        var nucleotides = new[] {"A", "C", "G", "T"};
        var blockLength = 25;
        var encoderType = EncoderType.ZeroToOne;

        var readerSettings = GetReaderSettings(encoderType, nucleotides, blockLength);

        AssertData(writer, readerSettings, nucleotides, blockLength);
    }

    [Fact]
    public void TestReadWriteGenericScoreEncoder()
    {
        var stream = new MemoryStream();
        var writer = new ExtendedBinaryWriter(stream, System.Text.Encoding.Default);

        var nucleotides = new[] {"N"};
        var blockLength = 25;
        var encoderType = EncoderType.Generic;

        var readerSettings = GetReaderSettings(encoderType, nucleotides, blockLength);

        AssertData(writer, readerSettings, nucleotides, blockLength);
    }

    [Fact]
    public void TestReadUnknownEncoder()
    {
        var writer = new ExtendedBinaryWriter(new MemoryStream(), System.Text.Encoding.Default);

        var         nucleotides = new[] {"N"};
        var         blockLength = 25;
        EncoderType encoderType = EncoderType.Generic;

        var readerSettings = GetReaderSettings(encoderType, nucleotides, blockLength);
        using (writer)
        {
            readerSettings.Write(writer);
            writer.BaseStream.Position = 1;

            // Changing EncoderType in base stream to unknown
            writer.Write(255);
            writer.BaseStream.Position = 0;

            Assert.Throws<Exception>(() => ReaderSettings.Read(new ExtendedBinaryReader(writer.BaseStream)));
        }
    }

    private void AssertData(ExtendedBinaryWriter writer, ReaderSettings readerSettings, string[] nucleotides, int blockLength)
    {
        using (writer)
        {
            readerSettings.Write(writer);

            writer.BaseStream.Position = 0;

            var            reader             = new ExtendedBinaryReader(writer.BaseStream);
            ReaderSettings deserializedReader = ReaderSettings.Read(reader);

            Assert.Equal(nucleotides, deserializedReader.Nucleotides);
            Assert.Equal(blockLength, deserializedReader.BlockLength);
        }
    }

    private ReaderSettings GetReaderSettings(EncoderType encoderType, string[] nucleotides, int blockLength)
    {
        IScoreEncoder scoreEncoder = encoderType switch
        {
            EncoderType.Generic   => new GenericScoreEncoder(),
            EncoderType.ZeroToOne => new ZeroToOneScoreEncoder(2, 1),
            _                     => null
        };

        return new ReaderSettings(
            false,
            encoderType,
            scoreEncoder,
            new ScoreJsonEncoder("TestKey", "TestSubKey"),
            nucleotides,
            blockLength
        );
    }
}