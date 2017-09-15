using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
	public class GnomadTsvWriter:ISaItemTsvWriter
	{
		#region members
		private readonly SaTsvWriter _writer;
		#endregion

		
		public GnomadTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider)
		{
			Console.WriteLine(version.ToString());

			_writer= new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.SchemaVersion, InterimSaCommon.GnomadTag, null, true, sequenceProvider);

		}

		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			if (saItems == null) return;

			var gnomadItems = new List<GnomadItem>();
			foreach (var item in saItems)
			{
			    if (!(item is GnomadItem gnomadItem))
					throw new InvalidDataException("Expected GnomadItems list!!");
				gnomadItems.Add(gnomadItem);
			}

		    SupplementaryDataItem.RemoveConflictingAlleles(gnomadItems);


            foreach (var gnomadItem in gnomadItems)
			{
				_writer.AddEntry(gnomadItem.Chromosome.EnsemblName, gnomadItem.Start, gnomadItem.ReferenceAllele, gnomadItem.AlternateAllele, null, new List<string> {gnomadItem.GetJsonString()});
			}
		}


	    public void Dispose()
	    {
	        _writer.Dispose();
	    }
	}
}
