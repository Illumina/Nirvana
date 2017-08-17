using System;

namespace VariantAnnotation.Interface.IO
{
    public interface IJsonWriter : IDisposable
    {
        void WriteJsonEntry(string entry);
    }
}