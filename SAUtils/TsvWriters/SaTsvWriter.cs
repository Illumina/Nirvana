using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using Compression.FileHandling;
using OptimizedCore;

namespace SAUtils.TsvWriters
{
    public sealed class SaTsvWriter : IDisposable
    {
        #region members

        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly TsvIndex _tsvIndex;
        private string _currentChromosome;
        private readonly ISequenceProvider _sequenceProvider;

        #endregion

        #region IDisposable

        private bool _disposed;

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

        public SaTsvWriter(string outputDir, DataSourceVersion dataSourceVersion, string assembly, int schemaVersion, string jsonKey, string vcfKeys,
            bool isAlleleSpecific, ISequenceProvider sequenceProvider, bool isArray = false) : this(outputDir, dataSourceVersion, assembly, schemaVersion, jsonKey, vcfKeys, isAlleleSpecific, isArray)
        {
            _sequenceProvider = sequenceProvider;
        }

        private SaTsvWriter(string outputDir, DataSourceVersion dataSourceVersion, string assembly, int schemaVersion,
            string jsonKey, string vcfKeys,
            bool isAlleleSpecific, bool isArray = false)
        {
            var fileName = jsonKey + "_" + dataSourceVersion.Version.Replace(" ", "_") + ".tsv.gz";

            _bgzipTextWriter = new BgzipTextWriter(Path.Combine(outputDir, fileName));

            _bgzipTextWriter.Write(GetHeader(dataSourceVersion, schemaVersion, assembly, jsonKey, vcfKeys, isAlleleSpecific, isArray));

            _tsvIndex = new TsvIndex(Path.Combine(outputDir, fileName + ".tvi"));
        }


        public static string GetHeader(DataSourceVersion dataSourceVersion, int schemaVersion, string assembly, string jsonKey, string vcfKeys, bool matchByAllele, bool isArray)
        {
            var sb = StringBuilderCache.Acquire();

            sb.Append($"#name={dataSourceVersion.Name}\n");
            if (!string.IsNullOrEmpty(assembly)) sb.Append($"#assembly={assembly}\n");
            sb.Append($"#version={dataSourceVersion.Version}\n");
            sb.Append($"#description={dataSourceVersion.Description}\n");
            var releaseDate = new DateTime(dataSourceVersion.ReleaseDateTicks, DateTimeKind.Utc);
            sb.Append($"#releaseDate={releaseDate:yyyy-MM-dd}\n");
            sb.Append($"#dataVersion={schemaVersion}\n");
            sb.Append($"#schemaVersion={SaTsvCommon.SupplementarySchemaVersion}\n");
            sb.Append($"#matchByAllele={matchByAllele}\n");
            sb.Append($"#isArray={isArray}\n");
            sb.Append($"#jsonKey={jsonKey}\n");
            sb.Append($"#vcfKeys={vcfKeys}\n");
            sb.Append("#CHROM\tPOS\tREF\tALT\tVCF\tJSON\n");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public void AddEntry(string chromosome, int position, string refAllele, string altAllele, string vcfString, List<string> jsonStrings)
        {
            if ((jsonStrings == null || jsonStrings.Count == 0) && string.IsNullOrEmpty(vcfString)) return;

            //validate the vcf reference
            if (!SaUtilsCommon.ValidateReference(chromosome, position, refAllele, _sequenceProvider)) return;

            if (!chromosome.Equals(_currentChromosome))
            {
                _tsvIndex.AddTagPosition(chromosome, _bgzipTextWriter.Position);
               // Console.WriteLine($"chr {chromosome}, filePos: {_bgzipTextWriter.Position}");
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

        
    }
}
