using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using Vcf.VariantCreator;

namespace Vcf
{
    public sealed class VcfReader : IVcfReader
    {
        #region members

        private readonly StreamReader _reader;
	    private readonly VariantFactory _variantFactory;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;

        private bool _isGatkGenomeVcf;

	    public bool IsRcrsMitochondrion { get; private set; }

	    private string[] _sampleNames;

        private List<string> _headerLines;

	    private const string CopyNumberTag = "CN";

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

        #endregion

        public string[] GetSampleNames() => _sampleNames;

        public VcfReader(Stream stream, IDictionary<string, IChromosome> refNameToChromosome,
            IRefMinorProvider refMinorProvider,bool enableVerboseTranscript)
        {
            _reader              = new StreamReader(stream);
			_variantFactory      = new VariantFactory(refNameToChromosome, refMinorProvider,enableVerboseTranscript);
            _refNameToChromosome = refNameToChromosome;

            ParseHeader();
        }

        private void ParseHeader()
        {
            string line;
            _headerLines = new List<string>();

            while (true)
            {
                // grab the next line - stop if we have reached the main header or read the entire file
                line = _reader.ReadLine();
                if (line == null || line.StartsWith(VcfCommon.ChromosomeHeader)) break;

                // skip headers already produced by Nirvana
                var duplicateTag = _nirvanaInfoTags.Any(infoTag => line.StartsWith(infoTag));
                if (duplicateTag) continue;

                // check if this is a GATK genome vcf
                if (line.StartsWith(VcfCommon.GatkNonRefAltTag)) _isGatkGenomeVcf = true;

                if (line.StartsWith("##contig=<ID") && line.Contains("M") && line.Contains("length=16569>")) IsRcrsMitochondrion = true;

                _headerLines.Add(line);
            }

            if (line == null || !line.StartsWith(VcfCommon.ChromosomeHeader))
            {
                throw new FormatException($"Could not find the vcf header (starts with {VcfCommon.ChromosomeHeader}). Is this a valid vcf file?");
            }

            _headerLines.Add(line);

            _sampleNames = ExtractSampleNames(line);
        }

        private static string[] ExtractSampleNames(string line)
        {
            var cols = line.Split('\t');
            var hasSampleGenotypes = cols.Length >= VcfCommon.MinNumColumnsSampleGenotypes;
            if (!hasSampleGenotypes) return null;

            var samplesList = new List<string>();
            for (var i = VcfCommon.GenotypeIndex; i < cols.Length; i++)
                samplesList.Add(cols[i]);

            return samplesList.ToArray();
        }

        public IEnumerable<string> GetHeaderLines() => _headerLines;

        public IPosition GetNextPosition()
        {
            VcfLine = _reader.ReadLine();
            return VcfLine == null ? null :VcfReaderUtils.ParseVcfLine(VcfLine,_variantFactory, _refNameToChromosome);
        }

        public string VcfLine { get; private set; }

       

        public void Dispose() => _reader?.Dispose();
    }
}
