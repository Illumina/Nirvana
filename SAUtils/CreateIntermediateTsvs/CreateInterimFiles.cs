using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.ClinGen;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.InputFileParsers.Cosmic;
using SAUtils.InputFileParsers.CustomAnnotation;
using SAUtils.InputFileParsers.CustomInterval;
using SAUtils.InputFileParsers.DbSnp;
using SAUtils.InputFileParsers.DGV;
using SAUtils.InputFileParsers.EVS;
using SAUtils.InputFileParsers.ExAc;
using SAUtils.InputFileParsers.OneKGen;
using SAUtils.TsvWriters;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using IChromosomeRenamer = VariantAnnotation.Interface.IChromosomeRenamer;

namespace SAUtils.CreateIntermediateTsvs
{
	internal class CreateInterimFiles
	{
		#region fileNames
		private readonly List<string> _customAnnotationFiles;
		private readonly List<string> _customIntervalFiles;
		private readonly string _onekGFileName;
		private readonly string _onekGSvFileName;
		private readonly string _clinGenFileName;
		private readonly string _clinVarFileName;
		private readonly string _cosmicTsvFileName;
		private readonly string _cosmicVcfFileName;
		private readonly string _dbSnpFileName;
		private readonly string _dgvFile;
		private readonly string _evsFile;
		private readonly string _exacFile;
		private readonly string _outputDirectory;
		#endregion

		#region members
		private readonly IChromosomeRenamer _renamer;
		private readonly GenomeAssembly _genomeAssembly;
		private readonly ICompressedSequence _compressedSequence;
		private readonly CompressedSequenceReader _sequenceReader;
		private readonly string _compressedReferencePath;

