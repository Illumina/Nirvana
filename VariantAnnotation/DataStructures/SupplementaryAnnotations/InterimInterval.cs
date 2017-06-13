using System;
using System.IO;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public class InterimInterval:IComparable<InterimInterval>, IInterimInterval
	{
		#region members
		public  string KeyName { get; }
		public  string ReferenceName { get; }
		public int Start { get; }
		public int End { get; }
		public string JsonString { get; }
		public ReportFor ReportingFor { get; }
		#endregion


		public InterimInterval(string keyName, string refName, int start, int end, string jsonString, ReportFor reportingFor)
		{
			KeyName       = keyName;
			ReferenceName = refName;
			Start         = start;
			End           = end;
			JsonString    = jsonString;
			ReportingFor  = reportingFor;
		}

		public InterimInterval(BinaryReader reader)
		{
			KeyName       = reader.ReadString();
			ReferenceName = reader.ReadString();
			Start         = reader.ReadInt32();
			End           = reader.ReadInt32();
			JsonString    = reader.ReadString();
			ReportingFor  = (ReportFor) reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(KeyName);
			writer.Write(ReferenceName);
			writer.Write(Start);
			writer.Write(End);
			writer.Write(JsonString);
			writer.Write((byte)ReportingFor);
		}

		public int CompareTo(InterimInterval other)
		{
			if (other == null) return -1;

			return ReferenceName.Equals(other.ReferenceName) ? Start.CompareTo(other.Start) : string.CompareOrdinal(ReferenceName, other.ReferenceName);
		}
	}
}
