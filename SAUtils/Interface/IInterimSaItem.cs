using System;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.Interface
{
    public interface IInterimSaItem : IComparable<IInterimSaItem>
    {
        string KeyName { get; }
        string Chromosome { get; }
        int Position { get; }
    }
}