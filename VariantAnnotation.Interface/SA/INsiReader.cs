using System;
using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace VariantAnnotation.Interface.SA
{
    public interface INsiReader:IDisposable
    {
        GenomeAssembly Assembly { get; }
        IDataSourceVersion Version { get; }
        string JsonKey { get; }
        ReportFor ReportFor { get; }
        IEnumerable<string> GetAnnotation(IVariant variant);
    }
}