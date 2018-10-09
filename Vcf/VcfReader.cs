using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf.VariantCreator;

namespace Vcf
{
    public sealed class VcfReader : IVcfReader
    {
        private readonly StreamReader _headerReader;
        private readonly StreamReader _reader;
        private readonly IRecomposer _recomposer;
        private readonly VariantFactory _variantFactory;
        private readonly IRefMinorProvider _refMinorProvider;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly IVcfFilter _vcfFilter;

        public bool IsRcrsMitochondrion { get; private set; }
        public string VcfLine { get; private set; }

        private string[] _sampleNames;
        private List<string> _headerLines;
        private readonly Queue<ISimplePosition> _queuedPositions = new Queue<ISimplePosition>();

        private readonly string[] _nirvanaInfoTags = {
            "##INFO=<ID=CSQT",
            "##INFO=<ID=CSQR",
            "##INFO=<ID=AF1000G",
            "##INFO=<ID=AA",
            "##INFO=<ID=GMAF",
            "##INFO=<ID=cosmic",
            "##INFO=<ID=clinvar",
            "##INFO=<ID=EVS",
            "##INFO=<ID=RefMinor",
            "##INFO=<ID=phyloP",
            "##annotatorDataVersion=",
            "##annotatorTranscriptSource=",
            "##annotator=",
            "##dataSource=dbSNP",
            "##dataSource=COSMIC",
            "##dataSource=1000 Genomes Project",
            "##dataSource=EVS",
            "##dataSource=ClinVar",
            "##dataSource=phyloP"
        };

        public string[] GetSampleNames() => _sampleNames;

        private VcfReader(StreamReader headerReader, StreamReader vcfReader, IDictionary<string, IChromosome> refNameToChromosome,
            IRefMinorProvider refMinorProvider, bool enableVerboseTranscript, IRecomposer recomposer, IVcfFilter vcfFilter)
        {
            _headerReader = headerReader;
            _reader = vcfReader;
            _variantFactory = new VariantFactory(refNameToChromosome, enableVerboseTranscript);
            _refMinorProvider = refMinorProvider;
            _vcfFilter = vcfFilter;
            _refNameToChromosome = refNameToChromosome;
            bool hasSampleColumn = ParseHeader();
            _recomposer = hasSampleColumn ? recomposer : new NullRecomposer();
        }

        private VcfReader(StreamReader headerReader, StreamReader vcfReader, IAnnotationResources annotationResources, IVcfFilter vcfFilter) : this (headerReader, vcfReader,
            annotationResources.SequenceProvider.RefNameToChromosome, annotationResources.RefMinorProvider,
            annotationResources.ReportAllSvOverlappingTranscripts, annotationResources.Recomposer, vcfFilter)
        { }

        public static VcfReader Create(Stream headerStream, Stream vcfStream,
            IDictionary<string, IChromosome> refNameToChromosome,
            IRefMinorProvider refMinorProvider, bool enableVerboseTranscript, IRecomposer recomposer,
            IVcfFilter vcfFilter) => new VcfReader(FileUtilities.GetStreamReader(headerStream),
            FileUtilities.GetStreamReader(vcfStream), refNameToChromosome,
            refMinorProvider, enableVerboseTranscript, recomposer, vcfFilter);

        public static VcfReader Create(Stream stream, IDictionary<string, IChromosome> refNameToChromosome,
            IRefMinorProvider refMinorProvider, bool enableVerboseTranscript, IRecomposer recomposer,
            IVcfFilter vcfFilter)
        {
            var reader = FileUtilities.GetStreamReader(stream);
            return new VcfReader(reader, reader, refNameToChromosome,
                refMinorProvider, enableVerboseTranscript, recomposer, vcfFilter);
        }

        private static VcfReader Create(Stream stream, IAnnotationResources annotationResources, IVcfFilter vcfFilter)
        {
            var reader = FileUtilities.GetStreamReader(stream);
            return new VcfReader(reader, reader, annotationResources, vcfFilter);
        }

        public static VcfReader Create(Stream headerStream, Stream vcfStream, IAnnotationResources annotationResources,
            IVcfFilter vcfFilter)
        {
            if (headerStream == null) return Create(vcfStream, annotationResources, vcfFilter);

            vcfStream.Position = Tabix.VirtualPosition.From(annotationResources.InputStartVirtualPosition).BlockOffset;
            return new VcfReader(FileUtilities.GetStreamReader(headerStream),
                    FileUtilities.GetStreamReader(vcfStream),
                    annotationResources.SequenceProvider.RefNameToChromosome, annotationResources.RefMinorProvider,
                    annotationResources.ReportAllSvOverlappingTranscripts, annotationResources.Recomposer, vcfFilter);
        }

        private bool ParseHeader()
        {
            string line;
            _headerLines = new List<string>();
            bool hasSampleColumn;

            while (true)
            {
                // grab the next line - stop if we have reached the main header or read the entire file
                line = _headerReader.ReadLine();
                if (line == null || line.StartsWith(VcfCommon.ChromosomeHeader))
                {
                    hasSampleColumn = HasSampleColumn(line);
                    break;
                }

                bool duplicateTag = FoundNirvanaHeaderLine(line);
                if (duplicateTag) continue;

                if (line.StartsWith("##contig=<ID") && line.Contains("M") && line.Contains("length=16569>")) IsRcrsMitochondrion = true;

                _headerLines.Add(line);
            }

            if (line == null || !line.StartsWith(VcfCommon.ChromosomeHeader))
            {
                throw new FormatException($"Could not find the vcf header (starts with {VcfCommon.ChromosomeHeader}). Is this a valid vcf file?");
            }

            _headerLines.Add(line);

            _sampleNames = ExtractSampleNames(line);

            _vcfFilter.FastForward(_reader);

            return hasSampleColumn;
        }

        private bool FoundNirvanaHeaderLine(string s)
        {
            foreach(string infoTag in _nirvanaInfoTags)
                if (s.StartsWith(infoTag))
                    return true;
            return false;
        }

        private static bool HasSampleColumn(string line)
        {
            var vcfHeaderFields = line?.Trim().OptimizedSplit('\t');
            return vcfHeaderFields?.Length >= VcfCommon.MinNumColumnsSampleGenotypes;
        }

        private static string[] ExtractSampleNames(string line)
        {
            var cols = line.OptimizedSplit('\t');
            bool hasSampleGenotypes = cols.Length >= VcfCommon.MinNumColumnsSampleGenotypes;
            if (!hasSampleGenotypes) return null;

            int numSamples = cols.Length - VcfCommon.GenotypeIndex;
            var samples = new string[numSamples];
            for (var i = 0; i < numSamples; i++) samples[i] = cols[VcfCommon.GenotypeIndex + i];
            return samples;
        }

        public IEnumerable<string> GetHeaderLines() => _headerLines;

        private ISimplePosition GetNextSimplePosition()
        {
            while (_queuedPositions.Count == 0)
            {
                VcfLine = _vcfFilter.GetNextLine(_reader);

                var simplePositions = _recomposer.ProcessSimplePosition(
                    SimplePosition.GetSimplePosition(VcfLine, _vcfFilter, _refNameToChromosome));
                foreach (var simplePosition in simplePositions)
                {
                    _queuedPositions.Enqueue(simplePosition);
                }
                if (VcfLine == null) break;
            }
            return _queuedPositions.Count == 0 ? null: _queuedPositions.Dequeue();
        }

        public IPosition GetNextPosition() => Position.ToPosition(GetNextSimplePosition(), _refMinorProvider, _variantFactory);

        public void Dispose() => _reader?.Dispose();
    }
}
