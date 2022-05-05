using System.Collections.Generic;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.Positions
{
    public interface ICustomFields: IJsonSerializer
    {
        void Add(string key, string value);
        void Clear();
        bool IsEmpty();
    }
}