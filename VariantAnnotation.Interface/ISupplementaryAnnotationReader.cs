using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ISupplementaryAnnotationReader
    {
        bool IsRefMinor(int position);
        ISupplementaryAnnotationPosition GetAnnotation(int referencePos);
        IIntervalForest<ISupplementaryInterval> GetIntervalForest(IChromosomeRenamer renamer);
        IEnumerable<ISupplementaryInterval> GetSupplementaryIntervals(IChromosomeRenamer renamer);
    }
}
