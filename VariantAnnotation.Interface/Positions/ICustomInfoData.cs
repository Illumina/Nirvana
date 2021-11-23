using System.Collections.Generic;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.Positions
{
    public interface ICustomInfoData: IJsonSerializer
    {
        void Add(string key, string value);
        void Clear();
        bool IsEmpty();
    }
}