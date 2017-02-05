namespace VariantAnnotation.Interface
{
	public interface IVariant
	{
		string ReferenceName { get; }
		int ReferencePosition { get; }
		string ReferenceAllele { get; }
		string[] AlternateAlleles { get; }
		string[] Fields { get; }

        bool IsGatkGenomeVcf { get; }
        string VcfLine { get; }
    }
}
