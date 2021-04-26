using System;using System.Collections.Generic;
using System.IO;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Transcript : ITranscript
    {
        public IChromosome         Chromosome        { get; }
        public int                 Start             { get; }
        public int                 End               { get; }
        public ICompactId          Id                { get; }
        public BioType             BioType           { get; }
        public bool                IsCanonical       { get; }
        public Source              Source            { get; }
        public IGene               Gene              { get; }
        public ITranscriptRegion[] TranscriptRegions { get; }
        public ushort              NumExons          { get; }
        public int                 TotalExonLength   { get; }
        public byte                StartExonPhase    { get; }
        public int                 SiftIndex         { get; }
        public int                 PolyPhenIndex     { get; }
        public ITranslation        Translation       { get; }
        public IInterval[]         MicroRnas         { get; }
        public int[]               Selenocysteines   { get; }
        public IRnaEdit[]          RnaEdits          { get; }
        public bool                CdsStartNotFound  { get; }
        public bool                CdsEndNotFound    { get; }
        public ISequence           CodingSequence    { get; set; }
        public ISequence           CdnaSequence      { get; set; }

        public Transcript(IChromosome chromosome, int start, int end, ICompactId id, ITranslation translation,
            BioType bioType, IGene gene, int totalExonLength, byte startExonPhase, bool isCanonical,
            ITranscriptRegion[] transcriptRegions, ushort numExons, IInterval[] microRnas, int siftIndex,
            int polyPhenIndex, Source source, bool cdsStartNotFound, bool cdsEndNotFound, int[] selenocysteines,
            IRnaEdit[] rnaEdits)
        {
            Chromosome        = chromosome;
            Start             = start;
            End               = end;
            Id                = id;
            Translation       = translation;
            BioType           = bioType;
            Gene              = gene;
            TotalExonLength   = totalExonLength;
            StartExonPhase    = startExonPhase;
            IsCanonical       = isCanonical;
            TranscriptRegions = transcriptRegions;
            NumExons          = numExons;
            MicroRnas         = microRnas;
            SiftIndex         = siftIndex;
            PolyPhenIndex     = polyPhenIndex;
            Source            = source;
            CdsStartNotFound  = cdsStartNotFound;
            CdsEndNotFound    = cdsEndNotFound;
            Selenocysteines   = selenocysteines;
            RnaEdits          = rnaEdits;
        }

        // SET-362 DEBUG: Remove the sequenceProvider argument in the future
        public static ITranscript Read(BufferedBinaryReader reader,
            IDictionary<ushort, IChromosome> chromosomeIndexDictionary, IGene[] cacheGenes,
            ITranscriptRegion[] cacheTranscriptRegions, IInterval[] cacheMirnas, string[] cachePeptideSeqs,
            ISequenceProvider sequenceProvider)
        {
            // transcript
            ushort referenceIndex = reader.ReadOptUInt16();
            int    start          = reader.ReadOptInt32();
            int    end            = reader.ReadOptInt32();
            var    id             = CompactId.Read(reader);

            // gene
            int geneIndex = reader.ReadOptInt32();
            var gene      = cacheGenes[geneIndex];

            // encoded data
            var encoded = EncodedTranscriptData.Read(reader);

            // transcript regions
            ITranscriptRegion[] transcriptRegions =
                encoded.HasTranscriptRegions ? ReadIndices(reader, cacheTranscriptRegions) : null;
            ushort numExons = reader.ReadOptUInt16();

            // protein function predictions
            int siftIndex     = encoded.HasSift ? reader.ReadOptInt32() : -1;
            int polyphenIndex = encoded.HasPolyPhen ? reader.ReadOptInt32() : -1;

            // translation
            var translation = encoded.HasTranslation ? DataStructures.Translation.Read(reader, cachePeptideSeqs) : null;

            // attributes
            IInterval[] mirnas          = encoded.HasMirnas ? ReadIndices(reader, cacheMirnas) : null;
            IRnaEdit[]  rnaEdits        = encoded.HasRnaEdits ? ReadItems(reader,        RnaEdit.Read) : null;
            int[]       selenocysteines = encoded.HasSelenocysteines ? ReadItems(reader, x => x.ReadOptInt32()) : null;

            var chromosome = chromosomeIndexDictionary[referenceIndex];

            byte startExonPhase = encoded.StartExonPhase;

            if (sequenceProvider.Assembly == GenomeAssembly.GRCh37)
            {
                bool updatedGeneModel = false;

                if (id.WithVersion == "NM_022148.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "AATTCGGCACGAGG"),
                        new RnaEdit(770, 769, "A"),
                        new RnaEdit(772, 772, "G"),
                        new RnaEdit(774, 774, "A"),
                        new RnaEdit(777, 777, "A"),
                        new RnaEdit(779, 779, "T"),
                        new RnaEdit(780, 779, "TT"),
                        new RnaEdit(783, 783, "A"),
                        new RnaEdit(785, 785, "T"),
                        new RnaEdit(788, 790, "CAG"),
                        new RnaEdit(795, 794,
                            "CCAGACCCGAAATCCATCTTCCCCGGGCTCTTTGAGATACACCAAGGGAACTTCCAGGAGTGGATCACAGACACCCAGAACGTGGCCCACCTCCACAAGATGGCAGGTGCAGAGCAAGAAAGTGGCCCCGAGGAGCCCCTGGTAGTCCAGTTGGCCAAGACTGAAGCCGAGTCTCCCAGGATGCTGGACCCACAGACCGAGGAGAAAGAGGCCTCTGGGGGATCCCTCCAGCTTCCCCACCAGCCCCTCCAAGGCGGTGATGTGGTCACAATCGGGGGCTTCACCTTTGTGATGAATGACCGCTCCTACGTGGCGTTGTGATGGACACACCACTGTCAAAGTCAACGTCAGGATCCACGTTGACATTTAAAGACAGAGGGGACTGTCCCGGGGACTCCACACCACCATGGATGGGAAGTCTCCACGCCAATGATGGTAGGACTAGGAGACTCTGAAGACCCAGCCTCACCGCCTAATGCGGCCACTGCCCTGCTAACTTTCCCCCACATGAGTCTCTGTGTTCAAAGGCTTGATGGCAGATGGGAGCCAATTGCTCCAGGAGATTTACTCCCAGTTCCTTTTCGTGCCTGAACGTTGTCACATAAACCCCAAGGCAGCACGTCCAAAATGCTGTAAAACCATCTTCCCACTCTGTGAGTCCCCAGTTCCGTCCATGTACCTGTTCCATAGCATTGGATTCTCGGAGGATTTTTTGTCTGTTTTGAGACTCCAAACCACCTCTACCCCTACAAAAAAAAAAAAAAAAAA")
                    };
                    
                    if (chromosome.UcscName == "chrX")
                    {
                        // we have two RNA-edit insertions in exon 6 - so it's split into three intervals
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1314869, 1314883, 797, 811),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1314884, 1314893, 785, 794),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1314894, 1315014, 663, 783),
                            new TranscriptRegion(TranscriptRegionType.Intron, 5, 1315015, 1317418, 662, 663),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1317419, 1317581, 500, 662),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1317582, 1321271, 499, 500),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1321272, 1321405, 366, 499),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1321406, 1325325, 365, 366),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1325326, 1325492, 199, 365),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1325493, 1327698, 198, 199),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1327699, 1327801, 96,  198),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1327802, 1331448, 95,  96),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1331449, 1331529, 15,  95)
                        };
                        
                        // covers 17-811, 812-1132 are covered by RNA-edit
                        var codingRegion = new CodingRegion(1314869, 1331527, 17, 1132, 1116);

                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");

                        updatedGeneModel = true;
                    }

                    if (chromosome.UcscName == "chrY")
                    {
                        // we have two RNA-edit insertions in exon 6 - so it's split into three intervals
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1264869, 1264883, 797, 811),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1264884, 1264893, 785, 794),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1264894, 1265014, 663, 783),
                            new TranscriptRegion(TranscriptRegionType.Intron, 5, 1265015, 1267418, 662, 663),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1267419, 1267581, 500, 662),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1267582, 1271271, 499, 500),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1271272, 1271405, 366, 499),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1271406, 1275325, 365, 366),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1275326, 1275492, 199, 365),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1275493, 1277698, 198, 199),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1277699, 1277801, 96,  198),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1277802, 1281448, 95,  96),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1281449, 1281529, 15,  95)
                        };

                        // covers 17-811, 812-1132 are covered by RNA-edit
                        var codingRegion = new CodingRegion(1264869, 1281527, 17, 1132, 1116);

                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");

                        updatedGeneModel = true;
                    }
                }

                if (id.WithVersion == "NM_012234.6")
                {
                    // first exon starts at 72495647, so the genomic portion of the coding region is clipped
                    var codingRegion = new CodingRegion(72427536, 72495647, 184, 870, 688);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MTMGDKKSPTRPKRQAKPAADEGFWDCSVCTFRNSAEAFKCSICDVRKGTSTRKPRINSQLVAQQVAQQYATPPPPKKEKKEKVEKQDKEKPEKDKEISPSVTKKNTNKKTKPKSDILKDPPSEANSIQSANATTKTSETNHTSRPRLKNVDRSTAQQLAVTVGNVTVIITDFKEKTRSSSTSSSTVTSSAGSEQQNQSSSGSESTDKGSSRSSTPKGDMSAVNDESF*");

                    updatedGeneModel = true;
                }

                // NM_001220773.1
                if (id.WithVersion == "NM_001220773.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAA"),
                        new RnaEdit(5,    8,    null),
                        new RnaEdit(5457, 5456, "AAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50455032, 50455035, 202, 205),
                        new TranscriptRegion(TranscriptRegionType.Gap,    1, 50455036, 50455039, 205, 206),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50455040, 50455168, 206, 334),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50455169, 50459426, 334, 335),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459427, 50459561, 335, 469),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50459562, 50467615, 469, 470),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50467616, 50472799, 470, 5653)
                    };

                    var codingRegion = new CodingRegion(50455032, 50468325, 169, 1179, 1011);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_152756.3")
                {
                    var newRnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,    0,    "GG"),
                        new RnaEdit(3196, 3196, "T")
                    };

                    rnaEdits = newRnaEdits;

                    var oldCodingRegion = translation.CodingRegion;
                    var codingRegion    = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, 25, 5151, 5127);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        translation.PeptideSeq);
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001242758.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(40,   40,   "T"),
                        new RnaEdit(287,  287,  "A"),
                        new RnaEdit(355,  355,  "A"),
                        new RnaEdit(366,  366,  "C"),
                        new RnaEdit(383,  383,  "C"),
                        new RnaEdit(385,  385,  "A"),
                        new RnaEdit(425,  425,  "A"),
                        new RnaEdit(469,  469,  "C"),
                        new RnaEdit(573,  573,  "A"),
                        new RnaEdit(605,  605,  "T"),
                        new RnaEdit(611,  611,  "C"),
                        new RnaEdit(622,  623,  "CG"),
                        new RnaEdit(629,  629,  "T"),
                        new RnaEdit(639,  639,  "G"),
                        new RnaEdit(643,  643,  "C"),
                        new RnaEdit(643,  644,  "CG"),
                        new RnaEdit(654,  655,  "CG"),
                        new RnaEdit(1161, 1161, "T"),
                        new RnaEdit(1324, 1324, "G"),
                        new RnaEdit(1380, 1380, "T"),
                        new RnaEdit(1492, 1492, "G"),
                        new RnaEdit(1580, 1580, "T"),
                        new RnaEdit(1588, 1589, "CG")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_002447.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(3847, 3847, "G"),
                        new RnaEdit(4773, 4772, "AAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_005228.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(2955, 2955, "C"),
                        new RnaEdit(5601, 5600, "AAAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_005922.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(612,  612,  "A"),
                        new RnaEdit(5485, 5484, "AAAAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_006724.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(612,  612,  "A"),
                        new RnaEdit(5335, 5334, "AAAAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_019063.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1109, 1109, "G"),
                        new RnaEdit(1406, 1406, "G"),
                        new RnaEdit(5550, 5549, "AAAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_175741.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(220, 220, "T"),
                        new RnaEdit(380, 380, "C")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NR_003085.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1703, 1703, "G"),
                        new RnaEdit(2832, 2831, "AAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001244937.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(3700, 3700, "G"),
                        new RnaEdit(4626, 4625, "AAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001278433.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0, "GAGCTGTGGTGGGCTCCACCCAGTTCGAGCTTCCCGGCTGCTTTGGTTACCTAATCAAGCCTGGGCAATGGCAGGCGCCCCTCCCCCAGCCTCGCTGCCGCCTTGCAGTTTGATCTCAGACTGCTGTGCTAGCAATCAGCGAGACTCCGTGGGCGTAGGACCCTCCGAGC"),
                        new RnaEdit(4138, 4137, "AAAAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1,  66511532, 66511717, 171,  356),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1,  66511718, 66518896, 356,  357),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2,  66518897, 66519067, 357,  527),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2,  66519068, 66519865, 527,  528),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3,  66519866, 66519957, 528,  619),
                        new TranscriptRegion(TranscriptRegionType.Intron, 3,  66519958, 66520156, 619,  620),
                        new TranscriptRegion(TranscriptRegionType.Exon,   4,  66520157, 66520218, 620,  681),
                        new TranscriptRegion(TranscriptRegionType.Intron, 4,  66520219, 66521052, 681,  682),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5,  66521053, 66521099, 682,  728),
                        new TranscriptRegion(TranscriptRegionType.Intron, 5,  66521100, 66521894, 728,  729),
                        new TranscriptRegion(TranscriptRegionType.Exon,   6,  66521895, 66522053, 729,  887),
                        new TranscriptRegion(TranscriptRegionType.Intron, 6,  66522054, 66523980, 887,  888),
                        new TranscriptRegion(TranscriptRegionType.Exon,   7,  66523981, 66524041, 888,  948),
                        new TranscriptRegion(TranscriptRegionType.Intron, 7,  66524042, 66525010, 948,  949),
                        new TranscriptRegion(TranscriptRegionType.Exon,   8,  66525011, 66525132, 949,  1070),
                        new TranscriptRegion(TranscriptRegionType.Intron, 8,  66525133, 66526060, 1070, 1071),
                        new TranscriptRegion(TranscriptRegionType.Exon,   9,  66526061, 66526142, 1071, 1152),
                        new TranscriptRegion(TranscriptRegionType.Intron, 9,  66526143, 66526417, 1152, 1153),
                        new TranscriptRegion(TranscriptRegionType.Exon,   10, 66526418, 66529572, 1153, 4307)
                    };
                    
                    startExonPhase = 0;

                    var codingRegion = new CodingRegion(66511541, 66526590, 180, 1325, 1146);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MESGSTAASEEARSLRECELYVQKHNIQALLKDSIVQLCTARPERPMAFLREYFERLEKEEAKQIQNLQKAGTRTDSREDEISPPPPNPVVKGRRRRGAISAEVYTEEDAASYVRKVIPKDYKTMAALAKAIEKNVLFSHLDDNERSDIFDAMFSVSFIAGETVIQQGDEGDNFYVIDQGETDVYVNNEWATSVGEGGSFGELALIYGTPRAATVKAKTNVKLWGIDRDSYRRILMGSTLRKRKMYEEFLSKVSILESLDKWERLTVADALEPVQFEDGQKIVVQGEPGDEFFIILEGSAAVLQRRSENEEFVEVGRLGPSDYFGEIALLMNRPRAATVVARGPLKCVKLDRPRFERVLGPCSDILKRNIQQYNSFVSLSV*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001260.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0, "GGG")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1,  26828756, 26828906, 4,    154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1,  26828907, 26911703, 154,  155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2,  26911704, 26911779, 155,  230),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2,  26911780, 26923208, 230,  231),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3,  26923209, 26923319, 231,  341),
                        new TranscriptRegion(TranscriptRegionType.Intron, 3,  26923320, 26927876, 341,  342),
                        new TranscriptRegion(TranscriptRegionType.Exon,   4,  26927877, 26928017, 342,  482),
                        new TranscriptRegion(TranscriptRegionType.Intron, 4,  26928018, 26956950, 482,  483),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5,  26956951, 26957008, 483,  540),
                        new TranscriptRegion(TranscriptRegionType.Intron, 5,  26957009, 26959347, 540,  541),
                        new TranscriptRegion(TranscriptRegionType.Exon,   6,  26959348, 26959479, 541,  672),
                        new TranscriptRegion(TranscriptRegionType.Intron, 6,  26959480, 26967503, 672,  673),
                        new TranscriptRegion(TranscriptRegionType.Exon,   7,  26967504, 26967647, 673,  816),
                        new TranscriptRegion(TranscriptRegionType.Intron, 7,  26967648, 26970421, 816,  817),
                        new TranscriptRegion(TranscriptRegionType.Exon,   8,  26970422, 26970491, 817,  886),
                        new TranscriptRegion(TranscriptRegionType.Intron, 8,  26970492, 26971289, 886,  887),
                        new TranscriptRegion(TranscriptRegionType.Exon,   9,  26971290, 26971362, 887,  959),
                        new TranscriptRegion(TranscriptRegionType.Intron, 9,  26971363, 26974589, 959,  960),
                        new TranscriptRegion(TranscriptRegionType.Exon,   10, 26974590, 26974687, 960,  1057),
                        new TranscriptRegion(TranscriptRegionType.Intron, 10, 26974688, 26975405, 1057, 1058),
                        new TranscriptRegion(TranscriptRegionType.Exon,   11, 26975406, 26975484, 1058, 1136),
                        new TranscriptRegion(TranscriptRegionType.Intron, 11, 26975485, 26975602, 1136, 1137),
                        new TranscriptRegion(TranscriptRegionType.Exon,   12, 26975603, 26975761, 1137, 1295),
                        new TranscriptRegion(TranscriptRegionType.Intron, 12, 26975762, 26978092, 1295, 1296),
                        new TranscriptRegion(TranscriptRegionType.Exon,   13, 26978093, 26978569, 1296, 1772)
                    };

                    var codingRegion = new CodingRegion(26828779, 26978218, 27, 1421, 1395);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDYDFKVKLSSERERVEDLFEYEGCKVGRGTYGHVYKAKRKDGKDDKDYALKQIEGTGISMSACREIALLRELKHPNVISLQKVFLSHADRKVWLLFDYAEHDLWHIIKFHRASKANKKPVQLPRGMVKSLLYQILDGIHYLHANWVLHRDLKPANILVMGEGPERGRVKIADMGFARLFNSPLKPLADLDPVVVTFWYRAPELLLGARHYTKAIDIWAIGCIFAELLTSEPIFHCRQEDIKTSNPYHHDQLDRIFNVMGFPADKDWEDIKKMPEHSTLMKDFRRNTYTNCSLIKYMEKHKVKPDSKAFHLLQKLLTMDPIKRITSEQAMQDPYFLEDPLPTSDVFAGCQIPYPKREFLTEEEPDDKGDKKNQQQQQGNNHTNGTGHPGNQDSSHTQGPPLKKVRVVPPTTTSGGLIMTSDYQRSNPHAAYPNPGPSTSQPQSSMGYSATSQQPPQYSHQTHRY*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_000314.4")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(667,  667,  null),
                        new RnaEdit(707,  707,  "C"),
                        new RnaEdit(5547, 5546, "AAAAAAAAAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 89623195, 89623860, 1,    666),
                        new TranscriptRegion(TranscriptRegionType.Gap,    1, 89623861, 89623861, 666,  667),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 89623862, 89624305, 667,  1110),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 89624306, 89653781, 1110, 1111),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 89653782, 89653866, 1111, 1195),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 89653867, 89685269, 1195, 1196),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 89685270, 89685314, 1196, 1240),
                        new TranscriptRegion(TranscriptRegionType.Intron, 3, 89685315, 89690802, 1240, 1241),
                        new TranscriptRegion(TranscriptRegionType.Exon,   4, 89690803, 89690846, 1241, 1284),
                        new TranscriptRegion(TranscriptRegionType.Intron, 4, 89690847, 89692769, 1284, 1285),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5, 89692770, 89693008, 1285, 1523),
                        new TranscriptRegion(TranscriptRegionType.Intron, 5, 89693009, 89711874, 1523, 1524),
                        new TranscriptRegion(TranscriptRegionType.Exon,   6, 89711875, 89712016, 1524, 1665),
                        new TranscriptRegion(TranscriptRegionType.Intron, 6, 89712017, 89717609, 1665, 1666),
                        new TranscriptRegion(TranscriptRegionType.Exon,   7, 89717610, 89717776, 1666, 1832),
                        new TranscriptRegion(TranscriptRegionType.Intron, 7, 89717777, 89720650, 1832, 1833),
                        new TranscriptRegion(TranscriptRegionType.Exon,   8, 89720651, 89720875, 1833, 2057),
                        new TranscriptRegion(TranscriptRegionType.Intron, 8, 89720876, 89725043, 2057, 2058),
                        new TranscriptRegion(TranscriptRegionType.Exon,   9, 89725044, 89728532, 2058, 5546)
                    };

                    var codingRegion = new CodingRegion(89624227, 89725229, 1032, 2243, 1212);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MTAIIKEIVSRNKRRYQEDGFDLDLTYIYPNIIAMGFPAERLEGVYRNNIDDVVRFLDSKHKNHYKIYNLCAERHYDTAKFNCRVAQYPFEDHNPPQLELIKPFCEDLDQWLSEDDNHVAAIHCKAGKGRTGVMICAYLLHRGKFLKAQEALDFYGEVRTRDKKGVTIPSQRRYVYYYSYLLKNHLDYRPVALLFHKMMFETIPMFSGGTCNPQFVVCQLKVKIYSSNSGPTRREDKFMYFEFPQPLPVCGDIKVEFFHKQNKMLKKDKMFHFWVNTFFIPGPEETSEKVENGSLCDQEIDSICSIERADNDKEYLVLTLTKNDLDKANKDKANRYFSPNFKVKLYFTKTVEEPSNPEASSSTSVTPDVSDNEPDHYRYSDTTDSDPENEPFDEDQHTQITKV*");
                    
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_000535.5")
                {
                    rnaEdits    = new IRnaEdit[2];
                    rnaEdits[0] = new RnaEdit(1708, 1708, "G");
                    rnaEdits[1] = new RnaEdit(2837, 2836, "AAAAAAAAAAAAAAA");

                    var oldCodingRegion = translation.CodingRegion;
                    var codingRegion    = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, 88, 2676, 2589);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        translation.PeptideSeq);

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_000545.5")
                {
                    rnaEdits    = new IRnaEdit[2];
                    rnaEdits[0] = new RnaEdit(1743, 1743, "G");
                    rnaEdits[1] = new RnaEdit(3240, 3239, "AA");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001145076.1")
                {
                    rnaEdits    = new IRnaEdit[3];
                    rnaEdits[0] = new RnaEdit(935,  935,  "G");
                    rnaEdits[1] = new RnaEdit(1232, 1232, "G");
                    rnaEdits[2] = new RnaEdit(5376, 5375, "AAAAAAAAAAAAAAAA");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220765.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,    0,    "GAATTCCGGCGT"),
                        new RnaEdit(6,    5,    "A"),
                        new RnaEdit(16,   16,   "T"),
                        new RnaEdit(97,   97,   "C"),
                        new RnaEdit(316, 315, "CCAGTAATGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367353, 209, 328)
                    };
                    
                    var codingRegion = new CodingRegion(50358658, 50367353, 169, 1602, 1434);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                    
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220766.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,    0,    "GAATTCCGGCGT"),
                        new RnaEdit(6,    5,    "A"),
                        new RnaEdit(16,   16,   "T"),
                        new RnaEdit(97,   97,   "C"),
                        new RnaEdit(317,   318,   null),
                        new RnaEdit(321, 320, "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367354, 209, 329),
                        new TranscriptRegion(TranscriptRegionType.Gap,    3, 50367355, 50367356, 329, 330),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367357, 50367358, 330, 331)
                    };
                    
                    var codingRegion = new CodingRegion(50358658, 50367358, 169, 1467, 1299);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220767.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,  0,  "GAATTCCGGCGT"),
                        new RnaEdit(6,  5,  "A"),
                        new RnaEdit(16, 16, "T"),
                        new RnaEdit(97, 97, "C"),
                        new RnaEdit(319, 318,
                            "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367354, 209, 329),
                        new TranscriptRegion(TranscriptRegionType.Gap,    3, 50367355, 50367356, 329, 330),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367357, 50367358, 330, 331)
                    };

                    // last exon ends before coding region finished
                    var codingRegion = new CodingRegion(50358658, 50367358, 169, 1437, 1269);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220769.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,    0,    "GAATTCCGGCGT"),
                        new RnaEdit(6,    5,    "A"),
                        new RnaEdit(16,   16,   "T"),
                        new RnaEdit(97,   97,   "C"),
                        new RnaEdit(317,  318,  null),
                        new RnaEdit(321, 320,"AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367354, 209, 329),
                        new TranscriptRegion(TranscriptRegionType.Gap,  3, 50367355, 50367356, 329, 330),
                        new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367357, 50367358, 330, 331)
                    };
                    
                    var codingRegion = new CodingRegion(50358658, 50367358, 169, 1341, 1173);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220770.1" && start == 50344378)
                {
                    // final RNA-edit offset by 2 to compensate for deletion
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(317, 318, null),
                        new RnaEdit(321, 320, "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367354, 209, 329),
                        new TranscriptRegion(TranscriptRegionType.Gap,    3, 50367355, 50367356, 329, 330),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367357, 50367358, 330, 331)
                    };
                    
                    var codingRegion = new CodingRegion(50358658, 50367358, 169, 1311, 1143);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220768.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(316, 315, "CCA"),
                        new RnaEdit(320, 319, "TGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367353, 209, 328),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367354, 50367357, 332, 335)
                    };
                    
                    var codingRegion = new CodingRegion(50358658, 50367357, 169, 1467, 1299);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                    
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_006060.4" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(316, 315, "CCA"),
                        new RnaEdit(320, 319, "TGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367353, 209, 328),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367354, 50367357, 332, 335)
                    };
                    
                    var codingRegion = new CodingRegion(50358658, 50367357, 169, 1728, 1560);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220775.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAG"),
                        new RnaEdit(4,    3, "C"),
                        new RnaEdit(5325, 5324, "AAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459422, 50459561, 204, 343),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50459562, 50467615, 343, 344),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50467616, 50472799, 344, 5527)
                    };

                    startExonPhase = 0;

                    // first exon starts at 50459422, so the genomic portion of the coding region is clipped
                    var codingRegion = new CodingRegion(50459422, 50468325, 169, 1053, 885);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                    
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220774.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAA"),
                        new RnaEdit(5,    8,    null),
                        new RnaEdit(5427, 5426, "AAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50455032, 50455035, 202, 205),
                        new TranscriptRegion(TranscriptRegionType.Gap,    1, 50455036, 50455039, 205, 206),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50455040, 50455168, 206, 334),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50455169, 50459426, 334, 335),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50459427, 50459531, 335, 439),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50459532, 50467615, 439, 440),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50467616, 50472799, 440, 5623)
                    };

                    // first exon starts at 50455032, so the genomic portion of the coding region is clipped
                    var codingRegion = new CodingRegion(50455032, 50468325, 169, 1149, 981);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220776.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAGT"),
                        new RnaEdit(3,  3,  "C"),
                        new RnaEdit(5295, 5294, "AAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459422, 50459531, 204, 313),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50459532, 50467615, 313, 314),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50467616, 50472799, 314, 5497)
                    };
                    
                    startExonPhase = 0;

                    // first exon starts at 50459422, so the genomic portion of the coding region is clipped
                    var codingRegion = new CodingRegion(50459422, 50468325, 169, 1023, 855);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                    
                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220772.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAGTTT"),
                        new RnaEdit(5188, 5187, "AAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon, 1, 50467613, 50472799, 206, 5392)
                    };

                    startExonPhase = 0;

                    // first exon starts at 50467613, so the genomic portion of the coding region is clipped
                    var codingRegion = new CodingRegion(50467613, 50468325, 169, 918, 750);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (id.WithVersion == "NM_001220771.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,  0,  "GAATTCCGGCGT"),
                        new RnaEdit(6,  5,  "A"),
                        new RnaEdit(16, 16, "T"),
                        new RnaEdit(97, 97, "C"),
                        new RnaEdit(316, 315, "CCAGTAATGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2, 50358644, 50358697, 155, 208),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2, 50358698, 50367233, 208, 209),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3, 50367234, 50367353, 209, 328)
                    };
                    
                    startExonPhase = 0;

                    var codingRegion = new CodingRegion(50358658, 50468325, 169, 1299, 1131);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (updatedGeneModel)
                {
                    TranscriptValidator.CheckTranscriptRegions(id.WithVersion, transcriptRegions);
                    TranscriptValidator.Validate(sequenceProvider, chromosome, id.WithVersion, gene.OnReverseStrand,
                        transcriptRegions, rnaEdits, translation);

                    int newStart = transcriptRegions[0].Start;
                    int newEnd   = transcriptRegions[transcriptRegions.Length - 1].End;

                    if (newStart != start)
                    {
                        Console.WriteLine($"Found new start for {id.WithVersion}: old: {start:N0}, new: {newStart:N0}");
                        // start = newStart;
                    }

                    if (newEnd != end)
                    {
                        Console.WriteLine($"Found new end for {id.WithVersion}: old: {end:N0}, new: {newEnd:N0}");
                        // end = newEnd;
                    }

                    if (newStart < gene.Start)
                    {
                        Console.WriteLine($"Found new GENE start for {gene.Symbol}: old: {gene.Start:N0}, new: {newStart:N0}");
                        // gene.Start = newStart;
                    }

                    if (newEnd > gene.End)
                    {
                        Console.WriteLine($"Found new GENE end for {gene.Symbol}: old: {gene.End:N0}, new: {newEnd:N0}");
                        // gene.End = newEnd;
                    }
                }
            }

            return new Transcript(chromosomeIndexDictionary[referenceIndex], start, end, id, translation,
                encoded.BioType, gene, ExonUtilities.GetTotalExonLength(transcriptRegions), startExonPhase,
                encoded.IsCanonical, transcriptRegions, numExons, mirnas, siftIndex, polyphenIndex,
                encoded.TranscriptSource, encoded.CdsStartNotFound, encoded.CdsEndNotFound, selenocysteines, rnaEdits);
        }

        /// <summary>
        /// writes the transcript to the binary writer
        /// </summary>
        public void Write(IExtendedBinaryWriter writer, Dictionary<IGene, int> geneIndices,
            Dictionary<ITranscriptRegion, int> transcriptRegionIndices, Dictionary<IInterval, int> microRnaIndices,
            Dictionary<string, int> peptideIndices)
        {
            // transcript
            writer.WriteOpt(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            Id.Write(writer);
            
            // gene
            writer.WriteOpt(GetIndex(Gene, geneIndices));

            // encoded data
            var encoded = EncodedTranscriptData.GetEncodedTranscriptData(BioType, CdsStartNotFound, CdsEndNotFound,
                Source, IsCanonical, SiftIndex != -1, PolyPhenIndex != -1, MicroRnas != null, RnaEdits != null,
                Selenocysteines != null, TranscriptRegions != null, Translation != null, StartExonPhase);
            encoded.Write(writer);

            // transcript regions
            if (encoded.HasTranscriptRegions) WriteIndices(writer, TranscriptRegions, transcriptRegionIndices);
            writer.WriteOpt(NumExons);

            // protein function predictions
            if (encoded.HasSift) writer.WriteOpt(SiftIndex);
            if (encoded.HasPolyPhen) writer.WriteOpt(PolyPhenIndex);

            // translation
            if (encoded.HasTranslation)
            {
                // ReSharper disable once PossibleNullReferenceException
                var peptideIndex = GetIndex(Translation.PeptideSeq, peptideIndices);
                Translation.Write(writer, peptideIndex);
            }

            // attributes
            if (encoded.HasMirnas)          WriteIndices(writer, MicroRnas, microRnaIndices);
            if (encoded.HasRnaEdits)        WriteItems(writer, RnaEdits, (x, y) => x.Write(y));
            if (encoded.HasSelenocysteines) WriteItems(writer, Selenocysteines, (x, y) => y.WriteOpt(x));
        }

        private static T[] ReadItems<T>(BufferedBinaryReader reader, Func<BufferedBinaryReader, T> readFunc)
        {
            int numItems = reader.ReadOptInt32();
            var items    = new T[numItems];
            for (int i = 0; i < numItems; i++) items[i] = readFunc(reader);
            return items;
        }

        private static void WriteItems<T>(IExtendedBinaryWriter writer, T[] items, Action<T, IExtendedBinaryWriter> writeAction)
        {
            writer.WriteOpt(items.Length);
            foreach (var item in items) writeAction(item, writer);
        }

        private static T[] ReadIndices<T>(IBufferedBinaryReader reader, T[] cachedItems)
        {
            int numItems = reader.ReadOptInt32();
            var items = new T[numItems];

            for (int i = 0; i < numItems; i++)
            {
                var index = reader.ReadOptInt32();
                items[i] = cachedItems[index];
            }

            return items;
        }

        private static void WriteIndices<T>(IExtendedBinaryWriter writer, T[] items, IReadOnlyDictionary<T, int> indices)
        {
            writer.WriteOpt(items.Length);
            foreach (var item in items) writer.WriteOpt(GetIndex(item, indices));
        }

        private static int GetIndex<T>(T item, IReadOnlyDictionary<T, int> indices)
        {
            if (item == null) return -1;

            if (!indices.TryGetValue(item, out var index))
            {
                throw new InvalidDataException($"Unable to locate the {typeof(T)} in the indices: {item}");
            }

            return index;
        }
    }
}