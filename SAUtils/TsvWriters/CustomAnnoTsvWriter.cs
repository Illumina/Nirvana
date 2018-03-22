using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
	public sealed class CustomAnnoTsvWriter: ISaItemTsvWriter
	{
		#region members
		private readonly SaTsvWriter _writer;
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
				_writer.Dispose();
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;
			// Free any other managed objects here.

		}
		#endregion

		public CustomAnnoTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, bool isPositional, ISequenceProvider sequenceProvider)
		{
			Console.WriteLine(version.ToString());

			_writer = new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.CustomItemSchemaVersion, version.Name, null, !isPositional,sequenceProvider, true);
		}

		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			if (saItems == null) return;
			var customItems = new List<CustomItem>();
			foreach (var item in saItems)
			{
			    if (!(item is CustomItem customItem))
					throw new InvalidDataException("Expected customItem list!!");
				customItems.Add(customItem);
			}

			if (customItems.Count == 0) return;
			var alleleGroupedItems = customItems.GroupBy(x => x.AlternateAllele);
			foreach (var groupedItem in alleleGroupedItems)
			{
				var jsonStrings = groupedItem.Select(x => x.GetJsonString()).ToList();

				// since the reference allele for different items in the group may be different, we only use the first base as it is supposed to be the common padding base.
				_writer.AddEntry(groupedItem.First().Chromosome.EnsemblName,
					groupedItem.First().Start,
					groupedItem.First().ReferenceAllele,
					groupedItem.Key, null, jsonStrings);
			}

		}
	}
}
