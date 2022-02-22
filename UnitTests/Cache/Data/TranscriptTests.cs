using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cache.Data;
using IO;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Cache.Data;

public sealed class TranscriptTests
{
    private const string ExpectedCdnaSeq =
        "ACTCCTGTTTCAGGCATGGGGCGGCTGGTTCTGCTGTGGGGAGCTGCCGTCTTTCTGCTGGGAGGCTGGATGGCTTTGGGGCAAGGAGGAGCAGCAGAAGGAGTACAGATTCAGATCATCTACTTCAATTTAGAAACCGTGCAGGTGACATGGAATGCCAGCAAATACTCCAGGACCAACCTGACTTTCCACTACAGATTCAACGGTGATGAGGCCTATGACCAGTGCACCAACTACCTTCTCCAGGAAGGTCACACTTCGGGGTGCCTCCTAGACGCAGAGCAGCGAGACGACATTCTCTATTTCTCCATCAGGAATGGGACGCACCCCGTTTTCACCGCAAGTCGCTGGATGGTTTATTACCTGAAACCCAGTTCCCCGAAGCACGTGAGATTTTCGTGGCATCAGGATGCAGTGACGGTGACGTGTTCTGACCTGTCCTACGGGGATCTCCTCTATGAGGTTCAGTACCGGAGCCCCTTCGACACCGAGTGGCAGTCCAAACAGGAAAATACCTGCAACGTCACCATAGAAGGCTTGGATGCCGAGAAGTGTTACTCTTTCTGGGTCAGGGTGAAGGCTATGGAGGATGTATATGGGCCAGACACATACCCAAGCGACTGGTCAGAGGTGACATGCTGGCAGAGAGGCGAGATTCGGGATGCCTGTGCAGAGACACCAACGCCTCCCAAACCAAAGCTGTCCAAATTTATTTTAATTTCCAGCCTGGCCATCCTTCTGATGGTGTCTCTCCTCCTTCTGTCTTTATGGAAATTATGGAGAGTGAAGAAGTTTCTCATTCCCAGCGTGCCAGACCCGAAATCCATCTTCCCCGGGCTCTTTGAGATACACCAAGGGAACTTCCAGGAGTGGATCACAGACACCCAGAACGTGGCCCACCTCCACAAGATGGCAGGTGCAGAGCAAGAAAGTGGCCCCGAGGAGCCCCTGGTAGTCCAGTTGGCCAAGACTGAAGCCGAGTCTCCCAGGATGCTGGACCCACAGACCGAGGAGAAAGAGGCCTCTGGGGGATCCCTCCAGCTTCCCCACCAGCCCCTCCAAGGCGGTGATGTGGTCACAATCGGGGGCTTCACCTTTGTGATGAATGACCGCTCCTACGTGGCGTTGTGATGGACACACCACTGTCAAAGTCAACGTCAGGATCCACGTTGACATTTAAAGACAGAGGGGACTGTCCCGGGGACTCCACACCACCATGGATGGGAAGTCTCCACGCCAATGATGGTAGGACTAGGAGACTCTGAAGACCCAGCCTCACCGCCTAATGCGGCCACTGCCCTGCTAACTTTCCCCCACATGAGTCTCTGTGTTCAAAGGCTTGATGGCAGATGGGAGCCAATTGCTCCAGGAGATTTACTCCCAGTTCCTTTTCGTGCCTGAACGTTGTCACATAAACCCCAAGGCAGCACGTCCAAAATGCTGTAAAACCATCTTCCCACTCTGTGAGTCCCCAGTTCCGTCCATGTACCATTCCCATAGCATTGGATTCTCGGAGGATTTTTTGTCTGTTTTGAGAC";

    private const string ExpectedProteinSeq =
        "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL";

    private readonly Dictionary<Gene, int>             _geneIndices             = new();
    private readonly Dictionary<TranscriptRegion, int> _transcriptRegionIndices = new();
    private readonly Dictionary<string, int>           _cdnaSeqIndices          = new();
    private readonly Dictionary<string, int>           _proteinSeqIndices       = new();

    private readonly Gene               _expectedGene = new("64109", "ENSG00000141510", true, 14281) {Symbol = "CRLF2"};
    private readonly TranscriptRegion[] _expectedTranscriptRegions;

