using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling
{
    public sealed class NirvanaDatabaseReader : IDisposable
    {
        #region members

        private readonly BinaryReader _binaryReader;
        private readonly ExtendedBinaryReader _reader;
        private readonly string _cachePath;

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
                    _binaryReader.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

        // constructor
        public NirvanaDatabaseReader(string dbPath)
        {
            _cachePath = dbPath;

            // open the database file
            _binaryReader = new BinaryReader(FileUtilities.GetFileStream(dbPath));
            _reader       = new ExtendedBinaryReader(_binaryReader);
        }

        // constructor
        public NirvanaDatabaseReader(Stream stream)
        {
            _cachePath = "(resource)";

            // open the database file
            _binaryReader = new BinaryReader(stream);
            _reader       = new ExtendedBinaryReader(_binaryReader);
        }

        /// <summary>
        /// check if the section guard is in place
        /// </summary>
        private void CheckGuard()
        {
            uint observedGuard = _binaryReader.ReadUInt32();
            if (observedGuard != NirvanaDatabaseCommon.GuardInt)
            {
                throw new GeneralException($"Expected a guard integer ({NirvanaDatabaseCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        /// <summary>
        /// checks if the header is good
        /// </summary>
        private static NirvanaDatabaseHeader GetHeader(BinaryReader reader, string dbPath, bool checkHeader = true)
        {
            string header = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(NirvanaDatabaseCommon.Header.Length));

            ushort schemaVersion                      = reader.ReadUInt16();
            ushort dataVersion                        = reader.ReadUInt16();
            long creationTimeTicks                    = reader.ReadInt64();
            long vepReleaseTicks                      = reader.ReadInt64();
            ushort vepVersion                         = reader.ReadUInt16();
            GenomeAssembly genomeAssembly             = (GenomeAssembly)reader.ReadByte();
            string referenceSequenceName              = reader.ReadString();
            TranscriptDataSource transcriptDataSource = (TranscriptDataSource)reader.ReadByte();

            if (checkHeader)
            {
                if ((header != NirvanaDatabaseCommon.Header) || (schemaVersion != NirvanaDatabaseCommon.SchemaVersion))
                {
                    throw new GeneralException($"The header check failed for the Nirvana VEP cache file ({dbPath}): ID: exp: {NirvanaDatabaseCommon.Header} obs: {header}, schema version: exp: {NirvanaDatabaseCommon.SchemaVersion} obs: {schemaVersion}");
                }
            }

            return new NirvanaDatabaseHeader(referenceSequenceName, creationTimeTicks, vepReleaseTicks, vepVersion,
                schemaVersion, dataVersion, genomeAssembly, transcriptDataSource);
        }

        /// <summary>
        /// checks if the footer is good
        /// </summary>
        private void CheckFooter()
        {
            const string expectedFooter = "EOF";
            string footer               = _reader.ReadAsciiString();

            if (footer != expectedFooter)
            {
                throw new GeneralException($"The footer check failed for the Nirvana VEP cache file ({_cachePath}): ID: exp: {expectedFooter} obs: {footer}");
            }
        }

        /// <summary>
        /// returns the header from the specified Nirvana database file
        /// </summary>
        public static NirvanaDatabaseHeader GetHeader(string ndbPath, bool checkHeader = true)
        {
            NirvanaDatabaseHeader header;

            using (var reader = new BinaryReader(FileUtilities.GetFileStream(ndbPath))) 
            {
                header = GetHeader(reader, ndbPath, checkHeader);
            }

            return header;
        }

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public void PopulateData(NirvanaDataStore dataStore, IntervalTree<Transcript> transcriptIntervalTree, IntervalTree<RegulatoryFeature> regulatoryIntervalTree = null,IntervalTree<Gene> geneIntervalTree = null )
        {
            dataStore.Clear();
            transcriptIntervalTree.Clear();
            regulatoryIntervalTree?.Clear();
			geneIntervalTree?.Clear();

            // check the header
            dataStore.CacheHeader = GetHeader(_binaryReader, _cachePath);

            // retrieve the counts for each data type
            int numCoordinateMaps     = _reader.ReadInt();
            int numExons              = _reader.ReadInt();
            int numGenes              = _reader.ReadInt();
            int numIntrons            = _reader.ReadInt();
            int numMicroRnas          = _reader.ReadInt();
            int numSifts              = _reader.ReadInt();
            int numPolyPhens          = _reader.ReadInt();
            int numRegulatoryFeatures = _reader.ReadInt();
            int numTranscripts        = _reader.ReadInt();

            CheckGuard();

            // read the genomic-cDNA coordinate maps
            dataStore.CdnaCoordinateMaps = new List<CdnaCoordinateMap>(numCoordinateMaps);

            for (int coordinateMapIndex = 0; coordinateMapIndex < numCoordinateMaps; coordinateMapIndex++)
            {
                dataStore.CdnaCoordinateMaps.Add(CdnaCoordinateMap.Read(_reader));
            }

            CheckGuard();

            // read exons
            dataStore.Exons = new List<Exon>(numExons);

            for (int exonIndex = 0; exonIndex < numExons; exonIndex++)
            {
                dataStore.Exons.Add(Exon.Read(_reader));
            }

            CheckGuard();

            // read the genes
            dataStore.Genes = new List<Gene>(numGenes);

            for (int geneIndex = 0; geneIndex < numGenes; geneIndex++)
            {
                dataStore.Genes.Add(Gene.Read(_reader));
            }

            CheckGuard();

            // read introns
            dataStore.Introns = new List<Intron>(numIntrons);

            for (int intronIndex = 0; intronIndex < numIntrons; intronIndex++)
            {
                dataStore.Introns.Add(Intron.Read(_reader));
            }

            CheckGuard();

            // read miRNAs
            dataStore.MicroRnas = new List<MicroRna>(numMicroRnas);

            for (int microRnaIndex = 0; microRnaIndex < numMicroRnas; microRnaIndex++)
            {
                dataStore.MicroRnas.Add(MicroRna.Read(_reader));
            }

            CheckGuard();

            // read SIFT objects
            if (numSifts > 0)
            {
                dataStore.Sifts = new List<Sift>();
                for (int siftIndex = 0; siftIndex < numSifts; siftIndex++)
                {
                    dataStore.Sifts.Add(Sift.Read(_reader));
                }
            }

            CheckGuard();

            // read PolyPhen objects
            if (numPolyPhens > 0)
            {
                dataStore.PolyPhens = new List<PolyPhen>();
                for (int polyPhenIndex = 0; polyPhenIndex < numPolyPhens; polyPhenIndex++)
                {
                    dataStore.PolyPhens.Add(PolyPhen.Read(_reader));
                }
            }

            CheckGuard();

            // read regulatory features
            dataStore.RegulatoryFeatures = new List<RegulatoryFeature>(numRegulatoryFeatures);

            for (int regulatoryIndex = 0; regulatoryIndex < numRegulatoryFeatures; regulatoryIndex++)
            {
                dataStore.RegulatoryFeatures.Add(RegulatoryFeature.Read(_reader));
            }

            CheckGuard();

            // preload the transcripts
            dataStore.Transcripts = new List<Transcript>(numTranscripts);

            for (int transcriptIndex = 0; transcriptIndex < numTranscripts; transcriptIndex++)
            {
                dataStore.Transcripts.Add(Transcript.Read(_reader, dataStore.CdnaCoordinateMaps, dataStore.Exons,
                    dataStore.Introns, dataStore.MicroRnas, dataStore.Sifts, dataStore.PolyPhens));
            }

            // check the footer
            CheckFooter();

            // populate the interval trees
            foreach (var transcript in dataStore.Transcripts)
            {
                transcriptIntervalTree.Add(new IntervalTree<Transcript>.Interval(dataStore.CacheHeader.ReferenceSequenceName, transcript.Start, transcript.End, transcript));
            }

            if (regulatoryIntervalTree != null)
            {
                foreach (var regulatoryFeature in dataStore.RegulatoryFeatures)
                {
                    regulatoryIntervalTree.Add(new IntervalTree<RegulatoryFeature>.Interval(dataStore.CacheHeader.ReferenceSequenceName, regulatoryFeature.Start, regulatoryFeature.End, regulatoryFeature));
                }
            }

	        if (geneIntervalTree != null)
	        {
		        foreach (var gene in dataStore.Genes)
		        {
			        geneIntervalTree.Add(new IntervalTree<Gene>.Interval(dataStore.CacheHeader.ReferenceSequenceName,gene.Start,gene.End,gene));
		        }
	        }
        }
    }
}
