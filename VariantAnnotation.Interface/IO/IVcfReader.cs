using System;
using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Interface.IO
{
    public interface IVcfReader : IDisposable
    {
		bool IsRcrsMitochondrion { get; }
		IEnumerable<string> GetHeaderLines();
        IPosition GetNextPosition();
	    string VcfLine { get; }
        string[] GetSampleNames();
    }
}