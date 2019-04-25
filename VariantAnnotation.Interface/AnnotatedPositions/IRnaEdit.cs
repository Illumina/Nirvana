using System;
using Intervals;
using IO;
using Variants;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IRnaEdit : IInterval, ISerializable, IComparable<IRnaEdit>
    {
        string Bases { get; }
        VariantType Type { get; set; }
    }
}
