using System;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Interface.IO
{
    public interface IJsonWriter : IDisposable
    {
        void WritePosition(IPosition position, string entry);
    }
}