using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.TsvWriters
{
	public sealed class OnekgTsvWriter:ISaItemTsvWriter
	{
		#region members
		private readonly SaTsvWriter _onekgWriter;
		private readonly SaMiscTsvWriter _refMinorWriter;
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
				_onekgWriter.Dispose();
				_refMinorWriter.Dispose();
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;
			// Free any other managed objects here.

		}
		#endregion

		public OnekgTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider)
		{

			Console.WriteLine(version.ToString());

			_onekgWriter = new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.OneKgenSchemaVersion, InterimSaCommon.OneKgenTag, "AF1000G",true, sequenceProvider);

            _refMinorWriter = new SaMiscTsvWriter(outputDirectory,version,genomeAssembly.ToString(),InterimSaCommon.RefMinorTag, sequenceProvider);

		}

		
		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			if (saItems == null) return;

			var onekGenItems = new List<OneKGenItem>();
			foreach (var item in saItems)
			{
			    if (!(item is OneKGenItem onekGenItem))
					throw new InvalidDataException("Expected OnekGenItems list!!");
				onekGenItems.Add(onekGenItem);
			}
			
			SupplementaryDataItem.RemoveConflictingAlleles(onekGenItems);

		    var totalAltAlleleFreq = 0.0;
            var alleleFrequencies = new Dictionary<string,double>();
			foreach (var onekGenItem in onekGenItems)
			{
				_onekgWriter.AddEntry(onekGenItem.Chromosome.EnsemblName, onekGenItem.Start,
					onekGenItem.ReferenceAllele, onekGenItem.AlternateAllele,
					onekGenItem.GetVcfString(), new List<string> { onekGenItem.GetJsonString() });
				if(!IsSnv(onekGenItem.ReferenceAllele) || !IsSnv(onekGenItem.AlternateAllele)) continue;
				
				if (onekGenItem.AllAlleleNumber != null && onekGenItem.AllAlleleCount!=null)
				{
                    var freq = 1.0 * onekGenItem.AllAlleleCount.Value / onekGenItem.AllAlleleNumber.Value;
                    totalAltAlleleFreq += freq;
				    alleleFrequencies[onekGenItem.AlternateAllele] = freq;
				}

			}

			var isRefMinor = totalAltAlleleFreq >= SaDataBaseCommon.RefMinorThreshold;


			if(isRefMinor)
				_refMinorWriter.AddEntry(onekGenItems[0].Chromosome.EnsemblName, onekGenItems[0].Start,GetMajorAllele(alleleFrequencies),onekGenItems[0].ReferenceAllele);
		}

	    private static string GetMajorAllele(Dictionary<string, double> alleleFrequencies)
	    {
	        var maxFreq        = 0.0;
	        string majorAllele = null;

            foreach (var kvp in alleleFrequencies)
	        {
	            if (kvp.Value > maxFreq)
	            {
	                maxFreq = kvp.Value;
	                majorAllele = kvp.Key;
	            }
	        }

	        return majorAllele;
	    }

	    private static bool IsSnv(string allele)
		{
			if (allele.Length != 1) return false;

			allele = allele.ToUpper();
			return allele == "A" || allele == "C" || allele == "G" || allele == "T";
		}
    }
}
