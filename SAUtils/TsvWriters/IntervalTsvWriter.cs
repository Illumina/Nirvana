using System;
using System.IO;
using System.IO.Compression;
using Compression.FileHandling;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.TsvWriters
{
    public sealed class IntervalTsvWriter : IDisposable
    {
        #region members

        private readonly BgzipTextWriter _bgzipTextWriter;
		private readonly TsvIndex _tsvIndex;
		private string _currentChromosome;
		#endregion

		public IntervalTsvWriter(string outputPath, IDataSourceVersion dataSourceVersion, string assembly, int dataVersion, string keyName,
			ReportFor reportingFor)
		{
			var fileName = keyName + "_" + dataSourceVersion.Version.Replace(" ","_") + ".interval.tsv.gz";
			_bgzipTextWriter = new BgzipTextWriter(new BlockGZipStream(FileUtilities.GetCreateStream(Path.Combine(outputPath, fileName)), CompressionMode.Compress));

			_bgzipTextWriter.Write(GetHeader(dataSourceVersion, dataVersion, assembly, keyName, reportingFor ));
			_tsvIndex = new TsvIndex(Path.Combine(outputPath, fileName) + ".tvi");
		}

		private static string GetHeader(IDataSourceVersion dataSourceVersion, int dataVersion, string assembly, string keyName, ReportFor reportingFor)
		{
			var sb = StringBuilderCache.Acquire();
			
			sb.Append($"#name={dataSourceVersion.Name}\n");
			sb.Append($"#assembly={assembly}\n");
			sb.Append($"#version={dataSourceVersion.Version}\n");
			sb.Append($"#description={dataSourceVersion.Description}\n");
			var releaseDate = new DateTime(dataSourceVersion.ReleaseDateTicks, DateTimeKind.Utc);
			sb.Append($"#releaseDate={releaseDate:yyyy-MM-dd}\n");
			sb.Append($"#dataVersion={dataVersion}\n");
			sb.Append($"#schemaVersion={JsonCommon.SupplementarySchemaVersion}\n");
			sb.Append($"#reportFor={reportingFor}\n");
			sb.Append($"#keyName={keyName}\n");
			sb.Append("#CHROM\tSTART\tEND\tJSON\n");
		    return StringBuilderCache.GetStringAndRelease(sb);
		}

		public void AddEntry(string chromosome, int start, int end, string jsonString)
		{
			if (string.IsNullOrEmpty(jsonString)) return;

			if (chromosome != _currentChromosome)
			{
				_tsvIndex.AddTagPosition(chromosome, _bgzipTextWriter.Position);
				_currentChromosome = chromosome;
			}

			_bgzipTextWriter.Write($"{chromosome}\t{start}\t{end}\t{jsonString}\n");
		}

        public void Dispose()
        {
            _bgzipTextWriter.Dispose();
            _tsvIndex.Dispose();
        }
    }
}
