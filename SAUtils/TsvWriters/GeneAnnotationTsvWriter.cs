using System;
using System.Collections.Generic;
using System.IO;
using CommonUtilities;
using Compression.Utilities;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class GeneAnnotationTsvWriter : IDisposable
    {
        #region members
        private readonly StreamWriter _writer;
        #endregion

        public GeneAnnotationTsvWriter(string outputDirectory, DataSourceVersion dataSourceVersion, string assembly, int dataVersion, string keyName,
            bool isArray)
        {
            var fileName = keyName + "_" + dataSourceVersion.Version.Replace(" ", "_") + ".gene.tsv.gz";
            _writer = GZipUtilities.GetStreamWriter(Path.Combine(outputDirectory, fileName));

            _writer.Write(GetHeader(dataSourceVersion, dataVersion, assembly, keyName, isArray));
        }

        private static string GetHeader(DataSourceVersion dataSourceVersion, int dataVersion, string assembly, string keyName, bool isArray)
        {
            var sb = StringBuilderCache.Acquire();

            sb.Append($"#name={dataSourceVersion.Name}\n");
            sb.Append($"#assembly={assembly}\n");
            sb.Append($"#version={dataSourceVersion.Version}\n");
            sb.Append($"#description={dataSourceVersion.Description}\n");
            var releaseDate = new DateTime(dataSourceVersion.ReleaseDateTicks, DateTimeKind.Utc);
            sb.Append($"#releaseDate={releaseDate:yyyy-MM-dd}\n");
            sb.Append($"#dataVersion={dataVersion}\n");
            sb.Append($"#schemaVersion={SaTsvCommon.SupplementarySchemaVersion}\n");
            sb.Append($"#isArray={isArray}\n");
            sb.Append($"#keyName={keyName}\n");
            sb.Append("#GENESYMBOL\tJSON\n");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public void AddEntry(string geneSymbol, List<string> jsonStrings)
        {
            if (jsonStrings == null || jsonStrings.Count == 0) return;
            _writer.Write($"{geneSymbol}\t{string.Join("\t",jsonStrings)}\n");
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
