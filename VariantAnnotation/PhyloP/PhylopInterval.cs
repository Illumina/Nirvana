using System;
using System.IO;

namespace VariantAnnotation.PhyloP
{
	public sealed class PhylopInterval : IEquatable<PhylopInterval>, IComparable<PhylopInterval>
	{
		public int Begin { get; private set; }

		public int Length;
		//fixedStep vs variableStep. We also have a point interval that is an interal of length 1. This is used for binary search of points in chromosome interval
	    private short StepSize { get; set; }
		//file location of the compressed phylop scores for this interval
		public long FilePosition;

		public PhylopInterval(int begin, int length, short stepSize)
		{
		    StepSize     = stepSize;
			Begin        = begin;
			Length       = length;
			FilePosition = -1;
		}

		public PhylopInterval(BinaryReader reader)
		{
			Read(reader);
		}
		
		public int CompareTo(PhylopInterval otherInterval)
		{
		    // A null value means that this object is greater. 
		    return otherInterval == null ? 1 : Begin.CompareTo(otherInterval.Begin);
		}

		public bool Equals(PhylopInterval other)
		{
		    return other != null && Begin.Equals(other.Begin);
		}

		public void Write(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(Begin);
			binaryWriter.Write(Length);
			binaryWriter.Write(StepSize);
			binaryWriter.Write(FilePosition);
		}

		private void Read(BinaryReader binaryReader)
		{
			Begin        = binaryReader.ReadInt32();
			Length       = binaryReader.ReadInt32();
			StepSize     = binaryReader.ReadInt16();
			FilePosition = binaryReader.ReadInt64();			
		}

		public override string ToString()
		{
			return "Start=" + Begin + "\tLength=" + Length + "\tstepSize=" + StepSize + "\tFilePosition=" + FilePosition;
		}

		public bool ContainsPosition(int position)
		{
			return position >= Begin && position < Begin + Length;
		}
	}
}
