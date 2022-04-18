using System.IO;
using Genome;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile;

public sealed class SaItemValidatorTests
{
    [Fact]
    public void TestParRegion()
    {
        var saItemValidator = new SaItemValidator(true, true);

        var sequence         = new SimpleSequence(new string('A', 15_000));
        var sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh38, sequence, ChromosomeUtilities.RefNameToChromosome);

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.ChrY, 10_011, "A", "C", 0.5),
            sequenceProvider
        ));

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.ChrY, 10_011, "N", "C", 0.5),
            sequenceProvider
        ));

        Assert.Throws<InvalidDataException>(() => saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.ChrY, 10_011, "C", "C", 0.5),
            sequenceProvider
        ));

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.ChrY, 10_011, "N", "N", 0.5),
            sequenceProvider
        ));

        saItemValidator = new SaItemValidator(true, false);
        Assert.False(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.ChrY, 10_011, "C", "N", 0.5),
            sequenceProvider
        ));
    }

    [Fact]
    public void TestIncorrectReference()
    {
        var sequence         = new SimpleSequence(new string('A', 99));
        var sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh38, sequence, ChromosomeUtilities.RefNameToChromosome);

        // Strict Checking throws exceptions
        var saItemValidator = new SaItemValidator(true, true);
        Assert.Throws<InvalidDataException>(() => saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "C", "G", 0.5),
            sequenceProvider
        ));
        
        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "G", 0.5),
            sequenceProvider
        ));
        
        // Will not throw exceptions
        saItemValidator = new SaItemValidator(true, false);
        Assert.False(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "C", "A", 0.5),
            sequenceProvider
        ));
        
        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "G", 0.5),
            sequenceProvider
        ));
        
        // Ref checking disabled
        saItemValidator = new SaItemValidator(true, null);
        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "C", "A", 0.5),
            sequenceProvider
        ));
        
        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "G", 0.5),
            sequenceProvider
        ));
        
    }

    [Fact]
    public void TestCheckSnv()
    {
        var sequence         = new SimpleSequence(new string('A', 99));
        var sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh38, sequence, ChromosomeUtilities.RefNameToChromosome);

        // Strict checking throws exceptions on invalid items
        var saItemValidator = new SaItemValidator(true, true);
        Assert.Throws<InvalidDataException>(() => saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "AA", "C", 0.5),
            sequenceProvider
        ));

        Assert.Throws<InvalidDataException>(() => saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "CG", 0.5),
            sequenceProvider
        ));

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "G", 0.5),
            sequenceProvider
        ));

        // SnvCheck will not throw exceptions
        saItemValidator = new SaItemValidator(false, true);
        Assert.False(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "AA", "C", 0.5),
            sequenceProvider
        ));

        Assert.False(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "CG", 0.5),
            sequenceProvider
        ));

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "G", 0.5),
            sequenceProvider
        ));

        // SnvCheck disabled
        saItemValidator = new SaItemValidator(null, true);
        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "AA", "C", 0.5),
            sequenceProvider
        ));

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "CG", 0.5),
            sequenceProvider
        ));

        Assert.True(saItemValidator.Validate(
            new GenericScoreItem(ChromosomeUtilities.Chr1, 11, "A", "G", 0.5),
            sequenceProvider
        ));
    }
}