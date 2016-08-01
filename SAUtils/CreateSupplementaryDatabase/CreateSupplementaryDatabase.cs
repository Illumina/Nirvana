using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.ClinGen;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.InputFileParsers.Cosmic;
using SAUtils.InputFileParsers.CustomAnnotation;
using SAUtils.InputFileParsers.DbSnp;
using SAUtils.InputFileParsers.DGV;
using SAUtils.InputFileParsers.EVS;
using SAUtils.InputFileParsers.ExAc;
using SAUtils.InputFileParsers.OneKGen;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.Utilities;
using VariantAnnotation.FileHandling;

namespace SAUtils.CreateSupplementaryDatabase
{
    public class CreateSupplementaryDatabase
    {
        #region members

        private readonly string _nsdBaseFileName; //nirvana supplementary database filename
        private readonly List<IEnumerator<SupplementaryDataItem>> _iSupplementaryDataItemList;
	    private readonly List<SupplementaryDataItem> _additionalItemsList;
	    private readonly List<SupplementaryInterval> _supplementaryIntervalList;
	    private readonly List<DataSourceVersion> _dataSources;
		private readonly ICompressedSequence _compressedSequence;
	    private const int ReferenceWindowSize = 10;
		private string _currentRefName;
	    private SupplementaryAnnotationWriter _saWriter;
	    private Benchmark _creationBench;
	    private int _numSaWritten;
        private SupplementaryAnnotation _prevSa;
	    private HashSet<string> _processedReferences; 

		#endregion

		// constructor
		public CreateSupplementaryDatabase(
			string nsdBaseFileName, 
			string dbSnpFileName = null,
            string cosmicVcfFile = null, 
			string cosmicTsvFile = null,
            string clinVarFileName = null, 
			string clinVarPubmedFileName= null, 
			string clinVarEvalDateFileName=null, 
			string oneKGenomeAfFileName = null, 
			string evsFileName = null,
            string exacFileName = null,
			List<string> customFiles = null, 
			string dgvFileName = null,
			string oneKSvFileName = null, 
			string clinGenFileName =null)
        {
            _nsdBaseFileName = nsdBaseFileName;
            _dataSources = new List<DataSourceVersion>();


			_iSupplementaryDataItemList = new List<IEnumerator<SupplementaryDataItem>>();
			_supplementaryIntervalList = new List<SupplementaryInterval>();

            Console.WriteLine("Creating supplementary annotation files... Data version: {0}, schema version: {1}", SupplementaryAnnotationCommon.DataVersion, SupplementaryAnnotationCommon.SchemaVersion);

            _compressedSequence = AnnotationLoader.Instance.CompressedSequence;
			
            if (dbSnpFileName != null)
            {
	            AddSourceVersion(dbSnpFileName);

	            var dbSnpReader = new DbSnpReader(new FileInfo(dbSnpFileName));
                IEnumerator<DbSnpItem> dbSnpEnumerator = dbSnpReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(dbSnpEnumerator);
            }

            if (cosmicVcfFile != null && cosmicTsvFile!=null)
	        {
				AddSourceVersion(cosmicVcfFile);

				var cosmicReader = new MergedCosmicReader(cosmicVcfFile,cosmicTsvFile);
		        IEnumerator<CosmicItem> cosmicEnumerator = cosmicReader.GetEnumerator();
				_iSupplementaryDataItemList.Add(cosmicEnumerator);
	        }

			if (oneKGenomeAfFileName != null)
            {
				AddSourceVersion(oneKGenomeAfFileName);

				var oneKGenReader = new OneKGenReader(new FileInfo(oneKGenomeAfFileName));
                IEnumerator<OneKGenItem> oneKGenEnumerator = oneKGenReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(oneKGenEnumerator);

            }
			
			if (oneKSvFileName != null)
			{
				if (oneKGenomeAfFileName== null)
					AddSourceVersion(oneKSvFileName);

				var oneKGenSvReader = new OneKGenSvReader(new FileInfo(oneKSvFileName));
				IEnumerator<OneKGenItem> oneKGenSvEnumerator = oneKGenSvReader.GetEnumerator();
				_iSupplementaryDataItemList.Add(oneKGenSvEnumerator);
			}

            if (evsFileName != null)
            {
				AddSourceVersion(evsFileName);

				var evsReader = new EvsReader(new FileInfo(evsFileName));
                IEnumerator<EvsItem> evsEnumerator = evsReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(evsEnumerator);
            }

            if (exacFileName != null)
            {
				AddSourceVersion(exacFileName);

				var exacReader = new ExacReader(new FileInfo(exacFileName));
                IEnumerator<ExacItem> exacEnumerator = exacReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(exacEnumerator);
            }

            if (clinVarFileName != null)
            {
				AddSourceVersion(clinVarFileName);

				var clinVarReader = new ClinVarReader(new FileInfo(clinVarFileName), 
					clinVarPubmedFileName == null? null: new FileInfo(clinVarPubmedFileName),
					clinVarEvalDateFileName == null ? null : new FileInfo(clinVarEvalDateFileName));
                IEnumerator<ClinVarItem> clinVarEnumerator = clinVarReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(clinVarEnumerator);
            }
			
			if (dgvFileName != null)
			{
				AddSourceVersion(dgvFileName);

				var dgvReader = new DgvReader(new FileInfo(dgvFileName));
				IEnumerator<DgvItem> dgvEnumerator = dgvReader.GetEnumerator();
				_iSupplementaryDataItemList.Add(dgvEnumerator);
			}

			if (clinGenFileName != null)
			{
				AddSourceVersion(clinGenFileName);
				var clinGenReader = new ClinGenReader(new FileInfo(clinGenFileName));
				IEnumerator<ClinGenItem> clinGenEnumerator = clinGenReader.GetEnumerator();
				_iSupplementaryDataItemList.Add(clinGenEnumerator);
			}
			
			if (customFiles != null)
            {
                foreach (var customFile in customFiles)
                {
					AddSourceVersion(customFile);

					var customReader = new CustomAnnotationReader(new FileInfo(customFile));
                    IEnumerator<CustomItem> customEnumerator = customReader.GetEnumerator();
                    _iSupplementaryDataItemList.Add(customEnumerator);
                }
            }

            // initializing the IEnumerators in the list
            foreach (var iDataEnumerator in _iSupplementaryDataItemList)
            {
                if (!iDataEnumerator.MoveNext()) _iSupplementaryDataItemList.Remove(iDataEnumerator);
            }

            _additionalItemsList = new List<SupplementaryDataItem>();
        }

