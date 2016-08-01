using System.IO;
using Illumina.VariantAnnotation.FileHandling;
using Xunit;
using VEP = Illumina.DataDumperImport.DataStructures.VEP;
using IVD = Illumina.VariantAnnotation.DataStructures;

namespace NirvanaUnitTests.SupplementaryAnnotations
{
    public sealed class SiftPolyPhenTests
    {
        private static string Combine(string prediction, string score)
        {
            return prediction + "(" + score + ")";
        }

        [Fact]
        public void ENST00000369535_61()
        {
            // chr 1 115000001-116000000_nirvana_transcripts.json.gz
            string siftString, polyPhenString;

            using (var textReader = new StreamReader(@"Resources\ENST00000369535_Matrices.txt"))
            {
                textReader.ReadLine();
                siftString     = textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var sift     = new VEP.Sift(siftString).Convert();
            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            string prediction;
            string score;

            const int position = 61;

            sift.GetPrediction('L', position, out prediction, out score);
            Assert.Equal("deleterious(0)", Combine(prediction, score));

            sift.GetPrediction('R', position, out prediction, out score);
            Assert.Equal("deleterious(0.03)", Combine(prediction, score));

            sift.GetPrediction('P', position, out prediction, out score);
            Assert.Equal("deleterious(0)", Combine(prediction, score));

            polyPhen.GetPrediction('L', position, out prediction, out score);
            Assert.Equal("probably damaging(0.919)", Combine(prediction, score));

            polyPhen.GetPrediction('R', position, out prediction, out score);
            Assert.Equal("benign(0.343)", Combine(prediction, score));

            polyPhen.GetPrediction('P', position, out prediction, out score);
            Assert.Equal("benign(0.431)", Combine(prediction, score));
        }

        [Fact]
        public void ENST00000369535_N13()
        {
            // chr 1 115000001-116000000_nirvana_transcripts.json.gz
            string siftString, polyPhenString;
            using (var textReader = new StreamReader(@"Resources\ENST00000369535_Matrices.txt"))
            {
                textReader.ReadLine();
                siftString     = textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var sift     = new VEP.Sift(siftString).Convert();
            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            const string aminoAcid = "N";
            const int position     = 13;

            string prediction;
            string score;

            sift.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("deleterious(0)", Combine(prediction, score));

            polyPhen.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("probably damaging(0.974)", Combine(prediction, score));
        }

        [Fact]
        public void ENST00000263967_V520()
        {
            // chr 3 178000001-179000000_nirvana_transcripts.json.gz
            string siftString, polyPhenString;
            using (var textReader = new StreamReader(@"Resources\ENST00000263967_Matrices.txt"))
            {
                textReader.ReadLine();
                siftString     = textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var sift = new VEP.Sift(siftString).Convert();
            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            const string aminoAcid = "V";
            const int position = 520;

            string prediction;
            string score;

            sift.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("deleterious(0.04)", Combine(prediction, score));

            polyPhen.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("benign(0.195)", Combine(prediction, score));
        }

        [Fact]
        public void ENST00000263967_K542()
        {
            // chr 3 178000001-179000000_nirvana_transcripts.json.gz
            string siftString, polyPhenString;
            using (var textReader = new StreamReader(@"Resources\ENST00000263967_Matrices.txt"))
            {
                textReader.ReadLine();
                siftString     = textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var sift = new VEP.Sift(siftString).Convert();
            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            const string aminoAcid = "K";
            const int position     = 542;

            string prediction;
            string score;

            sift.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("tolerated(0.11)", Combine(prediction, score));

            polyPhen.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("probably damaging(0.922)", Combine(prediction, score));
        }

        [Fact]
        public void ENSP00000390987_H812_VEP79()
        {
            string siftString, polyPhenString;
            using (var textReader = new StreamReader(@"Resources\ENSP00000390987_matrices.txt"))
            {
                textReader.ReadLine();
                siftString     = textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var sift = new VEP.Sift(siftString).Convert();
            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            const string aminoAcid = "H";
            const int position     = 812;

            string prediction;
            string score;

            sift.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("deleterious(0.01)", Combine(prediction, score));

            polyPhen.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("benign(0.192)", Combine(prediction, score));
        }

        [Fact]
        public void ENSP00000390987_H812()
        {
            // chr 1 115000001-116000000_nirvana_transcripts.json.gz
            string siftString, polyPhenString;
            using (var textReader = new StreamReader(@"Resources\ENSP00000390987_matrices.txt"))
            {
                textReader.ReadLine();
                siftString     = textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var sift = new VEP.Sift(siftString).Convert();
            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            // write the Sift and PolyPhen data to disk and read it back again
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var binaryWriter = new BinaryWriter(new FileStream(randomPath, FileMode.Create)))
            {
                var writer = new ExtendedBinaryWriter(binaryWriter);
                sift.Write(writer);
                polyPhen.Write(writer);
            }

            IVD.Sift sift2;
            IVD.PolyPhen polyPhen2;
            using (var binaryReader = new BinaryReader(new FileStream(randomPath, FileMode.Open)))
            {
                var reader = new ExtendedBinaryReader(binaryReader);
                sift2     = IVD.Sift.Read(reader);
                polyPhen2 = IVD.PolyPhen.Read(reader);
            }

            if (File.Exists(randomPath)) File.Delete(randomPath);

            const string aminoAcid = "H";
            const int position     = 812;

            string prediction;
            string score;

            sift2.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("deleterious(0.01)", Combine(prediction, score));

            polyPhen2.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("benign(0.192)", Combine(prediction, score));
        }

        [Fact]
        public void ENSP00000423325_K419_PolyPhen()
        {
            string polyPhenString;
            using (var textReader = new StreamReader(@"Resources\ENSP00000423325_matrices.txt"))
            {
                textReader.ReadLine();
                textReader.ReadLine();
                polyPhenString = textReader.ReadLine();
            }

            var polyPhen = new VEP.PolyPhen(polyPhenString).Convert();

            const string aminoAcid = "K";
            const int position     = 419;

            string prediction;
            string score;

            polyPhen.GetPrediction(aminoAcid[0], position, out prediction, out score);
            Assert.Equal("unknown(0)", Combine(prediction, score));
        }

        [Fact]
        public void ENSP00000358548_T59_Sift()
        {
            string siftString, peptide;
            using (var textReader = new StreamReader(@"Resources\ENSP00000358548_matrices.txt"))
            {
                peptide    = textReader.ReadLine();
                siftString = textReader.ReadLine();
                textReader.ReadLine();
            }

            var sift = new VEP.Sift(siftString).Convert();

            string prediction;
            string score;

            // the following should return null since we are passing the ref allele as the alt
            if (peptide != null)
            {
                for (int i = 0; i < peptide.Length; i++)
                {
                    sift.GetPrediction(peptide[i], i + 1, out prediction, out score);
                    Assert.Null(prediction);
                    Assert.Null(score);
                }
            }

            const char aminoAcid = 'T';
            const int position   = 59;

            sift.GetPrediction(aminoAcid, position, out prediction, out score);
            Assert.Equal("tolerated(0.07)", Combine(prediction, score));

            // write the Sift data to disk and read it back again
            string randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var binaryWriter = new BinaryWriter(new FileStream(randomPath, FileMode.Create)))
            {
                var writer = new ExtendedBinaryWriter(binaryWriter);
                sift.Write(writer);
            }

            IVD.Sift sift2;
            using (var binaryReader = new BinaryReader(new FileStream(randomPath, FileMode.Open)))
            {
                var reader = new ExtendedBinaryReader(binaryReader);
                sift2 = IVD.Sift.Read(reader);
            }

            if (File.Exists(randomPath)) File.Delete(randomPath);

            sift2.GetPrediction(aminoAcid, position, out prediction, out score);
            Assert.Equal("tolerated(0.07)", Combine(prediction, score));
        }
    }
}
