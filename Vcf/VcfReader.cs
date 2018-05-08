using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf.VariantCreator;

namespace Vcf
{
    public sealed class VcfReader : IVcfReader
    {
        private readonly StreamReader _reader;
        private readonly IRecomposer _recomposer;
        private readonly VariantFactory _variantFactory;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        public bool IsRcrsMitochondrion { get; private set; }
        private string[] _sampleNames;
        private List<string> _headerLines;
        public string VcfLine { get; private set; }
        private readonly Queue<ISimplePosition> _queuedPositions = new Queue<ISimplePosition>();

        private readonly HashSet<string> _nirvanaInfoTags = new HashSet<string>
        {
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

        public VcfReader(Stream stream, IDictionary<string, IChromosome> refNameToChromosome,
            IRefMinorProvider refMinorProvider, bool enableVerboseTranscript, IRecomposer recomposer)
        {
            _reader = FileUtilities.GetStreamReader(stream);
            _variantFactory = new VariantFactory(refNameToChromosome, refMinorProvider, enableVerboseTranscript);
            _refNameToChromosome = refNameToChromosome;
            bool hasSampleColumn = ParseHeader();
            _recomposer = hasSampleColumn ? recomposer : new NullRecomposer();
        }

        private bool ParseHeader()
        {
            string line;
            _headerLines = new List<string>();
            bool hasSampleColumn;

            while (true)
            {
                // grab the next line - stop if we have reached the main header or read the entire file
                line = _reader.ReadLine();
                if (line == null || line.StartsWith(VcfCommon.ChromosomeHeader))
                {
                    hasSampleColumn = HasSampleColumn(line);
                    break;
                }

                // skip headers already produced by Nirvana
                bool duplicateTag = _nirvanaInfoTags.Any(infoTag => line.StartsWith(infoTag));
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

            return hasSampleColumn;
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

            var samplesList = new List<string>();
            for (int i = VcfCommon.GenotypeIndex; i < cols.Length; i++)
                samplesList.Add(cols[i]);

            return samplesList.ToArray();
        }

        public IEnumerable<string> GetHeaderLines() => _headerLines;

        private ISimplePosition GetNextSimplePosition()
        {
            while (_queuedPositions.Count == 0)
            {
                VcfLine = _reader.ReadLine();
                var simplePositions = _recomposer.ProcessSimplePosition(SimplePosition.GetSimplePosition(VcfLine, _refNameToChromosome));
                foreach (var simplePosition in simplePositions)
                {
                    _queuedPositions.Enqueue(simplePosition);
                }
                if (VcfLine == null) break;
            }
            return _queuedPositions.Count == 0 ? null: _queuedPositions.Dequeue();
        }

        public IPosition GetNextPosition() => Position.ToPosition(GetNextSimplePosition(), _variantFactory);

        public void Dispose() => _reader?.Dispose();
    }
}
