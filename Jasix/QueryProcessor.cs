using System;
using System.Collections.Generic;
using System.IO;
using Jasix.DataStructures;
using Newtonsoft.Json;
using VariantAnnotation.Algorithms;

namespace Jasix
{
	public sealed class QueryProcessor:IDisposable
	{
		#region members
		private readonly StreamReader _jsonReader;
	    private readonly StreamWriter _writer;
		private readonly Stream _indexStream;
		private readonly JasixIndex _jasixIndex;

	    #endregion

		#region IDisposable

	    private bool _disposed;

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
				_jsonReader.Dispose();
				_indexStream.Dispose();
                _writer.Dispose();
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;
			// Free any other managed objects here.

		}
		#endregion

		public QueryProcessor(StreamReader jsonReader, Stream indexStream, StreamWriter writer=null)
		{
			_jsonReader  = jsonReader;
		    _writer      = writer ?? new StreamWriter(Console.OpenStandardOutput());
			_indexStream = indexStream;
			_jasixIndex  = new JasixIndex(_indexStream);

		}

		public string GetHeader()
		{
			return _jasixIndex.HeaderLine;
		}

		
		public void PrintChromosomeList()
		{
			foreach (var chrName in _jasixIndex.GetChromosomeList())
			{
                _writer.WriteLine(chrName);
			}
		}

		public void PrintHeader()
		{

			var headerString = _jasixIndex.HeaderLine;
			_writer.WriteLine("{" + headerString+"}");
		}

		
		public void ProcessQuery(IEnumerable<string> queryStrings, bool printHeader = false)
		{
			_writer.Write("{");
			if (printHeader)
			{
				var headerString = _jasixIndex.HeaderLine;
				_writer.Write(headerString + ",");
			}
			Utilities.PrintQuerySectionOpening(JasixCommons.SectionToIndex, _writer);

		    foreach (var queryString in queryStrings)
            {
                var query = Utilities.ParseQuery(queryString);
                if (!_jasixIndex.ContainsChr(query.Item1)) continue;
                var needComma = PrintLargeVariantsExtendingIntoQuery(query);
                PrintAllVariantsFromQueryBegin(query, needComma);
            }

            Utilities.PrintQuerySectionClosing(_writer);
			_writer.WriteLine("}");

		}

		private void PrintAllVariantsFromQueryBegin((string, int, int) query, bool needComma)
		{
			foreach (var line in ReadOverlappingJsonLines(query))
			{
				Utilities.PrintJsonEntry(line, needComma, _writer);
				needComma = true;
			}

		}
		private bool PrintLargeVariantsExtendingIntoQuery((string, int, int) query)
		{
			var needComma = false;
			foreach (var line in ReadJsonLinesExtendingInto(query))
			{
				Utilities.PrintJsonEntry(line, needComma, _writer);
				needComma = true;
			}

			return needComma;
		}

		internal IEnumerable<string> ReadJsonLinesExtendingInto((string Chr, int Start, int End) query)
		{
			// query for large variants like chr1:100-99 returns all overlapping large variants that start before 100
			var locations = _jasixIndex.LargeVariantPositions(query.Chr, query.Start, query.Start - 1);

			if (locations == null || locations.Length == 0) yield break;

			foreach (var location in locations)
			{
				RepositionReader(location);

				string line;
				if ((line = _jsonReader.ReadLine()) == null) continue;
				line = line.TrimEnd(',');

				yield return line;

			}
		}

		private void RepositionReader(long location)
		{
			_jsonReader.DiscardBufferedData();
			_jsonReader.BaseStream.Position = location;
		}

		internal IEnumerable<string> ReadOverlappingJsonLines((string Chr, int Start, int End) query)
		{
			var position = _jasixIndex.GetFirstVariantPosition(query.Chr, query.Start, query.End);

			if (position == -1) yield break;

			RepositionReader(position);

			string line;
			while ((line = _jsonReader.ReadLine()) != null && !line.StartsWith("]"))
				//The array of positions entry end with "]," Going past it will cause the json parser to crash
			{
				line = line.TrimEnd(',');
                if (string.IsNullOrEmpty(line)) continue;
			    
				JsonSchema jsonEntry;
				try
				{
					jsonEntry = JsonConvert.DeserializeObject<JsonSchema>(line);
				}
				catch (Exception)
				{
					Console.WriteLine($"Error in line:\n{line}");
					throw;
				}
                
				if (jsonEntry.chromosome != query.Item1) break;

				if (jsonEntry.Start > query.Item3) break;

				if (!jsonEntry.Overlaps(query.Item2, query.Item3)) continue;
				// if there is an SV that starts before the query start that is printed by the large variant printer
				if (Utilities.IsLargeVariant(jsonEntry.Start, jsonEntry.End) && jsonEntry.Start < query.Item2) continue;
				yield return line;
			}
		}

		
	}
}
