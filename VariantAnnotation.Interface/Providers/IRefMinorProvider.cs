using System;
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface IRefMinorProvider:IDisposable
    {
        string GetGlobalMajorAllele(IChromosome chromosome, int pos);
    }
}