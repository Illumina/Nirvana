using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.Caches.DataStructures
{
	public struct CdnaCoordinateMap : ICdnaCoordinateMap
	{
		public int Start { get; }
		public int End { get; }
		public int CdnaStart { get; }
		public int CdnaEnd { get; }

		public bool IsNull => Start == -1 && End == -1 && CdnaStart == -1 && CdnaEnd == -1;

		internal CdnaCoordinateMap(int start, int end, int cdnaStart, int cdnaEnd)
		{
			Start     = start;
			End       = end;
			CdnaStart = cdnaStart;
			CdnaEnd   = cdnaEnd;
		}

		public static ICdnaCoordinateMap Null() => new CdnaCoordinateMap(-1, -1, -1, -1);

		/// <summary>
		/// reads the cDNA coordinate map from the binary reader
		/// </summary>
		public static CdnaCoordinateMap Read(ExtendedBinaryReader reader)
		{
			// read the genomic interval
			int genomicStart = reader.ReadOptInt32();
			int genomicEnd   = reader.ReadOptInt32();

			// read the cDNA interval
			int cdnaStart = reader.ReadOptInt32();
			int cdnaEnd   = reader.ReadOptInt32();

			return new CdnaCoordinateMap(genomicStart, genomicEnd, cdnaStart, cdnaEnd);
		}

		/// <summary>
		/// writes the cDNA coordinate map to the binary writer
		/// </summary>
		public void Write(IExtendedBinaryWriter writer)
		{
			writer.WriteOpt(Start);
			writer.WriteOpt(End);
			writer.WriteOpt(CdnaStart);
			writer.WriteOpt(CdnaEnd);
		}		
	}
}