    private readonly Gene[]   _genes;
    private readonly string[] _cdnaSeqs;
    private readonly string[] _proteinSeqs;

    public TranscriptTests()
    {
        _genes       = new[] {_expectedGene};
        _cdnaSeqs    = new[] {ExpectedCdnaSeq};
        _proteinSeqs = new[] {ExpectedProteinSeq};

        var expectedCigarOps = new[]
        {
            new CigarOp(CigarType.Match, 121),
            new CigarOp(CigarType.Insertion, 1),
            new CigarOp(CigarType.Match, 10),
            new CigarOp(CigarType.Insertion, 2),
            new CigarOp(CigarType.Match, 15)
        };

        TranscriptRegion exon6   = new(1314869, 1315014, 662, 810, TranscriptRegionType.Exon, 6, expectedCigarOps);
        TranscriptRegion intron5 = new(1315015, 1317418, 661, 662, TranscriptRegionType.Intron, 5, null);
        TranscriptRegion exon5   = new(1317419, 1317581, 499, 661, TranscriptRegionType.Exon, 5, null);
        TranscriptRegion intron4 = new(1317582, 1321271, 498, 499, TranscriptRegionType.Intron, 4, null);
        TranscriptRegion exon4   = new(1321272, 1321405, 365, 498, TranscriptRegionType.Exon, 4, null);
        TranscriptRegion intron3 = new(1321406, 1325325, 364, 365, TranscriptRegionType.Intron, 3, null);
        TranscriptRegion exon3   = new(1325326, 1325492, 198, 364, TranscriptRegionType.Exon, 3, null);
        TranscriptRegion intron2 = new(1325493, 1327698, 197, 198, TranscriptRegionType.Intron, 2, null);
        TranscriptRegion exon2   = new(1327699, 1327801, 95, 197, TranscriptRegionType.Exon, 2, null);
        TranscriptRegion intron1 = new(1327802, 1331448, 94, 95, TranscriptRegionType.Intron, 1, null);
        TranscriptRegion exon1   = new(1331449, 1331542, 1, 94, TranscriptRegionType.Exon, 1, null);

        _expectedTranscriptRegions = new[]
        {
            exon6, intron5, exon5, intron4, exon4, intron3, exon3, intron2, exon2, intron1, exon1
        };

        _cdnaSeqIndices[ExpectedCdnaSeq]       = 0;
        _proteinSeqIndices[ExpectedProteinSeq] = 0;
        _geneIndices[_expectedGene]            = 0;

        var regionIndex = 0;
        foreach (TranscriptRegion transcriptRegion in _expectedTranscriptRegions)
        {
            _transcriptRegionIndices[transcriptRegion] = regionIndex++;
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void Write_EndToEnd_ExpectedResults(bool hasCodingRegion, bool hasAminoAcidEdits)
    {
        Transcript expected = GetTranscript(hasCodingRegion, hasAminoAcidEdits);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer, _geneIndices, _transcriptRegionIndices, _cdnaSeqIndices, _proteinSeqIndices);
        }

        byte[] bytes = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        Transcript actual = Transcript.Read(ref byteSpan, expected.Chromosome, _genes, _expectedTranscriptRegions,
            _cdnaSeqs, _proteinSeqs);

        Assert.Equal(expected, actual);
    }

    private Transcript GetTranscript(bool hasCodingRegion, bool hasAminoAcidEdits)
    {
        CodingRegion? codingRegion = null;

        if (hasCodingRegion)
        {
            string           proteinId      = "NP_071431.2";
            string           proteinSeq     = ExpectedProteinSeq;
            AminoAcidEdit[]? aminoAcidEdits = null;

            if (hasAminoAcidEdits)
                aminoAcidEdits = new AminoAcidEdit[]
                {
                    new(1, 'M'),
                    new(723, '*')
                };

            codingRegion = new(1314869, 1331527, 16, 810, proteinId, proteinSeq, 0, 0, 0, aminoAcidEdits, null);
        }

        return new(ChromosomeUtilities.ChrX, 1_314_869, 1_331_542, "NM_022148.4", BioType.mRNA, true, Source.RefSeq,
            _expectedGene, _expectedTranscriptRegions, ExpectedCdnaSeq, codingRegion);
    }
}