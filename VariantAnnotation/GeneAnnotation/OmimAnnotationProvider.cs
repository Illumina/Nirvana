using System.Collections.Generic;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.GeneAnnotation
{
    public class OmimAnnotationProvider:IGeneAnnotationProvider
    {
	    public string Name { get; }
	    public GenomeAssembly GenomeAssembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        private readonly Dictionary<string, IAnnotatedGene> _omimGeneDict;
        public IAnnotatedGene Annotate(string geneName)
        {
            if (!_omimGeneDict.ContainsKey(geneName)) return null;
            return _omimGeneDict[geneName];

        }

        public OmimAnnotationProvider(GeneDatabaseReader omimDatabaseReader)
        {
	        Name = "Omim annotation provider";
            DataSourceVersions = omimDatabaseReader.DataSourceVersions;
            _omimGeneDict = new Dictionary<string, IAnnotatedGene>();
            CreateGeneMapDict(omimDatabaseReader);
        }

        private void CreateGeneMapDict(GeneDatabaseReader omimDatabaseReader)
        {

            foreach (var omimAnnotation in omimDatabaseReader.Read())
            {
                if (!_omimGeneDict.ContainsKey(omimAnnotation.GeneName))
                
                    _omimGeneDict[omimAnnotation.GeneName] = omimAnnotation;
                
            }

        }
    }
}