using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.Compression;

namespace VariantAnnotation.FileHandling.VCF
{
    public sealed class LiteVcfReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;
        public bool IsGatkGenomeVcf { get; private set; }
		public bool IsRcrsMitochondrion { get; private set; }

        public string[] SampleNames;

        public List<string> HeaderLines { get; private set; }

        private readonly HashSet<string> _nirvanaInfoTags = new HashSet<string> { "##INFO=<ID=CSQT", "##INFO=<ID=CSQR", "##INFO=<ID=AF1000G", "##INFO=<ID=AA", "##INFO=<ID=GMAF", "##INFO=<ID=cosmic", "##INFO=<ID=clinvar", "##INFO=<ID=EVS", "##INFO=<ID=RefMinor", "##INFO=<ID=phyloP", "##annotatorDataVersion=", "##annotatorTranscriptSource=", "##annotator=", "##dataSource=dbSNP", "##dataSource=COSMIC", "##dataSource=1000 Genomes Project", "##dataSource=EVS", "##dataSource=ClinVar", "##dataSource=phyloP" };

        #endregion

        #region IDisposable

        private bool _isDisposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

	    public string ReadLine()
	    {
		    return _reader.ReadLine();
	    }

	    /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
	    private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    // Free any other managed objects here. 
                    _reader.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

		// constructor
        public LiteVcfReader(string vcfPath)
        {
            // sanity check
            if (!File.Exists(vcfPath))
            {
                throw new FileNotFoundException($"The specified vcf file ({vcfPath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(vcfPath);
            ParseHeader();
        }

        // constructor
        public LiteVcfReader(Stream stream)
        {
            // open the vcf file and parse the header
            _reader = new StreamReader(stream);
            ParseHeader();
        }

        private void ParseHeader()
        {
            string line;
            HeaderLines = new List<string>();

            while (true)
            {
                // grab the next line - stop if we have reached the main header or read the entire file
                line = _reader.ReadLine();
                if (line == null || line.StartsWith(VcfCommon.ChromosomeHeader)) break;

                // skip headers already produced by Nirvana
                var duplicateTag = _nirvanaInfoTags.Any(infoTag => line.StartsWith(infoTag));
                if (duplicateTag) continue;

                // check if this is a GATK genome vcf
                if (line.StartsWith(VcfCommon.GatkNonRefAltTag)) IsGatkGenomeVcf = true;

	            if (line.StartsWith("##contig=<ID") && line.Contains("M") && line.Contains("length=16569>")) IsRcrsMitochondrion = true;

                HeaderLines.Add(line);
            }

            // sanity check
            if (line == null || !line.StartsWith(VcfCommon.ChromosomeHeader))
            {
                throw new InvalidFileFormatException($"Could not find the vcf header (starts with {VcfCommon.ChromosomeHeader}). Is this a valid vcf file?");
            }

            HeaderLines.Add(line);

            // extract the sample name	
            SampleNames = ExtractSampleNames(line);
        }

        private static string[] ExtractSampleNames(string line)
	    {
		    var cols = line.Split('\t');
		    var hasSampleGenotypes = cols.Length >= VcfCommon.MinNumColumnsSampleGenotypes;

		    if (hasSampleGenotypes)
		    {
			    var samplesList = new List<string>();
			    for (var i = VcfCommon.GenotypeIndex; i < cols.Length; i++)
				    samplesList.Add(cols[i]);

			    return samplesList.ToArray();
		    }
		    return null;
	    }
    }
}