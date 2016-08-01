using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ICosmic
    {
        string AltAllele { get; }  
        string Gene { get; }       
        string ID { get; } 
        string IsAlleleSpecific { get; } 
        IEnumerable<ICosmicStudy> Studies { get; }
    }

    public interface ICosmicStudy
    {
        string ID { get; }
        string Histology { get; }
        string PrimarySite { get; }
    }
}