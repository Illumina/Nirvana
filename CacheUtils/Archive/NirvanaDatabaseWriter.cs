using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;

namespace CacheUtils.Archive
{
    public sealed class NirvanaDatabaseWriter : IDisposable
    {
        #region members

        private readonly BinaryWriter _binaryWriter;
        private readonly ExtendedBinaryWriter _writer;

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
                    _binaryWriter.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

        // constructor
        public NirvanaDatabaseWriter(string dbPath)
        {
            // open the database file
            _binaryWriter = new BinaryWriter(new FileStream(dbPath, FileMode.Create));
            _writer = new ExtendedBinaryWriter(_binaryWriter);
        }

        /// <summary>
        /// writes the annotations to the current database file
        /// </summary>
        public void Write(NirvanaDataStore dataStore, string currentRefSeq)
        {
            // create index dictionaries for each data type
            var cdnaMapIndices = new Dictionary<CdnaCoordinateMap, int>();
            for (int cdnaMapIndex = 0; cdnaMapIndex < dataStore.CdnaCoordinateMaps.Count; cdnaMapIndex++) cdnaMapIndices[dataStore.CdnaCoordinateMaps[cdnaMapIndex]] = cdnaMapIndex;

            var exonIndices = new Dictionary<Exon, int>();
            for (int exonIndex = 0; exonIndex < dataStore.Exons.Count; exonIndex++) exonIndices[dataStore.Exons[exonIndex]] = exonIndex;

            var intronIndices = new Dictionary<Intron, int>();
            for (int intronIndex = 0; intronIndex < dataStore.Introns.Count; intronIndex++) intronIndices[dataStore.Introns[intronIndex]] = intronIndex;

            var microRnaIndices = new Dictionary<MicroRna, int>();
            for (int microRnaIndex = 0; microRnaIndex < dataStore.MicroRnas.Count; microRnaIndex++) microRnaIndices[dataStore.MicroRnas[microRnaIndex]] = microRnaIndex;

            var siftIndices = new Dictionary<Sift, int>();
            for (int siftIndex = 0; siftIndex < dataStore.Sifts.Count; siftIndex++)
            {
                int oldIndex;
                if (siftIndices.TryGetValue(dataStore.Sifts[siftIndex], out oldIndex))
                {
                    throw new GeneralException("Found a duplicate in the sift indexes: " +
                                                   dataStore.Sifts[siftIndex].PredictionData.Length);
                }

                siftIndices[dataStore.Sifts[siftIndex]] = siftIndex;
            }

            var polyPhenIndices = new Dictionary<PolyPhen, int>();
            for (int polyPhenIndex = 0; polyPhenIndex < dataStore.PolyPhens.Count; polyPhenIndex++) polyPhenIndices[dataStore.PolyPhens[polyPhenIndex]] = polyPhenIndex;

            // write the header
            _binaryWriter.Write(System.Text.Encoding.ASCII.GetBytes(NirvanaDatabaseCommon.Header));
            _binaryWriter.Write(NirvanaDatabaseCommon.SchemaVersion);
            _binaryWriter.Write(NirvanaDatabaseCommon.DataVersion);
            _binaryWriter.Write(dataStore.CacheHeader.CreationTimeTicks);
            _binaryWriter.Write(dataStore.CacheHeader.VepReleaseTicks);
            _binaryWriter.Write(dataStore.CacheHeader.VepVersion);
            _binaryWriter.Write((byte)dataStore.CacheHeader.GenomeAssembly);
            _binaryWriter.Write(currentRefSeq);
            _binaryWriter.Write((byte)dataStore.CacheHeader.TranscriptDataSource);

            // write how many of each data type we have
            _writer.WriteInt(dataStore.CdnaCoordinateMaps.Count);
            _writer.WriteInt(dataStore.Exons.Count);
            _writer.WriteInt(dataStore.Genes.Count);
            _writer.WriteInt(dataStore.Introns.Count);
            _writer.WriteInt(dataStore.MicroRnas.Count);
            _writer.WriteInt(dataStore.Sifts.Count);
            _writer.WriteInt(dataStore.PolyPhens.Count);
            _writer.WriteInt(dataStore.RegulatoryFeatures.Count);
            _writer.WriteInt(dataStore.Transcripts.Count);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the cDNA coordinate maps
            foreach (var cdnaMap in dataStore.CdnaCoordinateMaps) cdnaMap.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the exons
            foreach (var exon in dataStore.Exons) exon.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the genes
            foreach (var gene in dataStore.Genes) gene.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the introns
            foreach (var intron in dataStore.Introns) intron.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the miRNAs
            foreach (var microRna in dataStore.MicroRnas) microRna.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the SIFT objects
            foreach (var sift in dataStore.Sifts) sift.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the PolyPhen objects
            foreach (var polyPhen in dataStore.PolyPhens) polyPhen.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the regulatory features
            foreach (var regulatoryFeature in dataStore.RegulatoryFeatures) regulatoryFeature.Write(_writer);

            _binaryWriter.Write(NirvanaDatabaseCommon.GuardInt);

            // write the transcripts
            foreach (var transcript in dataStore.Transcripts)
            {
                transcript.Write(_writer, cdnaMapIndices, exonIndices, intronIndices, microRnaIndices,
                    siftIndices, polyPhenIndices);
            }

            // write the EOF bytes
            const string footer = "EOF";
            _writer.WriteAsciiString(footer);
        }
    }
}
