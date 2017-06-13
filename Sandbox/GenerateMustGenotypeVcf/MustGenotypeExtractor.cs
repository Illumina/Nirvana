using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace GenerateMustGenotypeVcf
{
	public sealed class MustGenotypeExtractor:IDisposable
	{
		private readonly StreamReader _oneKGenomeReader;
		private readonly StreamReader _clinvarReader;
		private readonly StreamReader _cosmicReader;
		private readonly GenomeAssembly _assembly;

	    private readonly DataFileManager _dataFileManager;
	    private readonly ICompressedSequence _compressedSequence;

		private int _refMinorCount;
		private int _clinvarCount;
		private int _cosmicCount;

		private const int CosmicMinCount = 5;
		private const double RefMinorFreq = 0.95;
		private const int SmallVariantMaxLength = 50;
		private const string RefMinorFileName = "RefMinorAllelev5.preprocess.vcf.gz";
		private const string IsisClinicalIndelFileName = "IsasClinicalIndelsv4.preprocess.vcf.gz";
		private const string OncogenicFileName = "OncogenicSitesv3.preprocess.vcf.gz";

		private readonly List<string> _grch37Contigs = new List<string>()
		{
			"##contig=<ID=1,assembly=b37,length=249250621>",
			"##contig=<ID=2,assembly=b37,length=243199373>",
			"##contig=<ID=3,assembly=b37,length=198022430>",
			"##contig=<ID=4,assembly=b37,length=191154276>",
			"##contig=<ID=5,assembly=b37,length=180915260>",
			"##contig=<ID=6,assembly=b37,length=171115067>",
			"##contig=<ID=7,assembly=b37,length=159138663>",
			"##contig=<ID=8,assembly=b37,length=146364022>",
			"##contig=<ID=9,assembly=b37,length=141213431>",
			"##contig=<ID=10,assembly=b37,length=135534747>",
			"##contig=<ID=11,assembly=b37,length=135006516>",
			"##contig=<ID=12,assembly=b37,length=133851895>",
			"##contig=<ID=13,assembly=b37,length=115169878>",
			"##contig=<ID=14,assembly=b37,length=107349540>",
			"##contig=<ID=15,assembly=b37,length=102531392>",
			"##contig=<ID=16,assembly=b37,length=90354753>",
			"##contig=<ID=17,assembly=b37,length=81195210>",
			"##contig=<ID=18,assembly=b37,length=78077248>",
			"##contig=<ID=19,assembly=b37,length=59128983>",
			"##contig=<ID=20,assembly=b37,length=63025520>",
			"##contig=<ID=21,assembly=b37,length=48129895>",
			"##contig=<ID=22,assembly=b37,length=51304566>",
			"##contig=<ID=MT,assembly=b37,length=16569>",
			"##contig=<ID=X,assembly=b37,length=155270560>",
			"##contig=<ID=Y,assembly=b37,length=59373566>",

		};

		private readonly List<string> _hg19Contigs = new List<string>()
		{
			"##contig=<ID=chr1,assembly=b37,length=249250621>",
			"##contig=<ID=chr2,assembly=b37,length=243199373>",
			"##contig=<ID=chr3,assembly=b37,length=198022430>",
			"##contig=<ID=chr4,assembly=b37,length=191154276>",
			"##contig=<ID=chr5,assembly=b37,length=180915260>",
			"##contig=<ID=chr6,assembly=b37,length=171115067>",
			"##contig=<ID=chr7,assembly=b37,length=159138663>",
			"##contig=<ID=chr8,assembly=b37,length=146364022>",
			"##contig=<ID=chr9,assembly=b37,length=141213431>",
			"##contig=<ID=chr10,assembly=b37,length=135534747>",
			"##contig=<ID=chr11,assembly=b37,length=135006516>",
			"##contig=<ID=chr12,assembly=b37,length=133851895>",
			"##contig=<ID=chr13,assembly=b37,length=115169878>",
			"##contig=<ID=chr14,assembly=b37,length=107349540>",
			"##contig=<ID=chr15,assembly=b37,length=102531392>",
			"##contig=<ID=chr16,assembly=b37,length=90354753>",
			"##contig=<ID=chr17,assembly=b37,length=81195210>",
			"##contig=<ID=chr18,assembly=b37,length=78077248>",
			"##contig=<ID=chr19,assembly=b37,length=59128983>",
			"##contig=<ID=chr20,assembly=b37,length=63025520>",
			"##contig=<ID=chr21,assembly=b37,length=48129895>",
			"##contig=<ID=chr22,assembly=b37,length=51304566>",
			"##contig=<ID=chrX,assembly=b37,length=155270560>",
			"##contig=<ID=chrY,assembly=b37,length=59373566>",

		};
		private readonly List<string> _grch38Contigs = new List<string>()
		{
			"##contig=<ID=chr1,assembly=GCF_000001405.26,length=248956422>",
			"##contig=<ID=chr2,assembly=GCF_000001405.26,length=242193529>",
			"##contig=<ID=chr3,assembly=GCF_000001405.26,length=198295559>",
			"##contig=<ID=chr4,assembly=GCF_000001405.26,length=190214555>",
			"##contig=<ID=chr5,assembly=GCF_000001405.26,length=181538259>",
			"##contig=<ID=chr6,assembly=GCF_000001405.26,length=170805979>",
			"##contig=<ID=chr7,assembly=GCF_000001405.26,length=159345973>",
			"##contig=<ID=chr8,assembly=GCF_000001405.26,length=145138636>",
			"##contig=<ID=chr9,assembly=GCF_000001405.26,length=138394717>",
			"##contig=<ID=chr10,assembly=GCF_000001405.26,length=133797422>",
			"##contig=<ID=chr11,assembly=GCF_000001405.26,length=135086622>",
			"##contig=<ID=chr12,assembly=GCF_000001405.26,length=133275309>",
			"##contig=<ID=chr13,assembly=GCF_000001405.26,length=114364328>",
			"##contig=<ID=chr14,assembly=GCF_000001405.26,length=107043718>",
			"##contig=<ID=chr15,assembly=GCF_000001405.26,length=90338345>",
			"##contig=<ID=chr16,assembly=GCF_000001405.26,length=83257441>",
			"##contig=<ID=chr17,assembly=GCF_000001405.26,length=83257441>",
			"##contig=<ID=chr18,assembly=GCF_000001405.26,length=80373285>",
			"##contig=<ID=chr19,assembly=GCF_000001405.26,length=58617616>",
			"##contig=<ID=chr20,assembly=GCF_000001405.26,length=64444167>",
			"##contig=<ID=chr21,assembly=GCF_000001405.26,length=46709983>",
			"##contig=<ID=chr22,assembly=GCF_000001405.26,length=50818468>",
			"##contig=<ID=chrM,assembly=GCF_000001405.26,length=16569>",
			"##contig=<ID=chrX,assembly=GCF_000001405.26,length=156040895>",
			"##contig=<ID=chrY,assembly=GCF_000001405.26,length=57227415>"

		};

		private readonly List<string> _refMinorGrch37HeaderLines = new List<string>()
		{
			"##fileformat=VCFv4.1",
			"##Description=RefMinor positions (ref allele frequency < 0.05) extracted from 1000 Genomes data",
			"##FILTER=<ID=PASS,Description=\"All filters passed\">",
			"##reference=ftp://ftp.1000genomes.ebi.ac.uk//vol1/ftp/technical/reference/phase2_reference_assembly_sequence/hs37d5.fa.gz",
			"##source=1000GenomesPhase3Pipeline",
			"##contig=<ID=1,assembly=b37,length=249250621>",
			"##contig=<ID=2,assembly=b37,length=243199373>",
			"##contig=<ID=3,assembly=b37,length=198022430>",
			"##contig=<ID=4,assembly=b37,length=191154276>",
			"##contig=<ID=5,assembly=b37,length=180915260>",
			"##contig=<ID=6,assembly=b37,length=171115067>",
			"##contig=<ID=7,assembly=b37,length=159138663>",
			"##contig=<ID=8,assembly=b37,length=146364022>",
			"##contig=<ID=9,assembly=b37,length=141213431>",
			"##contig=<ID=10,assembly=b37,length=135534747>",
			"##contig=<ID=11,assembly=b37,length=135006516>",
			"##contig=<ID=12,assembly=b37,length=133851895>",
			"##contig=<ID=13,assembly=b37,length=115169878>",
			"##contig=<ID=14,assembly=b37,length=107349540>",
			"##contig=<ID=15,assembly=b37,length=102531392>",
			"##contig=<ID=16,assembly=b37,length=90354753>",
			"##contig=<ID=17,assembly=b37,length=81195210>",
			"##contig=<ID=18,assembly=b37,length=78077248>",
			"##contig=<ID=19,assembly=b37,length=59128983>",
			"##contig=<ID=20,assembly=b37,length=63025520>",
			"##contig=<ID=21,assembly=b37,length=48129895>",
			"##contig=<ID=22,assembly=b37,length=51304566>",
			"##contig=<ID=MT,assembly=b37,length=16569>",
			"##contig=<ID=X,assembly=b37,length=155270560>",
			"##contig=<ID=Y,assembly=b37,length=59373566>",
			"#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO"
		};

		private readonly List<string> _refMinorHg19HeaderLines = new List<string>()
		{
			"##fileformat=VCFv4.1",
			"##Description=RefMinor positions (ref allele frequency < 0.05) extracted from 1000 Genomes data",
			"##FILTER=<ID=PASS,Description=\"All filters passed\">",
			"##reference=ftp://ftp.1000genomes.ebi.ac.uk//vol1/ftp/technical/reference/phase2_reference_assembly_sequence/hs37d5.fa.gz",
			"##source=1000GenomesPhase3Pipeline",
			"##contig=<ID=chr1,assembly=b37,length=249250621>",
			"##contig=<ID=chr2,assembly=b37,length=243199373>",
			"##contig=<ID=chr3,assembly=b37,length=198022430>",
			"##contig=<ID=chr4,assembly=b37,length=191154276>",
			"##contig=<ID=chr5,assembly=b37,length=180915260>",
			"##contig=<ID=chr6,assembly=b37,length=171115067>",
			"##contig=<ID=chr7,assembly=b37,length=159138663>",
			"##contig=<ID=chr8,assembly=b37,length=146364022>",
			"##contig=<ID=chr9,assembly=b37,length=141213431>",
			"##contig=<ID=chr10,assembly=b37,length=135534747>",
			"##contig=<ID=chr11,assembly=b37,length=135006516>",
			"##contig=<ID=chr12,assembly=b37,length=133851895>",
			"##contig=<ID=chr13,assembly=b37,length=115169878>",
			"##contig=<ID=chr14,assembly=b37,length=107349540>",
			"##contig=<ID=chr15,assembly=b37,length=102531392>",
			"##contig=<ID=chr16,assembly=b37,length=90354753>",
			"##contig=<ID=chr17,assembly=b37,length=81195210>",
			"##contig=<ID=chr18,assembly=b37,length=78077248>",
			"##contig=<ID=chr19,assembly=b37,length=59128983>",
			"##contig=<ID=chr20,assembly=b37,length=63025520>",
			"##contig=<ID=chr21,assembly=b37,length=48129895>",
			"##contig=<ID=chr22,assembly=b37,length=51304566>",
			"##contig=<ID=chrX,assembly=b37,length=155270560>",
			"##contig=<ID=chrY,assembly=b37,length=59373566>",
			"#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO"
		};


	    private readonly List<string> _refMinorGRCh38HeaderLines = new List<string>()
	    {
	        "##fileformat=VCFv4.1",
	        "##Description=RefMinor positions (ref allele frequency < 0.05) extracted from 1000 Genomes data",
	        "##FILTER=<ID=PASS,Description=\"All filters passed\">",
	        "##reference=ftp://ftp.1000genomes.ebi.ac.uk//vol1/ftp/technical/reference/phase2_reference_assembly_sequence/hs37d5.fa.gz",
	        "##source=1000GenomesPhase3Pipeline",
	        "##contig=<ID=chr1,assembly=GCF_000001405.26,length=248956422>",
	        "##contig=<ID=chr2,assembly=GCF_000001405.26,length=242193529>",
	        "##contig=<ID=chr3,assembly=GCF_000001405.26,length=198295559>",
	        "##contig=<ID=chr4,assembly=GCF_000001405.26,length=190214555>",
	        "##contig=<ID=chr5,assembly=GCF_000001405.26,length=181538259>",
	        "##contig=<ID=chr6,assembly=GCF_000001405.26,length=170805979>",
	        "##contig=<ID=chr7,assembly=GCF_000001405.26,length=159345973>",
	        "##contig=<ID=chr8,assembly=GCF_000001405.26,length=145138636>",
	        "##contig=<ID=chr9,assembly=GCF_000001405.26,length=138394717>",
	        "##contig=<ID=chr10,assembly=GCF_000001405.26,length=133797422>",
	        "##contig=<ID=chr11,assembly=GCF_000001405.26,length=135086622>",
	        "##contig=<ID=chr12,assembly=GCF_000001405.26,length=133275309>",
	        "##contig=<ID=chr13,assembly=GCF_000001405.26,length=114364328>",
	        "##contig=<ID=chr14,assembly=GCF_000001405.26,length=107043718>",
	        "##contig=<ID=chr15,assembly=GCF_000001405.26,length=90338345>",
	        "##contig=<ID=chr16,assembly=GCF_000001405.26,length=83257441>",
	        "##contig=<ID=chr17,assembly=GCF_000001405.26,length=83257441>",
	        "##contig=<ID=chr18,assembly=GCF_000001405.26,length=80373285>",
	        "##contig=<ID=chr19,assembly=GCF_000001405.26,length=58617616>",
	        "##contig=<ID=chr20,assembly=GCF_000001405.26,length=64444167>",
	        "##contig=<ID=chr21,assembly=GCF_000001405.26,length=46709983>",
	        "##contig=<ID=chr22,assembly=GCF_000001405.26,length=50818468>",
	        "##contig=<ID=chrM,assembly=GCF_000001405.26,length=16569>",
	        "##contig=<ID=chrX,assembly=GCF_000001405.26,length=156040895>",
	        "##contig=<ID=chrY,assembly=GCF_000001405.26,length=57227415>",
            "#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO"
	    };

        public MustGenotypeExtractor(string compressedSeqPath, string oneKGenomeVcf,string clinvarVcf, string cosmicVcf, bool isHg19 = false)
		{
            _compressedSequence = new CompressedSequence();
            _dataFileManager = new DataFileManager(new CompressedSequenceReader(FileUtilities.GetReadStream(compressedSeqPath),_compressedSequence ),_compressedSequence);
		    _assembly = _compressedSequence.GenomeAssembly == GenomeAssembly.GRCh37 && isHg19? GenomeAssembly.hg19:_compressedSequence.GenomeAssembly;

			if (_assembly == GenomeAssembly.Unknown)
				throw new Exception("Genome assembly must be either GRCh37 or GRCh38");
            if(_compressedSequence.GenomeAssembly == GenomeAssembly.GRCh38 && isHg19)
                throw new Exception("reference sequence is GRCh38 while generating hg19 files");

			_oneKGenomeReader  = string.IsNullOrEmpty(oneKGenomeVcf)? null: GZipUtilities.GetAppropriateStreamReader(oneKGenomeVcf);
			_clinvarReader     = string.IsNullOrEmpty(clinvarVcf) ? null : GZipUtilities.GetAppropriateStreamReader(clinvarVcf);
			_cosmicReader      = string.IsNullOrEmpty(cosmicVcf) ? null : GZipUtilities.GetAppropriateStreamReader(cosmicVcf);
		}

		#region IDisposable

		private bool _isDisposed;
		
		/// <summary>
		/// public implementation of Dispose pattern callable by consumers. 
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
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
					Close();
				}

				// Free any unmanaged objects here. 
				_isDisposed = true;
			}
		}

		private void Close()
		{
			_oneKGenomeReader?.Dispose();
			_clinvarReader?.Dispose();
			_cosmicReader?.Dispose();
		}

		#endregion

		public void ExtractEntries()
		{
			ExtractFromClinVar();
			if (_clinvarCount > 0)
				Console.WriteLine($"{_clinvarCount} non-SNVs extracted from clinvar");

			ExtractFromCosmic();
			if (_cosmicCount > 0)
				Console.WriteLine($"{_cosmicCount} entries with count > {CosmicMinCount} extracted from Cosmic");

			ExtractFromOneKg();
			if (_refMinorCount > 0)
				Console.WriteLine($"{_refMinorCount} ref minor positions extracted from 1000 G");
				
			
		}

		private void ExtractFromCosmic()
		{
			if (_cosmicReader == null) return;

		    var needParseHeaderLine = true;
			using (var writer = GZipUtilities.GetStreamWriter(OncogenicFileName))
			{
				string line;
				
				while ((line = _cosmicReader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					//copy required header lines
					if (line.StartsWith("#") && needParseHeaderLine)
					{
						ProcessHeaderLine(writer, line);
						continue;
					}

                    if(line.StartsWith("#")) continue;

				    needParseHeaderLine = false;
                    var fields = line.Split('\t');

					if (IsLargeVariants(fields[VcfCommon.RefIndex], fields[VcfCommon.AltIndex]))
						continue;
					if (! HasMinCount(fields[VcfCommon.InfoIndex]))
						continue;

					_cosmicCount++;


					var chrName = GetChrName(fields[VcfCommon.ChromIndex]);

					//skip mito for hg19
					if (_assembly == GenomeAssembly.hg19 && (chrName == "chrM" || chrName == "MT"))
						continue;

				    var pos = Convert.ToInt32(fields[VcfCommon.PosIndex]);
				    var refAllele = fields[VcfCommon.RefIndex];

				    if (ValidateReference(chrName, pos, refAllele))
                        writer.Write(chrName + '\t' +
						pos + '\t' +
						fields[VcfCommon.IdIndex] + '\t' +
						refAllele + '\t' +
						fields[VcfCommon.AltIndex] + '\t' +
						".\t.\t.\n");
				}
			}
		}

		private void ProcessHeaderLine(StreamWriter writer, string line)
		{
		    if (line.StartsWith("##fileformat="))
		    {
		        writer.WriteLine("##fileformat=VCFv4.1");
		    }
			if (IsRequiredHeaderLine(line))
			{
				writer.Write(line + "\n");
				return;
			}

			//if we have seen the chrom header
			if (!line.StartsWith("#CHROM")) return;

			writer.Write($"##Description=COSMIC variants having count greater or equal to {CosmicMinCount}" + "\n");
			WriteContigLines(writer);
			writer.Write(line + "\n");
			
		}

		private void WriteContigLines(StreamWriter writer)
		{
			List<string> contigLines = null;
			if (_assembly == GenomeAssembly.GRCh37) contigLines = _grch37Contigs;
			if (_assembly == GenomeAssembly.GRCh38) contigLines = _grch38Contigs;
			if (_assembly == GenomeAssembly.hg19) contigLines = _hg19Contigs;

			if (contigLines == null) return;

			foreach (var contigLine in contigLines)
			{
				writer.Write(contigLine + "\n");
			}
		}

		private static bool IsRequiredHeaderLine(string line)
		{
			return line.StartsWith("##source=") ||
			       line.StartsWith("##reference=");
		}

		private static bool IsLargeVariants(string refAllele, string altAlleles)
		{
			foreach (var altAllele in altAlleles.Split(','))
			{
				var trimmedAlleles = BiDirectionalTrimmer.Trim(1, refAllele, altAllele);
				var trimmedRef = trimmedAlleles.Item2;
				var trimmedAlt = trimmedAlleles.Item3;

				if (trimmedRef.Length > SmallVariantMaxLength || trimmedAlt.Length > SmallVariantMaxLength) return true;
			}

			return false;

		}

		private static bool HasMinCount(string info)
		{
			var infoFields = info.Split(';');
			foreach (var infoField in infoFields)
			{
				if (!infoField.StartsWith("CNT=")) continue;

				var count = Convert.ToInt32(infoField.Substring(4));
				
				return count >= CosmicMinCount;
			}
			return false;
		}

		private void ExtractFromClinVar()
		{
			if (_clinvarReader == null) return;
			
			using (var writer = GZipUtilities.GetStreamWriter(IsisClinicalIndelFileName))
			{
				string line;
				while ((line = _clinvarReader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					
					//copy required header lines
					if (line.StartsWith("#"))
					{
						ProcessHeaderLine(writer, line);
						continue;
					}

					var fields = line.Split('\t');

					if (IsSnv(fields[VcfCommon.RefIndex], fields[VcfCommon.AltIndex]))
						continue;

					_clinvarCount++;
					var chrName = GetChrName(fields[VcfCommon.ChromIndex]);

                    //skip mito for hg19
                    if (_assembly == GenomeAssembly.hg19 && (chrName == "chrM" || chrName == "MT"))
						continue;
				    var pos = Convert.ToInt32(fields[VcfCommon.PosIndex]);
				    var refAllele = fields[VcfCommon.RefIndex];

                    if(ValidateReference(chrName,pos,refAllele))
                        writer.Write(chrName + '\t' +
								 pos + '\t' +
								 fields[VcfCommon.IdIndex] + '\t' +
								 refAllele + '\t' +
								 fields[VcfCommon.AltIndex] + '\t' +
								 ".\t.\t.\n");
				}
			}
			
		}

	    private bool ValidateReference(string chromosome, int pos, string refAllele)
	    {
	        var refIndex = _compressedSequence.Renamer.GetReferenceIndex(chromosome);
	        if (refIndex == ChromosomeRenamer.UnknownReferenceIndex) return false;
            _dataFileManager.LoadReference(refIndex, () => { });
	        return _compressedSequence.Substring(pos - 1, refAllele.Length) == refAllele;
	    }

	    private void ExtractFromOneKg()
		{
			if (_oneKGenomeReader == null) return;

			using (var writer = GZipUtilities.GetStreamWriter(RefMinorFileName))
			{
				List<string> headerLines = null;
				if (_assembly == GenomeAssembly.GRCh37) headerLines = _refMinorGrch37HeaderLines;
				if (_assembly == GenomeAssembly.hg19) headerLines = _refMinorHg19HeaderLines;
			    if (_assembly == GenomeAssembly.GRCh38) headerLines = _refMinorGRCh38HeaderLines;

                if (headerLines == null) 
					throw new Exception("Unknown assembly for RefMinor Extraction");

				foreach (var headerLine in headerLines)
					writer.Write(headerLine + "\n");

				string line;
				while ((line = _oneKGenomeReader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					// Skip comments.
					if (line.StartsWith("#"))continue;

					var fields = line.Split('\t');

					if (!IsRefMinorPosition(fields[VcfCommon.InfoIndex])) continue;


					_refMinorCount++;
					var chrName = GetChrName(fields[VcfCommon.ChromIndex]);

					//skip mito for hg19
					if (_assembly == GenomeAssembly.hg19 && (chrName == "chrM" || chrName == "MT"))
						continue;

				    var pos = Convert.ToInt32(fields[VcfCommon.PosIndex]);
				    var refAllele = fields[VcfCommon.RefIndex];

				    if (ValidateReference(chrName, pos, refAllele))
                        writer.Write(chrName + '\t' +
								 pos + '\t' +
								 fields[VcfCommon.IdIndex] + '\t' +
								 refAllele + '\t' +
								 fields[VcfCommon.AltIndex] + '\t' +
								 ".\t.\t.\n");
				}
			}
			
		}

		private string GetChrName(string chromosome)
		{
			var chrName = _assembly == GenomeAssembly.GRCh38 || _assembly == GenomeAssembly.hg19
				? "chr" + chromosome
				: chromosome;
			if (chrName == "chrMT")
				chrName = "chrM";
			return chrName;
		}

		private static bool IsSnv(string refAllele, string altAlleles)
		{
			if (!IsSnv(refAllele)) return false;

			return altAlleles.Split(',').All(IsSnv);
		}

		private static bool IsSnv(string allele)
		{
			if (allele.Length != 1) return false;

			allele = allele.ToUpper();

			if (allele == "A" || allele == "C" || allele == "G" || allele == "T") return true;

			return false;
		}

		private static bool IsRefMinorPosition(string info)
		{
			var infoFields = info.Split(';');
			foreach (var infoField in infoFields)
			{
				if (! infoField.StartsWith("AF=")) continue;

				var totalAltAlleleFreq = 0.0;

				foreach (var freq in infoField.Substring(3).Split(','))
				{
					totalAltAlleleFreq+=Convert.ToDouble(freq);
				}
					
				return totalAltAlleleFreq >= RefMinorFreq;
			}
			return false;
		}

		
	}
}