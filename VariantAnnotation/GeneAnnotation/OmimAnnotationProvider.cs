using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<string, List<OmimEntry>> _omimGeneDict;
        public IGeneAnnotation Annotate(string geneName)
        {
            if (!_omimGeneDict.ContainsKey(geneName)) return null;
            return new GeneAnnotation("omim",_omimGeneDict[geneName].Select(x=>x.ToString()).ToArray(),true);

        }

        public OmimAnnotationProvider(OmimDatabaseReader omimDatabaseReader)
        {
	        Name = "Omim annotation provider";
            DataSourceVersions = new []{ omimDatabaseReader.DataVersion};
            _omimGeneDict = new Dictionary<string, List<OmimEntry>>();
            CreateGeneMapDict(omimDatabaseReader);
        }

        private  void CreateGeneMapDict(OmimDatabaseReader omimDatabaseReader)
        {

            foreach (var omimAnnotation in omimDatabaseReader.Read())
            {
                if (!_omimGeneDict.ContainsKey(omimAnnotation.Hgnc))
                {
                    _omimGeneDict[omimAnnotation.Hgnc] = new List<OmimEntry>();
                }
                _omimGeneDict[omimAnnotation.Hgnc].Add(omimAnnotation);
            }

        }
    }
}