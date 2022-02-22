using System;
using System.Collections.Generic;
using System.IO;
using Cache.Data;
using Genome;
using SAUtils.Sift;
using UnitTests.TestUtilities;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;
using Versioning;

namespace UnitTests.VariantAnnotation.PSA
{
    public static class PsaTestUtilities
    {
        public const string PepSeq1 =
            "MKKVTAEAISWNESTSETNNSMVTEFIFLGLSDSQELQTFLFMLFFVFYGGIVFGNLLIVITVVSDSHLHSPMYFLLANLSLIDLSLSSVTAPKMITDFFSQRKVISFKGCLVQIFLLHFFGGSEMVILIAMGFDRYIAICKPLHYTTIMCGNACVGIMAVTWGIGFLHSVSQLAFAVHLLFCGPNEVDSFYCDLPRVIKLACTDTYRLDIMVIANSGVLTVCSFVLLIISYTIILMTIQHRPLDKSSKALSTLTAHITVVLLFFGPCVFIYAWPFPIKSLDKFLAVFYSVITPLLNPIIYTLRNKDMKTAIRQLRKWDAHSSVKFZ";

        public const string PepSeq2 =
            "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPT";

        public const string PepSeq3 =
            "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPTLISSVHRSRHLVMPEHQSRCEFQRGSLEIGLRPAGDLLGKRLGRSPRISSDCFSEKRARSESPQEALLLPRELGPSMAPEDHYRRLVSALSEASTFEDPQRLYHLGLPSHDLLRVRQEVAAAALRGPSGLEAHLPSSTAGYGFLPPAQAEMFAWQQELLRKQNLARLELPADLLRQKELESARPQLLAPETALRPNDGAEELQRRGALLVLNHGAAPLLALPPQGPPGSGPPTPSRDSARRA";

        public const string PepSeq4 =
            "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPTLISSGCSSEGPQWPGSPPALLHGRSASEAGPGSAPGGRRPSCRPVLLGEGAASAAPLAVAAECPSRRPGPPSQAPLPGGALGSVPDPRLRLPAPRAGGDVRLAAGAPAEAEPGPAGAARRPPAAEGAGERAPTAAGARDRPAPQRRRRGAAAARGPAGAEPRRGATAGPAPPGAPGLRTPHPVPGLCPASPPEGGSRPCLSAAQRVQGDDGGZ";

        public static IEnumerable<Transcript> GetTranscripts()
        {
            var gene1 = new Gene("1", "ENSG0001", false, null);
            var gene2 = new Gene("2", "ENSG0002", false, null);

            TranscriptRegion[] regions = {new(100, 200, 100, 200, TranscriptRegionType.Exon, 1, null)};

            var transcript = new Transcript(ChromosomeUtilities.Chr1, 100, 200, "ENST00000641515", BioType.mRNA, false,
                Source.Ensembl, gene1, regions, "ABC", GetCodingRegion(PepSeq1));
            yield return transcript;

            transcript = new Transcript(ChromosomeUtilities.Chr1, 100, 200, "ENST00000437963", BioType.mRNA, false,
                Source.Ensembl, gene1, regions, "ABC", GetCodingRegion(PepSeq2));
            yield return transcript;

            transcript = new Transcript(ChromosomeUtilities.Chr1, 100, 200, "ENST00000617307", BioType.mRNA, false,
                Source.Ensembl, gene2, regions, "ABC", GetCodingRegion(PepSeq3));
            yield return transcript;

            transcript = new Transcript(ChromosomeUtilities.Chr1, 100, 200, "ENST00000618323", BioType.mRNA, false,
                Source.Ensembl, gene2, regions, "ABC", GetCodingRegion(PepSeq4));
            yield return transcript;
        }

        private static CodingRegion GetCodingRegion(string proteinSeq) =>
            new(1, 2, 3, 4, string.Empty, proteinSeq, 0, 0, 0, null, null);

        public static PsaWriter GetSiftPsaWriter(Stream psaStream, Stream indexStream)
        {
            var schemaVersion = SaCommon.PsaSchemaVersion;
            IDataSourceVersion version =
                new DataSourceVersion("Sift", "sift as a supplementary data", "6.48", DateTime.Now.Ticks);
            var assembly = GenomeAssembly.GRCh38;
            var jsonKey  = SaCommon.SiftTag;
            var header   = new SaHeader(jsonKey, assembly, version, schemaVersion);

            var psaWriter = new PsaWriter(psaStream, indexStream, header);
            return psaWriter;
        }

        public static PsaReader GetSiftPsaReader()
        {
            byte[] readBuffer;
            byte[] indexBuffer;
            var    psaStream   = new MemoryStream();
            var    indexStream = new MemoryStream();


            using (var psaWriter = GetSiftPsaWriter(psaStream, indexStream))
            {
                psaWriter.AddGeneBlocks(0, new List<PsaGeneBlock> {GetGene1Block(), GetGene2Block()});
                psaWriter.AddGeneBlocks(1, new List<PsaGeneBlock> {GetGene3Block()});
                psaWriter.Write();
            }


            readBuffer  = psaStream.GetBuffer();
            indexBuffer = indexStream.GetBuffer();
            psaStream   = new MemoryStream(readBuffer);
            indexStream = new MemoryStream(indexBuffer);
            return new PsaReader(psaStream, indexStream);
        }

