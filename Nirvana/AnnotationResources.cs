﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloud.Messages;
using CommandLine.Utilities;
using Genome;
using IO;
using MitoHeteroplasmy;
using RepeatExpansions;
using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Vcf.VariantCreator;

namespace Nirvana
{
    public sealed class AnnotationResources : IAnnotationResources
    {
        private Dictionary<Chromosome, List<int>> _variantPositions;

        public          ISequenceProvider             SequenceProvider             { get; }
        public          ITranscriptAnnotationProvider TranscriptAnnotationProvider { get; }
        private         ProteinConservationProvider   ProteinConservationProvider  { get; }
        public          IAnnotationProvider           SaProvider                   { get; }
        public          IAnnotationProvider           GsaProvider                  { get; }
        public          IAnnotationProvider           ConservationProvider         { get; }
        public          IRefMinorProvider             RefMinorProvider             { get; }
        public          IAnnotationProvider           LcrProvider                  { get; }
        public          IGeneAnnotationProvider       GeneAnnotationProvider       { get; }
        public          IMitoHeteroplasmyProvider     MitoHeteroplasmyProvider     { get; }
        public          IAnnotator                    Annotator                    { get; }
        public          IVariantIdCreator             VidCreator                   { get; }
        public          List<IDataSourceVersion>      DataSourceVersions           { get; }
        public          string                        VepDataVersion               { get; }
        public          long                          InputStartVirtualPosition    { get; set; }
        public          string                        AnnotatorVersionTag          { get; set; } = "Nirvana " + CommandLineUtilities.Version;
        public          bool                          ForceMitochondrialAnnotation { get; }
        public readonly PerformanceMetrics            Metrics;

        public AnnotationResources(string refSequencePath, string inputCachePrefix, List<string> saDirectoryPaths, List<SaUrls> customAnnotations,
            string customStrTsvPath, bool forceMitochondrialAnnotation, bool useLegacyVids, PerformanceMetrics metrics)
        {
            Metrics = metrics;
            PerformanceMetrics.ShowInitializationHeader();

            SequenceProvider = ProviderUtilities.GetSequenceProvider(refSequencePath);

            var annotationFiles = new AnnotationFiles();
            saDirectoryPaths?.ForEach(x => annotationFiles.AddFiles(x));
            customAnnotations?.ForEach(x => annotationFiles.AddFiles(x));

            ProteinConservationProvider = ProviderUtilities.GetProteinConservationProvider(annotationFiles);
            ProteinConservationProvider?.Load();

            metrics.Cache.Start();
            TranscriptAnnotationProvider =
                ProviderUtilities.GetTranscriptAnnotationProvider(inputCachePrefix, SequenceProvider, ProteinConservationProvider);
            metrics.ShowCacheLoad();

            SaProvider             = ProviderUtilities.GetNsaProvider(annotationFiles);
            GsaProvider            = ProviderUtilities.GetGsaProvider(annotationFiles);
            ConservationProvider   = ProviderUtilities.GetConservationProvider(annotationFiles);
            LcrProvider            = ProviderUtilities.GetLcrProvider(annotationFiles);
            RefMinorProvider       = ProviderUtilities.GetRefMinorProvider(annotationFiles);
            GeneAnnotationProvider = ProviderUtilities.GetGeneAnnotationProvider(annotationFiles);

            IRepeatExpansionProvider repeatExpansionProvider = GetRepeatExpansionProvider(SequenceProvider.Assembly,
                SequenceProvider.RefNameToChromosome, SequenceProvider.RefIndexToChromosome.Count, customStrTsvPath);

            MitoHeteroplasmyProvider = MitoHeteroplasmyReader.GetProvider();

            Annotator = new Annotator(
                TranscriptAnnotationProvider,
                SequenceProvider,
                SaProvider,
                ConservationProvider,
                LcrProvider,
                GeneAnnotationProvider,
                repeatExpansionProvider,
                GsaProvider
            );

            if (useLegacyVids) VidCreator = new LegacyVariantId(SequenceProvider.RefNameToChromosome);
            else VidCreator               = new VariantId();

            DataSourceVersions = GetDataSourceVersions(
                    TranscriptAnnotationProvider,
                    SaProvider,
                    GsaProvider,
                    GeneAnnotationProvider,
                    ConservationProvider,
                    LcrProvider,
                    MitoHeteroplasmyProvider
                )
                .ToList();
            
            VepDataVersion = TranscriptAnnotationProvider.VepVersion + "." + CacheConstants.DataVersion + "." + SaCommon.DataVersion;

            ForceMitochondrialAnnotation = forceMitochondrialAnnotation;
        }

        private static IRepeatExpansionProvider GetRepeatExpansionProvider(GenomeAssembly genomeAssembly,
            Dictionary<string, Chromosome> refNameToChromosome, int numRefSeqs, string customStrTsvPath)
        {
            if (genomeAssembly != GenomeAssembly.GRCh37 && genomeAssembly != GenomeAssembly.GRCh38) return null;
            return new RepeatExpansionProvider(genomeAssembly, refNameToChromosome, numRefSeqs, customStrTsvPath);
        }

        private static IEnumerable<IDataSourceVersion> GetDataSourceVersions(params IProvider[] providers)
        {
            var dataSourceVersions = new List<IDataSourceVersion>();
            foreach (IProvider provider in providers)
                if (provider != null)
                    dataSourceVersions.AddRange(provider.DataSourceVersions);
            return dataSourceVersions.ToHashSet(new DataSourceVersionComparer());
        }

        public void SingleVariantPreLoad(IPosition position)
        {
            var chromToPositions = new Dictionary<Chromosome, List<int>>();
            PreLoadUtilities.TryAddPosition(chromToPositions, position.Chromosome, position.Start, position.RefAllele,
                position.VcfFields[VcfCommon.AltIndex], SequenceProvider.Sequence);
            _variantPositions = chromToPositions;
            PreLoad(position.Chromosome);
        }

        public void GetVariantPositions(Stream vcfStream, GenomicRange genomicRange)
        {
            if (genomicRange != null)
                vcfStream.Position = Tabix.VirtualPosition.From(InputStartVirtualPosition).BlockOffset;
            int numPositions;

            Metrics.SaPositionScan.Start();
            (_variantPositions, numPositions) = PreLoadUtilities.GetPositions(vcfStream, genomicRange, SequenceProvider, RefMinorProvider);
            Metrics.ShowSaPositionScanLoad(numPositions);
        }

        public void PreLoad(Chromosome chromosome)
        {
            SequenceProvider.LoadChromosome(chromosome);

            if (_variantPositions == null || !_variantPositions.TryGetValue(chromosome, out List<int> positions)) return;
            SaProvider?.PreLoad(chromosome, positions);
        }

        public void Dispose()
        {
            SequenceProvider?.Dispose();
            TranscriptAnnotationProvider?.Dispose();
            SaProvider?.Dispose();
            GsaProvider?.Dispose();
            ConservationProvider?.Dispose();
            RefMinorProvider?.Dispose();
            GeneAnnotationProvider?.Dispose();
        }
    }
}