using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
	public class DbsnpGaTsvWriter:ISaItemTsvWriter
	{

		#region members
		private readonly SaTsvWriter _dbsnpWriter;
		private readonly SaTsvWriter _globalAlleleWriter;
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
				_dbsnpWriter.Dispose();
				_globalAlleleWriter.Dispose();
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;
			// Free any other managed objects here.

		}
		#endregion

		
		public DbsnpGaTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider)
		{

			Console.WriteLine(version.ToString());

			_dbsnpWriter = new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.DbSnpSchemaVersion, InterimSaCommon.DbsnpTag, null, true,sequenceProvider);

			_globalAlleleWriter = new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.DbSnpSchemaVersion, InterimSaCommon.GlobalAlleleTag, "GMAF", false, sequenceProvider);

		}

		
		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			var itemsByAllele = GetItemsByAllele(saItems);
			WriteDbsnpTsv(itemsByAllele);
			WriteGlobalAlleleTsv(itemsByAllele);
		}

		private void WriteGlobalAlleleTsv(Dictionary<Tuple<string, string>, List<DbSnpItem>> itemsByAllele)
		{
			var alleleFreqDict = GetAlleleFrequencies(itemsByAllele);
			if (alleleFreqDict.Count == 0) return;

			var firstItem  = itemsByAllele.First().Value[0];
			var refAllele  = firstItem.ReferenceAllele;
			var chromosome = firstItem.Chromosome;
			var position   = firstItem.Start;

			string globalMinorAlleleFrequency=null;

            var globalMajorAllele = GetMostFrequentAllele(alleleFreqDict, refAllele);
			if (globalMajorAllele == null) return;
            
            alleleFreqDict.Remove(globalMajorAllele);

            var globalMinorAllele = GetMostFrequentAllele(alleleFreqDict, refAllele, false);
			if (globalMinorAllele != null)
				globalMinorAlleleFrequency = alleleFreqDict[globalMinorAllele].ToString(CultureInfo.InvariantCulture);

			string vcfString=null;
			if ( globalMinorAllele != null )
			{
				vcfString = globalMinorAllele + '|' + globalMinorAlleleFrequency;
			}

			var sb = new StringBuilder();
			var jsonObject = new JsonObject(sb);
			jsonObject.AddStringValue("globalMinorAllele", globalMinorAllele);
			jsonObject.AddStringValue("globalMinorAlleleFrequency", globalMinorAlleleFrequency,false);
			
			_globalAlleleWriter.AddEntry(chromosome.EnsemblName, position, refAllele, "N", vcfString, new List<string> { sb.ToString() });
		}

		private static string GetMostFrequentAllele(Dictionary<string, double> alleleFreqDict, string refAllele, bool isRefPreferred = true)
		{
			if (alleleFreqDict.Count == 0) return null;

			// find all alleles that have max frequency.
			var maxFreq = alleleFreqDict.Values.Max();
			if (Math.Abs(maxFreq - double.MinValue) < double.Epsilon) return null;

			var maxFreqAlleles = (from pair in alleleFreqDict where Math.Abs(pair.Value - maxFreq) < double.Epsilon select pair.Key).ToList();


			// if there is only one with max frequency, return it
			if (maxFreqAlleles.Count == 1)
				return maxFreqAlleles[0];

			// if ref is preferred (as in global major) it is returned
			if (isRefPreferred && maxFreqAlleles.Contains(refAllele))
				return refAllele;

			// else refAllele is removed and the first of the remaining allele is returned (arbitrary selection)
			maxFreqAlleles.Remove(refAllele);
			return maxFreqAlleles[0];

		}
		private static Dictionary<string, double> GetAlleleFrequencies(Dictionary<Tuple<string, string>, List<DbSnpItem>> itemsByAllele)
		{
			var alleleFreqDict = new Dictionary<string, double>();

			foreach (var kvp in itemsByAllele)
			{
			    var refAllele = kvp.Key.Item1;
				var altAllele = kvp.Key.Item2;

			    foreach (var dbSnpItem in kvp.Value)
				{
					if (!dbSnpItem.RefAlleleFreq.Equals(double.MinValue))
						alleleFreqDict[refAllele] = dbSnpItem.RefAlleleFreq;
					if (!dbSnpItem.AltAlleleFreq.Equals(double.MinValue))
						alleleFreqDict[altAllele] = dbSnpItem.AltAlleleFreq;
				}
			}
			return alleleFreqDict;
		}

		private void WriteDbsnpTsv(Dictionary<Tuple<string, string>, List<DbSnpItem>> itemsByAllele)
		{
			foreach (var kvp in itemsByAllele)
			{
				var refAllele = kvp.Key.Item1;
			    var altAllele = kvp.Key.Item2;
				var itemsGroup = kvp.Value;

				var uniqueIds   = new HashSet<long>(itemsGroup.Select(x => x.RsId).ToList());
				var vcfString   = string.Join(",", uniqueIds.Select(x => $"rs{x}").ToArray());
			    var jsonString = "\"ids\":[" + string.Join(",", uniqueIds.OrderBy(x=>x).Select(x => $"\"rs{x}\"").ToArray()) + "]";

				var chromosome = itemsGroup[0].Chromosome;
				var position   = itemsGroup[0].Start;
				
				_dbsnpWriter.AddEntry(chromosome.EnsemblName, position, refAllele, altAllele, vcfString, new List<string> {jsonString});
			}
		}

		private static Dictionary<Tuple<string, string>, List<DbSnpItem>> GetItemsByAllele(IEnumerable<SupplementaryDataItem> saItems )
		{
			var itemsForPosition = new List<DbSnpItem>();
			foreach (var item in saItems)
			{
				var dbSnpItem = item as DbSnpItem;
				if (dbSnpItem==null) 
					throw new InvalidDataException("Expecting enumerable of DbSnpItems!!");
				itemsForPosition.Add(dbSnpItem);
			}

			var itemsByAllele = new Dictionary<Tuple<string, string>, List<DbSnpItem>>();
			foreach (var item in itemsForPosition)
			{
			    var alleleTuple = Tuple.Create(item.ReferenceAllele, item.AlternateAllele);

                if (itemsByAllele.ContainsKey(alleleTuple))
					itemsByAllele[alleleTuple].Add(item);
				else itemsByAllele[alleleTuple] = new List<DbSnpItem> {item};
			}
			return itemsByAllele;
		}
	}
}