		#endregion
		public CreateInterimFiles(string compressedReferencePath, string outputDirectory, string dbSnpFileName, string cosmicVcfFileName, string cosmicTsvFileName, string clinVarFileName, string onekGFileName, string evsFile, string exacFile, string dgvFile, string onekGSvFileName, string clinGenFileName, List<string> customAnnotationFiles, List<string> customIntervalFiles )
		{
			_outputDirectory         = outputDirectory;
			_dbSnpFileName           = dbSnpFileName;
			_cosmicVcfFileName       = cosmicVcfFileName;
			_cosmicTsvFileName       = cosmicTsvFileName;
			_clinVarFileName         = clinVarFileName;
			_onekGFileName           = onekGFileName;
			_evsFile                 = evsFile;
			_exacFile                = exacFile;
			_customAnnotationFiles   = customAnnotationFiles;
			_dgvFile                 = dgvFile;
			_onekGSvFileName         = onekGSvFileName;
			_clinGenFileName         = clinGenFileName;
			_customIntervalFiles = customIntervalFiles;

			_compressedSequence = new CompressedSequence();
			_compressedReferencePath = compressedReferencePath;
			_sequenceReader     = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), _compressedSequence);
			_renamer            = _compressedSequence.Renamer;
			_genomeAssembly     = _sequenceReader.Assembly;

		}

		public void CreateTsvs()
		{
			//CreateDbsnpGaTsv(_dbSnpFileName);
			//CreateOnekgTsv(_onekGFileName);
			//CreateClinvarTsv(_clinVarFileName);
			//CreateExacTsv(_exacFile);
			//CreateEvsTsv(_evsFile);
			//CreateCosmicTsv(_cosmicVcfFileName, _cosmicTsvFileName);
			//CreateSvTsv(InterimSaCommon.DgvTag, _dgvFile);
			//CreateSvTsv(InterimSaCommon.ClinGenTag, _clinGenFileName);
			//CreateSvTsv(InterimSaCommon.OnekSvTag, _onekGSvFileName);
			//ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);

			var tasks = new List<Task>
			{
				Task.Factory.StartNew(() => CreateDbsnpGaTsv(_dbSnpFileName)),
				Task.Factory.StartNew(() => CreateOnekgTsv(_onekGFileName)),
				Task.Factory.StartNew(() => CreateClinvarTsv(_clinVarFileName)),
				Task.Factory.StartNew(() => CreateExacTsv(_exacFile)),
				Task.Factory.StartNew(() => CreateEvsTsv(_evsFile)),
				Task.Factory.StartNew(() => CreateCosmicTsv(_cosmicVcfFileName, _cosmicTsvFileName)),
				Task.Factory.StartNew(() => CreateSvTsv(InterimSaCommon.DgvTag, _dgvFile)),
				Task.Factory.StartNew(() => CreateSvTsv(InterimSaCommon.ClinGenTag, _clinGenFileName)),
				Task.Factory.StartNew(() => CreateSvTsv(InterimSaCommon.OnekSvTag, _onekGSvFileName)),
			};

			tasks.AddRange(_customAnnotationFiles.Select(customAnnotationFile => Task.Factory.StartNew(() => CreateCutomAnnoTsv(customAnnotationFile))));
			tasks.AddRange(_customIntervalFiles.Select(customIntervalFile => Task.Factory.StartNew(() => CreateCustIntervalTsv(customIntervalFile))));

			try
			{
				Task.WaitAll(tasks.ToArray());
			}
			catch (AggregateException ae)
			{
				ae.Handle((x) =>
				{
					Console.WriteLine(x);
					return true;
				});
				throw;
			}
		}

		private void CreateSvTsv(string sourceName, string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

			Console.WriteLine($"Creating TSV from {fileName}");
			switch (sourceName)
			{
				case InterimSaCommon.DgvTag:
					using (var writer = new IntervalTsvWriter(_outputDirectory, GetDataSourceVersion(fileName),
						_genomeAssembly.ToString(), JsonCommon.DgvSchemaVersion, InterimSaCommon.DgvTag, ReportFor.StructuralVariants))
					{
						CreateSvTsv(new DgvReader(new FileInfo(fileName),_renamer ).GetEnumerator(),writer);
					}
					break;
				case InterimSaCommon.ClinGenTag:
					using (var writer = new IntervalTsvWriter(_outputDirectory, GetDataSourceVersion(fileName),
						_genomeAssembly.ToString(), JsonCommon.ClinGenSchemaVersion, InterimSaCommon.ClinGenTag,
						ReportFor.StructuralVariants))
					{
						CreateSvTsv(new ClinGenReader(new FileInfo(fileName), _renamer).GetEnumerator(), writer);
					}
					
					break;
				case InterimSaCommon.OnekSvTag:
					using (var writer = new IntervalTsvWriter(_outputDirectory, GetDataSourceVersion(fileName),
						_genomeAssembly.ToString(), JsonCommon.OneKgenSchemaVersion, InterimSaCommon.OnekSvTag,
						ReportFor.StructuralVariants))
					{
						CreateSvTsv(new OneKGenSvReader(new FileInfo(fileName), _renamer).GetEnumerator(),writer);
					}
					
					break;
				default:
					Console.WriteLine("invalid source name");
					break;
			}
			Console.WriteLine($"Completed {fileName}");
		}

		private void CreateSvTsv( IEnumerator<SupplementaryDataItem> siItems, IntervalTsvWriter writer)
		{
			while (siItems.MoveNext())
			{
				var siItem = siItems.Current.GetSupplementaryInterval(_renamer);
				writer.AddEntry(siItem.ReferenceName, siItem.Start, siItem.End, siItem.GetJsonString());
			}
		}

		private void CreateCustIntervalTsv(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

			Console.WriteLine($"Creating TSV from {fileName}");

			var version = GetDataSourceVersion(fileName);
			var reader = new CustomIntervalParser(new FileInfo(fileName), _renamer);
			using (var writer = new IntervalTsvWriter(_outputDirectory, version ,
				_genomeAssembly.ToString(), JsonCommon.CustIntervalSchemaVersion, reader.KeyName,
				ReportFor.AllVariants))
			{
				foreach (var custInterval in reader)
				{
					writer.AddEntry(custInterval.ReferenceName, custInterval.Start, custInterval.End, custInterval.GetJsonString());
				}
			}
			Console.WriteLine($"Completed {fileName}");
		}

		private void CreateCutomAnnoTsv(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

			Console.WriteLine($"Creating TSV from {fileName}");
			var version = GetDataSourceVersion(fileName);

            var customReader = new CustomAnnotationReader(new FileInfo(fileName), _renamer);
            using (var writer = new CustomAnnoTsvWriter(version, _outputDirectory, _genomeAssembly, customReader.IsPositional, _compressedReferencePath))
			{
				WriteSortedItems(customReader.GetEnumerator(), writer);
			}

			Console.WriteLine($"Finished {fileName}");

		}

		private void CreateCosmicTsv(string vcfFile, string tsvFile)
		{
			if (string.IsNullOrEmpty(tsvFile) || string.IsNullOrEmpty(vcfFile)) return;

			Console.WriteLine($"Creating TSV from {vcfFile} and {tsvFile}");

			var version = GetDataSourceVersion(vcfFile);
			using (var writer = new CosmicTsvWriter(version, _outputDirectory, _genomeAssembly, _compressedReferencePath))
			{
				var cosmicReader = new MergedCosmicReader(vcfFile,tsvFile, _renamer);
				WriteSortedItems(cosmicReader.GetEnumerator(), writer);
			}

			Console.WriteLine($"Finished {vcfFile}, {tsvFile}");
		}

		private void CreateEvsTsv(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;
			Console.WriteLine($"Creating TSV from {fileName}");

			var version = GetDataSourceVersion(fileName);
			using (var writer = new EvsTsvWriter(version, _outputDirectory, _genomeAssembly, _compressedReferencePath))
			{
				var evsReader = new EvsReader(new FileInfo(fileName), _renamer);
				WriteSortedItems(evsReader.GetEnumerator(), writer);
			}
			Console.WriteLine($"Finished {fileName}");
		}

		private void CreateExacTsv(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;
			Console.WriteLine($"Creating TSV from {fileName}");

			var version = GetDataSourceVersion(fileName);
			using (var writer = new ExacTsvWriter(version, _outputDirectory, _genomeAssembly, _compressedReferencePath))
			{
				var exacReader = new ExacReader(new FileInfo(fileName), _renamer);
				WriteSortedItems(exacReader.GetEnumerator(), writer);
			}

			Console.WriteLine($"Finished {fileName}");
		}

		private void CreateClinvarTsv(string fileName)
		{
			if (fileName == null) return;
			Console.WriteLine($"Creating TSV from {fileName}");

			var version = GetDataSourceVersion(fileName);
			//clinvar items do not come in sorted order, hence we need to store them in an array, sort them and then flush them out
			using (var writer = new ClinvarTsvWriter(version, _outputDirectory, _genomeAssembly, _compressedReferencePath))
			{
				var clinvarReader = new ClinVarXmlReader(new FileInfo(fileName), _sequenceReader, _compressedSequence);
				var clinvarList = clinvarReader.ToList();
				clinvarList.Sort();
				WriteSortedItems(clinvarList.GetEnumerator(), writer);
			}

			Console.WriteLine($"Finished {fileName}");
		}

		private void CreateDbsnpGaTsv(string fileName)
		{
			if (fileName == null) return;

			Console.WriteLine($"Creating TSV from {fileName}");

			var version = GetDataSourceVersion(fileName);

			using (var tsvWriter = new DbsnpGaTsvWriter(version, _outputDirectory, _genomeAssembly, _compressedReferencePath))
			{
				var dbSnpReader = new DbSnpReader(new FileInfo(fileName), _renamer);
				WriteSortedItems(dbSnpReader.GetEnumerator(), tsvWriter);
			}

			Console.WriteLine($"Finished {fileName}");

		}
		
		private void CreateOnekgTsv(string fileName)
		{
			if (fileName == null) return;
			Console.WriteLine($"Creating TSV from {fileName}");

			var version = GetDataSourceVersion(fileName);

			using (var tsvWriter = new OnekgTsvWriter(version, _outputDirectory, _genomeAssembly, _compressedReferencePath))
			{
				var onekgReader = new OneKGenReader(new FileInfo(fileName), _renamer);
				WriteSortedItems(onekgReader.GetEnumerator(), tsvWriter);
			}
			Console.WriteLine($"Finished {fileName}");
		}

		private static DataSourceVersion GetDataSourceVersion(string dataFileName)
		{
			var versionFileName = dataFileName + ".version";

			var version = DataSourceVersionReader.GetSourceVersion(versionFileName);
			return version;
		}

		private void WriteSortedItems(IEnumerator<SupplementaryDataItem> saItems, ISaItemTsvWriter writer)
		{
			var itemsMinHeap = new MinHeap<SupplementaryDataItem>();
			var currentRefName = "";

			var benchmark = new Benchmark();
			while(saItems.MoveNext())
			{
				var saItem = saItems.Current;
				//if (!SupplementaryAnnotationUtilities.IsRefAlleleValid(_compressedSequence, saItem.Start, saItem.ReferenceAllele))
				//	continue;
				if (currentRefName != saItem.Chromosome)
				{
					if (!string.IsNullOrEmpty(currentRefName))
					{
						//flushing out the remaining items in buffer
						WriteToPosition(writer, itemsMinHeap, int.MaxValue);
						Console.WriteLine($"Wrote out chr{currentRefName} items in {benchmark.GetElapsedTime()}");
						benchmark.Reset();
					}
					currentRefName = saItem.Chromosome;
					Console.WriteLine("Writing items from chromosome:" + currentRefName);
				}

				//the items come in sorted order of the pre-trimmed position. 
				//So when writing out, we have to make sure that we do not write past this position. 
				//Once a position has been seen in the stream, we can safely write all positions before that.
				var writeToPos = saItem.Start;

				saItem.Trim();
				itemsMinHeap.Add(saItem);
				
				WriteToPosition(writer, itemsMinHeap, writeToPos);
			}

			//flushing out the remaining items in buffer
			WriteToPosition(writer, itemsMinHeap, int.MaxValue);
		}

		
		private static void WriteToPosition(ISaItemTsvWriter writer, MinHeap<SupplementaryDataItem> itemsHeap, int position)
		{
			if (itemsHeap.Count() == 0) return;
			var bufferMin = itemsHeap.GetMin();

			while (bufferMin.Start < position)
			{
				var itemsAtMinPosition = new List<SupplementaryDataItem>();

				while (itemsHeap.Count() > 0 && bufferMin.CompareTo(itemsHeap.GetMin()) == 0)
					itemsAtMinPosition.Add(itemsHeap.ExtractMin());
				
				writer.WritePosition(itemsAtMinPosition);
				
				if (itemsHeap.Count()==0) break;
				
				bufferMin = itemsHeap.GetMin();
			}
			
		}
		
	}
}