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
	public sealed class CosmicTsvWriter:ISaItemTsvWriter
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

		public CosmicTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider)
		{
			Console.WriteLine(version.ToString());

			_writer = new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.CosmicSchemaVersion, InterimSaCommon.CosmicTag, InterimSaCommon.CosmicVcfTag, false, sequenceProvider, true);

		}

		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			if (saItems == null) return;
			var cosmicItems = new List<CosmicItem>();
			foreach (var item in saItems)
			{
			    if (!(item is CosmicItem cosmicItem))
					throw new InvalidDataException("Expected CosmicItem list!!");
				cosmicItems.Add(cosmicItem);
			}

			if (cosmicItems.Count == 0) return;

			var combineStudies = CombineStudies(cosmicItems);
			var alleleGroupDict = GroupByAltAllele(combineStudies);
			foreach (var kvp in alleleGroupDict)
			{
			    var refAllele = kvp.Key.Item1;
				var altAllele = kvp.Key.Item2;

				var groupedItems = kvp.Value;
				var vcfString = string.Join(",", groupedItems.OrderBy(x=>x.Id).Select(x => x.Id));
				var jsonStrings = groupedItems.OrderBy(x => x.Id).Select(x => x.GetJsonString()).ToList();

				var firstItem = groupedItems[0];
				_writer.AddEntry(firstItem.Chromosome.EnsemblName,
					firstItem.Start,
					refAllele,
					altAllele, vcfString, jsonStrings);
			}

		}

		private static Dictionary<(string ReferenceAllele, string AlternateAllele), List<CosmicItem>> GroupByAltAllele(List<CosmicItem> cosmicItems)
		{
			var groups = new Dictionary<(string, string), List<CosmicItem>>();

			foreach (var cosmicItem in cosmicItems)
			{
			    var alleleTuple = (cosmicItem.ReferenceAllele, cosmicItem.AlternateAllele);
				if (groups.ContainsKey(alleleTuple))
					groups[alleleTuple].Add(cosmicItem);
				else groups[alleleTuple] = new List<CosmicItem> { cosmicItem };
			}

			return groups;
		}

		private static List<CosmicItem> CombineStudies(List<CosmicItem> cosmicItems)
		{
			var cosmicDict = new Dictionary<string, CosmicItem>();

			foreach (var cosmicItem in cosmicItems)
			{
				if (cosmicDict.ContainsKey(cosmicItem.Id))
				{
					cosmicDict[cosmicItem.Id].MergeStudies(cosmicItem);
				}
				else cosmicDict[cosmicItem.Id] = cosmicItem;
			}

			return cosmicDict.Values.ToList();
		}
	}
}
