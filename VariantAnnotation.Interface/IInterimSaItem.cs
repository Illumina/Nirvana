using System;

namespace VariantAnnotation.Interface
{
	public interface IInterimSaItem:IComparable<IInterimSaItem>
	{
		string KeyName { get; }
		string Chromosome { get; }
		int Position { get; }


	}
}