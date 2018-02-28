using System.Collections.Generic;
using ErrorHandling.Exceptions;

namespace Vcf
{
    public sealed class SortedVcfChecker
    {
        private readonly HashSet<string> _processedReferences = new HashSet<string>();
        private string _currentReferenceName;

        public void CheckVcfOrder(string referenceName)
        {
            if (referenceName == _currentReferenceName) return;

            if (_processedReferences.Contains(referenceName))
            {
                throw new FileNotSortedException("The current input vcf file is not sorted. Please sort the vcf file before running variant annotation using a tool like vcf-sort in vcftools.");
            }

            _processedReferences.Add(referenceName);
            _currentReferenceName = referenceName;
        }
    }
}
