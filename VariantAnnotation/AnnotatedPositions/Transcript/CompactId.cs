using System;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
	public struct CompactId : ICompactId
	{
		public IdType Id { get; }
		public int Info { get; }

		private const int NumShift = 4;
		private const int LengthMask = 0xf;

		public bool IsEmpty => Id == IdType.Unknown;
		public static CompactId Empty => new CompactId(IdType.Unknown, 0);
		private static int ToInfo(int num, int len) => num << 4 | (len & LengthMask);

		private int Length => Info & LengthMask;
		private int Num => Info >> NumShift;

		/// <summary>
		/// constructor
		/// </summary>
		private CompactId(IdType id, int info)
		{
			Id = id;
			Info = info;
		}

		//#region IEquatable methods

		//public override int GetHashCode() => Id.GetHashCode() ^ Info.GetHashCode();
		//public bool Equals(CompactId value) => Id == value.Id && Info == value.Info;

		//#endregion

		public static CompactId Convert(string s)
		{
			if (s == "") return new CompactId(IdType.Unknown, 0);

			if (s.StartsWith("ENSG")) return GetCompactId(s, 4, IdType.EnsemblGene);
			if (s.StartsWith("ENST")) return GetCompactId(s, 4, IdType.EnsemblTranscript);
			if (s.StartsWith("ENSP")) return GetCompactId(s, 4, IdType.EnsemblProtein);
			if (s.StartsWith("ENSESTG")) return GetCompactId(s, 7, IdType.EnsemblEstGene);
			if (s.StartsWith("ENSR")) return GetCompactId(s, 4, IdType.EnsemblRegulatory);
			if (s.StartsWith("CCDS")) return GetCompactId(s, 4, IdType.Ccds);
			if (s.StartsWith("NR_")) return GetCompactId(s, 3, IdType.RefSeqNonCodingRNA);
			if (s.StartsWith("NM_")) return GetCompactId(s, 3, IdType.RefSeqMessengerRNA);
			if (s.StartsWith("NP_")) return GetCompactId(s, 3, IdType.RefSeqProtein);
			if (s.StartsWith("XR_")) return GetCompactId(s, 3, IdType.RefSeqPredictedNonCodingRNA);
			if (s.StartsWith("XM_")) return GetCompactId(s, 3, IdType.RefSeqPredictedMessengerRNA);
			if (s.StartsWith("XP_")) return GetCompactId(s, 3, IdType.RefSeqPredictedProtein);

            if (int.TryParse(s, out int i))
            {
                return new CompactId(IdType.OnlyNumbers, ToInfo(i, s.Length));
            }

            Console.WriteLine("Unknown ID: [{0}] ({1})", s, s.Length);
			return Empty;
		}

		private static CompactId GetCompactId(string s, int prefixLen, IdType idType)
		{
			var tuple = FormatUtilities.SplitVersion(s);
			var num = int.Parse(tuple.Id.Substring(prefixLen));
			return new CompactId(idType, ToInfo(num, tuple.Id.Length - prefixLen));
		}

		/// <summary>
		/// returns a string representation of this compact ID
		/// </summary>
		public override string ToString()
		{
			var num = Num.ToString("D" + Length);

			switch (Id)
			{
				case IdType.EnsemblGene:
					return $"ENSG{num}";
				case IdType.EnsemblTranscript:
					return $"ENST{num}";
				case IdType.EnsemblProtein:
					return $"ENSP{num}";
				case IdType.EnsemblEstGene:
					return $"ENSESTG{num}";
				case IdType.EnsemblRegulatory:
					return $"ENSR{num}";
				case IdType.Ccds:
					return $"CCDS{num}";
				case IdType.RefSeqNonCodingRNA:
					return $"NR_{num}";
				case IdType.RefSeqMessengerRNA:
					return $"NM_{num}";
				case IdType.RefSeqProtein:
					return $"NP_{num}";
				case IdType.RefSeqPredictedNonCodingRNA:
					return $"XR_{num}";
				case IdType.RefSeqPredictedMessengerRNA:
					return $"XM_{num}";
				case IdType.RefSeqPredictedProtein:
					return $"XP_{num}";
				case IdType.OnlyNumbers:
					return Num.ToString();
			}

			return "";
		}

		public void Write(IExtendedBinaryWriter writer)
		{
			writer.Write((byte)Id);
			writer.WriteOpt(Info);
		}

		public static CompactId Read(IExtendedBinaryReader reader)
		{
			var id = (IdType)reader.ReadByte();
			var info = reader.ReadOptInt32();
			return new CompactId(id, info);
		}
		
	}

	
}