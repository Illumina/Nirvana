using System;
using System.Text;

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
			var sb = new StringBuilder();
			// all start with transcript name & numbering type
			sb.Append(_transcriptId + ':' + _transcriptType + '.');

			// handle single and multiple positions
			var coordinates = _start.Value == _end.Value
				? _start.Value
				: _start.Value + '_' + _end.Value;

			// format rest of string according to type
			// note: inversion and multiple are never assigned as genomic changes
		switch (_type)
			{
				case GenomicChange.Deletion:
					sb.Append(coordinates + "del" + _referenceBases);
					break;
				case GenomicChange.Inversion:
					sb.Append(coordinates + "inv" + _referenceBases);
					break;
				case GenomicChange.Duplication:
					sb.Append(coordinates + "dup" + _referenceBases);
					break;
				case GenomicChange.Substitution:
					sb.Append(_start.Value + _referenceBases + '>' + _alternateBases);
					break;
				case GenomicChange.DelIns:
					sb.Append(coordinates + "del" + _referenceBases + "ins" + _alternateBases);
					//sb.Append(coordinates + "delins" + _alternateBases);
					break;
				case GenomicChange.Insertion:
					sb.Append(coordinates + "ins" + _alternateBases);
					break;
				//case GenomicChange.Multiple:
				//	sb.Append(coordinates + '[' + _alleleMultiple + ']' + _referenceBases);
				//	break;
				default:
					throw new InvalidOperationException("Unhandled genomic change found: " + _type);
			}

			return sb.ToString();
		}
	}
}