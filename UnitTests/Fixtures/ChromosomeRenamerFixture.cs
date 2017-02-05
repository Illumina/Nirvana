using UnitTests.Utilities;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Fixtures
{
    public class ChromosomeRenamerFixture
    {
        public readonly ChromosomeRenamer Renamer;
        public readonly ICompressedSequence Sequence;
        public readonly CompressedSequenceReader Reader;

        /// <summary>
        /// constructor
        /// </summary>
        public ChromosomeRenamerFixture()
        {
            var referenceStream = ResourceUtilities.GetReadStream(Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg.bases"));
            Sequence            = new CompressedSequence();
            Reader              = new CompressedSequenceReader(referenceStream, Sequence);
            Renamer             = Sequence.Renamer;
        }
    }

    [CollectionDefinition("ChromosomeRenamer")]
    public class ChromosomeRenamerCollection : ICollectionFixture<ChromosomeRenamerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
