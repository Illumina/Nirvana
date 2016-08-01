using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling
{
    public sealed class LiteVcfReader : IDisposable
    {
        #region members

        private StreamReader _reader;
        private HashSet<string> _nirvanaHeaderEntries;

        public bool IsGatkGenomeVcf { get; private set; }
		public bool IsRcrsMitochondrion { get; private set; }
        public string[] SampleNames { get; private set; }
        public List<string> HeaderLines { get; private set; }

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
            }

            _reader = null;
            SampleNames = null;
            HeaderLines = null;
            _nirvanaHeaderEntries = null;
        }

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
	    //private void Dispose(bool disposing)
     //   {
     //       lock (this)
     //       {
     //           if (_isDisposed) return;

     //           if (disposing)
     //           {
     //               // Free any other managed objects here. 
     //               _reader.Dispose();
     //           }

     //           // Free any unmanaged objects here. 
     //           _isDisposed = true;
     //       }
     //   }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public LiteVcfReader(string vcfPath)
	    {
	        _nirvanaHeaderEntries = new HashSet<string>
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
            // sanity check
            if (!File.Exists(vcfPath))
            {
                throw new FileNotFoundException($"The specified vcf file ({vcfPath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(vcfPath);
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
                if ((line == null) || line.StartsWith(VcfCommon.ChromosomeHeader)) break;

                // skip headers already produced by Nirvana
                bool duplicateTag = _nirvanaHeaderEntries.Any(infoTag => line.StartsWith(infoTag));
                if (duplicateTag) continue;

                // check if this is a GATK genome vcf
                if (line.StartsWith(VcfCommon.GatkNonRefAltTag)) IsGatkGenomeVcf = true;

	            if (line.StartsWith("##contig=<ID") && line.Contains("M") && line.Contains("length=16569>")) IsRcrsMitochondrion = true;

                HeaderLines.Add(line);
            }

            // sanity check
            if ((line == null) || !line.StartsWith(VcfCommon.ChromosomeHeader))
            {
                throw new InvalidFileFormatException($"Could not find the vcf header (starts with {VcfCommon.ChromosomeHeader}). Is this a valid vcf file?");
            }

            HeaderLines.Add(line);

            // extract the sample name	
            SampleNames = GetSampleNames(line);
        }

        /// <summary>
        /// returns an array of the sample names present in this VCF file
        /// </summary>
        private static string[] GetSampleNames(string line)
	    {
		    var cols = line.Split('\t');
		    bool hasSampleGenotypes = cols.Length >= VcfCommon.MinNumColumnsSampleGenotypes;
            if (!hasSampleGenotypes) return null;

            var samplesList = new List<string>();
            for (int i = VcfCommon.GenotypeIndex; i < cols.Length; i++) samplesList.Add(cols[i]);
            return samplesList.ToArray();
	    }
    }
}