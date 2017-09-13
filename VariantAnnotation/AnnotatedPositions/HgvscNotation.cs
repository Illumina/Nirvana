namespace VariantAnnotation.AnnotatedPositions
{
	public sealed class PositionOffset
	{
		public int? Position;
		public int? Offset;
		public string Value;
		public bool HasStopCodonNotation;

		public PositionOffset(int position)
		{
			Position = position;
		}


	}


	public sealed class HgvscNotation
	{
		#region members

		private readonly string _referenceBases;
		private readonly string _alternateBases;

		private PositionOffset _start;
		private PositionOffset _end;

		private readonly string _transcriptId;

		private readonly char _transcriptType;

		private readonly GenomicChange _type;
		//private int _alleleMultiple;

		private const char CodingType = 'c';
		private const char NonCodingType = 'n';

		#endregion

		/// <summary>
		/// constructor
		/// </summary>
		public HgvscNotation(string referenceBases, string alternateBases, string transcriptId, GenomicChange changeType, PositionOffset start, PositionOffset end, bool isCoding)
		{
			_transcriptId = transcriptId;

			_start = start;
			_end = end;
			SwapEndpoints();
			_type = changeType;

			_referenceBases = referenceBases ?? "";
			_alternateBases = alternateBases ?? "";

			_transcriptType = isCoding ? CodingType : NonCodingType;
		}

		/// <summary>
		/// HGVS aligns changes 3' 
		/// e.g. given a ATG/- deletion in C[ATG]ATGT, we want to move to: CATG[ATG]T
		///      given a   A/- deletion in  TA[A]AAAA, we want to move to:  TAAAAA[A]
		///      given a  AA/- deletion in  TA[AA]AAA, we want to move to:  TAAAA[AA]
		/// </summary>
		private void SwapEndpoints()
		{
			if (_start.Offset == null) _start.Offset = 0;
			if (_end.Offset == null) _end.Offset = 0;

			var isInsertion = _start.Position  > _end.Position ||_start.Position == _end.Position && _start.Offset > _end.Offset;

		    if ( isInsertion)
            {
				var temp = _start;
				_start    = _end;
				_end      = temp;
			}
		}

		public override string ToString()
		{
		    return HgvsUtilities.FormatDnaNotation(_start.Value, _end.Value, _transcriptId, _referenceBases,
		        _alternateBases, _type, _transcriptType);

		}
	}
}