        public static PsaReader GetPolyPhenPsaReader()
        {
            byte[] readBuffer;
            byte[] indexBuffer;
            var    psaStream   = new MemoryStream();
            var    indexStream = new MemoryStream();


            using (var psaWriter = GetPolyPhenPsaWriter(psaStream, indexStream))
            {
                psaWriter.AddGeneBlocks(0, new List<PsaGeneBlock> {GetGene1Block(), GetGene2Block()});
                psaWriter.AddGeneBlocks(1, new List<PsaGeneBlock> {GetGene3Block()});
                psaWriter.Write();
                readBuffer  = psaStream.GetBuffer();
                indexBuffer = indexStream.GetBuffer();
            }

            psaStream   = new MemoryStream(readBuffer);
            indexStream = new MemoryStream(indexBuffer);
            return new PsaReader(psaStream, indexStream);
        }

        public static PsaWriter GetPolyPhenPsaWriter(Stream psaStream, Stream indexStream)
        {
            var schemaVersion = SaCommon.PsaSchemaVersion;
            IDataSourceVersion version = new DataSourceVersion("PolyPhen", "PolyPhen as a supplementary data", "2",
                DateTime.Now.Ticks);
            var assembly = GenomeAssembly.GRCh38;
            var jsonKey  = SaCommon.PolyPhenTag;
            var header   = new SaHeader(jsonKey, assembly, version, schemaVersion);

            var psaWriter = new PsaWriter(psaStream, indexStream, header);
            return psaWriter;
        }

        public static ProteinChangeScores GetScoreMatrix(string id, string peptideSeq)
        {
            var scoreMatrix = new ProteinChangeScores(new List<string> {id}, peptideSeq);

            var random = new Random(peptideSeq.GetHashCode());
            for (int i = 0; i < peptideSeq.Length; i++)
            {
                var refAa = peptideSeq[i];
                foreach (var altAa in ProteinChangeScores.AllAminoAcids)
                {
                    if (altAa == refAa) continue;
                    var score = random.NextDouble();
                    var annotationItem =
                        new SiftItem("TRAN0001", i + 1, refAa, altAa, PsaUtilities.GetShortScore(score));
                    scoreMatrix.AddScore(annotationItem);
                }
            }

            return scoreMatrix;
        }

        public static bool Equals(ProteinChangeScores one, ProteinChangeScores other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(one, other)) return true;

            if (one.TranscriptIds.Equals(other.TranscriptIds) || one.ProteinLength != other.ProteinLength) return false;

            for (int i = 0; i < one.ProteinLength; i++)
            {
                for (int j = 0; j < ProteinChangeScores.NumAminoAcids; j++)
                {
                    if (one.Scores[i, j] != other.Scores[i, j]) return false;
                }
            }

            return true;
        }

        public static bool Equals(PsaGeneBlock one, PsaGeneBlock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(one, other)) return true;

            if (one.GeneName != other.GeneName) return false;
            if (one.Start    != other.Start) return false;
            if (one.End      != other.End) return false;

            foreach (var transcriptId in one.GetTranscriptIds())
            {
                var oneChangeScore    = one.GetTranscriptScores(transcriptId);
                var otherChangeScores = other.GetTranscriptScores(transcriptId);
                if (!Equals(oneChangeScore, otherChangeScores)) return false;
            }

