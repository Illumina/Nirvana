using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Sequence
{
	public sealed class CompressedSequence : ISequence
	{
		#region members

	    public int Length { get; private set; }
		public GenomeAssembly GenomeAssembly { get; set; }

	    private int _sequenceOffset;
		private byte[] _buffer;

		private IIntervalSearch<MaskedEntry> _maskedIntervalSearch;

		private readonly char[] _convertNumberToBase;
		#endregion

		// constructor
		public CompressedSequence()
		{
			const string bases = "GCTA";
			_convertNumberToBase = bases.ToCharArray();
		}

		/// <summary>
		/// returns a tuple containing the base index and shift given a reference position
		/// </summary>
		private static (int BaseIndex, int Shift) GetBaseIndexAndShift(int referencePosition)
		{
			int refPos = referencePosition + 1;
			// ReSharper disable once SuggestUseVarKeywordEvident
			int baseIndex = (int)(refPos / 4.0);
			int shift = (3 - refPos % 4) * 2;
			return (baseIndex, shift);
		}

		/// <summary>
		/// returns the number of bytes required in the buffer given the number of bases
		/// </summary>
		internal static int GetNumBufferBytes(int numBases)
		{
			return (int)((double)numBases / CompressedSequenceCommon.NumBasesPerByte + 1);
		}

		/// <summary>
		/// sets the underlying data for the compressed sequence
		/// </summary>
		public void Set(int numBases, byte[] buffer, IIntervalSearch<MaskedEntry> maskedIntervalSearch, int sequenceOffset = 0)
		{
			Length = numBases;
			_buffer = buffer;
			_maskedIntervalSearch = maskedIntervalSearch;
			_sequenceOffset = sequenceOffset;
		}

		
		/// <summary>
		/// decompresses the 2-bit notation into bases
		/// </summary>
		public string Substring(int offset, int length)
		{
			offset -= _sequenceOffset;

			// handle negative offsets and lengths
			if (offset < 0 || length < 1 || offset >= Length) return null;

			// sanity check: avoid going past the end of the sequence
			if (offset + length > Length) length = Length - offset;

			// allocate more memory if needed
			var decompressBuffer = new char[length];

			// set the initial state of the buffer
			var indexAndShiftTuple = GetBaseIndexAndShift(offset - 1);

			int bufferIndex = indexAndShiftTuple.BaseIndex;
			int bufferShift = indexAndShiftTuple.Shift;
			byte currentBufferSeed = _buffer[bufferIndex];

			// get the overlapping masked interval
			var maskedIntervals = _maskedIntervalSearch.GetAllOverlappingValues(offset, offset + length - 1);

			// get the first masked interval
		    var numIntervals = maskedIntervals?.Length ?? 0;
		    bool hasMaskedIntervals = maskedIntervals != null;
			int currentOffset = 0;
			var currentInterval = hasMaskedIntervals ? maskedIntervals[0] : null;

			for (int baseIndex = 0; baseIndex < length; baseIndex++)
			{
				int currentPosition = offset + baseIndex;

				if (hasMaskedIntervals && currentPosition >= currentInterval.Begin && currentPosition <= currentInterval.End)
				{
					// evaluate the masked bases
					for (; baseIndex <= currentInterval.End - offset && baseIndex < length; baseIndex++) decompressBuffer[baseIndex] = 'N';
					baseIndex--;

					indexAndShiftTuple = GetBaseIndexAndShift(offset + baseIndex);

					bufferIndex = indexAndShiftTuple.BaseIndex;
					bufferShift = indexAndShiftTuple.Shift;
					currentBufferSeed = _buffer[bufferIndex];

					currentOffset++;
					hasMaskedIntervals = currentOffset < numIntervals;
					currentInterval = hasMaskedIntervals ? maskedIntervals[currentOffset] : null;
				}
				else
				{
					// evaluate normal bases
					decompressBuffer[baseIndex] = _convertNumberToBase[(currentBufferSeed >> bufferShift) & 3];

					bufferShift -= 2;

					if (bufferShift < 0)
					{
						bufferShift = CompressedSequenceReader.MaxShift;
						bufferIndex++;
						currentBufferSeed = _buffer[bufferIndex];
					}
				}
			}

			return new string(decompressBuffer, 0, length);
		}
		
		
	}
}