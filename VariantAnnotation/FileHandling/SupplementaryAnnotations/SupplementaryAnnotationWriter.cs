using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public sealed class SupplementaryAnnotationWriter : IDisposable
    {
        #region members

        private BinaryWriter _binaryWriter;
        private readonly ExtendedBinaryWriter _writer;
        private FileStream _stream;

        private long _dataSourceVersionsOffset;
        private long _dataOffset;
        private long _eofOffset;

        private long _offsetHeader;

        private readonly string _saPath;
        private readonly string _currentRefSeq;
        private readonly GenomeAssembly _currentGenomeAssembly;

        private readonly List<DataSourceVersion> _dataSourceVersions;
        private List<SupplementaryInterval> _supplementaryIntervals;

        private SaIndex _index;
	    public int RefMinorCount;
        
        private long _intervalOffset;

        #endregion

        #region IDisposable


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
            if (disposing)
            {
                // write a negative length
                _writer.WriteInt(-1);

                
                // write the intervals
                WriteSupplementaryIntervals();

                // write the guard integer
                _binaryWriter.Flush();
                _eofOffset = _stream.Position;
                _binaryWriter.Write(SupplementaryAnnotationCommon.GuardInt);

                // update the offsets in the header
                _stream.Position = _offsetHeader;
                _binaryWriter.Write(_dataSourceVersionsOffset);
                _binaryWriter.Write(_dataOffset);
                _binaryWriter.Write(_intervalOffset);
                _binaryWriter.Write(_eofOffset);

                // close the file streams
                _binaryWriter.Dispose();
                _stream.Dispose();
                _index.Write(_saPath + ".idx", _currentRefSeq);
            }

            // reset all the class variables
            _binaryWriter = null;
            _stream = null;

            _dataSourceVersionsOffset = 0;
            _dataOffset = 0;
            _eofOffset = 0;
            _offsetHeader = 0;
            _intervalOffset = 0;

            _index = null;
        }

        #endregion

        // constructor
        public SupplementaryAnnotationWriter(string saPath, string currentRefSeq, List<DataSourceVersion> dataSourceVersions,GenomeAssembly genomeAssembly =GenomeAssembly.Unknown)
        {
            _dataSourceVersions    = dataSourceVersions;
            _saPath                = saPath;
            _currentRefSeq         = currentRefSeq;
	        _currentGenomeAssembly = genomeAssembly;

            _stream       = new FileStream(saPath, FileMode.Create);
            _binaryWriter = new BinaryWriter(_stream);
            _writer       = new ExtendedBinaryWriter(_binaryWriter);
			_index        = new SaIndex();

            WriteHeader();
        }

        
        public void SetIntervalList(List<SupplementaryInterval> intervalList)
        {
            _supplementaryIntervals = intervalList;
        }


        private void WriteSupplementaryIntervals()
        {
            _binaryWriter.Flush();
            if (null == _supplementaryIntervals)
            {
                _intervalOffset = -1;
                return;
            }

            if (_supplementaryIntervals.Count == 0)
            {
                _intervalOffset = -1;
                return;
            }

            _intervalOffset = _stream.Position;
            _binaryWriter.Write(_supplementaryIntervals.Count);
            var extendedWriter = new ExtendedBinaryWriter(_binaryWriter);

            foreach (var interval in _supplementaryIntervals)
            {
                interval.Write(extendedWriter);
            }
            _supplementaryIntervals?.Clear();
        }

        /// <summary>
        /// writes the annotations to the current database file
        /// </summary>
        public void Write(SupplementaryAnnotation sa, int referencePos)
        {
			sa.FinalizePositionalAnnotations();
            var positionalRecords = sa.GetPositionalRecords();

            // sanity check: make sure we have some data to write
            if ((positionalRecords.Count == 0) &&
                (sa.AlleleSpecificAnnotations.Count == 0) &&
                (sa.CosmicItems.Count == 0) &&
                (sa.ClinVarItems.Count == 0) &&
                (sa.CustomItems.Count == 0)
                )
            {
                return;
            }

            // add this entry to the index
            long currentOffset = _stream.Position;
            _index.Add((uint)referencePos, (uint)currentOffset, AnnotationLoader.Instance.GenomeAssembly == GenomeAssembly.GRCh37 && sa.IsRefMinorAllele);
	        if (sa.IsRefMinorAllele) RefMinorCount++;

            WriteAnnotation(sa, positionalRecords, _writer);
        }

        /// <summary>
        /// writes the supplementary annotation to disk
        /// </summary>
        private static void WriteAnnotation(SupplementaryAnnotation sa, List<AbstractAnnotationRecord> positionalRecords, ExtendedBinaryWriter writer)
        {
			byte[] annotationBytes;

            // add everything to a memory stream so we can capture the length
            using (var ms = new MemoryStream())
            {
                using (var memoryWriter = new BinaryWriter(ms))
                {
                    var extendedWriter = new ExtendedBinaryWriter(memoryWriter);

                    // write the position-specific records
                    extendedWriter.WriteInt(positionalRecords.Count);

                    foreach (var record in positionalRecords) record.Write(extendedWriter);

                    // write the allele-specific records
                    extendedWriter.WriteInt(sa.AlleleSpecificAnnotations.Count);

                    foreach (var alleleKvp in sa.AlleleSpecificAnnotations)
                    {
                        // write the allele
                        extendedWriter.WriteAsciiString(alleleKvp.Key);

                        // grab all of the allele-specific records for that allele
                        var alleleSpecificRecords = sa.GetAlleleSpecificRecords(alleleKvp.Value);

                        extendedWriter.WriteInt(alleleSpecificRecords.Count);
                        foreach (var record in alleleSpecificRecords) record.Write(extendedWriter);
                    }

                    // write the cosmic items
                    extendedWriter.WriteInt(sa.CosmicItems.Count);
                    foreach (var cosmicItem in sa.CosmicItems)
                    {
                        cosmicItem.Write(extendedWriter);
                    }

                    // writing ClinVar items
                    extendedWriter.WriteInt(sa.ClinVarItems.Count);
                    foreach (var clinVarItem in sa.ClinVarItems)
                    {
                        clinVarItem.Write(extendedWriter);
                    }

                    // writing custom Annotations
                    extendedWriter.WriteInt(sa.CustomItems.Count);
                    foreach (var customItem in sa.CustomItems)
                    {
                        customItem.Write(extendedWriter);
                    }
                }

                annotationBytes = ms.ToArray();
            }

            // write the supplementary annotation to disk
            //writer.WriteInt(referencePos);
            //writer.WriteInt(annotationBytes.Length);
            writer.WriteBytes(annotationBytes);
        }

        /// <summary>
        /// writes the header to the current database file
        /// </summary>
        private void WriteHeader()
        {
            _binaryWriter.Write(System.Text.Encoding.ASCII.GetBytes(SupplementaryAnnotationCommon.DataHeader));
            _binaryWriter.Write(SupplementaryAnnotationCommon.DataVersion);
            _binaryWriter.Write(SupplementaryAnnotationCommon.SchemaVersion);
			_binaryWriter.Write((byte)_currentGenomeAssembly);
            _binaryWriter.Write(DateTime.UtcNow.Ticks);
            _binaryWriter.Write(_currentRefSeq);

            // reserve space for the offsets
            _binaryWriter.Flush();
            _offsetHeader = _stream.Position;
            _binaryWriter.Write(_dataSourceVersionsOffset);
            _binaryWriter.Write(_dataOffset);
            _binaryWriter.Write(_intervalOffset);
            _binaryWriter.Write(_eofOffset);

            // write the data source versions
            _binaryWriter.Flush();
            _dataSourceVersionsOffset = _stream.Position;

            int numDataSourceVersions = _dataSourceVersions?.Count ?? 0;
            _writer.WriteInt(numDataSourceVersions);

            if (_dataSourceVersions != null)
            {
                foreach (var dataSourceVersion in _dataSourceVersions) dataSourceVersion.Write(_binaryWriter);
            }

            // grab the data offset
            _binaryWriter.Flush();
            _dataOffset = _stream.Position;
        }
    }
}
