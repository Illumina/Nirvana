using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
	public sealed class EvsTsvWriter : ISaItemTsvWriter
	{
		#region members

		private readonly SaTsvWriter _writer;

		#endregion

		public EvsTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider) :this(new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
				SaTsvCommon.OneKgenSchemaVersion, InterimSaCommon.EvsTag, InterimSaCommon.EvsVcfTag, true, sequenceProvider))
		{
			Console.WriteLine(version.ToString());
		}

	    private EvsTsvWriter(SaTsvWriter writer)
		{
			_writer = writer;
		}

		public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
		{
			if (saItems == null) return;

			var evsItems = new List<EvsItem>();
			foreach (var item in saItems)
			{
			    if (!(item is EvsItem evsItem))
					throw new InvalidDataException("Expected EvsItems list!!");
				evsItems.Add(evsItem);
			}

		    SupplementaryDataItem.RemoveConflictingAlleles(evsItems);

            foreach (var evsItem in evsItems)
			{
				_writer.AddEntry(evsItem.Chromosome.EnsemblName, evsItem.Start, evsItem.ReferenceAllele, evsItem.AlternateAllele, evsItem.GetVcfString(),
					new List<string> {evsItem.GetJsonString()});
			}
		}

	    public void Dispose() => _writer.Dispose();
	}
}

