using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine.Utilities;
using Compression.FileHandling;
using Genome;
using IO;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Vcf;

namespace Nirvana
{
    public sealed class AnnotationResources : IAnnotationResources
    {
        private ImmutableDictionary<IChromosome, List<int>> _variantPositions;
        public ISequenceProvider SequenceProvider { get; }
        public ITranscriptAnnotationProvider TranscriptAnnotationProvider { get; }
        public IAnnotationProvider SaProvider { get; }
        public IAnnotationProvider ConservationProvider { get; }
        public IRefMinorProvider RefMinorProvider { get; }
        public IGeneAnnotationProvider GeneAnnotationProvider { get; }
        public IPlugin[] Plugins { get; }
        public IAnnotator Annotator { get; }
        public IRecomposer Recomposer { get; }
        public List<IDataSourceVersion> DataSourceVersions { get; }
        public string VepDataVersion { get; }
        public long InputStartVirtualPosition { get; set; }
        public string AnnotatorVersionTag { get; } = "Nirvana " + CommandLineUtilities.Version;
        public bool OutputVcf { get; }
        public bool OutputGvcf { get; }

        public bool ReportAllSvOverlappingTranscripts { get; }
        public bool ForceMitochondrialAnnotation { get; }

        public AnnotationResources(string refSequencePath, string inputCachePrefix, string saManifestUrl, string pluginDirectory, bool outputVcf, bool outputGvcf, bool disableRecomposition, bool reportAllSvOverlappingTranscripts, bool forceMitochondrialAnnotation)

        {
            SequenceProvider = ProviderUtilities.GetSequenceProvider(refSequencePath);
            //read VCF to get positions for all variants
            //_variantPositions = vcfStream == null ? null : PreLoadUtilities.GetPositions(vcfStream, SequenceProvider.RefNameToChromosome);
            //preload annotation providers
            var streamSourceCollections = StreamSourceUtils.GetStreamSourceCollection(saManifestUrl);
            var annotationStreamSourceCollections = streamSourceCollections==null? null: new[]{streamSourceCollections};

            TranscriptAnnotationProvider         = ProviderUtilities.GetTranscriptAnnotationProvider(inputCachePrefix, SequenceProvider);
            SaProvider                           = ProviderUtilities.GetNsaProvider(annotationStreamSourceCollections);
            ConservationProvider                 = ProviderUtilities.GetConservationProvider(annotationStreamSourceCollections);
            RefMinorProvider                     = ProviderUtilities.GetRefMinorProvider(annotationStreamSourceCollections);
            GeneAnnotationProvider               = ProviderUtilities.GetGeneAnnotationProvider(annotationStreamSourceCollections);
            Plugins                              = PluginUtilities.LoadPlugins(pluginDirectory);

            Annotator = ProviderUtilities.GetAnnotator(TranscriptAnnotationProvider, SequenceProvider, SaProvider,
                ConservationProvider, GeneAnnotationProvider, Plugins);
            Recomposer = disableRecomposition
                ? new NullRecomposer()
                : Phantom.Recomposer.Recomposer.Create(SequenceProvider, inputCachePrefix);
            DataSourceVersions = GetDataSourceVersions(Plugins, TranscriptAnnotationProvider, SaProvider,
                GeneAnnotationProvider, ConservationProvider).ToList();
            VepDataVersion = TranscriptAnnotationProvider.VepVersion + "." + CacheConstants.DataVersion + "." +
                             SaCommon.DataVersion;

            OutputVcf = outputVcf;
            OutputGvcf = outputGvcf;
            ReportAllSvOverlappingTranscripts = reportAllSvOverlappingTranscripts;
            ForceMitochondrialAnnotation = forceMitochondrialAnnotation;
        }


        private static IEnumerable<IDataSourceVersion> GetDataSourceVersions(IEnumerable<IPlugin> plugins,
            params IProvider[] providers)
        {
            var dataSourceVersions = new List<IDataSourceVersion>();
            if (plugins != null) foreach (var provider in plugins) if (provider.DataSourceVersions != null) dataSourceVersions.AddRange(provider.DataSourceVersions);
            foreach (var provider in providers) if (provider != null) dataSourceVersions.AddRange(provider.DataSourceVersions);
            return dataSourceVersions.ToHashSet(new DataSourceVersionComparer());
        }

        public void GetVariantPositions(BlockGZipStream vcfStream, AnnotationRange annotationRange)
        {
            if (vcfStream == null)
            {
                _variantPositions = null;
                return;
            }

            vcfStream.Position = Tabix.VirtualPosition.From(InputStartVirtualPosition).BlockOffset;
            _variantPositions = PreLoadUtilities.GetPositions(vcfStream, annotationRange, SequenceProvider.RefNameToChromosome).ToImmutableDictionary();
        }

        public void PreLoad(IChromosome chromosome)
        {
            if (!_variantPositions.TryGetValue(chromosome, out var positions)) return;

            SaProvider?.PreLoad(chromosome, positions);
        }
    }
}