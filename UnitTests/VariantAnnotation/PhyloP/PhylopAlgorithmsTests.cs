using System;
using System.IO;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.PhyloP;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.PhyloP
{
    public class PhylopAlgorithmsTests
    {
        //[Fact(Skip = "Need to refactor PhylopWriter")]
        //public void ScoreToAndFromByteArray()
        //{
            //var randShorts = GetRandShorts();

            //var scoreBytes = new byte[PhylopCommon.MaxIntervalLength * 2];

            //var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //var writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(randomPath));

            //var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");
            //using (var phylopWriter = new PhylopWriter("chr1", version, GenomeAssembly.Unknown, randShorts, writer))
            //{
            //    phylopWriter.ScoresToBytes(scoreBytes, randShorts, randShorts.Length);
            //}

            //var scores = new short[PhylopCommon.MaxIntervalLength];

            //using (var phylopReader = new PhylopReader(FileUtilities.GetReadStream(randomPath)))
            //{
            //    phylopReader.BytesToScores(scoreBytes.Length, scoreBytes, scores);
            //}

            //for (var i = 0; i < randShorts.Length; i++)
            //{
            //    Assert.Equal(randShorts[i], scores[i]);
            //}

            //File.Delete(randomPath);
        //}

        //[Fact(Skip = "Need to refactor PhylopWriter")]
        //public void WriteIntervalTest()
        //{
            //var randShorts = GetRandShorts();
            //var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //var writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(randomPath));

            //var testInterval = new PhylopInterval(100, 0, 1);
            //var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            //using (var phylopWriter = new PhylopWriter("chr1", version, GenomeAssembly.Unknown, randShorts, writer))
            //{
            //    phylopWriter.WriteInterval(testInterval, writer);
            //}

            //using (var phylopReader = new PhylopReader(FileUtilities.GetReadStream(randomPath)))
            //{
            //    var scoreCount = phylopReader.ReadIntervalScores(testInterval);
            //    Assert.Equal(randShorts.Length, scoreCount);
            //    var observedScores = phylopReader.GetAllScores();
            //    for (var i = 0; i < randShorts.Length; i++)
            //    {
            //        Assert.Equal(randShorts[i], observedScores[i]);
            //    }
            //}

            //File.Delete(randomPath);
        //}

        //[Fact(Skip = "Need to refactor PhylopWriter")]
        //public void AddScoreTest()
        //{
            //var randShorts = GetRandShorts();
            //var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //var writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(randomPath));

            //var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            //var phylopWriter = new PhylopWriter("chr1", version, GenomeAssembly.Unknown, 100, writer);

            //foreach (short s in randShorts)
            //    phylopWriter.AddScore(s); // artificially forcing the writer to flush at every 100 scores

            //// we should have 41 intervals but note that the last interval is not dumped by AddScore. Therefore, we have 40
            //Assert.Equal(40, phylopWriter.ChromosomeIntervals.Count);

            //writer.Dispose();
            //File.Delete(randomPath);
        //}

        //[Fact]
        //public void PhylopDirectoryCheck()
        //{
        //    var dataSrcVersions = new List<DataSourceVersion>();
        //    var currentDirPath = Directory.GetCurrentDirectory();
        //    PhylopDirectory phylopDir;
        //    PhylopCommon.CheckDirectoryIntegrity(currentDirPath + Path.DirectorySeparatorChar + "Resources", dataSrcVersions, out phylopDir);
        //    Assert.Equal(1, dataSrcVersions.Count);
        //}

        //[Fact]
        //public void PhylopNullDirectoryCheck()
        //{
        //    var dataSrcVersions = new List<DataSourceVersion>();
        //    PhylopDirectory phylopDir;
        //    PhylopCommon.CheckDirectoryIntegrity(null, dataSrcVersions, out phylopDir);
        //    Assert.Equal(0, dataSrcVersions.Count);
        //}

        [Fact]
        public void LoopbackTest()
        {
            var wigFixFile = new FileInfo(Resources.TopPath("mini.WigFix"));
            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            var phylopWriter = new PhylopWriter(wigFixFile.FullName, version, GenomeAssembly.Unknown, Path.GetTempPath(), 50);
			
            using (phylopWriter)
            {
                phylopWriter.ExtractPhylopScores();
            }

            var phylopReader = new PhylopReader(FileUtilities.GetReadStream(Path.GetTempPath() + Path.DirectorySeparatorChar + "chr1.npd"));

            using (phylopReader)
            {
                Assert.Equal(0.064, phylopReader.GetScore(100));//first position of first block
                Assert.Equal(0.058, phylopReader.GetScore(101));// second position
                Assert.Equal(0.064, phylopReader.GetScore(120));// some internal position
                Assert.Equal(0.058, phylopReader.GetScore(130));//last position of first block

                //moving on to the next block: should cause reloading from file
                Assert.Equal(0.064, phylopReader.GetScore(175));//first position of first block
                Assert.Equal(-2.088, phylopReader.GetScore(182));// some negative value
            }

            File.Delete(Path.GetTempPath() + Path.DirectorySeparatorChar + "chr1.npd");
        }

        // ReSharper disable once UnusedMember.Local
        private static short[] GetRandShorts()
        {
            var randShorts = new short[PhylopCommon.MaxIntervalLength];

            var randomNumberGenerator = new Random();

            for (var i = 0; i < randShorts.Length; i++)
            {
                randShorts[i] = (short)randomNumberGenerator.Next();
            }

            return randShorts;
        }
    }
}
