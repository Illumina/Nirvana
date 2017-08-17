using System.Collections.Generic;
using VariantAnnotation.Interface.GeneAnnotation;

namespace VariantAnnotation.GeneAnnotation
{
    public static class GeneAnnotator
    {

        public static List<IAnnotatedGene> Annotate(IEnumerable<string> geneNames, IGeneAnnotationProvider[] annotationProviders)
        {
            var annotatedGenes = new List<IAnnotatedGene>();
            foreach (var geneName in geneNames)
            {
                var annotations = new List<IGeneAnnotation>();
                foreach (var geneAnnotationProvider in annotationProviders)
                {
                    var annotation = geneAnnotationProvider.Annotate(geneName);
                    if (annotation != null) annotations.Add(annotation);
                }
                if(annotations.Count>0) annotatedGenes.Add(new AnnotatedGene(geneName,annotations.ToArray()));
            }

            return annotatedGenes;
        }
    }
}