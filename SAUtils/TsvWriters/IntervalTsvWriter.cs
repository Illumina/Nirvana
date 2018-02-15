using System;
using System.IO;
using CommonUtilities;
using Compression.FileHandling;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
	public sealed class IntervalTsvWriter:IDisposable
	{
		#region members

		private readonly BgzipTextWriter _bgzipTextWriter;
		private readonly TsvIndex _tsvIndex;
		private string _currentChromosome;
		#endregion

		#region IDisposable

	    private bool _disposed ;

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

		public IntervalTsvWriter(string outputPath, DataSourceVersion dataSourceVersion, string assembly, int dataVersion, string keyName,
			ReportFor reportingFor)
		{
			var fileName = keyName + "_" + dataSourceVersion.Version.Replace(" ","_") + ".interval.tsv.gz";
			_bgzipTextWriter = new BgzipTextWriter(Path.Combine(outputPath, fileName));

			_bgzipTextWriter.Write(GetHeader(dataSourceVersion, dataVersion, assembly, keyName, reportingFor ));
			_tsvIndex = new TsvIndex(Path.Combine(outputPath, fileName) + ".tvi");
		}

		private static string GetHeader(DataSourceVersion dataSourceVersion, int dataVersion, string assembly, string keyName, ReportFor reportingFor)
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
	}
}