	    private void AddSourceVersion(string dataFileName)
	    {
		    var versionFileName = dataFileName + ".version";

		    if (!File.Exists(versionFileName))
		    {
			    throw new FileNotFoundException(versionFileName);
		    }

		    var versionReader = new DataSourceVersionReader(versionFileName);
		    var version = versionReader.GetVersion();
		    Console.WriteLine(version.ToString());
		    _dataSources.Add(version);
	    }

	    public void CreateDatabase()
        {

            int unsorted = 0;

			_creationBench = new Benchmark();

			_prevSa = null;

			_processedReferences = new HashSet<string>();

			// loading ref sequence
			var sa = GetNextSupplementaryAnnotation();

			while (sa != null)
		    {
			    if (!_currentRefName.Equals(sa.RefSeqName)) //sanity check
			    {
				    throw new Exception("Error: currentRef != sa ref");
			    }

			    if (_saWriter == null) //check for empty writer
			    {
				    Console.WriteLine("Supplementary annotationa writer was not initialized");
				    return;
			    }


			    // this SA is not the first one in current contig
			    if (_prevSa != null)
			    {
				    if (sa.ReferencePosition == _prevSa.ReferencePosition)
				    {
					    _prevSa.MergeAnnotations(sa);
				    }
				    else
				    {
					    if (_prevSa.RefSeqName == sa.RefSeqName && _prevSa.ReferencePosition > sa.ReferencePosition)
					    {
						    Console.WriteLine("Unsorted records:{0}, {1}, {2}, {3}", _prevSa.RefSeqName,
							    _prevSa.ReferencePosition, sa.RefSeqName, sa.ReferencePosition);
						    unsorted++;
					    }

					    if (_prevSa.NotEmpty())
					    {
						    _saWriter.Write(_prevSa, _prevSa.ReferencePosition);
						    _numSaWritten++;
					    }
					    _prevSa = sa;
				    }
			    }
			    else
			    {
					_prevSa = sa;
				}

			    
			    sa = GetNextSupplementaryAnnotation();
		    }


			// do not forgot to write the last item 
			CloseCurrentSaWriter();

			Console.WriteLine("");
	        Console.WriteLine("Bad clinvar entries:{0}",ClinVarItem.InconsistantClinvarItemCount);
            Console.WriteLine("unsorted records:{0}", unsorted);
			Console.WriteLine($"the total number of alleles processed: {SupplementaryAnnotation.TotalAlleleSpecificEntryCount}");
			Console.WriteLine($"the conflicting number of alleles processed: {SupplementaryAnnotation.ConflictingAlleleSpecificEntryCount}");
		}

	    private void OpenNewSaWriter()
	    {
            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;
            string currentEnsemblRefName = chromosomeRenamer.GetEnsemblReferenceName(_currentRefName);
		    string currentUcscRefName = chromosomeRenamer.GetUcscReferenceName(_currentRefName);

			if (_processedReferences.Contains(currentEnsemblRefName))
		    {
			    throw new Exception($"usorted file, for chromsome {_currentRefName}, SA will be rewritten");
		    }
		    _processedReferences.Add(currentEnsemblRefName);
			var saPath = Path.Combine(_nsdBaseFileName, currentUcscRefName + ".nsa");
			_saWriter = new SupplementaryAnnotationWriter(saPath, _currentRefName, _dataSources, AnnotationLoader.Instance.GenomeAssembly);
			Console.WriteLine("Populating {0} data...", currentUcscRefName);

			_creationBench.Reset();
			_numSaWritten = 0;
		}
	    


