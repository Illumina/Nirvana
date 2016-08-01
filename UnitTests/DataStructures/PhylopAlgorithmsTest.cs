using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.PhyloPScores;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public class PhylopAlgorithmsTest
    {
        [Fact]
        public void ScoreToAndFromByteArray()
        {
            var randShorts = GetRandShorts();

            var scoreBytes = new byte[PhylopCommon.MaxIntervalLength * 2];

            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var writer = new BinaryWriter(new FileStream(randomPath, FileMode.Create));

            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");
            using (var phylopWriter = new PhylopWriter("chr1", version, GenomeAssembly.Unknown, randShorts, writer))
            {
                phylopWriter.ScoresToBytes(scoreBytes, randShorts, randShorts.Length);
            }

            var scores = new short[PhylopCommon.MaxIntervalLength];

            using (var phylopReader = new PhylopReader(new BinaryReader(FileUtilities.GetFileStream(randomPath))))
            {
                phylopReader.BytesToScores(scoreBytes.Length, scoreBytes, scores);
            }

            for (int i = 0; i < randShorts.Length; i++)
            {
                Assert.Equal(randShorts[i], scores[i]);
            }

            File.Delete(randomPath);
        }

        [Fact]
        public void WriteIntervalTest()
        {
            var randShorts = GetRandShorts();
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var writer = new BinaryWriter(new FileStream(randomPath, FileMode.Create));


            var testInterval = new PhylopInterval(100, 0, 1);
            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            using (var phylopWriter = new PhylopWriter("chr1", version, GenomeAssembly.Unknown, randShorts, writer))
            {
                phylopWriter.WriteInterval(testInterval, writer);
            }

            var reader = new BinaryReader(FileUtilities.GetFileStream(randomPath));
            using (var phylopReader = new PhylopReader(reader, randShorts.Length))
            {
                phylopReader.ReadIntervalScores(testInterval);

                var observedScores = phylopReader.Scores;
                for (int i = 0; i < randShorts.Length; i++)
                {
                    Assert.Equal(randShorts[i], observedScores[i]);
                }
            }

            File.Delete(randomPath);
        }

        [Fact]
        public void AddScoreTest()
        {
            var randShorts = GetRandShorts();
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var writer = new BinaryWriter(new FileStream(randomPath, FileMode.Create));

            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            var phylopWriter = new PhylopWriter("chr1", version, GenomeAssembly.Unknown, 100, writer);

            foreach (short score in randShorts) phylopWriter.AddScore(score);

            // we should have 41 intervals but note that the last interval is not dumped by AddScore. Therefore, we have 40
            Assert.Equal(40, phylopWriter.ChromosomeIntervals.Count);

            writer.Dispose();
            File.Delete(randomPath);
        }

        [Fact]
        public void PhylopDirectoryCheck()
        {
            var dataSrcVersions = new List<DataSourceVersion>();
            var currentDirPath = Directory.GetCurrentDirectory();
            PhylopDirectory phylopDir;
            PhylopCommon.CheckDirectoryIntegrity(currentDirPath + Path.DirectorySeparatorChar + "Resources", dataSrcVersions, out phylopDir);
            Assert.Equal(1, dataSrcVersions.Count);
        }

        [Fact]
        public void LoopbackTest()
        {
            var wigFixFile = new FileInfo(@"Resources\mini.WigFix");
            var version = new DataSourceVersion("phylop", "0", DateTime.Now.Ticks, "unit test");

            var phylopWriter = new PhylopWriter(wigFixFile.FullName, version, GenomeAssembly.Unknown, Path.GetTempPath(), 50);

            using (phylopWriter)
            {
                phylopWriter.ExtractPhylopScores();
            }

            var phylopReader = new PhylopReader(new BinaryReader(FileUtilities.GetFileStream(Path.Combine(Path.GetTempPath(), "chr1.npd"))));

            using (phylopReader)
            {
                Assert.Equal("0.064", phylopReader.GetScore(100));//first position of first block
                Assert.Equal("0.058", phylopReader.GetScore(101));// second position
                Assert.Equal("0.064", phylopReader.GetScore(120));// some internal position
                Assert.Equal("0.058", phylopReader.GetScore(130));//last position of first block

                //moving on to the next block: should cause reloading from file
                Assert.Equal("0.064", phylopReader.GetScore(175));//first position of first block
                Assert.Equal("-2.088", phylopReader.GetScore(182));// some negative value
            }
            File.Delete(Path.GetTempPath() + Path.DirectorySeparatorChar + "chr1.npd");
        }

        private static short[] GetRandShorts()
        {
            var randShorts = new short[PhylopCommon.MaxIntervalLength];

            var randomNumberGenerator = new Random();

            for (int i = 0; i < randShorts.Length; i++)
            {
                randShorts[i] = (short)randomNumberGenerator.Next();
            }
            return randShorts;
        }
    }
}
