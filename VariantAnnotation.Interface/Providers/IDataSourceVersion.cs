using IO;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.Providers
{
	public interface IDataSourceVersion:IJsonSerializer
	{
		string Name { get; }
		string Description { get; }
		string Version { get; }
		long ReleaseDateTicks { get; }
	    void Write(IExtendedBinaryWriter writer);
    }
}