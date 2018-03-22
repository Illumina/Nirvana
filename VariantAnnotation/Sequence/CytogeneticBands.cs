using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace VariantAnnotation.Sequence
{
	public sealed class CytogeneticBands : ICytogeneticBands
	{
		#region members

		private readonly Band[][] _cytogeneticBands;

		#endregion

		/// <summary>
		/// constructor
		/// </summary>
		public CytogeneticBands(Band[][] cytogeneticBands)
		{
			_cytogeneticBands = cytogeneticBands;
		}

		/// <inheritdoc />
		/// <summary>
		/// returns the correct cytogenetic band representation for this chromosome given
		/// the start and end coordinates
		/// </summary>
		public string GetCytogeneticBand(IChromosome chromosome, int start, int end)
		{
			var startCytogeneticBand = GetCytogeneticBand(chromosome.Index, start);
			if (startCytogeneticBand == null) return null;

			// handle the single coordinate case
			if (start == end) return $"{chromosome.EnsemblName}{startCytogeneticBand}";

			// handle the dual coordinate case
			var endCytogeneticBand = GetCytogeneticBand(chromosome.Index, end);
			if (endCytogeneticBand == null) return null;

			return startCytogeneticBand == endCytogeneticBand
				? $"{chromosome.EnsemblName}{startCytogeneticBand}"
				: $"{chromosome.EnsemblName}{startCytogeneticBand}-{endCytogeneticBand}";
		}

		/// <summary>
		/// returns the cytogenetic band corresponding to the specified position
		/// </summary>
		private string GetCytogeneticBand(ushort referenceIndex, int pos)
		{
			if (referenceIndex >= _cytogeneticBands.Length || referenceIndex == CompressedSequenceReader.UnknownReferenceIndex)
				return null;

			var bands = _cytogeneticBands[referenceIndex];
			var index = BinarySearch(bands, pos);

			return index < 0 ? null : bands[index].Name;
		}

		/// <summary>
		/// returns the index of the desired element, otherwise it returns a negative number
		/// </summary>
		private static int BinarySearch(Band[] array, int position)
		{
			var begin = 0;
			var end = array.Length - 1;

			while (begin <= end)
			{
				var index = begin + (end - begin >> 1);

				var ret = array[index].Compare(position);
				if (ret == 0) return index;
				if (ret < 0) begin = index + 1;
				else end = index - 1;
			}

			return ~begin;
		}

		/// <summary>
		/// reads the cytogenetic bands from the file
		/// </summary>
		public static Band[][] Read(ExtendedBinaryReader reader)
		{
			var numReferences = reader.ReadOptInt32();
			var cytogeneticBands = new Band[numReferences][];

			for (var refIndex = 0; refIndex < numReferences; refIndex++)
			{
				var numBands = reader.ReadOptInt32();
				cytogeneticBands[refIndex] = new Band[numBands];

				for (var bandIndex = 0; bandIndex < numBands; bandIndex++)
				{
					var begin = reader.ReadOptInt32();
					var end = reader.ReadOptInt32();
					var name = reader.ReadAsciiString();

					cytogeneticBands[refIndex][bandIndex] = new Band(begin, end, name);
				}
			}

			return cytogeneticBands;
		}

		/// <summary>
		/// writes the cytogenetic bands to the file
		/// </summary>
		public void Write(IExtendedBinaryWriter writer)
		{
			var numReferences = _cytogeneticBands.Length;
			writer.WriteOpt(numReferences);

			for (var refIndex = 0; refIndex < numReferences; refIndex++)
			{
				var numRefEntries = _cytogeneticBands[refIndex].Length;
				writer.WriteOpt(numRefEntries);

				foreach (var band in _cytogeneticBands[refIndex])
				{
					writer.WriteOpt(band.Begin);
					writer.WriteOpt(band.End);
					writer.WriteOptAscii(band.Name);
				}
			}
		}
	}
}