using System;
using System.Collections.Generic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace UnitTests.Utilities
{
    internal static class SupplementaryAnnotationUtilities
    {
        /// <summary>
        /// writes the specified supplementary annotation to a random file. The random filename is returned.
        /// </summary>
        internal static void Write(SupplementaryAnnotation sa, string ucscReferenceName, string randomPath)
        {
            var versions = new List<DataSourceVersion> { new DataSourceVersion("ClinVar", "13.5", DateTime.Parse("2015-01-19").Ticks) };

            using (var writer = new SupplementaryAnnotationWriter(randomPath, ucscReferenceName, versions))
            {
                writer.Write(sa, sa.ReferencePosition);
            }
        }
    }
}
