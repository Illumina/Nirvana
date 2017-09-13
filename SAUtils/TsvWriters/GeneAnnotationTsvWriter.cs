using Compression.FileHandling;
using SAUtils.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    class GeneAnnotationTsvWriter : IDisposable
    {
        #region members
        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly TsvIndex _tsvIndex;
        private string _currentChromosome;
        #endregion

        #region IDisposable

        bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        public GeneAnnotationTsvWriter(string outputDirectory, DataSourceVersion dataSourceVersion, string assembly, int dataVersion, string keyName,
            bool isArray)
        {
            var fileName = keyName + "_" + dataSourceVersion.Version.Replace(" ", "_") + ".gene.tsv.gz";
            _bgzipTextWriter = new BgzipTextWriter(Path.Combine(outputDirectory, fileName));

            _bgzipTextWriter.Write(GetHeader(dataSourceVersion, dataVersion, assembly, keyName, isArray));
            _tsvIndex = new TsvIndex(Path.Combine(outputDirectory, fileName) + ".tvi");
        }

        private string GetHeader(DataSourceVersion dataSourceVersion, int dataVersion, string assembly, string keyName, bool isArray)
        {
            var sb = new StringBuilder();

            sb.Append($"#name={dataSourceVersion.Name}\n");
            sb.Append($"#assembly={assembly}\n");
            sb.Append($"#version={dataSourceVersion.Version}\n");
            sb.Append($"#description={dataSourceVersion.Description}\n");
            var releaseDate = new DateTime(dataSourceVersion.ReleaseDateTicks, DateTimeKind.Utc);
            sb.Append($"#releaseDate={releaseDate:yyyy-MM-dd}\n");
            sb.Append($"#dataVersion={dataVersion}\n");
            sb.Append($"#schemaVersion={SaTSVCommon.SupplementarySchemaVersion}\n");
            sb.Append($"#isArray={isArray}\n");
            sb.Append($"#keyName={keyName}\n");
            sb.Append("#GENESYMBOL\tJSON\n");
            return sb.ToString();
        }

        public void AddEntry(string geneSymbol, List<string> jsonStrings)
        {
            if (jsonStrings == null || jsonStrings.Count == 0) return;
            _bgzipTextWriter.Write($"{geneSymbol}\t{String.Join("\t",jsonStrings)}\n");
        }
    }
}
