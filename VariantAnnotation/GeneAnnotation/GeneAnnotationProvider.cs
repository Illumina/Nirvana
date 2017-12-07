using System.Collections.Generic;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotationProvider:IGeneAnnotationProvider
    {
	    public string Name { get; }
	    public GenomeAssembly GenomeAssembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        private readonly Dictionary<string, IAnnotatedGene> _geneAnnotationDict;

        public IAnnotatedGene Annotate(string geneName)
        {
            return !_geneAnnotationDict.ContainsKey(geneName) ? null : _geneAnnotationDict[geneName];
        }

        public GeneAnnotationProvider(GeneDatabaseReader geneDatabaseReader)
        {
	        Name = "Gene annotation provider";
            DataSourceVersions = geneDatabaseReader.DataSourceVersions;
            _geneAnnotationDict = new Dictionary<string, IAnnotatedGene>();
            CreateGeneMapDict(geneDatabaseReader);
        }

        private void CreateGeneMapDict(GeneDatabaseReader geneDatabaseReader)
        {
            foreach (var geneAnnotation in geneDatabaseReader.Read())
            {
                if (!_geneAnnotationDict.ContainsKey(geneAnnotation.GeneName))
                
                    _geneAnnotationDict[geneAnnotation.GeneName] = geneAnnotation;
                
            }
        }
    }
}