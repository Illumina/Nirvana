namespace SAUtils.Omim.EntryApiResponse
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable ClassNeverInstantiated.Global
    public class GeneMap
    {
        public string         geneName;
        public int            mimNumber;
        public PhenotypeMap[] phenotypeMapList;
    }

    public class PhenotypeMap
    {
        public PhenotypeMapItem phenotypeMap;
    }

    public class PhenotypeMapItem
    {
        public int    phenotypeMimNumber;
        public string phenotype;
        public int    phenotypeMappingKey;
        public string phenotypeInheritance;
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore InconsistentNaming
}