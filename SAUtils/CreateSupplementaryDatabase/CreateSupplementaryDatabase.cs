using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
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
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateSupplementaryDatabase
{
    public sealed class CreateSupplementaryDatabase
    {
        #region members

        private readonly string _nsdBaseFileName; //nirvana supplementary database filename
        private readonly List<IEnumerator<SupplementaryDataItem>> _iSupplementaryDataItemList;
        private readonly List<SupplementaryDataItem> _additionalItemsList;
        private readonly List<SupplementaryInterval> _supplementaryIntervalList;
        private readonly List<DataSourceVersion> _dataSources;

        private readonly ICompressedSequence _compressedSequence;
        private readonly DataFileManager _dataFileManager;

        private const int ReferenceWindowSize = 10;
        private string _currentRefName;
        private SupplementaryAnnotationWriter _saWriter;
        private Benchmark _creationBench;
        private int _numSaWritten;
        private SupplementaryPositionCreator _prevSaCreator;

        private readonly ChromosomeRenamer _renamer;

        #endregion

        // constructor
        public CreateSupplementaryDatabase(
            string compressedReferencePath,
            string nsdBaseFileName,
            string dbSnpFileName        = null,
            string cosmicVcfFile        = null,
            string cosmicTsvFile        = null,
            string clinVarFileName      = null,
            string oneKGenomeAfFileName = null,
            string evsFileName          = null,
            string exacFileName         = null,
            List<string> customFiles    = null,
            string dgvFileName          = null,
            string oneKSvFileName       = null,
            string clinGenFileName      = null,
            string chrWhiteList         = null)
        {
            _nsdBaseFileName = nsdBaseFileName;
            _dataSources = new List<DataSourceVersion>();

            _iSupplementaryDataItemList = new List<IEnumerator<SupplementaryDataItem>>();
            _supplementaryIntervalList = new List<SupplementaryInterval>();

            Console.WriteLine("Creating supplementary annotation files... Data version: {0}, schema version: {1}", SupplementaryAnnotationCommon.DataVersion, SupplementaryAnnotationCommon.SchemaVersion);

            _compressedSequence          = new CompressedSequence();
            var compressedSequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), _compressedSequence);
            _renamer                     = _compressedSequence.Renamer;
            _dataFileManager             = new DataFileManager(compressedSequenceReader, _compressedSequence);

            if (!string.IsNullOrEmpty(chrWhiteList))
            {
                Console.WriteLine("Creating SA for the following chromosomes only:");
                foreach (var refSeq in chrWhiteList.Split(','))
                {
                    InputFileParserUtilities.ChromosomeWhiteList.Add(_renamer.GetEnsemblReferenceName(refSeq));
                    Console.Write(refSeq + ",");
                }
                Console.WriteLine();
            }
            else InputFileParserUtilities.ChromosomeWhiteList = null;

            if (dbSnpFileName != null)
            {
                AddSourceVersion(dbSnpFileName);

                var dbSnpReader = new DbSnpReader(new FileInfo(dbSnpFileName), _renamer);
                var dbSnpEnumerator = dbSnpReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(dbSnpEnumerator);
            }

            if (cosmicVcfFile != null && cosmicTsvFile != null)
            {
                AddSourceVersion(cosmicVcfFile);

                var cosmicReader = new MergedCosmicReader(cosmicVcfFile, cosmicTsvFile, _renamer);
                var cosmicEnumerator = cosmicReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(cosmicEnumerator);
            }

            if (oneKGenomeAfFileName != null)
            {
                AddSourceVersion(oneKGenomeAfFileName);

                var oneKGenReader = new OneKGenReader(new FileInfo(oneKGenomeAfFileName), _renamer);
                var oneKGenEnumerator = oneKGenReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(oneKGenEnumerator);

            }

            if (oneKSvFileName != null)
            {
                if (oneKGenomeAfFileName == null)
                    AddSourceVersion(oneKSvFileName);

                var oneKGenSvReader = new OneKGenSvReader(new FileInfo(oneKSvFileName), _renamer);
                var oneKGenSvEnumerator = oneKGenSvReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(oneKGenSvEnumerator);
            }

            if (evsFileName != null)
            {
                AddSourceVersion(evsFileName);

                var evsReader = new EvsReader(new FileInfo(evsFileName), _renamer);
                var evsEnumerator = evsReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(evsEnumerator);
            }

            if (exacFileName != null)
            {
                AddSourceVersion(exacFileName);

                var exacReader = new ExacReader(new FileInfo(exacFileName), _renamer);
                var exacEnumerator = exacReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(exacEnumerator);
            }

            if (clinVarFileName != null)
            {
                AddSourceVersion(clinVarFileName);

                var clinVarReader = new ClinVarXmlReader(new FileInfo(clinVarFileName), compressedSequenceReader, _compressedSequence);

                var clinVarList = clinVarReader.ToList();

                clinVarList.Sort();
                Console.WriteLine($"{clinVarList.Count} clinvar items read form XML file");

                IEnumerator<ClinVarItem> clinVarEnumerator = clinVarList.GetEnumerator();
                _iSupplementaryDataItemList.Add(clinVarEnumerator);
            }

            if (dgvFileName != null)
            {
                AddSourceVersion(dgvFileName);

                var dgvReader = new DgvReader(new FileInfo(dgvFileName), _renamer);
                var dgvEnumerator = dgvReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(dgvEnumerator);
            }

            if (clinGenFileName != null)
            {
                AddSourceVersion(clinGenFileName);
                var clinGenReader = new ClinGenReader(new FileInfo(clinGenFileName), _renamer);
                var clinGenEnumerator = clinGenReader.GetEnumerator();
                _iSupplementaryDataItemList.Add(clinGenEnumerator);
            }

            if (customFiles != null)
            {
                foreach (var customFile in customFiles)
                {
                    AddSourceVersion(customFile);

                    var customReader = new CustomAnnotationReader(new FileInfo(customFile), _renamer);
                    var customEnumerator = customReader.GetEnumerator();
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

            var version = DataSourceVersionReader.GetSourceVersion(versionFileName);
            Console.WriteLine(version.ToString());
            _dataSources.Add(version);
        }


        public void CreateDatabase()
        {

            var unsorted = 0;

            _creationBench = new Benchmark();

            _prevSaCreator = null;

            // loading ref sequence
            var saCreator = GetNextSupplementaryAnnotation();

            while (saCreator != null)
            {
                if (!_currentRefName.Equals(saCreator.RefSeqName)) //sanity check
                {
                    throw new Exception("Error: currentRef != sa ref");
                }

                if (_saWriter == null) //check for empty writer
                {
                    Console.WriteLine("Supplementary annotationa writer was not initialized");
                    return;
                }


                // this SA is not the first one in current contig
                if (_prevSaCreator != null)
                {
                    if (saCreator.ReferencePosition == _prevSaCreator.ReferencePosition)
                    {
                        _prevSaCreator.MergeSaCreator(saCreator);
                    }
                    else
                    {
                        if (_prevSaCreator.RefSeqName == saCreator.RefSeqName && _prevSaCreator.ReferencePosition > saCreator.ReferencePosition)
                        {
                            Console.WriteLine("Unsorted records:{0}, {1}, {2}, {3}", _prevSaCreator.RefSeqName,
                                _prevSaCreator.ReferencePosition, saCreator.RefSeqName, saCreator.ReferencePosition);
                            unsorted++;
                        }

                        if (!_prevSaCreator.IsEmpty())
                        {
                            _saWriter.Write(_prevSaCreator, _prevSaCreator.ReferencePosition);
                            _numSaWritten++;
                        }
                        _prevSaCreator = saCreator;
                    }
                }
                else
                {
                    _prevSaCreator = saCreator;
                }


                saCreator = GetNextSupplementaryAnnotation();
            }


            // do not forgot to write the last item 
            CloseCurrentSaWriter();

            Console.WriteLine("");
            Console.WriteLine("unsorted records: {0}", unsorted);
        }

        private void OpenNewSaWriter()
        {
            var currentEnsemblRefName = _renamer.GetEnsemblReferenceName(_currentRefName);
            var currentUcscRefName = _renamer.GetUcscReferenceName(_currentRefName);

            if (InputFileParserUtilities.ProcessedReferences.Contains(currentEnsemblRefName))
            {
                throw new Exception($"usorted file, for chromsome {_currentRefName}, SA will be rewritten");
            }

            InputFileParserUtilities.ProcessedReferences.Add(currentEnsemblRefName);
            var saPath = Path.Combine(_nsdBaseFileName, currentUcscRefName + ".nsa");
            _saWriter = new SupplementaryAnnotationWriter(saPath, _currentRefName, _dataSources, _compressedSequence.GenomeAssembly);
            Console.WriteLine("Populating {0} data...", currentUcscRefName);

            _creationBench.Reset();
            _numSaWritten = 0;
        }

        private void CloseCurrentSaWriter()
        {
            if (_currentRefName == null)
                return;

            //write last SA item
            if (!_prevSaCreator.IsEmpty())
            {
                _saWriter.Write(_prevSaCreator, _prevSaCreator.ReferencePosition);
                _numSaWritten++;
            }
            // reset _prevSa
            _prevSaCreator = null;

            // write the intervals
            _saWriter.SetIntervalList(_supplementaryIntervalList);

            Console.WriteLine("No of intervals: {0}", _supplementaryIntervalList.Count);

            _saWriter.Dispose();
			double lookupsPerSecond;

			Console.WriteLine("No of annotations : {0}", _numSaWritten);
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

        private SupplementaryPositionCreator GetNextSupplementaryAnnotation()
        {
            // no more active iterators left
            if (_iSupplementaryDataItemList.Count == 0 && _additionalItemsList.Count == 0)
                return null;

            var minSupplementaryDataItem = CurrentMinSupplementaryDataItem();

            if (minSupplementaryDataItem == null) return null;//nothing more to retun. All enumerators are empty.


            var sa = new SupplementaryAnnotationPosition(minSupplementaryDataItem.Start);


            var saCreator = new SupplementaryPositionCreator(sa)
            {
                RefSeqName = minSupplementaryDataItem.Chromosome
            };

            string refSequence = null;

            if (_currentRefName == null || !_currentRefName.Equals(saCreator.RefSeqName))
            {
                CloseCurrentSaWriter();

                _currentRefName = saCreator.RefSeqName;

                var refIndex = _renamer.GetReferenceIndex(_currentRefName);
                if (refIndex == ChromosomeRenamer.UnknownReferenceIndex) throw new GeneralException($"Could not find the reference index for: {_currentRefName}");
                _dataFileManager.LoadReference(refIndex, () => {});

                OpenNewSaWriter();
            }

            if (_compressedSequence != null)
                refSequence = _compressedSequence.Substring(sa.ReferencePosition - 1, ReferenceWindowSize);
            // list of data items to be removed and added
            var deleteList = new List<IEnumerator<SupplementaryDataItem>>();

            foreach (var iDataEnumerator in _iSupplementaryDataItemList)
            {
                // only using items at the same location as minSuppDataItem
                if (!iDataEnumerator.Current.Equals(minSupplementaryDataItem)) continue;

                if (iDataEnumerator.Current.IsInterval)
                {
                    var suppInterval = iDataEnumerator.Current.GetSupplementaryInterval(_renamer);

                    _supplementaryIntervalList.Add(suppInterval);

                }
                else
                {
                    var additionalSuppData = iDataEnumerator.Current.SetSupplementaryAnnotations(saCreator, refSequence);

                    if (additionalSuppData != null)
                        _additionalItemsList.Add(additionalSuppData);

                }
                // adding empty enumerators to deleteList
                if (!iDataEnumerator.MoveNext()) deleteList.Add(iDataEnumerator);
            }

            // add annotations from additional items if applicable.
            AddAdditionalItems(minSupplementaryDataItem, saCreator);

            // removing lists that are empty and therfore should be removed from the list of enumerators
            _iSupplementaryDataItemList.RemoveAll(x => deleteList.Contains(x));

            return saCreator;

        }

        private void AddAdditionalItems(SupplementaryDataItem minSupplementaryDataItem, SupplementaryPositionCreator sa)
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
