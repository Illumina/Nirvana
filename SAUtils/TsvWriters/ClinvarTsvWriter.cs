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
    public sealed class ClinvarTsvWriter:ISaItemTsvWriter
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

        private ClinvarTsvWriter(SaTsvWriter saTsvWriter)
		{
			_writer = saTsvWriter;
		}
	
		public ClinvarTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider) :this(new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.ClinvarSchemaVersion, InterimSaCommon.ClinvarTag, InterimSaCommon.ClinvarVcfTag, false, sequenceProvider, true))
		{
			Console.WriteLine(version.ToString());
			
		}

		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			if (saItems == null ) return;
			var clinvarItems = new List<ClinVarItem>();
			foreach (var item in saItems)
			{
			    if (!(item is ClinVarItem clinvarItem))
					throw new InvalidDataException("Expected ClinvarItems list!!");
				clinvarItems.Add(clinvarItem);
			}

			if (clinvarItems.Count == 0) return;
            var alleleGroupDict = GroupByAltAllele(clinvarItems);
            foreach (var kvp in alleleGroupDict)
            {
                var refAllele = kvp.Key.ReferenceAllele;
                var altAllele = kvp.Key.AlternateAllele;

                var groupedItems = kvp.Value;
                var vcfString = string.Join(",", groupedItems.OrderBy(x => x.Id).Select(x => SupplementaryAnnotationUtilities.ConvertToVcfInfoString(x.Significance)));
                var jsonStrings = groupedItems.OrderBy(x => x.Id).Select(x => x.GetJsonString()).ToList();

                var firstItem = groupedItems[0];
                _writer.AddEntry(firstItem.Chromosome.EnsemblName,
                    firstItem.Start,
                    refAllele,
                    altAllele, vcfString, jsonStrings);
            }

   //         var alleleGroupedItems = clinvarItems.GroupBy(x => x.AlternateAllele);
			//foreach (var groupedItem in alleleGroupedItems)
			//{
			//	var uniqueItems = groupedItem.GroupBy(p => p.ID).Select(x => x.First()).ToList();
			//	var vcfString = string.Join(",", uniqueItems.Select(x => SupplementaryAnnotationUtilities.ConvertToVcfInfoString(x.Significance)));
			//	var jsonStrings = uniqueItems.Select(x => x.GetVariantJsonString()).ToList();

			//	// since the reference allele for different items in the group may be different, we only use the first base as it is supposed to be the common padding base.
			//	_writer.AddEntry(groupedItem.First().Chromosome,
			//		groupedItem.First().Start,
			//		groupedItem.First().ReferenceAllele, 
			//		groupedItem.Key, vcfString, jsonStrings);
			//}
			

		}

        private static Dictionary<(string ReferenceAllele, string AlternateAllele), List<ClinVarItem>> GroupByAltAllele(List<ClinVarItem> clinVarItems)
        {
            var groups = new Dictionary<(string, string), List<ClinVarItem>>();

            foreach (var clinVarItem in clinVarItems)
            {
                var alleleTuple = (clinVarItem.ReferenceAllele, clinVarItem.AlternateAllele);
                if (groups.ContainsKey(alleleTuple))
                    groups[alleleTuple].Add(clinVarItem);
                else groups[alleleTuple] = new List<ClinVarItem> { clinVarItem };
            }

            return groups;
        }

    }
}
