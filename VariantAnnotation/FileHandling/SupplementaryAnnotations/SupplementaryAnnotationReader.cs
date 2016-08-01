using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public sealed class SupplementaryAnnotationReader : IDisposable
    {
        #region members

        private BinaryReader _binaryReader;
        private ExtendedBinaryReader _reader;
        private Stream _stream;
	    private Stream _idxStream;

        private SaIndex _index;
        
        private static long _intervalsPosition;
		
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
                _binaryReader.Dispose();
                _stream.Dispose();
				_idxStream.Dispose();
            }

            _binaryReader = null;
            _reader       = null;
            _stream       = null;
	        _idxStream    = null;
            _index        = null;
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public SupplementaryAnnotationReader(string saPath)
            : this(
                FileUtilities.GetFileStream(saPath), FileUtilities.GetFileStream(saPath + ".idx"),
                saPath)
        {}

        /// <summary>
        /// constructor
        /// </summary>
        private SupplementaryAnnotationReader(Stream dbStream, Stream idxStream, string saPath = null)
        {
            // open the database file
            _stream       = dbStream;
            _idxStream    = idxStream;
            _binaryReader = new BinaryReader(_stream);
            _reader       = new ExtendedBinaryReader(_binaryReader);
            _index        = new SaIndex(new BinaryReader(idxStream));

            // check the header
            GetHeader(_binaryReader, saPath);
        }

        /// <summary>
        /// finds the positional record that starts at the specified position
        /// </summary>
        public SupplementaryAnnotation GetAnnotation(int referencePos)
        {
	        // grab the index file offset
			long fileOffset = _index.GetFileLocation((uint)referencePos);
            if (fileOffset == uint.MinValue ) return null;

            // read the position
            _stream.Position = fileOffset;
            
            // read the rest of the supplementary annotation
            return Read(_reader, referencePos);	     
        }
     
        /// <summary>
        /// returns the header from the specified Nirvana database file
        /// </summary>
        public static SupplementaryAnnotationHeader GetHeader(string saPath)
        {
            SupplementaryAnnotationHeader header;

            using (var reader = new BinaryReader(FileUtilities.GetFileStream(saPath)))
            {
                header = GetHeader(reader, saPath);
			}
			
			return header;
        }

        /// <summary>
        /// checks if the header is good
        /// </summary>
        private static SupplementaryAnnotationHeader GetHeader(BinaryReader binaryReader, string saPath=null)
        {
            // check the header and data version
            string header                 = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(SupplementaryAnnotationCommon.DataHeader.Length));
            ushort dataVersion            = binaryReader.ReadUInt16();
            ushort schemaVersion          = binaryReader.ReadUInt16();
	        GenomeAssembly genomeAssembly = (GenomeAssembly) binaryReader.ReadByte();

            if ((header != SupplementaryAnnotationCommon.DataHeader) || (schemaVersion != SupplementaryAnnotationCommon.SchemaVersion))
            {
                throw new UserErrorException($"The header check failed for the supplementary annotation file ({saPath ?? "(resource)"}): ID: exp: {SupplementaryAnnotationCommon.DataHeader} obs: {header}, schema version: exp:{SupplementaryAnnotationCommon.SchemaVersion} obs: {schemaVersion}");
            }

            long creationTimeTicks       = binaryReader.ReadInt64();
            string referenceSequenceName = binaryReader.ReadString();

            // skip over the offsets since they're not currently used
            binaryReader.ReadInt64(); // _dataSourceVersionsOffset
            binaryReader.ReadInt64(); // _dataOffset
			_intervalsPosition = binaryReader.ReadInt64();
	        binaryReader.ReadInt64(); // _eofOffset

			var reader = new ExtendedBinaryReader(binaryReader);
            
            // load the data source versions
            int numDataSourceVersions = reader.ReadInt();
            var dataSourceVersions    = new List<DataSourceVersion>();

            for (int i = 0; i < numDataSourceVersions; i++) dataSourceVersions.Add(DataSourceVersion.Read(reader));

			
			return new SupplementaryAnnotationHeader(referenceSequenceName, creationTimeTicks, dataVersion, dataSourceVersions, genomeAssembly);
        }

	    public IEnumerable<SupplementaryInterval> GetSupplementaryIntervals()
	    {
		    if (_intervalsPosition == -1) return null;
		    var returnPosition = _stream.Position;
		    _stream.Position = _intervalsPosition;
			
			var count = _binaryReader.ReadInt32();// how many supplementary intervals to read

			if (count == 0) return null;

			var intervalList = new List<SupplementaryInterval>(count);
		    
			for (int i = 0; i < count; i++)
				intervalList.Add(SupplementaryInterval.Read(_reader));
			
			_stream.Position = returnPosition;

		    return intervalList;
	    }
        
        /// <summary>
        /// reads the supplementary annotation from disk
        /// </summary>
        private static SupplementaryAnnotation Read(ExtendedBinaryReader reader, int referencePos)
	    {
		    
            var sa = new SupplementaryAnnotation(referencePos);
            
            // read the position-specific records
            int numPositionalRecords = reader.ReadByte();
            for (int posIndex = 0; posIndex < numPositionalRecords; posIndex++) ReadPositionalRecord(sa, reader);

            // read the allele-specific records
            int numAlleles = reader.ReadByte();

            for (int alleleIndex = 0; alleleIndex < numAlleles; alleleIndex++)
            {
                // read the allele
                string allele = reader.ReadAsciiString();

                var asa = new SupplementaryAnnotation.AlleleSpecificAnnotation();

                // grab all of the allele-specific records for that allele
                int numAlleleSpecificRecords = reader.ReadByte();
                for (int asIndex = 0; asIndex < numAlleleSpecificRecords; asIndex++)
					ReadAlleleSpecificRecord(asa, reader);

                sa.AlleleSpecificAnnotations[allele] = asa;
            }

            // read cosmic records
            int numCosmic = reader.ReadInt();
            for (int i = 0; i < numCosmic; i++)
            {
                var cosmicItem= new CosmicItem(reader);
                sa.CosmicItems.Add(cosmicItem);
            }

            // reading logic for clinVar items
            int numClinVar = reader.ReadInt();
            for (int i = 0; i < numClinVar; i++)
            {
                var clinVarItem = new ClinVarItem(reader);
                sa.ClinVarItems.Add(clinVarItem);
            }

	        var numCustom = reader.ReadInt();
	        for (int i = 0; i < numCustom; i++)
	        {
		        var customItem = new CustomItem(reader);
				sa.CustomItems.Add(customItem);
	        }
	        return sa;
	    }

        /// <summary>
        /// reads the next allele-specific record and sets the appropriate field
        /// </summary>
        private static void ReadAlleleSpecificRecord(SupplementaryAnnotation.AlleleSpecificAnnotation asa, ExtendedBinaryReader reader)
        {
            SupplementaryAnnotation.SetAlleleSpecificAnnotation(asa, AbstractAnnotationRecord.Read(reader));
        }

        /// <summary>
        /// reads the next positional record and sets the appropriate field
        /// </summary>
        private static void ReadPositionalRecord(SupplementaryAnnotation sa, ExtendedBinaryReader reader)
        {
            sa.SetPositionalAnnotation(AbstractAnnotationRecord.Read(reader));
        }

	    public bool IsRefMinor(int position)
	    {
		    return _index.IsRefMinor((uint) position);
	    }
    }
}
