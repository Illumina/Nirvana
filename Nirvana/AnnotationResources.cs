using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Cloud;
using CommandLine.Utilities;
using Genome;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Positions;
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
        public string AnnotatorVersionTag { get; set; } = "Nirvana " + CommandLineUtilities.Version;
        public bool ForceMitochondrialAnnotation { get; }

        public AnnotationResources(string refSequencePath, string inputCachePrefix, List<string> saDirectoryPaths, List<SaUrls> customAnnotations, string pluginDirectory, bool disableRecomposition, bool forceMitochondrialAnnotation)
        {
            SequenceProvider = ProviderUtilities.GetSequenceProvider(refSequencePath);
            
            var annotationFiles = new AnnotationFiles();
            saDirectoryPaths?.ForEach(x => annotationFiles.AddFiles(x));
            customAnnotations?.ForEach(x => annotationFiles.AddFiles(x));

            TranscriptAnnotationProvider = ProviderUtilities.GetTranscriptAnnotationProvider(inputCachePrefix, SequenceProvider);
            SaProvider                   = ProviderUtilities.GetNsaProvider(annotationFiles);
            ConservationProvider         = ProviderUtilities.GetConservationProvider(annotationFiles);
            RefMinorProvider             = ProviderUtilities.GetRefMinorProvider(annotationFiles);
            GeneAnnotationProvider       = ProviderUtilities.GetGeneAnnotationProvider(annotationFiles);
            Plugins                      = PluginUtilities.LoadPlugins(pluginDirectory);

            Annotator = ProviderUtilities.GetAnnotator(TranscriptAnnotationProvider, SequenceProvider, SaProvider,
                ConservationProvider, GeneAnnotationProvider, Plugins);

            Recomposer = disableRecomposition
                ? new NullRecomposer()
                : Phantom.Recomposer.Recomposer.Create(SequenceProvider, TranscriptAnnotationProvider);
            DataSourceVersions = GetDataSourceVersions(Plugins, TranscriptAnnotationProvider, SaProvider,
                GeneAnnotationProvider, ConservationProvider).ToList();
            VepDataVersion = TranscriptAnnotationProvider.VepVersion + "." + CacheConstants.DataVersion + "." +
                             SaCommon.DataVersion;

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

        public void SingleVariantPreLoad(IPosition position)
        {
            var chromToPositions = new Dictionary<IChromosome, List<int>>();
            PreLoadUtilities.UpdateChromToPositions(chromToPositions, position.Chromosome, position.Start, position.RefAllele, position.VcfFields[VcfCommon.AltIndex], SequenceProvider.Sequence);
            _variantPositions = chromToPositions.ToImmutableDictionary();
            PreLoad(position.Chromosome);
        }

        public void GetVariantPositions(Stream vcfStream, GenomicRange genomicRange)
        {
            if (vcfStream == null)
            {
                _variantPositions = null;
                return;
            }

            vcfStream.Position = Tabix.VirtualPosition.From(InputStartVirtualPosition).BlockOffset;
            _variantPositions = PreLoadUtilities.GetPositions(vcfStream, genomicRange, SequenceProvider).ToImmutableDictionary();
        }

        public void PreLoad(IChromosome chromosome)
        {
            SequenceProvider.LoadChromosome(chromosome);

            if (_variantPositions == null || !_variantPositions.TryGetValue(chromosome, out var positions)) return;
            SaProvider?.PreLoad(chromosome, positions);
        }

        public void Dispose()
        {
            SequenceProvider?.Dispose();
            TranscriptAnnotationProvider?.Dispose();
            SaProvider?.Dispose();
            ConservationProvider?.Dispose();
            RefMinorProvider?.Dispose();
            GeneAnnotationProvider?.Dispose();
        }
    }
}