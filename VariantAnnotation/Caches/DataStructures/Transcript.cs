using System;
using System.Collections.Generic;
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
        public IRnaEdit[]          RnaEdits          { get; }
        public AminoAcidEdit[]     AminoAcidEdits    { get; set; }
        public bool                CdsStartNotFound  { get; }
        public bool                CdsEndNotFound    { get; }
        public ISequence           CodingSequence    { get; set; }
        public ISequence           CdnaSequence      { get; set; }

        public Transcript(IChromosome chromosome, int start, int end, ICompactId id, ITranslation translation,
            BioType bioType, IGene gene, int totalExonLength, byte startExonPhase, bool isCanonical,
            ITranscriptRegion[] transcriptRegions, ushort numExons, IInterval[] microRnas, int siftIndex,
            int polyPhenIndex, Source source, bool cdsStartNotFound, bool cdsEndNotFound,
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

            byte   startExonPhase = encoded.StartExonPhase;
            string transcriptId   = id.WithVersion;

            if (sequenceProvider.Assembly == GenomeAssembly.GRCh37)
            {
                bool updatedGeneModel = false;

                if (transcriptId == "NM_022148.2")
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

                if (transcriptId == "NM_001012288.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(665, 664, "A"),
                        new RnaEdit(667, 667, "G"),
                        new RnaEdit(672, 672, "A"),
                        new RnaEdit(674, 674, "T"),
                        new RnaEdit(675, 674, "TT"),
                        new RnaEdit(678, 678, "A"),
                        new RnaEdit(680, 680, "T"),
                        new RnaEdit(683, 685, "CAG"),
                        new RnaEdit(690, 689,
                            "CCAGACCCGAAATCCATCTTCCCCGGGCTCTTTGAGATACACCAAGGGAACTTCCAGGAGTGGATCACAGACACCCAGAACGTGGCCCACCTCCACAAGATGGCAGGTGCAGAGCAAGGAAGTGGCCCTGAGGAGCCCCTGGTGGTCCAGTTGGCCAAGACTGAAGCCGAGTCCCCCAGGATGCTGGACCCACAGACCGAGGAGAAAGAGGCCTCTGGGGGATCCCTCCAGCTTCCCCACCAGCCCCTCCAAGGTGGTGATGTGGTCACAATCGGGGACTTCACCTTTGTGATGAATGACCGCTCCTACGTGGCGTTGTGA"),
                    };

                    if (chromosome.UcscName == "chrX")
                    {
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1314869, 1314883, 678, 692),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1314884, 1314893, 666, 675),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1314894, 1315014, 544, 664),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1315015, 1317418, 543, 544),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1317419, 1317581, 381, 543),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1317582, 1321271, 380, 381),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1321272, 1321405, 247, 380),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1321406, 1325325, 246, 247),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1325326, 1325492, 80,  246),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1325493, 1331448, 79,  80),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1331449, 1331527, 1,   79),
                        };

                        var codingRegion = new CodingRegion(1314869, 1325338, 234, 1013, 780);
                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVRKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQGSGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGDFTFVMNDRSYVAL*");
                    }

                    if (chromosome.UcscName == "chrY")
                    {
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1264869, 1264883, 678, 692),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1264884, 1264893, 666, 675),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1264894, 1265014, 544, 664),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1265015, 1267418, 543, 544),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1267419, 1267581, 381, 543),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1267582, 1271271, 380, 381),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1271272, 1271405, 247, 380),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1271406, 1275325, 246, 247),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1275326, 1275492, 80,  246),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1275493, 1281448, 79,  80),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1281449, 1281527, 1,   79),
                        };

                        var codingRegion = new CodingRegion(1264869, 1275338, 234, 1013, 780);
                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVRKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQGSGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGDFTFVMNDRSYVAL*");
                    }
                }

                if (transcriptId == "NM_001012288.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(754, 753, "A"),
                        new RnaEdit(756, 756, "G"),
                        new RnaEdit(758, 758, "A"),
                        new RnaEdit(761, 761, "A"),
                        new RnaEdit(763, 763, "T"),
                        new RnaEdit(764, 763, "TT"),
                        new RnaEdit(767, 767, "A"),
                        new RnaEdit(769, 769, "T"),
                        new RnaEdit(772, 774, "CAG"),
                        new RnaEdit(779, 778,
                            "CCAGACCCGAAATCCATCTTCCCCGGGCTCTTTGAGATACACCAAGGGAACTTCCAGGAGTGGATCACAGACACCCAGAACGTGGCCCACCTCCACAAGATGGCAGGTGCAGAGCAAGAAAGTGGCCCCGAGGAGCCCCTGGTAGTCCAGTTGGCCAAGACTGAAGCCGAGTCTCCCAGGATGCTGGACCCACAGACCGAGGAGAAAGAGGCCTCTGGGGGATCCCTCCAGCTTCCCCACCAGCCCCTCCAAGGCGGTGATGTGGTCACAATCGGGGGCTTCACCTTTGTGATGAATGACCGCTCCTACGTGGCGTTGTGATGGACACACCACTGTCAAAGTCAACGTCAGGATCCACGTTGACATTTAAAGACAGAGGGGACTGTCCCGGGGACTCCACACCACCATGGATGGGAAGTCTCCACGCCAATGATGGTAGGACTAGGAGACTCTGAAGACCCAGCCTCACCGCCTAATGCGGCCACTGCCCTGCTAACTTTCCCCCACATGAGTCTCTGTGTTCAAAGGCTTGATGGCAGATGGGAGCCAATTGCTCCAGGAGATTTACTCCCAGTTCCTTTTCGTGCCTGAACGTTGTCACATAAACCCCAAGGCAGCACGTCCAAAATGCTGTAAAACCATCTTCCCACTCTGTGAGTCCCCAGTTCCGTCCATGTACCATTCCCATAGCATTGGATTCTCGGAGGATTTTTTGTCTGTTT"),
                    };

                    if (chromosome.UcscName == "chrX")
                    {
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1314869, 1314883, 767, 781),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1314884, 1314893, 755, 764),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1314894, 1315014, 633, 753),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1315015, 1317418, 632, 633),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1317419, 1317581, 470, 632),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1317582, 1321271, 469, 470),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1321272, 1321405, 336, 469),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1321406, 1325325, 335, 336),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1325326, 1325492, 169, 335),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1325493, 1331448, 168, 169),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1331449, 1331616, 1,   168),
                        };

                        var codingRegion = new CodingRegion(1314869, 1325338, 323, 1102, 780);
                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");
                    }

                    if (chromosome.UcscName == "chrY")
                    {
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1264869, 1264883, 767, 781),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1264884, 1264893, 755, 764),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1264894, 1265014, 633, 753),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1265015, 1267418, 632, 633),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1267419, 1267581, 470, 632),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1267582, 1271271, 469, 470),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1271272, 1271405, 336, 469),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1271406, 1275325, 335, 336),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1275326, 1275492, 169, 335),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1275493, 1281448, 168, 169),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1281449, 1281616, 1,   168),
                        };

                        var codingRegion = new CodingRegion(1264869, 1275338, 323, 1102, 780);
                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");
                    }
                }

                if (transcriptId == "NM_022148.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(857, 856, "A"),
                        new RnaEdit(859, 859, "G"),
                        new RnaEdit(861, 861, "A"),
                        new RnaEdit(864, 864, "A"),
                        new RnaEdit(866, 866, "T"),
                        new RnaEdit(867, 866, "TT"),
                        new RnaEdit(870, 870, "A"),
                        new RnaEdit(872, 872, "T"),
                        new RnaEdit(875, 877, "CAG"),
                        new RnaEdit(882, 881,
                            "CCAGACCCGAAATCCATCTTCCCCGGGCTCTTTGAGATACACCAAGGGAACTTCCAGGAGTGGATCACAGACACCCAGAACGTGGCCCACCTCCACAAGATGGCAGGTGCAGAGCAAGAAAGTGGCCCCGAGGAGCCCCTGGTAGTCCAGTTGGCCAAGACTGAAGCCGAGTCTCCCAGGATGCTGGACCCACAGACCGAGGAGAAAGAGGCCTCTGGGGGATCCCTCCAGCTTCCCCACCAGCCCCTCCAAGGCGGTGATGTGGTCACAATCGGGGGCTTCACCTTTGTGATGAATGACCGCTCCTACGTGGCGTTGTGATGGACACACCACTGTCAAAGTCAACGTCAGGATCCACGTTGACATTTAAAGACAGAGGGGACTGTCCCGGGGACTCCACACCACCATGGATGGGAAGTCTCCACGCCAATGATGGTAGGACTAGGAGACTCTGAAGACCCAGCCTCACCGCCTAATGCGGCCACTGCCCTGCTAACTTTCCCCCACATGAGTCTCTGTGTTCAAAGGCTTGATGGCAGATGGGAGCCAATTGCTCCAGGAGATTTACTCCCAGTTCCTTTTCGTGCCTGAACGTTGTCACATAAACCCCAAGGCAGCACGTCCAAAATGCTGTAAAACCATCTTCCCACTCTGTGAGTCCCCAGTTCCGTCCATGTACCATTCCCATAGCATTGGATTCTCGGAGGATTTTTTGTCTGTTT"),
                    };

                    if (chromosome.UcscName == "chrX")
                    {
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1314869, 1314883, 870, 884),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1314884, 1314893, 858, 867),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1314894, 1315014, 736, 856),
                            new TranscriptRegion(TranscriptRegionType.Intron, 5, 1315015, 1317418, 735, 736),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1317419, 1317581, 573, 735),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1317582, 1321271, 572, 573),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1321272, 1321405, 439, 572),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1321406, 1325325, 438, 439),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1325326, 1325492, 272, 438),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1325493, 1327698, 271, 272),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1327699, 1327801, 169, 271),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1327802, 1331448, 168, 169),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1331449, 1331616, 1,   168),
                        };

                        var codingRegion = new CodingRegion(1314869, 1331527, 90, 1205, 1116);
                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");
                    }
                    
                    if (chromosome.UcscName == "chrY")
                    {
                        transcriptRegions = new ITranscriptRegion[]
                        {
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1264869, 1264883, 870, 884),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1264884, 1264893, 858, 867),
                            new TranscriptRegion(TranscriptRegionType.Exon,   6, 1264894, 1265014, 736, 856),
                            new TranscriptRegion(TranscriptRegionType.Intron, 5, 1265015, 1267418, 735, 736),
                            new TranscriptRegion(TranscriptRegionType.Exon,   5, 1267419, 1267581, 573, 735),
                            new TranscriptRegion(TranscriptRegionType.Intron, 4, 1267582, 1271271, 572, 573),
                            new TranscriptRegion(TranscriptRegionType.Exon,   4, 1271272, 1271405, 439, 572),
                            new TranscriptRegion(TranscriptRegionType.Intron, 3, 1271406, 1275325, 438, 439),
                            new TranscriptRegion(TranscriptRegionType.Exon,   3, 1275326, 1275492, 272, 438),
                            new TranscriptRegion(TranscriptRegionType.Intron, 2, 1275493, 1277698, 271, 272),
                            new TranscriptRegion(TranscriptRegionType.Exon,   2, 1277699, 1277801, 169, 271),
                            new TranscriptRegion(TranscriptRegionType.Intron, 1, 1277802, 1281448, 168, 169),
                            new TranscriptRegion(TranscriptRegionType.Exon,   1, 1281449, 1281616, 1,   168),
                        };

                        var codingRegion = new CodingRegion(1264869, 1281527, 90, 1205, 1116);
                        translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                            "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");
                    }
                }

                if (transcriptId == "NM_012234.6")
                {
                    // first exon starts at 72495647, so the genomic portion of the coding region is clipped
                    var codingRegion = new CodingRegion(72427536, 72495647, 184, 870, 688);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MTMGDKKSPTRPKRQAKPAADEGFWDCSVCTFRNSAEAFKCSICDVRKGTSTRKPRINSQLVAQQVAQQYATPPPPKKEKKEKVEKQDKEKPEKDKEISPSVTKKNTNKKTKPKSDILKDPPSEANSIQSANATTKTSETNHTSRPRLKNVDRSTAQQLAVTVGNVTVIITDFKEKTRSSSTSSSTVTSSAGSEQQNQSSSGSESTDKGSSRSSTPKGDMSAVNDESF*");

                    updatedGeneModel = true;
                }

                // NM_001220773.1
                if (transcriptId == "NM_001220773.1")
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

                if (transcriptId == "NM_152756.3")
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

                if (transcriptId == "NM_001242758.1")
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
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MAVMAPRTLLLLLSGALALTQTWAGSHSMRYFFTSVSRPGRGEPRFIAVGYVDDTQFVRFDSDAASQKMEPRAPWIEQEGPEYWDQETRNMKAHSQTDRANLGTLRGYYNQSEDGSHTIQIMYGCDVGPDGRFLRGYRQDAYDGKDYIALNEDLRSWTAADMAAQITKRKWEAVHAAEQRRVYLEGRCVDGLRRYLENGKETLQRTDPPKTHMTHHPISDHEATLRCWALGFYPAEITLTWQRDGEDQTQDTELVETRPAGDGTFQKWAAVVVPSGEEQRYTCHVQHEGLPKPLTLRWELSSQPTIPIVGIIAGLVLLGAVITGAVVAAVMWRRKSSDRKGGSYTQAASSDSAQGSDVSLTACKV*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_002447.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(3847, 3847, "G"),
                        new RnaEdit(4773, 4772, "AAAAAAAAAAAAA")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MELLPPLPQSFLLLLLLPAKPAAGEDWQCPRTPYAASRDFDVKYVVPSFSAGGLVQAMVTYEGDRNESAVFVAIRNRLHVLGPDLKSVQSLATGPAGDPGCQTCAACGPGPHGPPGDTDTKVLVLDPALPALVSCGSSLQGRCFLHDLEPQGTAVHLAAPACLFSAHHNRPDDCPDCVASPLGTRVTVVEQGQASYFYVASSLDAAVAASFSPRSVSIRRLKADASGFAPGFVALSVLPKHLVSYSIEYVHSFHTGAFVYFLTVQPASVTDDPSALHTRLARLSATEPELGDYRELVLDCRFAPKRRRRGAPEGGQPYPVLRVAHSAPVGAQLATELSIAEGQEVLFGVFVTGKDGGPGVGPNSVVCAFPIDLLDTLIDEGVERCCESPVHPGLRRGLDFFQSPSFCPNPPGLEALSPNTSCRHFPLLVSSSFSRVDLFNGLLGPVQVTALYVTRLDNVTVAHMGTMDGRILQVELVRSLNYLLYVSNFSLGDSGQPVQRDVSRLGDHLLFASGDQVFQVPIQGPGCRHFLTCGRCLRAWHFMGCGWCGNMCGQQKECPGSWQQDHCPPKLTEFHPHSGPLRGSTRLTLCGSNFYLHPSGLVPEGTHQVTVGQSPCRPLPKDSSKLRPVPRKDFVEEFECELEPLGTQAVGPTNVSLTVTNMPPGKHFRVDGTSVLRGFSFMEPVLIAVQPLFGPRAGGTCLTLEGQSLSVGTSRAVLVNGTECLLARVSEGQLLCATPPGATVASVPLSLQVGGAQVPGSWTFQYREDPVVLSISPNCGYINSHITICGQHLTSAWHLVLSFHDGLRAVESRCERQLPEQQLCRLPEYVVRDPQGWVAGNLSARGDGAAGFTLPGFRFLPPPHPPSANLVPLKPEEHAIKFEYIGLGAVADCVGINVTVGGESCQHEFRGDMVVCPLPPSLQLGQDGAPLQVCVDGECHILGRVVRPGPDGVPQSTLLGILLPLLLLVAALATALVFSYWWRRKQLVLPPNLNDLASLDQTAGATPLPILYSGSDYRSGLALPAIDGLDSTTCVHGASFSDSEDESCVPLLRKESIQLRDLDSALLAEVKDVLIPHERVVTHSDRVIGKGHFGVVYHGEYIDQAQNRIQCAIKSLSRITEMQQVEAFLREGLLMRGLNHPNVLALIGIMLPPEGLPHVLLPYMCHGDLLQFIRSPQRNPTVKDLISFGLQVARGMEYLAEQKFVHRDLAARNCMLDESFTVKVADFGLARDILDREYYSVQQHRHARLPVKWMALESLQTYRFTTKSDVWSFGVLLWELLTRGAPPYRHIDPFDLTHFLAQGRRLPQPEYCPDSLYQVMQQCWEADPAVRPTFRVLVGEVEQIVSALLGDHYVQLPATYMNLGPSTSHEMNVRPEQPQFSPMPGNVRRPRPLSEPPRPT*");
                    
                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_005228.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(2955, 2955, "C"),
                        new RnaEdit(5601, 5600, "AAAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_005922.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(612,  612,  "A"),
                        new RnaEdit(5485, 5484, "AAAAAAAAAAAAAAAAA")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MREAAAALVPPPAFAVTPAAAMEEPPPPPPPPPPPPEPETESEPECCLAARQEGTLGDSACKSPESDLEDFSDETNTENLYGTSPPSTPRQMKRMSTKHQRNNVGRPASRSNLKEKMNAPNQPPHKDTGKTVENVEEYSYKQEKKIRAALRTTERDHKKNVQCSFMLDSVGGSLPKKSIPDVDLNKPYLSLGCSNAKLPVSVPMPIARPARQTSRTDCPADRLKFFETLRLLLKLTSVSKKKDREQRGQENTSGFWLNRSNELIWLELQAWHAGRTINDQDFFLYTARQAIPDIINEILTFKVDYGSFAFVRDRAGFNGTSVEGQCKATPGTKIVGYSTHHEHLQRQRVSFEQVKRIMELLEYIEALYPSLQALQKDYEKYAAKDFQDRVQALCLWLNITKDLNQKLRIMGTVLGIKNLSDIGWPVFEIPSPRPSKGNEPEYEGDDTEGELKELESSTDESEEEQISDPRVPEIRQPIDNSFDIQSRDCISKKLERLESEDDSLGWGAPDWSTEAGFSRHCLTSIYRPFVDKALKQMGLRKLILRLHKLMDGSLQRARIALVKNDRPVEFSEFPDPMWGSDYVQLSRTPPSSEEKCSAVSWEELKAMDLPSFEPAFLVLCRVLLNVIHECLKLRLEQRPAGEPSLLSIKQLVRECKEVLKGGLLMKQYYQFMLQEVLEDLEKPDCNIDAFEEDLHKMLMVYFDYMRSWIQMLQQLPQASHSLKNLLEEEWNFTKEITHYIRGGEAQAGKLFCDIAGMLLKSTGSFLEFGLQESCAEFWTSADDSSASDEIRRSVIEISRALKELFHEARERASKALGFAKMLRKDLEIAAEFRLSAPVRDLLDVLKSKQYVKVQIPGLENLQMFVPDTLAEEKSIILQLLNAAAGKDCSKDSDDVLIDAYLLLTKHGDRARDSEDSWGTWEAQPVKVVPQVETVDTLRSMQVDNLLLVVMQSAHLTIQRKAFQQSIEGLMTLCQEQTSSQPVIAKALQQLKNDALELCNRISNAIDRVDHMFTSEFDAEVDESESVTLQQYYREAMIQGYNFGFEYHKEVVRLMSGEFRQKIGDKYISFARKWMNYVLTKCESGRGTRPRWATQGFDFLQAIEPAFISALPEDDFLSLQALMNECIGHVIGKPHSPVTGLYLAIHRNSPRPMKVPRCHSDPPNPHLIIPTPEGFSTRSMPSDARSHGSPAAAAAAAAAAVAASRPSPSGGDSVLPKSISSAHDTRGSSVPENDRLASIAAELQFRSLSRHSSPTEERDEPAYPRGDSSGSTRRSWELRTLISQSKDTASKLGPIEAIQKSVRLFEEKRYREMRRKNIIGQVCDTPKSYDNVMHVGLRKVTFKWQRGNKIGEGQYGKVYTCISVDTGELMAMKEIRFQPNDHKTIKETADELKIFEGIKHPNLVRYFGVELHREEMYIFMEYCDEGTLEEVSRLGLQEHVIRLYSKQITIAINVLHEHGIVHRDIKGANIFLTSSGLIKLGDFGCSVKLKNNAQTMPGEVNSTLGTAAYMAPEVITRAKGEGHGRAADIWSLGCVVIEMVTGKRPWHEYEHNFQIMYKVGMGHKPPIPERLSPEGKDFLSHCLESDPKMRWTASQLLDHSFVKVCTDEE*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_006724.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(612,  612,  "A"),
                        new RnaEdit(5335, 5334, "AAAAAAAAAAAAAAAAA")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MREAAAALVPPPAFAVTPAAAMEEPPPPPPPPPPPPEPETESEPECCLAARQEGTLGDSACKSPESDLEDFSDETNTENLYGTSPPSTPRQMKRMSTKHQRNNVGRPASRSNLKEKMNAPNQPPHKDTGKTVENVEEYSYKQEKKIRAALRTTERDHKKNVQCSFMLDSVGGSLPKKSIPDVDLNKPYLSLGCSNAKLPVSVPMPIARPARQTSRTDCPADRLKFFETLRLLLKLTSVSKKKDREQRGQENTSGFWLNRSNELIWLELQAWHAGRTINDQDFFLYTARQAIPDIINEILTFKVDYGSFAFVRDRAGFNGTSVEGQCKATPGTKIVGYSTHHEHLQRQRVSFEQVKRIMELLEYIEALYPSLQALQKDYEKYAAKDFQDRVQALCLWLNITKDLNQKLRIMGTVLGIKNLSDIGWPVFEIPSPRPSKGNEPEYEGDDTEGELKELESSTDESEEEQISDPRVPEIRQPIDNSFDIQSRDCISKKLERLESEDDSLGWGAPDWSTEAGFSRHCLTSIYRPFVDKALKQMGLRKLILRLHKLMDGSLQRARIALVKNDRPVEFSEFPDPMWGSDYVQLSRTPPSSEEKCSAVSWEELKAMDLPSFEPAFLVLCRVLLNVIHECLKLRLEQRPAGEPSLLSIKQLVRECKEVLKGGLLMKQYYQFMLQEVLEDLEKPDCNIDAFEEDLHKMLMVYFDYMRSWIQMLQQLPQASHSLKNLLEEEWNFTKEITHYIRGGEAQAGKLFCDIAGMLLKSTGSFLEFGLQESCAEFWTSADDSSASDEIRRSVIEISRALKELFHEARERASKALGFAKMLRKDLEIAAEFRLSAPVRDLLDVLKSKQYVKVQIPGLENLQMFVPDTLAEEKSIILQLLNAAAGKDCSKDSDDVLIDAYLLLTKHGDRARDSEDSWGTWEAQPVKVVPQVETVDTLRSMQVDNLLLVVMQSAHLTIQRKAFQQSIEGLMTLCQEQTSSQPVIAKALQQLKNDALELCNRISNAIDRVDHMFTSEFDAEVDESESVTLQQYYREAMIQGYNFGFEYHKEVVRLMSGEFRQKIGDKYISFARKWMNYVLTKCESGRGTRPRWATQGFDFLQAIEPAFISALPEDDFLSLQALMNECIGHVIGKPHSPVTGLYLAIHRNSPRPMKVPRCHSDPPNPHLIIPTPEGFRGSSVPENDRLASIAAELQFRSLSRHSSPTEERDEPAYPRGDSSGSTRRSWELRTLISQSKDTASKLGPIEAIQKSVRLFEEKRYREMRRKNIIGQVCDTPKSYDNVMHVGLRKVTFKWQRGNKIGEGQYGKVYTCISVDTGELMAMKEIRFQPNDHKTIKETADELKIFEGIKHPNLVRYFGVELHREEMYIFMEYCDEGTLEEVSRLGLQEHVIRLYSKQITIAINVLHEHGIVHRDIKGANIFLTSSGLIKLGDFGCSVKLKNNAQTMPGEVNSTLGTAAYMAPEVITRAKGEGHGRAADIWSLGCVVIEMVTGKRPWHEYEHNFQIMYKVGMGHKPPIPERLSPEGKDFLSHCLESDPKMRWTASQLLDHSFVKVCTDEE*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_019063.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1109, 1109, "G"),
                        new RnaEdit(1406, 1406, "G"),
                        new RnaEdit(5550, 5549, "AAAAAAAAAAAAAAAA")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MDGFAGSLDDSISAASTSDVQDRLSALESRVQQQEDEITVLKAALADVLRRLAISEDHVASVKKSVSSKGQPSPRAVIPMSCITNGSGANRKPSHTSAVSIAGKETLSSAAKSGTEKKKEKPQGQREKKEESHSNDQSPQIRASPSPQPSSQPLQIHRQTPESKNATPTKSIKRPSPAEKSHNSWENSDDSRNKLSKIPSTPKLIPKVTKTADKHKDVIINQEGEYIKMFMRGRPITMFIPSDVDNYDDIRTELPPEKLKLEWAYGYRGKDCRANVYLLPTGEIVYFIASVVVLFNYEERTQRHYLGHTDCVKCLAIHPDKIRIATGQIAGVDKDGRPLQPHVRVWDSVTLSTLQIIGLGTFERGVGCLDFSKADSGVHLCVIDDSNEHMLTVWDWQKKAKGAEIKTTNEVVLAVEFHPTDANTIITCGKSHIFFWTWSGNSLTRKQGIFGKYEKPKFVQCLAFLGNGDVLTGDSGGVMLIWSKTTVEPTPGKGPKGVYQISKQIKAHDGSVFTLCQMRNGMLLTGGGKDRKIILWDHDLNPEREIEVPDQYGTIRAVAEGKADQFLVGTSRNFILRGTFNDGFQIEVQGHTDELWGLATHPFKDLLLTCAQDRQVCLWNSMEHRLEWTRLVDEPGHCADFHPSGTVVAIGTHSGRWFVLDAETRDLVSIHTDGNEQLSVMRYSIDGTFLAVGSHDNFIYLYVVSENGRKYSRYGRCTGHSSYITHLDWSPDNKYIMSNSGDYEILYWDIPNGCKLIRNRSDCKDIDWTTYTCVLGFQVFGVWPEGSDGTDINALVRSHNRKVIAVADDFCKVHLFQYPCSKAKAPSHKYSAHSSHVTNVSFTHNDSHLISTGGKDMSIIQWKLVEKLSLPQNETVADTTLTKAPVSSTESVIQSNTPTPPPSQPLNETAEEESRISSSPTLLENSLEQTVEPSEDHSEEESEEGSGDLGEPLYEEPCNEISKEQAKATLLEDQQDPSPSS*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_175741.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(220, 220, "T"),
                        new RnaEdit(380, 380, "C")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MASDGASALPGPDMSMKPSAALSPSPALPFLPPTSDPPDHPPREPPPQPIMPSVFSPDNPLMLSAFPSSLLVTGDGGPCLSGAGAGKVIVKVKTEGGSAEPSQTQNFILTQTALNSTAPGTPCGGLEGPAPPFVTASNVKTILPSKAVGVSQEGPPGLPPQPPPPVAQLVPIVPLEKAWPGPHGTTGEGGPVATLSKPSLGDRSKISKDVYENFRQWQRYKALARRHLSQSPDTEALSCFLIPVLRSLARLKPTMTLEEGLPLAVQEWEHTSNFDRMIFYEMAERFMEFEAEEMQIQNTQLMNGSQGLSPATPLKLDPLGPLASEVCQQPVYIPKKAASKTRAPRRRQRKAQRPPAPEAPKEIPPEAVKEYVDIMEWLVGTHLATGESDGKQEEEGQQQEEEGMYPDPGLLSYINELCSQKVFVSKVEAVIHPQFLADLLSPEKQRDPLALIEELEQEEGLTLAQLVQKRLMALEEEEDAEAPPSFSGAQLDSSPSGSVEDEDGDGRLRPSPGLQGAGGAACLGKVSSSGKRAREVHGGQEQALDSPRGMHRDGNTLPSPSSWDLQPELAAPQGTPGPLGVERRGSGKVINQVSLHQDGHLGGAGPPGHCLVADRTSEALPLCWQGGFQPESTPSLDAGLAELAPLQGQGLEKQVLGLQKGQQTGGRGVLPQGKEPLAVPWEGSSGAMWGDDRGTPMAQSYDQNPSPRAAGERDDVCLSPGVWLSSEMDAVGLELPVQIEEVIESFQVEKCVTEYQEGCQGLGSRGNISLGPGETLVPGDTESSVIPCGGTVAAAALEKRNYCSLPGPLRANSPPLRSKENQEQSCETVGHPSDLWAEGCFPLLESGDSTLGSSKETLPPTCQGNLLIMGTEDASSLPEASQEAGSRGNSFSPLLETIEPVNILDVKDDCGLQLRVSEDTCPLNVHSYDPQGEGRVDPDLSKPKNLAPLQESQESYTTGTPKATSSHQGLGSTLPRRGTRNAIVPRETSVSKTHRSADRAKGKEKKKKEAEEEDEELSNFAYLLASKLSLSPREHPLSPHHASGGQGSQRASHLLPAGAKGPSKLPYPVAKSGKRALAGGPAPTEKTPHSGAQLGVPREKPLALGVVRPSQPRKRRCDSFVTGRRKKRRRSQ*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NR_003085.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1703, 1703, "G"),
                        new RnaEdit(2832, 2831, "AAAAAAAAAAAAAAA")
                    };

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_001244937.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(3700, 3700, "G"),
                        new RnaEdit(4626, 4625, "AAAAAAAAAAAAA")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MELLPPLPQSFLLLLLLPAKPAAGEDWQCPRTPYAASRDFDVKYVVPSFSAGGLVQAMVTYEGDRNESAVFVAIRNRLHVLGPDLKSVQSLATGPAGDPGCQTCAACGPGPHGPPGDTDTKVLVLDPALPALVSCGSSLQGRCFLHDLEPQGTAVHLAAPACLFSAHHNRPDDCPDCVASPLGTRVTVVEQGQASYFYVASSLDAAVAASFSPRSVSIRRLKADASGFAPGFVALSVLPKHLVSYSIEYVHSFHTGAFVYFLTVQPASVTDDPSALHTRLARLSATEPELGDYRELVLDCRFAPKRRRRGAPEGGQPYPVLRVAHSAPVGAQLATELSIAEGQEVLFGVFVTGKDGGPGVGPNSVVCAFPIDLLDTLIDEGVERCCESPVHPGLRRGLDFFQSPSFCPNPPGLEALSPNTSCRHFPLLVSSSFSRVDLFNGLLGPVQVTALYVTRLDNVTVAHMGTMDGRILQVELVRSLNYLLYVSNFSLGDSGQPVQRDVSRLGDHLLFASGDQVFQVPIQGPGCRHFLTCGRCLRAWHFMGCGWCGNMCGQQKECPGSWQQDHCPPKLTEFHPHSGPLRGSTRLTLCGSNFYLHPSGLVPEGTHQVTVGQSPCRPLPKDSSKLRPVPRKDFVEEFECELEPLGTQAVGPTNVSLTVTNMPPGKHFRVDGTSVLRGFSFMEPVLIAVQPLFGPRAGGTCLTLEGQSLSVGTSRAVLVNGTECLLARVSEGQLLCATPPGATVASVPLSLQVGGAQVPGSWTFQYREDPVVLSISPNCGYINSHITICGQHLTSAWHLVLSFHDGLRAVESRCERQLPEQQLCRLPEYVVRDPQGWVAGNLSARGDGAAGFTLPGFRFLPPPHPPSANLVPLKPEEHAIKFEVCVDGECHILGRVVRPGPDGVPQSTLLGILLPLLLLVAALATALVFSYWWRRKQLVLPPNLNDLASLDQTAGATPLPILYSGSDYRSGLALPAIDGLDSTTCVHGASFSDSEDESCVPLLRKESIQLRDLDSALLAEVKDVLIPHERVVTHSDRVIGKGHFGVVYHGEYIDQAQNRIQCAIKSLSRITEMQQVEAFLREGLLMRGLNHPNVLALIGIMLPPEGLPHVLLPYMCHGDLLQFIRSPQRNPTVKDLISFGLQVARGMEYLAEQKFVHRDLAARNCMLDESFTVKVADFGLARDILDREYYSVQQHRHARLPVKWMALESLQTYRFTTKSDVWSFGVLLWELLTRGAPPYRHIDPFDLTHFLAQGRRLPQPEYCPDSLYQVMQQCWEADPAVRPTFRVLVGEVEQIVSALLGDHYVQLPATYMNLGPSTSHEMNVRPEQPQFSPMPGNVRRPRPLSEPPRPT*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_001278433.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAGCTGTGGTGGGCTCCACCCAGTTCGAGCTTCCCGGCTGCTTTGGTTACCTAATCAAGCCTGGGCAATGGCAGGCGCCCCTCCCCCAGCCTCGCTGCCGCCTTGCAGTTTGATCTCAGACTGCTGTGCTAGCAATCAGCGAGACTCCGTGGGCGTAGGACCCTCCGAGC"),
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

                if (transcriptId == "NM_001260.1")
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

                if (transcriptId == "NM_000314.4")
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

                if (transcriptId == "NM_000535.5")
                {
                    rnaEdits    = new IRnaEdit[2];
                    rnaEdits[0] = new RnaEdit(1708, 1708, "G");
                    rnaEdits[1] = new RnaEdit(2837, 2836, "AAAAAAAAAAAAAAA");

                    var oldCodingRegion = translation.CodingRegion;
                    var codingRegion    = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, 88, 2676, 2589);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MERAESSSTEPAKAIKPIDRKSVHQICSGQVVLSLSTAVKELVENSLDAGATNIDLKLKDYGVDLIEVSDNGCGVEEENFEGLTLKHHTSKIQEFADLTQVETFGFRGEALSSLCALSDVTISTCHASAKVGTRLMFDHNGKIIQKTPYPRPRGTTVSVQQLFSTLPVRHKEFQRNIKKEYAKMVQVLHAYCIISAGIRVSCTNQLGQGKRQPVVCTGGSPSIKENIGSVFGQKQLQSLIPFVQLPPSDSVCEEYGLSCSDALHNLFYISGFISQCTHGVGRSSTDRQFFFINRRPCDPAKVCRLVNEVYHMYNRHQYPFVVLNISVDSECVDINVTPDKRQILLQEEKLLLAVLKTSLIGMFDSDVNKLNVSQQPLLDVEGNLIKMHAADLEKPMVEKQDQSPSLRTGEEKKDVSISRLREAFSLRHTTENKPHSPKTPEPRRSPLGQKRGMLSSSTSGAISDKGVLRPQKEAVSSSHGPSDPTDRAEVEKDSGHGSTSVDSEGFSIPDTGSHCSSEYAASSPGDRGSQEHVDSQEKAPETDDSFSDVDCHSNQEDTGCKFRVLPQPTNLATPNTKRFKKEEILSSSDICQKLVNTQDMSASQVDVAVKINKKVVPLDFSMSSLAKRIKQLHHEAQQSEGEQNYRKFRAKICPGENQAAEDELRKEISKTMFAEMEIIGQFNLGFIITKLNEDIFIVDQHATDEKYNFEMLQQHTVLQGQRLIAPQTLNLTAVNEAVLIENLEIFRKNGFDFVIDENAPVTERAKLISLPTSKNWTFGPQDVDELIFMLSDSPGVMCRPSRVKQMFASRACRKSVMIGTALNTSEMKKLITHMGEMDHPWNCPHGRPTMRHIANLGVISQN*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_000545.5")
                {
                    rnaEdits    = new IRnaEdit[2];
                    rnaEdits[0] = new RnaEdit(1743, 1743, "G");
                    rnaEdits[1] = new RnaEdit(3240, 3239, "AA");
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MVSKLSQLQTELLAALLESGLSKEALIQALGEPGPYLLAGEGPLDKGESCGGGRGELAELPNGLGETRGSEDETDDDGEDFTPPILKELENLSPEEAAHQKAVVETLLQEDPWRVAKMVKSYLQQHNIPQREVVDTTGLNQSHLSQHLNKGTPMKTQKRAALYTWYVRKQREVAQQFTHAGQGGLIEEPTGDELPTKKGRRNRFKWGPASQQILFQAYERQKNPSKEERETLVEECNRAECIQRGVSPSQAQGLGSNLVTEVRVYNWFANRRKEEAFRHKLAMDTYSGPPPGPGPGPALPAHSSPGLPPPALSPSKVHGVRYGQPATSETAEVPSSSGGPLVTVSTPLHQVSPTGLEPSHSLLSTEAKLVSAAGGPLPPVSTLTALHSLEQTSPGLNQQPQNLIMASLPGVMTIGPGEPASLGPTFTNTGASTLVIGLASTQAQSVPVINSMGSSLTTLQPVQFSQPLHPSYQQPLMPPVQSHVTQSPFMATMAQLQSPHALYSHKPEVAQYTHTGLLPQTMLITDTTNLSALASLTPTKQVFTSDTEASSESGLHTPASQATTLHVPSQDPAGIQHLQPAHRLSASPTVSSSSLVLYQSSDSSNGQSHLLPSNHSVIETFISTQMASSSQ*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_001145076.1")
                {
                    rnaEdits    = new IRnaEdit[3];
                    rnaEdits[0] = new RnaEdit(935,  935,  "G");
                    rnaEdits[1] = new RnaEdit(1232, 1232, "G");
                    rnaEdits[2] = new RnaEdit(5376, 5375, "AAAAAAAAAAAAAAAA");
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "MDGFAGSLDDSISAASTSDVQDRLSALESRVQQQEDEITVLKAALADVLRRLAISEDHVASVKKSVSSKGQPSPRAVIPMSCITNGSGANRKPSHTSAVSIAGKETLSSAAKSIKRPSPAEKSHNSWENSDDSRNKLSKIPSTPKLIPKVTKTADKHKDVIINQEGEYIKMFMRGRPITMFIPSDVDNYDDIRTELPPEKLKLEWAYGYRGKDCRANVYLLPTGEIVYFIASVVVLFNYEERTQRHYLGHTDCVKCLAIHPDKIRIATGQIAGVDKDGRPLQPHVRVWDSVTLSTLQIIGLGTFERGVGCLDFSKADSGVHLCVIDDSNEHMLTVWDWQKKAKGAEIKTTNEVVLAVEFHPTDANTIITCGKSHIFFWTWSGNSLTRKQGIFGKYEKPKFVQCLAFLGNGDVLTGDSGGVMLIWSKTTVEPTPGKGPKGVYQISKQIKAHDGSVFTLCQMRNGMLLTGGGKDRKIILWDHDLNPEREIEVPDQYGTIRAVAEGKADQFLVGTSRNFILRGTFNDGFQIEVQGHTDELWGLATHPFKDLLLTCAQDRQVCLWNSMEHRLEWTRLVDEPGHCADFHPSGTVVAIGTHSGRWFVLDAETRDLVSIHTDGNEQLSVMRYSIDGTFLAVGSHDNFIYLYVVSENGRKYSRYGRCTGHSSYITHLDWSPDNKYIMSNSGDYEILYWDIPNGCKLIRNRSDCKDIDWTTYTCVLGFQVFGVWPEGSDGTDINALVRSHNRKVIAVADDFCKVHLFQYPCSKAKAPSHKYSAHSSHVTNVSFTHNDSHLISTGGKDMSIIQWKLVEKLSLPQNETVADTTLTKAPVSSTESVIQSNTPTPPPSQPLNETAEEESRISSSPTLLENSLEQTVEPSEDHSEEESEEGSGDLGEPLYEEPCNEISKEQAKATLLEDQQDPSPSS*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_001220765.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,  0,  "GAATTCCGGCGT"),
                        new RnaEdit(6,  5,  "A"),
                        new RnaEdit(16, 16, "T"),
                        new RnaEdit(97, 97, "C"),
                        new RnaEdit(316, 315,
                            "CCAGTAATGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                if (transcriptId == "NM_001220766.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(317, 318, null),
                        new RnaEdit(321, 320,
                            "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                if (transcriptId == "NM_001220767.1" && start == 50344378)
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
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_001220769.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(317, 318, null),
                        new RnaEdit(321, 320,
                            "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                    var codingRegion = new CodingRegion(50358658, 50367358, 169, 1341, 1173);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                    updatedGeneModel = true;
                }

                if (transcriptId == "NM_001220770.1" && start == 50344378)
                {
                    // final RNA-edit offset by 2 to compensate for deletion
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(317, 318, null),
                        new RnaEdit(321, 320,
                            "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                if (transcriptId == "NM_001220768.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(316, 315, "CCA"),
                        new RnaEdit(320, 319,
                            "TGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                if (transcriptId == "NM_006060.4" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,   0,   "GAATTCCGGCGT"),
                        new RnaEdit(6,   5,   "A"),
                        new RnaEdit(16,  16,  "T"),
                        new RnaEdit(97,  97,  "C"),
                        new RnaEdit(316, 315, "CCA"),
                        new RnaEdit(320, 319,
                            "TGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                if (transcriptId == "NM_001220775.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAG"),
                        new RnaEdit(4,    3,    "C"),
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

                if (transcriptId == "NM_001220774.1")
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

                if (transcriptId == "NM_001220776.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1, 0,
                            "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAGT"),
                        new RnaEdit(3,    3,    "C"),
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

                if (transcriptId == "NM_001220772.1")
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

                if (transcriptId == "NM_001220771.1" && start == 50344378)
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1,  0,  "GAATTCCGGCGT"),
                        new RnaEdit(6,  5,  "A"),
                        new RnaEdit(16, 16, "T"),
                        new RnaEdit(97, 97, "C"),
                        new RnaEdit(316, 315,
                            "CCAGTAATGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
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

                if (transcriptId == "NM_003820.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1708, 1707, "AAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_032017.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3847, 3846, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001001740.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2703, 2702, "AAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_022457.5")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2775, 2774, "AAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_005378.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2606, 2605, "AAAAAAAA")};
                }

                if (transcriptId == "NM_001008540.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1896, 1895, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001145413.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2722, 2721, "AAAA")};
                }

                if (transcriptId == "NM_001145412.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2743, 2742, "AAAA")};
                }

                if (transcriptId == "NM_012433.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4293, 4292, "AAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001005526.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(620, 619, "AAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000465.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2608, 2607, "AAA")};
                }

                if (transcriptId == "NM_001018115.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5103, 5102, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001664.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1923, 1922, "AAAA")};
                }

                if (transcriptId == "NM_006218.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3713, 3712, "AAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_020640.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3153, 3152, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_003866.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4125, 4124, "AAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001101669.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4026, 4025, "AAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_004168.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2391, 2390, "AAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001903.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3762, 3761, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_213647.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3071, 3070, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002011.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3027, 3026, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_022963.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2793, 2792, "AAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_005514.6")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1573, 1572, "AAAAAA")};
                }

                if (transcriptId == "NM_001760.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2052, 2051, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001136125.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1836, 1835, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001136017.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2061, 2060, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001136126.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1845, 1844, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_005375.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3315, 3314, "AAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001010932.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2791, 2790, "AAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000601.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2806, 2805, "AAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001010933.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1275, 1274, "AAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001010931.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1290, 1289, "AAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001010934.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(2011, 2010, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_001127500.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(6677, 6676, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000245.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(6623, 6622, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002052.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3409, 3408, "AAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002072.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2199, 2198, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_031263.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2944, 2943, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002140.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2979, 2978, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_031262.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2919, 2918, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001135052.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5005, 5004, "AAAAAA")};
                }

                if (transcriptId == "NM_003177.5")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5074, 5073, "AAAAAA")};
                }

                if (transcriptId == "NM_001174168.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4922, 4921, "AAAAAA")};
                }

                if (transcriptId == "NM_001174167.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5067, 5066, "AAAAAA")};
                }

                if (transcriptId == "NM_004235.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2934, 2933, "AAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_017617.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(9296, 9295, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NR_028036.2")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2581, 2580, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NR_028033.2")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2518, 2517, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NR_028035.2")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2443, 2442, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_152871.2")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2627, 2626, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NR_028034.2")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2380, 2379, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_152872.2")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2665, 2664, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000043.4")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(2690, 2689, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_005343.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1045, 1044, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_176795.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1235, 1234, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001130442.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1153, 1152, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000612.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5166, 5165, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001127598.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4840, 4839, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001007139.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5140, 5139, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_020193.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5512, 5511, "AAAAAAA")};
                }

                if (transcriptId == "NM_152991.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2418, 2417, "AAAAAAAAAA")};
                }

                if (transcriptId == "NM_003797.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2011, 2010, "AAAAAAAAAA")};
                }

                if (transcriptId == "NM_001273.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(6498, 6497, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_080601.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2054, 2053, "AAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002834.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(6284, 6283, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_006231.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(7841, 7840, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001128226.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(7555, 7554, "AAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_014953.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(7645, 7644, "AAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_030621.3")
                {
                    rnaEdits = new IRnaEdit[]
                        {new RnaEdit(10222, 10221, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001271282.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(6175, 6174, "AAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002168.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1734, 1733, "AAAAAAA")};
                }

                if (transcriptId == "NM_001077183.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5432, 5431, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000548.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5633, 5632, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001114382.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5564, 5563, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_032444.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(7294, 7293, "AAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001134407.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(14685, 14684, "AAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000833.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(14447, 14446, "AAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001134408.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4736, 4735, "AAAAAAAAAA")};
                }

                if (transcriptId == "NM_016507.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(8291, 8290, "AAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_015083.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(8264, 8263, "AAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_017763.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(4559, 4558, "AAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001039933.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1265, 1264, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000626.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1262, 1261, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_021602.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(950, 949, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002647.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3070, 3069, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_002067.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1614, 1613, "AA")};
                }

                if (transcriptId == "NM_001379.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5353, 5352, "AAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001130823.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5401, 5400, "AAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001238.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1948, 1947,
                            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_138578.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2560, 2559, "AAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001191.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2371, 2370, "AAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_003600.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2340, 2339, "AAAAAAA")};
                }

                if (transcriptId == "NM_198433.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2548, 2547, "AAAAAAA")};
                }

                if (transcriptId == "NM_198434.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2239, 2238, "AAAAAAA")};
                }

                if (transcriptId == "NM_198435.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2129, 2128, "AAAAAAA")};
                }

                if (transcriptId == "NM_198436.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2225, 2224, "AAAAAAA")};
                }

                if (transcriptId == "NM_198437.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2115, 2114, "AAAAAAA")};
                }

                if (transcriptId == "NM_016592.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(2563, 2562, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001077490.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3766, 3765, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_080425.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3766, 3765, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001077489.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1863, 1862, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000516.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1908, 1907, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001077488.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1911, 1910, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_080426.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1866, 1865, "AAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001243432.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1515, 1514, "AAAAAA")};
                }

                if (transcriptId == "NM_003073.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1704, 1703, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001007468.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(1677, 1676, "AAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_021140.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(5773, 5772, "AAAAAA")};
                }

                if (transcriptId == "NM_006521.4")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(3394, 3393, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_138923.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(7630, 7629, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_004606.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(7693, 7692, "AAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_138270.2")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(11071, 11070, "AAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_000489.3")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(11185, 11184, "AAAAAAAAAAAAAAAAAA")};
                }

                if (transcriptId == "NM_001042749.1")
                {
                    rnaEdits = new IRnaEdit[] {new RnaEdit(6271, 6270, "AAAAAAA")};
                }

                if (transcriptId == "NM_005896.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1971, 1971, "T"),
                        new RnaEdit(2330, 2329, "AAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_004787.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(6,    6,    "C"),
                        new RnaEdit(72,   72,   "T"),
                        new RnaEdit(4473, 4473, "C"),
                        new RnaEdit(4845, 4845, "A")
                    };
                }

                if (transcriptId == "NM_016222.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(2001, 2001, "A"),
                        new RnaEdit(2105, 2104, "AAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_020861.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(80,   80,   "C"),
                        new RnaEdit(3087, 3086, "AAAAAAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_002649.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1282, 1282, "G"),
                        new RnaEdit(4444, 4444, "A")
                    };
                }

                if (transcriptId == "NM_025069.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(2092, 2092, "A")
                    };
                }

                if (transcriptId == "NM_001304717.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(706,  706,  "C"),
                        new RnaEdit(8702, 8701, "AAAAAAAAAAAAAAAAA")
                    };
                    
                    translation = new Translation(translation.CodingRegion, (CompactId) translation.ProteinId,
                        "LERGGEAAAAAAAAAAAPGRGSESPVTISRAGNAGELVSPLLLPPTRRRRRRHIQGPGPVLNLPSAAAAPPVARAPEAAGGGSRSEDYSSSPHSAAAAARPLAAEEKQAQSLQPSSSRRSSHYPAAVQSQAAAERGASATAKSRAISILQKKPRHQQLLPSLSSFFFSHRLPDMTAIIKEIVSRNKRRYQEDGFDLDLTYIYPNIIAMGFPAERLEGVYRNNIDDVVRFLDSKHKNHYKIYNLCAERHYDTAKFNCRVAQYPFEDHNPPQLELIKPFCEDLDQWLSEDDNHVAAIHCKAGKGRTGVMICAYLLHRGKFLKAQEALDFYGEVRTRDKKGVTIPSQRRYVYYYSYLLKNHLDYRPVALLFHKMMFETIPMFSGGTCNPQFVVCQLKVKIYSSNSGPTRREDKFMYFEFPQPLPVCGDIKVEFFHKQNKMLKKDKMFHFWVNTFFIPGPEETSEKVENGSLCDQEIDSICSIERADNDKEYLVLTLTKNDLDKANKDKANRYFSPNFKVKLYFTKTVEEPSNPEASSSTSVTPDVSDNEPDHYRYSDTTDSDPENEPFDEDQHTQITKV*");
                }

                if (transcriptId == "NM_033360.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1136, 1136, "T"),
                        new RnaEdit(3463, 3463, "G"),
                        new RnaEdit(5422, 5421, "AAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_004985.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1012, 1012, "T"),
                        new RnaEdit(3339, 3339, "G"),
                        new RnaEdit(5298, 5297, "AAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_002661.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(352,  352,  "C"),
                        new RnaEdit(4273, 4272, "AAAAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_005324.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(2110, 2110, "G"),
                        new RnaEdit(2114, 2114, "T"),
                        new RnaEdit(2706, 2705, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_015898.2")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(4139, 4139, "G"),
                        new RnaEdit(4442, 4441, "AAAAAAAAAAAAAAA")
                    };
                }

                if (transcriptId == "NM_006145.1")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(1407, 1408, "GG")
                    };
                }

                if (transcriptId == "NM_003954.3")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(765,  764,  "C"),
                        new RnaEdit(4468, 4467, "AAAAAAAA"),
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   16, 43340488, 43342167, 2789, 4468),
                        new TranscriptRegion(TranscriptRegionType.Intron, 15, 43342168, 43342529, 2788, 2789),
                        new TranscriptRegion(TranscriptRegionType.Exon,   15, 43342530, 43342630, 2688, 2788),
                        new TranscriptRegion(TranscriptRegionType.Intron, 14, 43342631, 43343903, 2687, 2688),
                        new TranscriptRegion(TranscriptRegionType.Exon,   14, 43343904, 43344048, 2543, 2687),
                        new TranscriptRegion(TranscriptRegionType.Intron, 13, 43344049, 43344458, 2687, 2543),
                        new TranscriptRegion(TranscriptRegionType.Exon,   13, 43344459, 43344565, 2436, 2687),
                        new TranscriptRegion(TranscriptRegionType.Intron, 12, 43344566, 43344772, 2435, 2436),
                        new TranscriptRegion(TranscriptRegionType.Exon,   12, 43344773, 43345126, 2082, 2435),
                        new TranscriptRegion(TranscriptRegionType.Intron, 11, 43345127, 43347779, 2081, 2082),
                        new TranscriptRegion(TranscriptRegionType.Exon,   11, 43347780, 43347930, 1931, 2081),
                        new TranscriptRegion(TranscriptRegionType.Intron, 10, 43347931, 43348424, 1930, 1931),
                        new TranscriptRegion(TranscriptRegionType.Exon,   10, 43348425, 43348588, 1767, 1930),
                        new TranscriptRegion(TranscriptRegionType.Intron, 9,  43348589, 43350869, 1766, 1767),
                        new TranscriptRegion(TranscriptRegionType.Exon,   9,  43350870, 43350974, 1662, 1766),
                        new TranscriptRegion(TranscriptRegionType.Intron, 8,  43350975, 43351489, 1661, 1662),
                        new TranscriptRegion(TranscriptRegionType.Exon,   8,  43351490, 43351621, 1530, 1661),
                        new TranscriptRegion(TranscriptRegionType.Intron, 7,  43351622, 43351830, 1529, 1530),
                        new TranscriptRegion(TranscriptRegionType.Exon,   7,  43351831, 43351960, 1400, 1529),
                        new TranscriptRegion(TranscriptRegionType.Intron, 6,  43351961, 43362178, 1399, 1400),
                        new TranscriptRegion(TranscriptRegionType.Exon,   6,  43362179, 43362316, 1262, 1399),
                        new TranscriptRegion(TranscriptRegionType.Intron, 5,  43362317, 43363797, 1261, 1262),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5,  43363798, 43364293, 766,  1261),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5,  43364294, 43364411, 647,  764),
                        new TranscriptRegion(TranscriptRegionType.Intron, 4,  43364412, 43364519, 646,  647),
                        new TranscriptRegion(TranscriptRegionType.Exon,   4,  43364520, 43364730, 436,  646),
                        new TranscriptRegion(TranscriptRegionType.Intron, 3,  43364731, 43366601, 435,  436),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3,  43366602, 43366671, 366,  435),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2,  43366672, 43367855, 365,  366),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2,  43367856, 43368131, 90,   365),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1,  43368132, 43394325, 89,   90),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1,  43394326, 43394414, 1,    89),
                    };

                    var codingRegion = new CodingRegion(43342003, 43368111, 110, 2953, 2844);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MAVMEMACPGAPGSAVGQQKELPKAKEKTPPLGKKQSSVYKLEAVEKSPVFCGKWEILNDVITKGTAKEGSEAGPAAISIIAQAECENSQEFSPTFSERIFIAGSKQYSQSESLDQIPNNVAHATEGKMARVCWKGKRRSKARKKRKKKSSKSLAHAGVALAKPLPRTPEQESCTIPVQEDESPLGAPYVRNTPQFTKPLKEPGLGQLCFKQLGEGLRPALPRSELHKLISPLQCLNHVWKLHHPQDGGPLPLPTHPFPYSRLPHPFPFHPLQPWKPHPLESFLGKLACVDSQKPLPDPHLSKLACVDSPKPLPGPHLEPSCLSRGAHEKFSVEEYLVHALQGSVSSGQAHSLTSLAKTWAARGSRSREPSPKTEDNEGVLLTEKLKPVDYEYREEVHWATHQLRLGRGSFGEVHRMEDKQTGFQCAVKKVRLEVFRAEELMACAGLTSPRIVPLYGAVREGPWVNIFMELLEGGSLGQLVKEQGCLPEDRALYYLGQALEGLEYLHSRRILHGDVKADNVLLSSDGSHAALCDFGHAVCLQPDGLGKSLLTGDYIPGTETHMAPEVVLGRSCDAKVDVWSSCCMMLHMLNGCHPWTQFFRGPLCLKIASEPPPVREIPPSCAPLTAQAIQEGLRKEPIHRVSAAELGGKVNRALQQVGGLKSPWRGEYKEPRHPPPNQANYHQTLHAQPRELSPRAPGPRPAEETTGRAPKLQPPLPPEPPEPNKSPPLTLSKEESGMWEPLPLSSLEPAPARNPSSPERKATVPEQELQQLEIELFLNSLSQPFSLEEQEQILSCLSIDSLSLSDDSEKNPSKASQSSRDTLSSGVHSWSSQAEARSSSWNMVLARGRPTDTPSYFNGVKVQIQSLNGEHLHIREFHRVKVGDIATGISSQIPAAAFSLVTKDGQPVRYDMEVPDSGIDLQCTLAPDGSFAWSWRVKHGQLENRP*");
                }

                if (transcriptId == "NM_003954.4")
                {
                    rnaEdits = new IRnaEdit[]
                    {
                        new RnaEdit(781,  780,  "C"),
                        new RnaEdit(4486, 4485, "AAAAAAAAAAAAA"),
                    };

                    transcriptRegions = new ITranscriptRegion[]
                    {
                        new TranscriptRegion(TranscriptRegionType.Exon,   16, 43340486, 43342167, 2805, 4486),
                        new TranscriptRegion(TranscriptRegionType.Intron, 15, 43342168, 43342529, 2804, 2805),
                        new TranscriptRegion(TranscriptRegionType.Exon,   15, 43342530, 43342630, 2704, 2804),
                        new TranscriptRegion(TranscriptRegionType.Intron, 14, 43342631, 43343903, 2703, 2704),
                        new TranscriptRegion(TranscriptRegionType.Exon,   14, 43343904, 43344048, 2559, 2703),
                        new TranscriptRegion(TranscriptRegionType.Intron, 13, 43344049, 43344458, 2558, 2559),
                        new TranscriptRegion(TranscriptRegionType.Exon,   13, 43344459, 43344565, 2452, 2558),
                        new TranscriptRegion(TranscriptRegionType.Intron, 12, 43344566, 43344772, 2451, 2452),
                        new TranscriptRegion(TranscriptRegionType.Exon,   12, 43344773, 43345126, 2098, 2451),
                        new TranscriptRegion(TranscriptRegionType.Intron, 11, 43345127, 43347779, 2097, 2098),
                        new TranscriptRegion(TranscriptRegionType.Exon,   11, 43347780, 43347930, 1947, 2097),
                        new TranscriptRegion(TranscriptRegionType.Intron, 10, 43347931, 43348424, 1946, 1947),
                        new TranscriptRegion(TranscriptRegionType.Exon,   10, 43348425, 43348588, 1783, 1946),
                        new TranscriptRegion(TranscriptRegionType.Intron, 9,  43348589, 43350869, 1782, 1783),
                        new TranscriptRegion(TranscriptRegionType.Exon,   9,  43350870, 43350974, 1678, 1782),
                        new TranscriptRegion(TranscriptRegionType.Intron, 8,  43350975, 43351489, 1677, 1678),
                        new TranscriptRegion(TranscriptRegionType.Exon,   8,  43351490, 43351621, 1546, 1677),
                        new TranscriptRegion(TranscriptRegionType.Intron, 7,  43351622, 43351830, 1545, 1546),
                        new TranscriptRegion(TranscriptRegionType.Exon,   7,  43351831, 43351960, 1416, 1545),
                        new TranscriptRegion(TranscriptRegionType.Intron, 6,  43351961, 43362178, 1415, 1416),
                        new TranscriptRegion(TranscriptRegionType.Exon,   6,  43362179, 43362316, 1278, 1415),
                        new TranscriptRegion(TranscriptRegionType.Intron, 5,  43362317, 43363797, 1277, 1278),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5,  43363798, 43364293, 782,  1277),
                        new TranscriptRegion(TranscriptRegionType.Exon,   5,  43364294, 43364411, 663,  780),
                        new TranscriptRegion(TranscriptRegionType.Intron, 4,  43364412, 43364519, 662,  663),
                        new TranscriptRegion(TranscriptRegionType.Exon,   4,  43364520, 43364730, 452,  662),
                        new TranscriptRegion(TranscriptRegionType.Intron, 3,  43364731, 43366601, 451,  452),
                        new TranscriptRegion(TranscriptRegionType.Exon,   3,  43366602, 43366671, 382,  451),
                        new TranscriptRegion(TranscriptRegionType.Intron, 2,  43366672, 43367855, 381,  382),
                        new TranscriptRegion(TranscriptRegionType.Exon,   2,  43367856, 43368131, 106,  381),
                        new TranscriptRegion(TranscriptRegionType.Intron, 1,  43368132, 43394325, 105,  106),
                        new TranscriptRegion(TranscriptRegionType.Exon,   1,  43394326, 43394430, 1,    105),
                    };

                    var codingRegion = new CodingRegion(43342003, 43368111, 126, 2969, 2844);
                    translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                        "MAVMEMACPGAPGSAVGQQKELPKAKEKTPPLGKKQSSVYKLEAVEKSPVFCGKWEILNDVITKGTAKEGSEAGPAAISIIAQAECENSQEFSPTFSERIFIAGSKQYSQSESLDQIPNNVAHATEGKMARVCWKGKRRSKARKKRKKKSSKSLAHAGVALAKPLPRTPEQESCTIPVQEDESPLGAPYVRNTPQFTKPLKEPGLGQLCFKQLGEGLRPALPRSELHKLISPLQCLNHVWKLHHPQDGGPLPLPTHPFPYSRLPHPFPFHPLQPWKPHPLESFLGKLACVDSQKPLPDPHLSKLACVDSPKPLPGPHLEPSCLSRGAHEKFSVEEYLVHALQGSVSSGQAHSLTSLAKTWAARGSRSREPSPKTEDNEGVLLTEKLKPVDYEYREEVHWATHQLRLGRGSFGEVHRMEDKQTGFQCAVKKVRLEVFRAEELMACAGLTSPRIVPLYGAVREGPWVNIFMELLEGGSLGQLVKEQGCLPEDRALYYLGQALEGLEYLHSRRILHGDVKADNVLLSSDGSHAALCDFGHAVCLQPDGLGKSLLTGDYIPGTETHMAPEVVLGRSCDAKVDVWSSCCMMLHMLNGCHPWTQFFRGPLCLKIASEPPPVREIPPSCAPLTAQAIQEGLRKEPIHRVSAAELGGKVNRALQQVGGLKSPWRGEYKEPRHPPPNQANYHQTLHAQPRELSPRAPGPRPAEETTGRAPKLQPPLPPEPPEPNKSPPLTLSKEESGMWEPLPLSSLEPAPARNPSSPERKATVPEQELQQLEIELFLNSLSQPFSLEEQEQILSCLSIDSLSLSDDSEKNPSKASQSSRDTLSSGVHSWSSQAEARSSSWNMVLARGRPTDTPSYFNGVKVQIQSLNGEHLHIREFHRVKVGDIATGISSQIPAAAFSLVTKDGQPVRYDMEVPDSGIDLQCTLAPDGSFAWSWRVKHGQLENRP*");
                }
                
                // update the protein sequences
                switch (transcriptId)
                {
                    case "NM_002006.4":
                    case "NM_001243186.1":
                    case "NM_003376.5":
                    case "NM_001171622.1":
                    case "NM_001033756.2":
                    case "NM_001025366.2":
                    case "NM_001025368.2":
                    case "NM_001204385.1":
                    case "NM_001025370.2":
                    case "NM_001025369.2":
                    case "NM_001025367.2":
                    case "NM_002467.4":
                    case "NM_024424.3":
                    case "NM_000378.4":
                    case "NM_024426.4":
                    case "NM_001287424.1":
                        char[] aaSequence = translation.PeptideSeq.ToCharArray();
                        aaSequence[0] = 'M';
                        translation = new Translation(translation.CodingRegion, (CompactId)translation.ProteinId,
                            new string(aaSequence));
                        break;

                    case "NM_001317010.1":
                        char[] aaSequence2 = translation.PeptideSeq.ToCharArray();
                        aaSequence2[191] = 'S';
                        translation = new Translation(translation.CodingRegion, (CompactId)translation.ProteinId,
                            new string(aaSequence2));
                        break;
                }
                
                if (updatedGeneModel)
                {
                    int newStart = transcriptRegions[0].Start;
                    int newEnd   = transcriptRegions[transcriptRegions.Length - 1].End;

                    if (newStart != start)
                    {
                        Console.WriteLine($"Found new start for {transcriptId}: old: {start:N0}, new: {newStart:N0}");
                        // start = newStart;
                    }

                    if (newEnd != end)
                    {
                        Console.WriteLine($"Found new end for {transcriptId}: old: {end:N0}, new: {newEnd:N0}");
                        // end = newEnd;
                    }

                    if (newStart < gene.Start)
                    {
                        Console.WriteLine(
                            $"Found new GENE start for {gene.Symbol}: old: {gene.Start:N0}, new: {newStart:N0}");
                        // gene.Start = newStart;
                    }

                    if (newEnd > gene.End)
                    {
                        Console.WriteLine(
                            $"Found new GENE end for {gene.Symbol}: old: {gene.End:N0}, new: {newEnd:N0}");
                        // gene.End = newEnd;
                    }
                }
            }

            var transcript = new Transcript(chromosomeIndexDictionary[referenceIndex], start, end, id, translation,
                encoded.BioType, gene, ExonUtilities.GetTotalExonLength(transcriptRegions), startExonPhase,
                encoded.IsCanonical, transcriptRegions, numExons, mirnas, siftIndex, polyphenIndex,
                encoded.TranscriptSource, encoded.CdsStartNotFound, encoded.CdsEndNotFound, rnaEdits);

            // add the AA edits
            switch (transcriptId)
            {
                case "NM_002006.4":
                case "NM_001243186.1":
                case "NM_003376.5":
                case "NM_001171622.1":
                case "NM_001033756.2":
                case "NM_001025366.2":
                case "NM_001025368.2":
                case "NM_001204385.1":
                case "NM_001025370.2":
                case "NM_001025369.2":
                case "NM_001025367.2":
                case "NM_002467.4":
                case "NM_024424.3":
                case "NM_000378.4":
                case "NM_024426.4":
                case "NM_001287424.1":
                    transcript.AminoAcidEdits = new[] {new AminoAcidEdit(1, 'M')};
                    break;

                case "NM_001317010.1":
                    transcript.AminoAcidEdits = new[] {new AminoAcidEdit(192, 'S')};
                    break;
            }

            return transcript;
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
                false, TranscriptRegions != null, Translation != null, StartExonPhase);
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
            if (encoded.HasMirnas) WriteIndices(writer, MicroRnas, microRnaIndices);
            if (encoded.HasRnaEdits) WriteItems(writer,        RnaEdits,        (x, y) => x.Write(y));
        }

        private static T[] ReadItems<T>(BufferedBinaryReader reader, Func<BufferedBinaryReader, T> readFunc)
        {
            int numItems                                = reader.ReadOptInt32();
            var items                                   = new T[numItems];
            for (int i = 0; i < numItems; i++) items[i] = readFunc(reader);
            return items;
        }

        private static void WriteItems<T>(IExtendedBinaryWriter writer, T[] items,
            Action<T, IExtendedBinaryWriter> writeAction)
        {
            writer.WriteOpt(items.Length);
            foreach (var item in items) writeAction(item, writer);
        }

        private static T[] ReadIndices<T>(IBufferedBinaryReader reader, T[] cachedItems)
        {
            int numItems = reader.ReadOptInt32();
            var items    = new T[numItems];

            for (int i = 0; i < numItems; i++)
            {
                var index = reader.ReadOptInt32();
                items[i] = cachedItems[index];
            }

            return items;
        }

        private static void WriteIndices<T>(IExtendedBinaryWriter writer, T[] items,
            IReadOnlyDictionary<T, int> indices)
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