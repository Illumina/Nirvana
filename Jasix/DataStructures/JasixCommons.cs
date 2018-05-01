
namespace Jasix.DataStructures
{
	public  static class JasixCommons
	{
	    public const int Version = 1;
	    public const string FileExt = ".jsi";

	    public const string GenesSectionTag = "genes";
	    public const string HeaderSectionTag = "header";
	    public const string PositionsSectionTag = "positions";

	    private const int MaxVariantLength = 50;
		public const int MinNodeWidth = MaxVariantLength;
		public const int PreferredNodeCount = MaxVariantLength*2;
	}
}
