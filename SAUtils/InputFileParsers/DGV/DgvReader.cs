using System.Collections;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Interface;

namespace SAUtils.InputFileParsers.DGV
{
	public sealed class DgvReader:IEnumerable<DgvItem>
	{
		#region members

		private readonly FileInfo _dgvFileInfo;
        private readonly IChromosomeRenamer _renamer;

        #endregion

        #region IEnumerable implementation

        public IEnumerator<DgvItem> GetEnumerator()
		{
			return GetDgvItems().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		// constructor
		public DgvReader(FileInfo dgvFileInfo, IChromosomeRenamer renamer)
		{
			_dgvFileInfo = dgvFileInfo;
		    _renamer     = renamer;
		}

		/// <summary>
		/// returns a ClinVar object given the vcf line
		/// </summary>
		public static DgvItem ExtractDgvItem(string line, IChromosomeRenamer renamer)
		{
			var cols = line.Split('\t');
			if (cols.Length < 8) return null;

			var id = cols[0];
			var chromosome = cols[1];
			if (!InputFileParserUtilities.IsDesiredChromosome(chromosome, renamer)) return null;

			var start = int.Parse(cols[2]);
			var end = int.Parse(cols[3]);
			var variantType = cols[4];
			var variantSubType = cols[5];
			var sampleSize = int.Parse(cols[14]);
			var observedGains = cols[15] == "" ? 0:int.Parse(cols[15]);
			var observedLosses = cols[16] == "" ? 0 : int.Parse(cols[16]);

			var seqAltType = SequenceAlterationUtilities.GetSequenceAlteration(variantType,variantSubType);

			return new DgvItem(id, chromosome,start,end,sampleSize,observedGains,observedLosses, seqAltType);
		}



		/// <summary>
		/// Parses a ClinVar file and return an enumeration object containing all the ClinVar objects
		/// that have been extracted
		/// </summary>
		private IEnumerable<DgvItem> GetDgvItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_dgvFileInfo.FullName))
			{
				while (true)
				{
					// grab the next line
					string line = reader.ReadLine();
					if (line == null) break;

					// skip header and empty lines
					if(string.IsNullOrWhiteSpace(line) || IsDgvHeader(line)) continue;
					var dgvItem = ExtractDgvItem(line, _renamer);
					if (dgvItem == null) continue;
					yield return dgvItem;
				}
			}
		}

		private static bool IsDgvHeader(string line)
		{
			return line.StartsWith("variantaccession");
		}
	}
}