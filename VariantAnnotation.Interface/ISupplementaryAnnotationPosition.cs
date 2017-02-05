using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ISupplementaryAnnotationPosition
    {
        void AddSaPositionToVariant(IAnnotatedAlternateAllele jsonVariant);
        List<ICustomItem> CustomItems { get; set; }
        string GlobalMajorAllele { get; }
        void SetIsAlleleSpecific(string saAltAllele);
    }
}
