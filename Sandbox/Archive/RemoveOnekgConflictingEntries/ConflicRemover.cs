using System;
using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Illumina.VariantAnnotation.FileHandling;

namespace RemoveOnekgConflictingEntries
{
	public class ConflicRemover
	{
		private readonly string _inFile;
		private readonly string _outFile;
		private const int VcfBufferSize = 512;
		private int _maxVidPosition = 0;
		private string _currentRefSeq = "";
		private int _noLinesRemoved = 0;
		// constructor
		public ConflicRemover(string inputFile, string outputFile)
		{
			_inFile  = inputFile;
			_outFile = outputFile;
		}

		public int RemoveConflictingLines()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_inFile))
			using (var writer = GZipUtilities.GetStreamWriter(_outFile))
			{
				string line;
				var vcfLines = new List<string>(VcfBufferSize);//all lines for the last few positions will be tracked in this dictionary
				var hasConflictingEntry = new Dictionary<string, bool>();//indicates if there is a conflicting entry for a certain allele.

				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("#"))
					{
						// streaming the header lines
						writer.WriteLine(line);
						continue;
					}

					// parsing vcf line
					var vcfColumns = line.Split(new[] { '\t' }, VcfCommon.InfoIndex + 1);

					var chromosome  = vcfColumns[VcfCommon.ChromIndex];
					var vcfPosition = Convert.ToInt32(vcfColumns[VcfCommon.PosIndex]);
					var refAllele   = vcfColumns[VcfCommon.RefIndex];
					var altAlleles  = vcfColumns[VcfCommon.AltIndex].Split(',');

					if (chromosome != _currentRefSeq || vcfPosition > _maxVidPosition)
					{
						FlushVcfLineBuffer(vcfLines, hasConflictingEntry, writer);
						vcfLines.Clear();
						hasConflictingEntry.Clear();

						_currentRefSeq = chromosome;

					}

					foreach (var altAllele in altAlleles)
					{
						var alleleId = GetAlleleId(chromosome, vcfPosition,refAllele, altAllele);

						if (hasConflictingEntry.ContainsKey(alleleId))
							hasConflictingEntry[alleleId] = true; //wipe out any lines containing this alt allele
						else
						{
							hasConflictingEntry[alleleId] = false;
						}
					}

					vcfLines.Add(line);
					
				}
				// flushing out the remaining lines
				FlushVcfLineBuffer(vcfLines,hasConflictingEntry, writer);
			}
			return _noLinesRemoved;
		}

		private void FlushVcfLineBuffer(List<string> vcfLines, Dictionary<string, bool> hasConflictingEntry, StreamWriter writer)
		{
			foreach (var vcfLine in vcfLines)
			{
				// we need to check if any entry for this vcf line has conflict
				var vcfColumns = vcfLine.Split(new[] { '\t' }, VcfCommon.InfoIndex + 1);

				var chromosome  = vcfColumns[VcfCommon.ChromIndex];
				var vcfPosition = Convert.ToInt32(vcfColumns[VcfCommon.PosIndex]);
				var refAllele   = vcfColumns[VcfCommon.RefIndex];
				var altAlleles  = vcfColumns[VcfCommon.AltIndex].Split(',');

				var conflictingLine = false;
				foreach (var altAllele in altAlleles)
				{
					var alleleId = GetAlleleId(chromosome, vcfPosition, refAllele, altAllele);
					if (hasConflictingEntry[alleleId])
					{
						conflictingLine = true;
						break;
					}
				}

				if (!conflictingLine)
					writer.WriteLine(vcfLine);
				else _noLinesRemoved++;
			}
			
		}

		private string GetAlleleId(string chromosome, int vcfPosition, string refAllele, string altAllele)
		{
			int newStart = vcfPosition;
			var newAlleles = SupplementaryAnnotation.GetReducedAlleles(refAllele, altAllele, ref newStart);

			if (newStart > _maxVidPosition)
				_maxVidPosition = newStart;

			var newAltAllele = newAlleles.Item2;
			return chromosome + ':' + newStart + ':' + newAltAllele;
		}
	}
}
