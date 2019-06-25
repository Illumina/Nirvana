using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
	public sealed class DbSnpItem: ISupplementaryDataItem
	{
	    public IChromosome Chromosome { get; }
	    public int Position { get; set; }
	    public string RefAllele { get; set; }
	    public string AltAllele { get; set; }

        public long RsId { get; }
	    
	    public DbSnpItem(IChromosome chromosome,
			int position,
			long rsId,
			string refAllele,
			string alternateAllele)
		{
			Chromosome = chromosome;
			Position   = position;
			RsId       = rsId;
			RefAllele  = refAllele;
			AltAllele  = alternateAllele;
			
		}


		public string GetJsonString()
	    {
	        return $"\"rs{RsId}\"";
	    }
	    
	}
}