            return true;
        }

        public static PsaGeneBlock GetGene1Block()
        {
            var geneName  = "GENE1";
            var geneBlock = new PsaGeneBlock(geneName, 500, 3600);

            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000641515",
                    "MKKVTAEAISWNESTSETNNSMVTEFIFLGLSDSQELQTFLFMLFFVFYGGIVFGNLLIVITVVSDSHLHSPMYFLLANLSLIDLSLSSVTAPKMITDFFSQRKVISFKGCLVQIFLLHFFGGSEMVILIAMGFDRYIAICKPLHYTTIMCGNACVGIMAVTWGIGFLHSVSQLAFAVHLLFCGPNEVDSFYCDLPRVIKLACTDTYRLDIMVIANSGVLTVCSFVLLIISYTIILMTIQHRPLDKSSKALSTLTAHITVVLLFFGPCVFIYAWPFPIKSLDKFLAVFYSVITPLLNPIIYTLRNKDMKTAIRQLRKWDAHSSVKFZ"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000437963",
                    "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPT"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000617307",
                    "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPTLISSVHRSRHLVMPEHQSRCEFQRGSLEIGLRPAGDLLGKRLGRSPRISSDCFSEKRARSESPQEALLLPRELGPSMAPEDHYRRLVSALSEASTFEDPQRLYHLGLPSHDLLRVRQEVAAAALRGPSGLEAHLPSSTAGYGFLPPAQAEMFAWQQELLRKQNLARLELPADLLRQKELESARPQLLAPETALRPNDGAEELQRRGALLVLNHGAAPLLALPPQGPPGSGPPTPSRDSARRA"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000618323",
                    "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPTLISSGCSSEGPQWPGSPPALLHGRSASEAGPGSAPGGRRPSCRPVLLGEGAASAAPLAVAAECPSRRPGPPSQAPLPGGALGSVPDPRLRLPAPRAGGDVRLAAGAPAEAEPGPAGAARRPPAAEGAGERAPTAAGARDRPAPQRRRRGAAAARGPAGAEPRRGATAGPAPPGAPGLRTPHPVPGLCPASPPEGGSRPCLSAAQRVQGDDGGZ"));

            return geneBlock;
        }

        public static PsaGeneBlock GetGene2Block()
        {
            var geneName  = "GENE2";
            var geneBlock = new PsaGeneBlock(geneName, 3500, 5500);

            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000641515",
                    "MKKVTAEAISWNESTSETNNSMVTEFIFLGLSDSQELQTFLFMLFFVFYGGIVFGNLLIVITVVSDSHLHSPMYFLLANLSLIDLSLSSVTAPKMITDFFSQRKVISFKGCLVQIFLLHFFGGSEMVILIAMGFDRYIAICKPLHYTTIMCGNACVGIMAVTWGIGFLHSVSQLAFAVHLLFCGPNEVDSFYCDLPRVIKLACTDTYRLDIMVIANSGVLTVCSFVLLIISYTIILMTIQHRPLDKSSKALSTLTAHITVVLLFFGPCVFIYAWPFPIKSLDKFLAVFYSVITPLLNPIIYTLRNKDMKTAIRQLRKWDAHSSVKFZ"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000437963",
                    "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPT"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000617307",
                    "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPTLISSVHRSRHLVMPEHQSRCEFQRGSLEIGLRPAGDLLGKRLGRSPRISSDCFSEKRARSESPQEALLLPRELGPSMAPEDHYRRLVSALSEASTFEDPQRLYHLGLPSHDLLRVRQEVAAAALRGPSGLEAHLPSSTAGYGFLPPAQAEMFAWQQELLRKQNLARLELPADLLRQKELESARPQLLAPETALRPNDGAEELQRRGALLVLNHGAAPLLALPPQGPPGSGPPTPSRDSARRA"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000618323",
                    "MSKGILQVHPPICDCPGCRISSPVNRGRLADKRTVALPAARNLKKERTPSFSASDGDSDGSGPTCGRRPGLKQEDGPHIRIMKRRVHTHWDVNISFREASCSQDGNLPTLISSGCSSEGPQWPGSPPALLHGRSASEAGPGSAPGGRRPSCRPVLLGEGAASAAPLAVAAECPSRRPGPPSQAPLPGGALGSVPDPRLRLPAPRAGGDVRLAAGAPAEAEPGPAGAARRPPAAEGAGERAPTAAGARDRPAPQRRRRGAAAARGPAGAEPRRGATAGPAPPGAPGLRTPHPVPGLCPASPPEGGSRPCLSAAQRVQGDDGGZ"));
            
            return geneBlock;
        }

        public static PsaGeneBlock GetGene3Block()
        {
            var geneName  = "GENE3";
            var geneBlock = new PsaGeneBlock(geneName, 3500, 5500);

            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000626873",
                    "MVSDLVIILNYDRAVEAFAKGGNLTLGGNLTVAVGPLGRNLEGNVALRSSAAVFTYCKSRGLFAGVSLEGSCLIERKETNRKFYCQDIRAYDILFGDTPRPAQAEDLYEILDSFTEKYENEGQRINARKAAREQRKSSAKELPPKPLSRPQQSSAPVQLNSGSQSNRNEYKLYPGLSSYHERVGNLNQPIEVTALYSFEGQQPGDLNFQAGDRITVISKTDSHFDWWEGKLRGQTGIFPANYVTMNZ"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000479739",
                    "MVSDLVIILNYDRAVEAFAKGGNLTLGGNLTVAVGPLGRNLEGNVALRSSAAVFTYCKSRGLFAGVSLEGSCLIERKETNRKFYCQDIRAYDILFGDTPRPAQAEDLYEILDSFTEKYENEGQRINARKAAREQRKSSVPFGFMFHSVFSENLFLZ"));
            geneBlock.TryAddProteinScores(geneName,
                GetScoreMatrix("ENST00000439645",
                    "MAEQATKSVLFVCLGNICRSPIAEAVFRKLVTDQNISENWVIDSGAVSDWNVGRSPDPRAVSCLRNHGIHTAHKARQVDKLLFNFZ"));
            
            return geneBlock;
        }
    }
}