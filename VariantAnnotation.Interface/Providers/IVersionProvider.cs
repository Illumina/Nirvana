namespace VariantAnnotation.Interface.Providers
{
	public interface IVersionProvider
	{
		string GetProgramVersion();

		string GetDataVersion();
	}
}