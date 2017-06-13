using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace SAUtils.TsvWriters
{
    public class SaTsvWriter : IDisposable
    {
        #region members

        private const int ReferenceWindow = 10;
        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly TsvIndex _tsvIndex;
        private string _currentChromosome;
        private readonly DataFileManager _dataFileManager;
        private readonly ICompressedSequence _compressedSequence;

        #endregion

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

        public SaTsvWriter(string outputPath, DataSourceVersion dataSourceVersion, string assembly, int schemaVersion, string jsonKey, string vcfKeys,
            bool isAlleleSpecific, string compressedReferencePath, bool isArray = false) : this(outputPath, dataSourceVersion, assembly, schemaVersion, jsonKey, vcfKeys, isAlleleSpecific, isArray)
        {
            _compressedSequence = new CompressedSequence();
            var sequenceReader  = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), _compressedSequence);
            _dataFileManager    = new DataFileManager(sequenceReader, _compressedSequence);
        }

        private SaTsvWriter(string outputPath, DataSourceVersion dataSourceVersion, string assembly, int schemaVersion,
            string jsonKey, string vcfKeys,
            bool isAlleleSpecific, bool isArray = false)
        {
            var fileName = jsonKey + "_" + dataSourceVersion.Version.Replace(" ", "_") + ".tsv.gz";

            _bgzipTextWriter = new BgzipTextWriter(Path.Combine(outputPath, fileName));

            _bgzipTextWriter.Write(GetHeader(dataSourceVersion, schemaVersion, assembly, jsonKey, vcfKeys, isAlleleSpecific, isArray));

            _tsvIndex = new TsvIndex(Path.Combine(outputPath, fileName + ".tvi"));
        }

        private string GetHeader(DataSourceVersion dataSourceVersion, int schemaVersion, string assembly, string jsonKey, string vcfKeys, bool matchByAllele, bool isArray)
        {
            var sb = new StringBuilder();

            sb.Append($"#name={dataSourceVersion.Name}\n");
            sb.Append($"#assembly={assembly}\n");
            sb.Append($"#version={dataSourceVersion.Version}\n");
            sb.Append($"#description={dataSourceVersion.Description}\n");
            var releaseDate = new DateTime(dataSourceVersion.ReleaseDateTicks, DateTimeKind.Utc);
            sb.Append($"#releaseDate={releaseDate:yyyy-MM-dd}\n");
            sb.Append($"#dataVersion={schemaVersion}\n");
            sb.Append($"#schemaVersion={JsonCommon.SupplementarySchemaVersion}\n");
            sb.Append($"#matchByAllele={matchByAllele}\n");
            sb.Append($"#isArray={isArray}\n");
            sb.Append($"#jsonKey={jsonKey}\n");
            sb.Append($"#vcfKeys={vcfKeys}\n");
            sb.Append("#CHROM\tPOS\tREF\tALT\tVCF\tJSON\n");
            return sb.ToString();
        }

        public void AddEntry(string chromosome, int position, string refAllele, string altAllele, string vcfString, List<string> jsonStrings)
        {
            if ((jsonStrings == null || jsonStrings.Count == 0) && string.IsNullOrEmpty(vcfString)) return;

            //validate the vcf reference
            if (!ValidateReference(chromosome, position, refAllele)) return;

            if (!chromosome.Equals(_currentChromosome))
            {
                _tsvIndex.AddTagPosition(chromosome, _bgzipTextWriter.Position);
                Console.WriteLine($"chr {chromosome}, filePos: {_bgzipTextWriter.Position}");
                _currentChromosome = chromosome;
            }

            refAllele = string.IsNullOrEmpty(refAllele) ? "-" : refAllele;
            altAllele = string.IsNullOrEmpty(altAllele) ? "-" : altAllele;

            _bgzipTextWriter.Write($"{chromosome}\t{position}\t{refAllele}\t{altAllele}\t{vcfString}");

            if (jsonStrings != null)
            {
                foreach (var jsonString in jsonStrings)
                {
                    _bgzipTextWriter.Write($"\t{jsonString}");
                }
            }

            _bgzipTextWriter.Write("\n");
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
