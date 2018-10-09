namespace OrchestrationLambda
{
    public class ChromosomePartition
    {
        public string UcscName;
        public string EnsemblName;
        public ushort ChrIndex;
        public int[] PartitionEnds;

        public ChromosomePartition(string ucscName, string ensemblName, ushort chrIndex, int[] partitionEnds)
        {
            UcscName = ucscName;
            EnsemblName = ensemblName;
            ChrIndex = chrIndex;
            PartitionEnds = partitionEnds;
        }
    }
}