using System;

namespace VariantAnnotation.Interface.IO
{
    public interface IVcfReader : IDisposable
    {
		bool IsRcrsMitochondrion { get; }
    }
}