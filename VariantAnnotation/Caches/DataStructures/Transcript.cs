using System;using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Transcript : ITranscript
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public ICompactId Id { get; }
        public BioType BioType { get; }
        public bool IsCanonical { get; }
        public Source Source { get; }
        public IGene Gene { get; }
        public ITranscriptRegion[] TranscriptRegions { get; }
        public ushort NumExons { get; }
        public int TotalExonLength { get; }
        public byte StartExonPhase { get; }
        public int SiftIndex { get; }
        public int PolyPhenIndex { get; }
        public ITranslation Translation { get; }
        public IInterval[] MicroRnas { get; }
        public int[] Selenocysteines { get; }
        public IRnaEdit[] RnaEdits { get; }
        public bool CdsStartNotFound { get; }
        public bool CdsEndNotFound { get; }
        public ISequence CodingSequence { get; set; }
        public ISequence CdnaSequence { get; set; }

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
            ITranscriptRegion[] cacheTranscriptRegions, IInterval[] cacheMirnas, string[] cachePeptideSeqs, ISequenceProvider sequenceProvider)
        {
            // transcript
            var referenceIndex = reader.ReadOptUInt16();
            var start          = reader.ReadOptInt32();
            var end            = reader.ReadOptInt32();
            var id             = CompactId.Read(reader);

            // gene
            var geneIndex = reader.ReadOptInt32();
            var gene      = cacheGenes[geneIndex];

            // encoded data
            var encoded = EncodedTranscriptData.Read(reader);

            // transcript regions
            var transcriptRegions = encoded.HasTranscriptRegions ? ReadIndices(reader, cacheTranscriptRegions) : null;
            ushort numExons       = reader.ReadOptUInt16();

            // protein function predictions
            int siftIndex     = encoded.HasSift     ? reader.ReadOptInt32() : -1;
            int polyphenIndex = encoded.HasPolyPhen ? reader.ReadOptInt32() : -1;

            // translation
            var translation = encoded.HasTranslation ? DataStructures.Translation.Read(reader, cachePeptideSeqs) : null;

            // attributes
            var mirnas          = encoded.HasMirnas          ? ReadIndices(reader, cacheMirnas)         : null;
            var rnaEdits        = encoded.HasRnaEdits        ? ReadItems(reader, RnaEdit.Read)          : null;
            var selenocysteines = encoded.HasSelenocysteines ? ReadItems(reader, x => x.ReadOptInt32()) : null;

            var chromosome = chromosomeIndexDictionary[referenceIndex];

            var startExonPhase = encoded.StartExonPhase;
            
            //NM_022148.2
            if (id.WithVersion == "NM_022148.2" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                var transcriptId = "NM_022148.2";
                rnaEdits = new IRnaEdit[]
                {
                    new RnaEdit(1, 0, "AATTCGGCACGAGG"),
                    new RnaEdit(770, 780, "AGTGAAGAAGT"),
                    new RnaEdit(782, 782, "T"),
                    new RnaEdit(785, 788, "CATT"),
                    new RnaEdit(792, 794, "AGC"),
                    new RnaEdit(795, 794, "GTGCCAGACCCGAAATCCATCTTCCCCGGGCTCTTTGAGATACACCAAGGGAACTTCCAGGAGTGGATCACAGACACCCAGAACGTGGCCCACCTCCACAAGATGGCAGGTGCAGAGCAAGAAAGTGGCCCCGAGGAGCCCCTGGTAGTCCAGTTGGCCAAGACTGAAGCCGAGTCTCCCAGGATGCTGGACCCACAGACCGAGGAGAAAGAGGCCTCTGGGGGATCCCTCCAGCTTCCCCACCAGCCCCTCCAAGGCGGTGATGTGGTCACAATCGGGGGCTTCACCTTTGTGATGAATGACCGCTCCTACGTGGCGTTGTGATGGACACACCACTGTCAAAGTCAACGTCAGGATCCACGTTGACATTTAAAGACAGAGGGGACTGTCCCGGGGACTCCACACCACCATGGATGGGAAGTCTCCACGCCAATGATGGTAGGACTAGGAGACTCTGAAGACCCAGCCTCACCGCCTAATGCGGCCACTGCCCTGCTAACTTTCCCCCACATGAGTCTCTGTGTTCAAAGGCTTGATGGCAGATGGGAGCCAATTGCTCCAGGAGATTTACTCCCAGTTCCTTTTCGTGCCTGAACGTTGTCACATAAACCCCAAGGCAGCACGTCCAAAATGCTGTAAAACCATCTTCCCACTCTGTGAGTCCCCAGTTCCGTCCATGTACCTGTTCCATAGCATTGGATTCTCGGAGGATTTTTTGTCTGTTTTGAGACTCCAAACCACCTCTACCCCTACAAAAAAAAAAAAAAAAAA"),
                };
                
                var codingRegion = new CodingRegion(translation.CodingRegion.Start, translation.CodingRegion.End, 17, 1132, 1116);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, transcriptId, gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
            }
            
            //NM_012234.6
            if (id.WithVersion == "NM_012234.6"  && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                var transcriptId = "NM_012234.6";
                var codingRegion = new CodingRegion(translation.CodingRegion.Start, translation.CodingRegion.End, translation.CodingRegion.CdnaStart+183, translation.CodingRegion.CdnaEnd, 687);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MTMGDKKSPTRPKRQAKPAADEGFWDCSVCTFRNSAEAFKCSICDVRKGTSTRKPRINSQLVAQQVAQQYATPPPPKKEKKEKVEKQDKEKPEKDKEISPSVTKKNTNKKTKPKSDILKDPPSEANSIQSANATTKTSETNHTSRPRLKNVDRSTAQQLAVTVGNVTVIITDFKEKTRSSSTSSSTVTSSAGSEQQNQSSSGSESTDKGSSRSSTPKGDMSAVNDESF*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, transcriptId, gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }
            // NM_001220773.1
            if (id.WithVersion == "NM_001220773.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                var transcriptId = "NM_001220773.1";
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
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50467616, 50472799, 470, 5653),
                };
                
                var codingRegion = new CodingRegion(50455032, 50468325, 169, 1179, 1011);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, transcriptId, gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }
            
            if (id.WithVersion == "NM_152756.3" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                var newRnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(1,0,"GG"), 
                    new RnaEdit(3196,3196,"T")
                };

                rnaEdits = newRnaEdits;
                
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, 25, 5151, 5127);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, translation.PeptideSeq);

                
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_152756.3", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001242758.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(40,40,"T"),
                    new RnaEdit(287,287,"A"),
                    new RnaEdit(355,355,"A"),
                    new RnaEdit(366,366,"C"),
                    new RnaEdit(383,383,"C"),
                    new RnaEdit(385,385,"A"),
                    new RnaEdit(425,425,"A"),
                    new RnaEdit(469,469,"C"),
                    new RnaEdit(573,573,"A"),
                    new RnaEdit(605,605,"T"),
                    new RnaEdit(611,611,"C"),
                    new RnaEdit(622,623,"CG"),
                    new RnaEdit(629,629,"T"),
                    new RnaEdit(639,639,"G"),
                    new RnaEdit(643,643,"C"),
                    new RnaEdit(643,644,"CG"),
                    new RnaEdit(654,655,"CG"),
                    new RnaEdit(1161,1161,"T"),
                    new RnaEdit(1324,1324,"G"),
                    new RnaEdit(1380,1380,"T"),
                    new RnaEdit(1492,1492,"G"),
                    new RnaEdit(1580,1580,"T"),
                    new RnaEdit(1588,1589,"CG"),
                };
                
                TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001242758.1", gene.OnReverseStrand,
                    transcriptRegions,
                    rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_002447.2" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(3847,3847,"G"), 
                    new RnaEdit(4773,4772,"AAAAAAAAAAAAA"),
                };
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_002447.2", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
            }
            
            if (id.WithVersion == "NM_005228.3" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(2955,2955,"C"), 
                    new RnaEdit(5601,5600,"AAAAAAAAAAAAAAAA"),
                };
                
                TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_005228.3", gene.OnReverseStrand,
                    transcriptRegions,
                    rnaEdits, translation);
                
            }
            
            if (id.WithVersion == "NM_005922.2" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(612,612,"A"), 
                    new RnaEdit(5485,5484,"AAAAAAAAAAAAAAAAA"),
                };

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_005922.2", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }
            
            if (id.WithVersion == "NM_006724.2" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(612,612,"A"), 
                    new RnaEdit(5335,5334,"AAAAAAAAAAAAAAAAA"),
                };

                TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_006724.2", gene.OnReverseStrand,
                    transcriptRegions,
                    rnaEdits, translation);
            }
            
            if (id.WithVersion == "NM_019063.3" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(1109,1109,"G"), 
                    new RnaEdit(1406,1406,"G"), 
                    new RnaEdit(5550,5549,"AAAAAAAAAAAAAAAA"),
                };

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_019063.3", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_175741.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(220,220,"T"),
                    new RnaEdit(380,380,"C")
                };

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_175741.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NR_003085.2" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(1703,1703,"G"), 
                    new RnaEdit(2832,2831,"AAAAAAAAAAAAAAA"),
                };
                
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NR_003085.2", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001244937.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[] 
                {
                    new RnaEdit(3700,3700,"G"),
                    new RnaEdit(4626,4625,"AAAAAAAAAAAAA"),
                };
                
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001244937.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }
            
            if (id.WithVersion == "NM_001278433.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[]
                {
                    new RnaEdit(4308, 4307, "AAAAAAAAAAAAAAAAAAAAA")
                };
            
                transcriptRegions = new ITranscriptRegion[]
                {
                    new TranscriptRegion(TranscriptRegionType.Exon,   1,  66409764, 66409936, 1,    173),
                    new TranscriptRegion(TranscriptRegionType.Intron, 1,  66409937, 66511534, 173,  174),
                    new TranscriptRegion(TranscriptRegionType.Exon,   2,  66511535, 66511717, 174,  356),
                    new TranscriptRegion(TranscriptRegionType.Intron, 2,  66511718, 66518896, 356,  357),
                    new TranscriptRegion(TranscriptRegionType.Exon,   3,  66518897, 66519067, 357,  527),
                    new TranscriptRegion(TranscriptRegionType.Intron, 3,  66519068, 66519865, 527,  528),
                    new TranscriptRegion(TranscriptRegionType.Exon,   4,  66519866, 66519957, 528,  619),
                    new TranscriptRegion(TranscriptRegionType.Intron, 4,  66519958, 66520156, 619,  620),
                    new TranscriptRegion(TranscriptRegionType.Exon,   5,  66520157, 66520218, 620,  681),
                    new TranscriptRegion(TranscriptRegionType.Intron, 5,  66520219, 66521052, 681,  682),
                    new TranscriptRegion(TranscriptRegionType.Exon,   6,  66521053, 66521099, 682,  728),
                    new TranscriptRegion(TranscriptRegionType.Intron, 6,  66521100, 66521894, 728,  729),
                    new TranscriptRegion(TranscriptRegionType.Exon,   7,  66521895, 66522053, 729,  887),
                    new TranscriptRegion(TranscriptRegionType.Intron, 7,  66522054, 66523980, 887,  888),
                    new TranscriptRegion(TranscriptRegionType.Exon,   8,  66523981, 66524041, 888,  948),
                    new TranscriptRegion(TranscriptRegionType.Intron, 8,  66524042, 66525010, 948,  949),
                    new TranscriptRegion(TranscriptRegionType.Exon,   9,  66525011, 66525132, 949,  1070),
                    new TranscriptRegion(TranscriptRegionType.Intron, 9,  66525133, 66526060, 1070, 1071),
                    new TranscriptRegion(TranscriptRegionType.Exon,   10, 66526061, 66526142, 1071, 1152),
                    new TranscriptRegion(TranscriptRegionType.Intron, 10, 66526143, 66526417, 1152, 1153),
                    new TranscriptRegion(TranscriptRegionType.Exon,   11, 66526418, 66529572, 1153, 4307)
                };
                
                startExonPhase = 0;
                
                var codingRegion = new CodingRegion(66511541, 66526590, 180, 1325, 1146);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MESGSTAASEEARSLRECELYVQKHNIQALLKDSIVQLCTARPERPMAFLREYFERLEKEEAKQIQNLQKAGTRTDSREDEISPPPPNPVVKGRRRRGAISAEVYTEEDAASYVRKVIPKDYKTMAALAKAIEKNVLFSHLDDNERSDIFDAMFSVSFIAGETVIQQGDEGDNFYVIDQGETDVYVNNEWATSVGEGGSFGELALIYGTPRAATVKAKTNVKLWGIDRDSYRRILMGSTLRKRKMYEEFLSKVSILESLDKWERLTVADALEPVQFEDGQKIVVQGEPGDEFFIILEGSAAVLQRRSENEEFVEVGRLGPSDYFGEIALLMNRPRAATVVARGPLKCVKLDRPRFERVLGPCSDILKRNIQQYNSFVSLSV*");
            
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001278433.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation, startExonPhase);
            }
            
            if (id.WithVersion == "NM_001260.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
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
            
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001260.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation, startExonPhase);
                
            }

            if (id.WithVersion == "NM_000314.4" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                var newRegions = new List<ITranscriptRegion>();

                var oldExon = transcriptRegions[0];
                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, 89623860,
                    oldExon.CdnaStart, 666);
                var gap1 = new TranscriptRegion(TranscriptRegionType.Gap, 1, 89623861, 89623861, 666, 667);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, 89623862, oldExon.End, 667,
                    oldExon.CdnaEnd - 1);

                newRegions.Add(exon1a);
                newRegions.Add(gap1);
                newRegions.Add(exon1b);
 
                for (int i = 1; i < transcriptRegions.Length; i++)
                {
                    var region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart - 1, region.CdnaEnd - 1));
                }

                transcriptRegions = newRegions.ToArray();
                
                rnaEdits    = new IRnaEdit[3];
                rnaEdits[0] = new RnaEdit(667,  667, null);
                rnaEdits[1] = new RnaEdit(707,  707, "C");
                rnaEdits[2] = new RnaEdit(5548, 5547, "AAAAAAAAAAAAAAAAAAAAAAAAAA");

                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, 1032, 2243, 1212);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, translation.PeptideSeq);
                
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_000314.4", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation, startExonPhase);
                
            }

            if (id.WithVersion == "NM_000535.5" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[2];
                rnaEdits[0] = new RnaEdit(1708, 1708, "G");
                rnaEdits[1] = new RnaEdit(2837, 2836, "AAAAAAAAAAAAAAA");
                
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, 88, 2676, 2589);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, translation.PeptideSeq);
                
                TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_000535.5", gene.OnReverseStrand,
                    transcriptRegions,
                    rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_000545.5" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[2];
                rnaEdits[0] = new RnaEdit(1743, 1743, "G");
                rnaEdits[1] = new RnaEdit(3240, 3239, "AA");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_000545.5", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001145076.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[3];
                rnaEdits[0] = new RnaEdit(935,  935,  "G");
                rnaEdits[1] = new RnaEdit(1232, 1232, "G");
                rnaEdits[2] = new RnaEdit(5376, 5375, "AAAAAAAAAAAAAAAA");

                TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001145076.1", gene.OnReverseStrand,
                    transcriptRegions,
                    rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001220765.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[5];
                rnaEdits[0] = new RnaEdit(1,  0,  "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,  5,  "A");
                rnaEdits[2] = new RnaEdit(16, 16, "T");
                rnaEdits[3] = new RnaEdit(97, 97, "C");
                rnaEdits[4] = new RnaEdit(316, 315, "CCAGTAATGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];

                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);

                newRegions.Add(exon1a);
                newRegions.Add(exon1b);

                for (int i = 1; i < transcriptRegions.Length; i++)
                {
                    var region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }

                transcriptRegions = newRegions.ToArray();
                
                // in genomic coordinates we only have enough information until 50367353, but because of RNA-edits, our cDNA end is now at 1602
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367353, 169, 1602, 1434);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220765.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);

                
            }

            if (id.WithVersion == "NM_001220766.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[6];
                rnaEdits[0] = new RnaEdit(1,   0,   "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,   5,   "A");
                rnaEdits[2] = new RnaEdit(16,  16,  "T");
                rnaEdits[3] = new RnaEdit(97,  97,  "C");
                rnaEdits[4] = new RnaEdit(317, 318, null);
                rnaEdits[5] = new RnaEdit(321, 320,
                    "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];
                
                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);
                
                newRegions.Add(exon1a);
                newRegions.Add(exon1b);
                ITranscriptRegion region;

                for (int i = 1; i < transcriptRegions.Length - 1; i++)
                {
                    region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }
                
                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367234, 50367354,
                    209, 329));
                
                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Gap, 3, 50367355, 50367356,
                    329, 330));
                
                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367357, 50367358,
                    330, 331));
                
                transcriptRegions = newRegions.ToArray();
                
                // in genomic coordinates we only have enough information until 50367358, but because of RNA-edits, our cDNA end is now at 1467
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367358, 169, 1467, 1299);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
                
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220766.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);

                
            }

            if (id.WithVersion == "NM_001220767.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[6];
                rnaEdits[0] = new RnaEdit(1,   0,   "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,   5,   "A");
                rnaEdits[2] = new RnaEdit(16,  16,  "T");
                rnaEdits[3] = new RnaEdit(97,  97,  "C");
                rnaEdits[4] = new RnaEdit(317, 318, null);
                rnaEdits[5] = new RnaEdit(321, 320, "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];

                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);

                newRegions.Add(exon1a);
                newRegions.Add(exon1b);
                ITranscriptRegion region;

                for (int i = 1; i < transcriptRegions.Length - 1; i++)
                {
                    region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367234, 50367354,
                    209, 329));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Gap, 3, 50367355, 50367356,
                    329, 330));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367357, 50367358,
                    330, 331));

                transcriptRegions = newRegions.ToArray();
                
                // in genomic coordinates we only have enough information until 50367358, but because of RNA-edits, our cDNA end is now at 1467
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367358, 169, 1311, 1143);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220767.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001220769.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[6];
                rnaEdits[0] = new RnaEdit(1,   0,   "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,   5,   "A");
                rnaEdits[2] = new RnaEdit(16,  16,  "T");
                rnaEdits[3] = new RnaEdit(97,  97,  "C");
                rnaEdits[4] = new RnaEdit(317, 318, null);
                rnaEdits[5] = new RnaEdit(321, 320, "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];

                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);

                newRegions.Add(exon1a);
                newRegions.Add(exon1b);
                ITranscriptRegion region;

                for (int i = 1; i < transcriptRegions.Length - 1; i++)
                {
                    region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367234, 50367354,
                    209, 329));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Gap, 3, 50367355, 50367356,
                    329, 330));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367357, 50367358,
                    330, 331));

                transcriptRegions = newRegions.ToArray();
                
                // in genomic coordinates we only have enough information until 50367358, but because of RNA-edits, our cDNA end is now at 1341
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367358, 169, 1341, 1173);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId, "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220769.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001220770.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[6];
                rnaEdits[0] = new RnaEdit(1,   0,   "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,   5,   "A");
                rnaEdits[2] = new RnaEdit(16,  16,  "T");
                rnaEdits[3] = new RnaEdit(97,  97,  "C");
                rnaEdits[4] = new RnaEdit(317, 318, null);
                rnaEdits[5] = new RnaEdit(321, 320,
                    "AACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];

                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);

                newRegions.Add(exon1a);
                newRegions.Add(exon1b);
                ITranscriptRegion region;

                for (int i = 1; i < transcriptRegions.Length - 1; i++)
                {
                    region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367234, 50367354,
                    209, 329));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Gap, 3, 50367355, 50367356,
                    329, 330));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367357, 50367358,
                    330, 331));

                transcriptRegions = newRegions.ToArray();

                // in genomic coordinates we only have enough information until 50367358, but because of RNA-edits, our cDNA end is now at 1341
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367358, 169, 1311, 1143);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220770.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001220768.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[6];
                rnaEdits[0] = new RnaEdit(1,   0,   "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,   5,   "A");
                rnaEdits[2] = new RnaEdit(16,  16,  "T");
                rnaEdits[3] = new RnaEdit(97,  97,  "C");
                rnaEdits[4] = new RnaEdit(316, 315, "CCA");
                rnaEdits[5] = new RnaEdit(320, 319, "TGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];

                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);

                newRegions.Add(exon1a);
                newRegions.Add(exon1b);
                ITranscriptRegion region;

                for (int i = 1; i < transcriptRegions.Length - 1; i++)
                {
                    region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367234, 50367353,
                    209, 329));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367354, 50367357,
                    332, 335));

                transcriptRegions = newRegions.ToArray();

                // in genomic coordinates we only have enough information until 50367357, but because of RNA-edits, our cDNA end is now at 1467
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367357, 169, 1467, 1299);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220768.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
                
            }
            
            if (id.WithVersion == "NM_006060.4" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits    = new IRnaEdit[6];
                rnaEdits[0] = new RnaEdit(1,   0,   "GAATTCCGGCGT");
                rnaEdits[1] = new RnaEdit(6,   5,   "A");
                rnaEdits[2] = new RnaEdit(16,  16,  "T");
                rnaEdits[3] = new RnaEdit(97,  97,  "C");
                rnaEdits[4] = new RnaEdit(316, 315, "CCA");
                rnaEdits[5] = new RnaEdit(320, 319, "TGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGAGAACGGCCCTTCCAGTGCAATCAGTGCGGGGCCTCATTCACCCAGAAGGGCAACCTGCTCCGGCACATCAAGCTGCATTCCGGGGAGAAGCCCTTCAAATGCCACCTCTGCAACTACGCCTGCCGCCGGAGGGACGCCCTCACTGGCCACCTGAGGACGCACTCCGTTGGTAAACCTCACAAATGTGGATATTGTGGCCGAAGCTATAAACAGCGAAGCTCTTTAGAGGAACATAAAGAGCGCTGCCACAACTACTTGGAAAGCATGGGCCTTCCGGGCACACTGTACCCAGTCATTAAAGAAGAAACTAATCACAGTGAAATGGCAGAAGACCTGTGCAAGATAGGATCAGAGAGATCTCTCGTGCTGGACAGACTAGCAAGTAACGTCGCCAAACGTAAGAGCTCTATGCCTCAGAAATTTCTTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA");

                var               newRegions = new List<ITranscriptRegion>();
                ITranscriptRegion oldExon    = transcriptRegions[0];

                var exon1a = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start, oldExon.Start + 5, 13,
                    17);
                var exon1b = new TranscriptRegion(TranscriptRegionType.Exon, 1, oldExon.Start + 6, oldExon.End, 19,
                    oldExon.CdnaEnd                                                           + 13);

                newRegions.Add(exon1a);
                newRegions.Add(exon1b);
                ITranscriptRegion region;

                for (int i = 1; i < transcriptRegions.Length - 1; i++)
                {
                    region = transcriptRegions[i];
                    newRegions.Add(new TranscriptRegion(region.Type, region.Id, region.Start, region.End,
                        region.CdnaStart + 13, region.CdnaEnd + 13));
                }

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367234, 50367353,
                    209, 329));

                newRegions.Add(new TranscriptRegion(TranscriptRegionType.Exon, 3, 50367354, 50367357,
                    332, 335));

                transcriptRegions = newRegions.ToArray();

                // in genomic coordinates we only have enough information until 50367357, but because of RNA-edits, our cDNA end is now at 1728
                var oldCodingRegion = translation.CodingRegion;
                var codingRegion    = new CodingRegion(oldCodingRegion.Start, 50367357, 169, 1728, 1560);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGERPFQCNQCGASFTQKGNLLRHIKLHSGEKPFKCHLCNYACRRRDALTGHLRTHSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_006060.4", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }
            
            if (id.WithVersion == "NM_001220775.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
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
                    // insertion
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459422, 50459424, 204, 206),
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459425, 50459561, 208, 343),
                    new TranscriptRegion(TranscriptRegionType.Intron, 1, 50459562, 50467615, 343, 344),
                    new TranscriptRegion(TranscriptRegionType.Exon,   2, 50467616, 50472799, 344, 5527)
                };
                
                startExonPhase = 0;
                
                var codingRegion    = new CodingRegion(50459422, 50468325, 169, 1053, 885);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRKSSMPQKFLGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");
            
                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220775.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);

            }

            if (id.WithVersion == "NM_001220774.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
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

                var codingRegion = new CodingRegion(50455032, 50468325, 169, 1149, 981);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSVGKPHKCGYCGRSYKQRSSLEEHKERCHNYLESMGLPGTLYPVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220774.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }

            if (id.WithVersion == "NM_001220776.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[]
                {
                    new RnaEdit(1, 0,
                        "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAG"),
                    new RnaEdit(4,    3,    "C"),
                    new RnaEdit(5295, 5294, "AAAAAAAAAAAAAAA")
                };
                
                transcriptRegions = new ITranscriptRegion[]
                {
                    // insertion
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459422, 50459424, 204, 206),
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50459425, 50459531, 208, 313),
                    new TranscriptRegion(TranscriptRegionType.Intron, 1, 50459532, 50467615, 313, 314),
                    new TranscriptRegion(TranscriptRegionType.Exon,   2, 50467616, 50472799, 314, 5497),
                };
                
                startExonPhase = 0;

                var codingRegion = new CodingRegion(50459422, 50468325, 169, 1023, 855);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSVIKEETNHSEMAEDLCKIGSERSLVLDRLASNVAKRDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220776.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                
            }
            
            if (id.WithVersion == "NM_001220772.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[]
                {
                    new RnaEdit(1, 0,
                        "GAATTCCGGCGTCGCGGACGCATCCCAGTCTGGGCGGGACGCTCGGCCGCGGCGAGGCGGGCAAGCCTGGCAGGGCAGAGGGAGCCCCGGCTCCGAGGTTGCTCTTCGCCCCCGAGGATCAGTCTTGGCCCCAAAGCGCGACGCACAAATCCACATAACCTGAGGACCATGGATGCTGATGAGGGTCAAGACATGTCCCAAGTTT"),
                    new RnaEdit(5188, 5187, "AAAAAAAAAAAAAAA")
                };
                
                transcriptRegions = new ITranscriptRegion[]
                {
                    new TranscriptRegion(TranscriptRegionType.Exon, 1, 50467613, 50472799, 206, 5392),
                };
                
                startExonPhase = 0;

                var codingRegion = new CodingRegion(50467613, 50468325, 169, 918, 750);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220772.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);
                //
            }
            
            if (id.WithVersion == "NM_001220771.1" && sequenceProvider.Assembly != GenomeAssembly.GRCh38)
            {
                rnaEdits = new IRnaEdit[]
                {
                    new RnaEdit(1, 0, "GAATTCCGGCGT"),
                    new RnaEdit(6, 5, "A"),
                    new RnaEdit(16, 16, "T"),
                    new RnaEdit(97, 97, "C"),
                    new RnaEdit(316, 315, "CCAGTAATGTTAAAGTAGAGACTCAGAGTGATGAAGAGAATGGGCGTGCCTGTGAAATGAATGGGGAAGAATGTGCGGAGGATTTACGAATGCTTGATGCCTCGGGAGAGAAAATGAATGGCTCCCACAGGGACCAAGGCAGCTCGGCTTTGTCGGGAGTTGGAGGCATTCGACTTCCTAACGGAAAACTAAAGTGTGATATCTGTGGGATCATTTGCATCGGGCCCAATGTGCTCATGGTTCACAAAAGAAGCCACACTGGGGACAAGGGCCTGTCCGACACGCCCTACGACAGCAGCGCCAGCTACGAGAAGGAGAACGAAATGATGAAGTCCCACGTGATGGACCAAGCCATCAACAACGCCATCAACTACCTGGGGGCCGAGTCCCTGCGCCCGCTGGTGCAGACGCCCCCGGGCGGTTCCGAGGTGGTCCCGGTCATCAGCCCGATGTACCAGCTGCACAAGCCGCTCGCGGAGGGCACCCCGCGCTCCAACCACTCGGCCCAGGACAGCGCCGTGGAGAACCTGCTGCTGCTCTCCAAGGCCAAGTTGGTGCCCTCGGAGCGCGAGGCGTCCCCGAGCAACAGCTGCCAAGACTCCACGGACACCGAGAGCAACAACGAGGAGCAGCGCAGCGGTCTCATCTACCTGACCAACCACATCGCCCCGCACGCGCGCAACGGGCTGTCGCTCAAGGAGGAGCACCGCGCCTACGACCTGCTGCGCGCCGCCTCCGAGAACTCGCAGGACGCGCTCCGCGTGGTCAGCACCAGCGGGGAGCAGATGAAGGTGTACAAGTGCGAACACTGCCGGGTGCTCTTCCTGGATCACGTCATGTACACCATCCACATGGGCTGCCACGGCTTCCGTGATCCTTTTGAGTGCAACATGTGCGGCTACCACAGCCAGGACCGGTACGAGTTCTCGTCGCACATAACGCGAGGGGAGCACCGCTTCCACATGAGCTAAAGCCCTCCCGCGCCCCCACCCCAGACCCCGAGCCACCCCAGGAAAAGCACAAGGACTGCCGCCTTCTCGCTCCCGCCAGCAGCATAGACTGGACTGGACCAGACAATGTTGTGTTTGGATTTGTAACTGTTTTTTGTTTTTTGTTTGAGTTGGTTGATTGGGGTTTGATTTGCTTTTGAAAAGATTTTTATTTTTAGAGGCAGGGCTGCATTGGGAGCATCCAGAACTGCTACCTTCCTAGATGTTTCCCCAGACCGCTGGCTGAGATTCCCTCACCTGTCGCTTCCTAGAATCCCCTTCTCCAAACGATTAGTCTAAATTTTCAGAGAGAAATAGATAAAACACGCCACAGCCTGGGAAGGAGCGTGCTCTACCCTGTGCTAAGCACGGGGTTCGCGCACCAGGTGTCTTTTTCCAGTCCCCAGAAGCAGAGAGCACAGCCCCTGCTGTGTGGGTCTGCAGGTGAGCAGACAGGACAGGTGTGCCGCCACCCAAGTGCCAAGACACAGCAGGGCCAACAACCTGTGCCCAGGCCAGCTTCGAGCTACATGCATCTAGGGCGGAGAGGCTGCACTTGTGAGAGAAAATACTATTTCAAGTCATATTCTGCGTAGGAAAATGAATTGGTTGGGGAAAGTCGTGTCTGTCAGACTGCCCTGGGTGGAGGGAGACGCCGGGCTAGAGCCTTTGGGATCGTCCTGGATTCACTGGCTTTGCGGAGGCTGCTCAGATGGCCTGAGCCTCCCGAGGCTTGCTGCCCCGTAGGAGGAGACTGTCTTCCCGTGGGCATATCTGGGGAGCCCTGTTCCCCGCTTTTTCACTCCCATACCTTTAATGGCCCCCAAAATCTGTCACTACAATTTAAACACCAGTCCCGAAATTTGGATCTTCTTTCTTTTTGAATCTCTCAAACGGCAACATTCCTCAGAAACCAAAGCTTTATTTCAAATCTCTTCCTTCCCTGGCTGGTTCCATCTAGTACCAGAGGCCTCTTTTCCTGAAGAAATCCAATCCTAGCCCTCATTTTAATTATGTACATCTGTTTGTAGCCACAAGCCTGAATTTCTCAGTGTTGGTAAGTTTCTTTACCTACCCTCACTATATATTATTCTCGTTTTAAAACCCATAAAGGAGTGATTTAGAACAGTCATTAATTTTCAACTCAATGAAATATGTGAAGCCCAGCATCTCTGTTGCTAACACACAGAGCTCACCTGTTTGAAACCAAGCTTTCAAACATGTTGAAGCTCTTTACTGTAAAGGCAAGCCAGCATGTGTGTCCACACATACATAGGATGGCTGGCTCTGCACCTGTAGGATATTGGAATGCACAGGGCAATTGAGGGACTGAGCCAGACCTTCGGAGAGTAATGCCACCAGATCCCCTAGGAAAGAGGAGGCAAATGGCACTGCAGGTGAGAACCCCGCCCATCCGTGCTATGACATGGAGGCACTGAAGCCCGAGGAAGGTGTGTGGAGATTCTAATCCCAACAAGCAAGGGTCTCCTTCAAGATTAATGCTATCAATCATTAAGGTCATTACTCTCAACCACCTAGGCAATGAAGAATATACCATTTCAAATATTTACAGTACTTGTCTTCACCAACACTGTCCCAAGGTGAAATGAAGCAACAGAGAGGAAATTGTACATAAGTACCTCAGCATTTAATCCAAACAGGGGTTCTTAGTCTCAGCACTATGACATTTTGGGCTGACTACTTATTTGTTAGGCGGGAGCTCTCCTGTGCATTGTAGGATAATTAGCAGTATCCCTGGTGGCTACCCAATAGACGCCAGTAGCACCCCGAATTGACAACCCAAACTCTCCAGACATCACCAACTGTCCCCTGCGAGGAGAAATCACTCCTGGGGGAGAACCACTGACCCAAATGAATTCTAAACCAATCAAATGTCTGGGAAGCCCTCCAAGAAAAAAAATAGAAAAGCACTTGAAGAATATTCCCAATATTCCCGGTCAGCAGTATCAAGGCTGACTTGTGTTCATGTGGAGTCATTATAAATTCTATAAATCAATTATTCCCCTTCGGTCTTAAAAATATATTTCCTCATAAACATTTGAGTTTTGTTGAAAAGATGGAGTTTACAAAGATACCATTCTTGAGTCATGGATTTCTCTGCTCACAGAAGGGTGTGGCATTTGGAAACGGGAATAAACAAAATTGCTGCACCAATGCACTGAGTGAAGGAAGAGAGACAGAGGATCAAGGGCTTTAGACAGCACTCCTTCAATATGCAATCACAGAGAAAGATGCGCCTTATCCAAGTTAATATCTCTAAGGTGAGAGCCTTCTTAGAGTCAGTTTGTTGCAAATTTCACCTACTCTGTTCTTTTCCATCCATCCCCCTGAGTCAGTTGGTTGAAGGGAGTTATTTTTTCAAGTGGAATTCAAACAAAGCTCAAACCAGAACTGTAAATAGTGATTGCAGGAATTCTTTTCTAAACTGCTTTGCCCTTTCCTCTCACTGCCTTTTATAGCCAATATAAATGTCTCTTTGCACACCTTTTGTTGTGGTTTTATATTGTAACACCATTTTTCTTTGAAACTATTGTATTTAAAGTAAGGTTTCATATTATGTCAGCAAGTAATTAACTTATGTTTAAAAGGTGGCCATATCATGTACCAAAAGTTGCTGAAGTTTCTCTTCTAGCTGGTAAAGTAGGAGTTTGCATGACTTCACACTTTTTTTGCGTAGTTTCTTCTGTTGTATGATGGCGTGAGTGTGTGTCTTGGGTACCGCTGTGTACTACTGTGTGCCTAGATTCCATGCACTCTCGTTGTGTTTGAAGTAAATATTGGAGACCGGAGGGTAACAGGTTGGCCTGTTGATTACAGCTAGTAATCGCTGTGTCTTGTTCCGCCCCCTCCCTGACACCCCAGCTTCCCAGGATGTGGAAAGCCTGGATCTCAGCTCCTTGCCCCATATCCCTTCTGTAATTTGTACCTAAAGAGTGTGATTATCCTAATTCAAGAGTCACTAAAACTCATCACATTATCATTGCATATCAGCAAAGGGTAAAGTCCTAGCACCAATTGCTTCACATACCAGCATGTTCCATTTCCAATTTAGAATTAGCCACATAATAAAATCTTAGAATCTTCCTTGAGAAAGAGCTGCCTGAGATGTAGTTTTGTTATATGGTTCCCCACCGACCATTTTTGTGCTTTTTTCTTGTTTTGTTTTGTTTTGACTGCACTGTGAGTTTTGTAGTGTCCTCTTCTTGCCAAAACAAACGCGAGATGAACTGGACTTATGTAGACAAATCGTGATGCCAGTGTATCCTTCCTTTCTTCAGTTCCAGCAATAATGAATGGTCAACTTTTTTAAAATCTAGATCTCTCTCATTCATTTCAATGTATTTTTACTTTAAGATGAACCAAAATTATTAGACTTATTTAAGATGTACAGGCATCAGAAAAAAGAAGCACATAATGCTTTTGGTGCGATGGCACTCACTGTGAACATGTGTAACCACATATTAATATGCAATATTGTTTCCAATACTTTCTAATACAGTTTTTTATAATGTTGTGTGTGGTGATTGTTCAGGTCGAATCTGTTGTATCCAGTACAGCTTTAGGTCTTCAGCTGCCCTTCTGGCGAGTACATGCACAGGATTGTAAATGAGAAATGCAGTCATATTTCCAGTCTGCCTCTATGATGATGTTAAATTATTGCTGTTTAGCTGTGAACAAGGGATGTACCACTGGAGGAATAGAGTATCCTTTTGTACACATTTTGAAATGCTTCTTCTGTAGTGATAGAACAAATAAATGCAACGAATACTCTGTCTGCCCTATCCCGTGAAGTCCACACTGGCGTAAGAGAAGGCCCAGCAGAGCAGGAATCTGCCTAGACTTTCTCCCAATGAGATCCCAATATGAGAGGGAGAAGAGATGGGCCTCAGGACAGCTGCAATACCACTTGGGAACACATGTGGTGTCTTGATGTGGCCAGCGCAGCAGTTCAGCACAACGTACCTCCCATCTACAACAGTGCTGGACGTGGGAATTCTAAGTCCCAGTCTTGAGGGTGGGTGGAGATGGAGGGCAACAAGAGATACATTTCCAGTTCTCCACTGCAGCATGCTTCAGTCATTCTGTGAGTGGCCGGGCCCAGGGCCCTCACAATTTCACTACCTTGTCTTTTACATAGTCATAAGAATTATCCTCAACATAGCCTTTTGACGCTGTAAATCTTGAGTATTCATTTACCCTTTTCTGATCTCCTGGAAACAGCTGCCTGCCTGCATTGCACTTCTCTTCCCGAGGAGTGGGGTAAATTTAAAAGTCAAGTTATAGTTTGGATGTTAGTATAGAATTTTGAAATTGGGAATTAAAAATCAGGACTGGGGACTGGGAGACCAAAAATTTCTGATCCCATTTCTGATGGATGTGTCACACCTTTTCTGTCAAAATAAAATGTCTTGGAGGTTATGACTCCTTGGTGAAAAAAAAAAAAAAAAAA")
                };

                transcriptRegions = new ITranscriptRegion[]
                {
                    // insertion
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344378, 50344382, 13,  17),
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50344383, 50344518, 19,  154),
                    new TranscriptRegion(TranscriptRegionType.Intron, 1, 50344519, 50358643, 154, 155),
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50358644, 50358697, 155, 208),
                    new TranscriptRegion(TranscriptRegionType.Intron, 1, 50358698, 50367233, 208, 209),
                    new TranscriptRegion(TranscriptRegionType.Exon,   1, 50367234, 50367353, 209, 328),
                };
                
                startExonPhase = 0;

                var codingRegion = new CodingRegion(50358658, 50367353, 169, 1299, 1131);
                translation = new Translation(codingRegion, (CompactId) translation.ProteinId,
                    "MDADEGQDMSQVSGKESPPVSDTPDEGDEPMPIPEDLSTTSGGQQSSKSDRVVASNVKVETQSDEENGRACEMNGEECAEDLRMLDASGEKMNGSHRDQGSSALSGVGGIRLPNGKLKCDICGIICIGPNVLMVHKRSHTGDKGLSDTPYDSSASYEKENEMMKSHVMDQAINNAINYLGAESLRPLVQTPPGGSEVVPVISPMYQLHKPLAEGTPRSNHSAQDSAVENLLLLSKAKLVPSEREASPSNSCQDSTDTESNNEEQRSGLIYLTNHIAPHARNGLSLKEEHRAYDLLRAASENSQDALRVVSTSGEQMKVYKCEHCRVLFLDHVMYTIHMGCHGFRDPFECNMCGYHSQDRYEFSSHITRGEHRFHMS*");

                // TranscriptValidator.Validate(sequenceProvider, chromosome, "NM_001220771.1", gene.OnReverseStrand,
                //     transcriptRegions,
                //     rnaEdits, translation);

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