using System.Collections.Generic;
using Genome;
using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.GeneFusions.HGVS;
using VariantAnnotation.GeneFusions.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.GeneFusions.Calling
{
    public sealed class GeneFusionCaller
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly IIntervalForest<ITranscript>     _transcriptIntervalForest;

        public GeneFusionCaller(IDictionary<string, IChromosome> refNameToChromosome, IIntervalForest<ITranscript> transcriptIntervalForest)
        {
            _refNameToChromosome      = refNameToChromosome;
            _transcriptIntervalForest = transcriptIntervalForest;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public void AddGeneFusions(IAnnotatedVariant[] annotatedVariants, bool isImprecise, bool isInv3, bool isInv5)
        {
            var transcriptIdToGeneFusions = new Dictionary<string, IAnnotatedGeneFusion[]>();

            foreach (IAnnotatedVariant annotatedVariant in annotatedVariants)
            {
                IVariant variant = annotatedVariant.Variant;
                if (!variant.IsStructuralVariant) continue;

                BreakEndAdjacency[] adjacencies = BreakEndAdjacencyFactory.CreateAdjacencies(variant, _refNameToChromosome, isInv3, isInv5);
                if (adjacencies == null) continue;

                transcriptIdToGeneFusions.Clear();

                foreach (BreakEndAdjacency adjacency in adjacencies)
                {
                    ITranscript[] originTranscripts  = GetOverlappingTranscripts(adjacency.Origin);
                    ITranscript[] partnerTranscripts = GetOverlappingTranscripts(adjacency.Partner);
                    if (originTranscripts == null || partnerTranscripts == null) continue;
                    AddGeneFusionsToDictionary(transcriptIdToGeneFusions, adjacency, originTranscripts, partnerTranscripts, isImprecise);
                }

                foreach (IAnnotatedTranscript transcript in annotatedVariant.Transcripts)
                {
                    string transcriptId = transcript.Transcript.Id.WithVersion;
                    if (!transcriptIdToGeneFusions.TryGetValue(transcriptId, out IAnnotatedGeneFusion[] annotatedGeneFusions)) continue;
                    transcript.AddGeneFusions(annotatedGeneFusions);
                }
            }
        }

        private ITranscript[] GetOverlappingTranscripts(BreakPoint bp) =>
            bp == null ? null : _transcriptIntervalForest.GetAllOverlappingValues(bp.Chromosome.Index, bp.Position, bp.Position);

        internal static void AddGeneFusionsToDictionary(Dictionary<string, IAnnotatedGeneFusion[]> transcriptIdToGeneFusions,
            // ReSharper disable once ParameterTypeCanBeEnumerable.Global
            BreakEndAdjacency adjacency, ITranscript[] originTranscripts, ITranscript[] partnerTranscripts, bool isImprecise)
        {
            var geneKeys    = new HashSet<ulong>();
            var geneFusions = new List<IAnnotatedGeneFusion>();

            foreach (ITranscript originTranscript in originTranscripts)
            {
                geneFusions.Clear();
                (int originIndex, ITranscriptRegion _) =
                    MappedPositionUtilities.FindRegion(originTranscript.TranscriptRegions, adjacency.Origin.Position);

                foreach (ITranscript partnerTranscript in partnerTranscripts)
                {
                    EvaluateGeneFusionCandidate(geneFusions, geneKeys, adjacency, originTranscript, originIndex, partnerTranscript, isImprecise);
                }

                if (geneFusions.Count == 0) continue;
                transcriptIdToGeneFusions[originTranscript.Id.WithVersion] = geneFusions.ToArray();
            }
        }

        // ReSharper disable once UseDeconstructionOnParameter
        private static void EvaluateGeneFusionCandidate(List<IAnnotatedGeneFusion> geneFusions, HashSet<ulong> geneKeys, BreakEndAdjacency adjacency,
            ITranscript originTranscript, int originIndex, ITranscript partnerTranscript, bool isImprecise)
        {
            IGene originGene  = originTranscript.Gene;
            IGene partnerGene = partnerTranscript.Gene;

            if (!FoundViableGeneFusion(adjacency, originGene, originTranscript, originTranscript.Source, partnerGene, partnerTranscript,
                partnerTranscript.Source)) return;

            (int partnerIndex, ITranscriptRegion partnerRegion) =
                MappedPositionUtilities.FindRegion(partnerTranscript.TranscriptRegions, adjacency.Partner.Position);

            int? partnerExon   = partnerRegion.Type == TranscriptRegionType.Exon ? partnerRegion.Id : null;
            int? partnerIntron = partnerRegion.Type == TranscriptRegionType.Intron ? partnerRegion.Id : null;

            var origin  = new BreakPointTranscript(originTranscript,  adjacency.Origin.Position,  originIndex);
            var partner = new BreakPointTranscript(partnerTranscript, adjacency.Partner.Position, partnerIndex);

            bool originOnReverseStrand = originGene.OnReverseStrand ^ adjacency.Origin.OnReverseStrand;
            (BreakPointTranscript first, BreakPointTranscript second) = originOnReverseStrand ? (partner, origin) : (origin, partner);

            bool     isInFrame   = !isImprecise && DetermineInFrameFusion(first, second);
            string   hgvsr       = HgvsRnaNomenclature.GetHgvs(first, second);

            (ulong fusionKey, string firstGeneSymbol, uint firstGeneKey, string secondGeneSymbol, uint secondGeneKey) =
                GetGeneAndFusionKeys(originGene, partnerGene);

            geneFusions.Add(new AnnotatedGeneFusion(partnerTranscript, partnerExon, partnerIntron, hgvsr, isInFrame, fusionKey, firstGeneSymbol,
                firstGeneKey, secondGeneSymbol, secondGeneKey));
            geneKeys.Add(fusionKey);
        }

        internal static (ulong FusionKey, string FirstGeneSymbol, uint FirstGeneKey, string SecondGeneSymbol, uint SecondGeneKey)
            GetGeneAndFusionKeys(IGene originGene, IGene partnerGene)
        {
            (IGene firstGene, IGene secondGene) = SortGenes(originGene, partnerGene);

            string firstGeneId   = firstGene.EnsemblId.WithoutVersion;
            string secondGeneId  = secondGene.EnsemblId.WithoutVersion;
            uint   firstGeneKey  = GeneFusionKey.CreateGeneKey(firstGeneId);
            uint   secondGeneKey = GeneFusionKey.CreateGeneKey(secondGeneId);
            ulong  fusionKey     = GeneFusionKey.Create(firstGeneKey, secondGeneKey);
            return (fusionKey, firstGene.Symbol, firstGeneKey, secondGene.Symbol, secondGeneKey);
        }

        private static (IGene FirstGene, IGene SecondGene) SortGenes(IGene originGene, IGene partnerGene)
        {
            if (originGene.Chromosome.Index == partnerGene.Chromosome.Index)
            {
                return originGene.Start < partnerGene.Start
                    ? (originGene, partnerGene)
                    : (partnerGene, originGene);
            }

            return originGene.Chromosome.Index < partnerGene.Chromosome.Index
                ? (originGene, partnerGene)
                : (partnerGene, originGene);
        }

        // ReSharper disable UseDeconstructionOnParameter
        internal static bool DetermineInFrameFusion(BreakPointTranscript first, BreakPointTranscript second)
            // ReSharper restore UseDeconstructionOnParameter
        {
            ITranscriptRegion firstRegion  = first.Transcript.TranscriptRegions[first.RegionIndex];
            ITranscriptRegion secondRegion = second.Transcript.TranscriptRegions[second.RegionIndex];

            byte? firstCodonPosition = GetCodonPosition(firstRegion, first.Transcript.Translation, first.Transcript.StartExonPhase,
                first.Transcript.Gene.OnReverseStrand,               first.GenomicPosition);

            byte? secondCodonPosition = GetCodonPosition(secondRegion, second.Transcript.Translation, second.Transcript.StartExonPhase,
                second.Transcript.Gene.OnReverseStrand,                second.GenomicPosition);

            // nothing to do if we landed outside of the CDS or outside an exon
            if (firstCodonPosition == null || secondCodonPosition == null) return false;

            return firstCodonPosition == 1 && secondCodonPosition == 2 ||
                   firstCodonPosition == 2 && secondCodonPosition == 3 ||
                   firstCodonPosition == 3 && secondCodonPosition == 1;
        }

        internal static byte? GetCodonPosition(ITranscriptRegion region, ITranslation translation, byte startExonPhase, bool onReverseStrand,
            int genomicPosition)
        {
            if (translation == null || region.Type != TranscriptRegionType.Exon) return null;

            var variant = new Interval(genomicPosition, genomicPosition);
            (int cdnaPosition, int _) = MappedPositionUtilities.GetCdnaPositions(region, region, variant, onReverseStrand, false);

            (int cdsPosition, int _) =
                MappedPositionUtilities.GetCdsPositions(translation.CodingRegion, cdnaPosition, cdnaPosition, startExonPhase, false);
            if (cdsPosition == -1) return null;
            
            return (byte) ((cdsPosition - 1) % 3 + 1);
        }

        // ReSharper disable once UseDeconstructionOnParameter
        internal static bool FoundViableGeneFusion(BreakEndAdjacency adjacency, IGene originGene, IChromosomeInterval originInterval,
            Source originSource, IGene partnerGene, IChromosomeInterval partnerInterval, Source partnerSource)
        {
            bool originOnReverseStrand     = originGene.OnReverseStrand  ^ adjacency.Origin.OnReverseStrand;
            bool partnerOnReverseStrand    = partnerGene.OnReverseStrand ^ adjacency.Partner.OnReverseStrand;
            bool differentStrand           = originOnReverseStrand != partnerOnReverseStrand;
            bool differentTranscriptSource = originSource          != partnerSource;
            bool sameGeneSymbol            = originGene.Symbol     == partnerGene.Symbol;

            bool transcriptAlreadyOverlaps =
                originInterval.Chromosome.Index == partnerInterval.Chromosome.Index && originInterval.Overlaps(partnerInterval);

            return !differentStrand && !differentTranscriptSource && !sameGeneSymbol && !transcriptAlreadyOverlaps;
        }
    }
}