		private void CloseCurrentSaWriter()
		{
			if (_currentRefName == null)
				return;

			//write last SA item
			if (_prevSa.NotEmpty())
			{
				_saWriter.Write(_prevSa, _prevSa.ReferencePosition);
				_numSaWritten++;
			}
			// reset _prevSa
			_prevSa = null;

			// write the intervals
			_saWriter.SetIntervalList(_supplementaryIntervalList);

			Console.WriteLine("No of intervals: {0}", _supplementaryIntervalList.Count);

			_saWriter.Dispose();

			Console.WriteLine("No of annotations : {0}",  _numSaWritten);
			
			double lookupsPerSecond;

			Console.WriteLine("{0} supplementary annotations written - {1}", _numSaWritten,
				_creationBench.GetElapsedIterationTime(_numSaWritten, "variants", out lookupsPerSecond));

			Console.WriteLine("No of ref minor positions: {0}", _saWriter.RefMinorCount);

		}

		private SupplementaryDataItem CurrentMinSupplementaryDataItem()
        {
			SupplementaryDataItem minSupplementaryDataItem = null;

            foreach (var iDataEnumerator in _iSupplementaryDataItemList)
            {
                if (minSupplementaryDataItem == null)
                    minSupplementaryDataItem = iDataEnumerator.Current;
                else
                    if (minSupplementaryDataItem.CompareTo(iDataEnumerator.Current) > 0)
                    {
                        minSupplementaryDataItem = iDataEnumerator.Current;
                    }
            }

			// checking if one of the additional items is the min.
	        if (_additionalItemsList.Count <= 0) return minSupplementaryDataItem;

	        if (minSupplementaryDataItem == null)
	        {
		        return _additionalItemsList[0];
	        }

	        if (minSupplementaryDataItem.CompareTo(_additionalItemsList[0]) > 0)
		        minSupplementaryDataItem = _additionalItemsList[0];
	        return minSupplementaryDataItem;
        }

        private SupplementaryAnnotation GetNextSupplementaryAnnotation()
        {
			// no more active iterators left
            if (_iSupplementaryDataItemList.Count == 0 && _additionalItemsList.Count ==0 )
                return null;

            var minSupplementaryDataItem = CurrentMinSupplementaryDataItem();

			
            if (minSupplementaryDataItem == null) return null;//nothing more to retun. All enumerators are empty.

			var sa = new SupplementaryAnnotation (minSupplementaryDataItem.Start)
			{	
				RefSeqName        = minSupplementaryDataItem.Chromosome 
			};

	        string refSequence = null;

	        if (_currentRefName == null || !_currentRefName.Equals(sa.RefSeqName))
	        {
		        CloseCurrentSaWriter();

		        _currentRefName = sa.RefSeqName;
		        AnnotationLoader.Instance.Load(_currentRefName);
		        OpenNewSaWriter();

			}


			if (_compressedSequence != null)
				refSequence = _compressedSequence.Substring(sa.ReferencePosition-1, ReferenceWindowSize);
            // list of data items to be removed and added
            var deleteList = new List<IEnumerator<SupplementaryDataItem>>();
            
            foreach (var iDataEnumerator in _iSupplementaryDataItemList)
            {
				// only using items at the same location as minSuppDataItem
	            if (!iDataEnumerator.Current.Equals(minSupplementaryDataItem)) continue;

	            if (iDataEnumerator.Current.IsInterval)
	            {
		            var suppInterval = iDataEnumerator.Current.GetSupplementaryInterval();

		            _supplementaryIntervalList.Add(suppInterval);
		            
	            }
	            else
	            {
					var additionalSuppData = iDataEnumerator.Current.SetSupplementaryAnnotations(sa, refSequence);

					if (additionalSuppData != null)
						_additionalItemsList.Add(additionalSuppData);

				}
				// adding empty enumerators to deleteList
				if (!iDataEnumerator.MoveNext()) deleteList.Add(iDataEnumerator);
            }

			// add annotations from additional items if applicable.
	        AddAdditionalItems(minSupplementaryDataItem, sa);
	        
			// removing lists that are empty and therfore should be removed from the list of enumerators
	        _iSupplementaryDataItemList.RemoveAll(x => deleteList.Contains(x));

			return sa;

        }

	    private void AddAdditionalItems(SupplementaryDataItem minSupplementaryDataItem, SupplementaryAnnotation sa)
	    {
		    if (_additionalItemsList.Count <= 0) return;

			if (_additionalItemsList.Count > 1)
				_additionalItemsList.Sort();
		    while (_additionalItemsList.Count > 0)
		    {
			    var firstItem = _additionalItemsList[0];
			    if (firstItem.Equals(minSupplementaryDataItem))
			    {
				    firstItem.SetSupplementaryAnnotations(sa);
				    _additionalItemsList.RemoveAt(0);
			    }
			    else break;
		    }
	    }
    }
}
