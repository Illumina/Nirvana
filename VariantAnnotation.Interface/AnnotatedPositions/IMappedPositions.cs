using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IMappedPositions
    {
         NullableInterval CdnaInterval { get;  }
         NullableInterval CdsInterval { get; }
         IInterval ImpactedCdnaInterval { get;  }
         IInterval ImpactedCdsInterval { get;  }
         NullableInterval ProteinInterval { get; }
         IInterval Exons { get;  }
         IInterval Introns { get; }

    }
}