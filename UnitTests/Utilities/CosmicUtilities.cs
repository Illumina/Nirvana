using System.Linq;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;

namespace UnitTests.Utilities
{
    public static class CosmicUtilities
    {
        public static bool ContainsId(SupplementaryAnnotation sa, string id)
        {
            return sa.CosmicItems.Any(x => x.ID.Equals(id));
        }
    }
}
