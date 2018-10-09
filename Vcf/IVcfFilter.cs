using System.IO;
using Genome;

namespace Vcf
{
    public interface IVcfFilter
    {
        void FastForward(StreamReader reader);

        string GetNextLine(StreamReader reader);

        bool PassedTheEnd(IChromosome chromosome, int position);
    }
}