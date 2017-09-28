using System;
using VariantAnnotation.Interface.IO;

namespace Jasix.DataStructures { 
	public sealed class JasixNode : IComparable<JasixNode>
	{
		private readonly int _start;
		private int _end;
		public readonly long FileLocation;
		private int _count;
		public JasixNode(int start, int end, long location)
		{
			_start        = start;
			_end          = end;
			_count = 1;
			FileLocation = location;
		}

		public JasixNode(IExtendedBinaryReader reader)
		{
			_start        = reader.ReadOptInt32();
			//on disk we will store the end as an offset to save space
			_end          = _start + reader.ReadOptInt32();
			FileLocation = reader.ReadOptInt64();
		}

		public bool Overlaps(JasixNode other)
		{
			return other._end >= _start && other._start <= _end;
		}

		public int CompareTo(JasixNode other)
		{
			if (other == null) return -1;
			// ReSharper disable once ImpureMethodCallOnReadonlyValueField
			return _start.CompareTo(other._start);
		}

		public bool TryAdd(int start, int end)
		{
			if (start < _start) return false;
			if (end - _start > JasixCommons.MinNodeWidth
				&& _count >= JasixCommons.PreferredNodeCount)
				return false;
			_end = end;
			_count++;
			return true;
		}

		public void Write(IExtendedBinaryWriter writer)
		{
			writer.WriteOpt(_start);
			writer.WriteOpt(_end-_start);
			writer.WriteOpt(FileLocation);
		}

	}
}
