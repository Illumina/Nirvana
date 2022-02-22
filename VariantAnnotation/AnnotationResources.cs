using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloud;
using CommandLine.Utilities;
using Genome;
using IO;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Vcf;
using Versioning;

namespace VariantAnnotation
{
    public sealed class AnnotationResources
    {
        private Dictionary<Chromosome, List<int>> _variantPositions;
        public ReferenceSequenceProvider SequenceProvider { get; }
        private PsaProvider PsaProvider { get; }
        public TranscriptAnnotationProvider TranscriptAnnotationProvider { get; }
        public ISaAnnotationProvider SaProvider { get; }
        public IAnnotationProvider ConservationProvider { get; }
        public IRefMinorProvider RefMinorProvider { get; }
        public IGeneAnnotationProvider GeneAnnotationProvider { get; }
        public Annotator Annotator { get; }
        public IRecomposer Recomposer { get; }
        public List<IDataSourceVersion> DataSourceVersions { get; }
        public string DataVersion { get; }
        public long InputStartVirtualPosition { get; set; }
        public string AnnotatorVersionTag { get; set; } = "Nirvana " + CommandLineUtilities.Version;
        public bool OutputVcf { get; }
        public bool OutputGvcf { get; }
        public bool ForceMitochondrialAnnotation { get; }

        public AnnotationResources(string refSequencePath, string inputCacheDir, List<string> saDirectoryPaths,
            IS3Client s3Client, List<S3Path> annotationsInS3, bool outputVcf, bool outputGvcf,
            bool disableRecomposition, bool forceMitochondrialAnnotation)
        {
            SequenceProvider = ProviderUtilities.GetSequenceProvider(refSequencePath);
            
            var dataAndIndexPaths = new List<(string DataFile, string IndexFile)>();

            foreach (var saDirectoryPath in saDirectoryPaths)
            {
                var saPaths = ProviderUtilities.GetSaDataAndIndexPaths(saDirectoryPath);
                if (saPaths != null) dataAndIndexPaths.AddRange(saPaths);
            }

            var psaAndIndexPaths = new List<(string psaFile, string indexFile)>();
            foreach (var saDirectoryPath in saDirectoryPaths)
            {
                var psaPaths = ProviderUtilities.GetPsaPaths(saDirectoryPath);
                if (psaPaths !=null) psaAndIndexPaths.AddRange(psaPaths);
            }
            
            PsaProvider                  = ProviderUtilities.GetPsaProvider(psaAndIndexPaths);
            TranscriptAnnotationProvider = ProviderUtilities.GetTranscriptAnnotationProvider(inputCacheDir, SequenceProvider, PsaProvider);
            SaProvider                   = ProviderUtilities.GetNsaProvider(dataAndIndexPaths, s3Client, annotationsInS3);
            ConservationProvider         = ProviderUtilities.GetConservationProvider(dataAndIndexPaths);
            RefMinorProvider             = ProviderUtilities.GetRefMinorProvider(dataAndIndexPaths);
            GeneAnnotationProvider       = ProviderUtilities.GetGeneAnnotationProvider(dataAndIndexPaths);

            Annotator = ProviderUtilities.GetAnnotator(TranscriptAnnotationProvider, SequenceProvider, SaProvider,
                ConservationProvider, GeneAnnotationProvider);

            // N.B. Phantom has been disabled for VEP Independence
            Recomposer = new NullRecomposer();
            DataSourceVersions = GetDataSourceVersions(TranscriptAnnotationProvider, SaProvider,
                GeneAnnotationProvider, ConservationProvider).ToList();
            DataVersion = SaCommon.DataVersion.ToString();

            OutputVcf                    = outputVcf;
            OutputGvcf                   = outputGvcf;
            ForceMitochondrialAnnotation = forceMitochondrialAnnotation;
        }

        private static IEnumerable<IDataSourceVersion> GetDataSourceVersions(params IProvider[] providers)
        {
            var dataSourceVersions = new List<IDataSourceVersion>();
            foreach (var provider in providers) if (provider != null) dataSourceVersions.AddRange(provider.DataSourceVersions);
            return dataSourceVersions.ToHashSet();
        }

        public void GetVariantPositions(Stream vcfStream, AnnotationRange annotationRange)
        {
            if (vcfStream == null)
            {
                _variantPositions = null;
                return;
            }

            vcfStream.Position = Tabix.VirtualPosition.From(InputStartVirtualPosition).BlockOffset;
            _variantPositions = PreLoadUtilities.GetPositions(vcfStream, annotationRange, SequenceProvider);
        }

        public void PreLoad(Chromosome chromosome)
        {
            SequenceProvider.LoadChromosome(chromosome);

            if (_variantPositions == null || !_variantPositions.TryGetValue(chromosome, out var positions)) return;
            SaProvider?.PreLoad(chromosome, positions);
        }
    }
}