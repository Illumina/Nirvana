using System.Collections.Generic;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public interface IAnnotatedSaDataSource
	{
        ISaDataSource SaDataSource { get; }
        bool IsAlleleSpecific { get; }
	    IList<string> GetJsonStrings();
	    IEnumerable<string> GetVcfStrings();
	}
}