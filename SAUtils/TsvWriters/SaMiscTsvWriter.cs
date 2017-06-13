using System;
using System.IO;
using System.Text;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace SAUtils.TsvWriters
{
    public class SaMiscTsvWriter:IDisposable
    {

        private const int ReferenceWindow = 10;
        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly TsvIndex _tsvIndex;
        private string _currentChromosome;
        private readonly DataFileManager _dataFileManager;
        private readonly ICompressedSequence _compressedSequence;

        #region IDisposable

        bool _disposed;

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
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                _bgzipTextWriter.Dispose();
                _tsvIndex.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion


        public SaMiscTsvWriter(string outputPath,DataSourceVersion dataSourceVersion, string assembly,string keyName, string compressedReferencePath)
        {
            var fileName = keyName + "_" + dataSourceVersion.Version.Replace(" ", "_") + ".misc.tsv.gz";
            _bgzipTextWriter = new BgzipTextWriter(Path.Combine(outputPath, fileName));

            _bgzipTextWriter.Write(GetHeader(dataSourceVersion,assembly));

            _tsvIndex = new TsvIndex(Path.Combine(outputPath, fileName + ".tvi"));

            _compressedSequence = new CompressedSequence();
            var sequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), _compressedSequence);
            _dataFileManager = new DataFileManager(sequenceReader, _compressedSequence);
        }

        private string GetHeader(DataSourceVersion dataSourceVersion, string assembly)
        {
            var sb = new StringBuilder();

            sb.Append($"#name={dataSourceVersion.Name}\n");
            sb.Append($"#assembly={assembly}\n");
            sb.Append($"#version={dataSourceVersion.Version}\n");
            sb.Append($"#description={dataSourceVersion.Description}\n");
            sb.Append("#CHROM\tPOS\tGlobalMajorAllele\n");
            return sb.ToString();
        }


        public void AddEntry(string chromosome, int position, string globalMajorAllele,string refAllele)
        {
            if (globalMajorAllele == null)
            {
                throw new Exception($"no global major allele for {chromosome}:{position}");
            }

            //validate the vcf reference
            if (!ValidateReference(chromosome, position, refAllele)) return;

            if (!chromosome.Equals(_currentChromosome))
            {
                _tsvIndex.AddTagPosition(chromosome, _bgzipTextWriter.Position);
                //Console.WriteLine($"chr {chromosome}, filePos: {_bgzipTextWriter.Position}");
                _currentChromosome = chromosome;
            }

            _bgzipTextWriter.Write($"{chromosome}\t{position}\t{globalMajorAllele}\n");

        }


        private bool ValidateReference(string chromosome, int position, string refAllele)
        {
            if (_dataFileManager == null) return true;

            var refIndex = _compressedSequence.Renamer.GetReferenceIndex(chromosome);
            if (refIndex == ChromosomeRenamer.UnknownReferenceIndex) return false;
            _dataFileManager.LoadReference(refIndex, () => { });
            var refSequence = _compressedSequence.Substring(position - 1, ReferenceWindow);
            return SupplementaryAnnotationUtilities.ValidateRefAllele(refAllele, refSequence);
        }


    }
}