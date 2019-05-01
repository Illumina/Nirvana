using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
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
        private readonly StreamReader _headerReader;
        private readonly StreamReader _reader;
        private IRecomposer _recomposer;
        private readonly VariantFactory _variantFactory;
        private readonly IRefMinorProvider _refMinorProvider;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly IVcfFilter _vcfFilter;
        public bool IsRcrsMitochondrion { get; private set; }
        public string VcfLine { get; private set; }
        public GenomeAssembly InferredGenomeAssembly { get; private set; } = GenomeAssembly.Unknown;

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

        private VcfReader(StreamReader headerReader, StreamReader vcfLineReader, ISequenceProvider sequenceProvider,
            IRefMinorProvider refMinorProvider, IVcfFilter vcfFilter)
        {
            _headerReader = headerReader;
            _reader = vcfLineReader;
            _variantFactory = new VariantFactory(sequenceProvider);
            _refMinorProvider = refMinorProvider;
            _vcfFilter = vcfFilter;
            _refNameToChromosome = sequenceProvider.RefNameToChromosome;
        }

        public static VcfReader Create(StreamReader headerReader, StreamReader vcfLineReader, ISequenceProvider sequenceProvider,
            IRefMinorProvider refMinorProvider, IRecomposer recomposer, IVcfFilter vcfFilter)
        {
            var vcfReader = new VcfReader(headerReader, vcfLineReader, sequenceProvider, refMinorProvider, vcfFilter);
            vcfReader.ParseHeader();
            vcfReader.SetRecomposer(recomposer);
            return vcfReader;
        }

        private void SetRecomposer(IRecomposer recomposer) => _recomposer = _sampleNames == null ? new NullRecomposer() : recomposer;

        private void ParseHeader()
        {
            _headerLines = new List<string>();

            string line;
            while ((line = _headerReader.ReadLine()) != null)
            {
                if (IsNirvanaHeaderLine(line)) continue;
                CheckContigId(line);
                _headerLines.Add(line);
                if (line.StartsWith(VcfCommon.ChromosomeHeader)) break;
            }

            ValidateVcfHeader();
            _sampleNames = ExtractSampleNames(line);
            _vcfFilter.FastForward(_reader);
        }

        internal void CheckContigId(string line)
        {
            var chromAndLengthInfo = GetChromAndLengthInfo(line);
            if (chromAndLengthInfo.Length == 0) return;

            if (!_refNameToChromosome.TryGetValue(chromAndLengthInfo[0], out IChromosome chromosome)) return;
            if (!int.TryParse(chromAndLengthInfo[1], out int length)) return;

            var assemblyThisChrom = ContigInfo.GetGenomeAssembly(chromosome, length);

            if (assemblyThisChrom == GenomeAssembly.rCRS)
            {
                IsRcrsMitochondrion = true;
                return;
            }

            if (!GenomeAssemblyHelper.AutosomeAndAllosomeAssemblies.Contains(assemblyThisChrom)) return;

            if (InferredGenomeAssembly == GenomeAssembly.Unknown) InferredGenomeAssembly = assemblyThisChrom;

            if (InferredGenomeAssembly != assemblyThisChrom)
                throw new UserErrorException($"Inconsistent genome assemblies inferred:\ncurrent line \"{line}\" indicates {assemblyThisChrom}, whereas the lines above it indicate {InferredGenomeAssembly}.");
        }

        internal static string[] GetChromAndLengthInfo(string line)
        {
            if (!line.StartsWith("##contig=<ID=")) return Array.Empty<string>();
            if (!line.Contains(",length=")) return Array.Empty<string>();
            var chromAndLength = line.TrimEnd('>').Substring(13).Split(",length=");
            return chromAndLength.Length == 2 ? chromAndLength : Array.Empty<string>();
        }

        private void ValidateVcfHeader()
        {
            if (_headerLines.Count == 0 || !_headerLines[0].StartsWith("##fileformat=VCFv"))
                throw new UserErrorException("Please provide a valid VCF file with proper fileformat field.");

            if (!_headerLines[_headerLines.Count - 1].StartsWith(VcfCommon.ChromosomeHeader))
                throw new UserErrorException($"Could not find the vcf header line starting with {VcfCommon.ChromosomeHeader}. Is this a valid vcf file?");
        }

        private bool IsNirvanaHeaderLine(string s) => _nirvanaInfoTags.Any(s.StartsWith);

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
            return _queuedPositions.Count == 0 ? null : _queuedPositions.Dequeue();
        }

        public IPosition GetNextPosition() => Position.ToPosition(GetNextSimplePosition(), _refMinorProvider, _variantFactory);

        public void Dispose() => _reader?.Dispose();
    }
}
