namespace VariantAnnotation.Interface.Sequence
{
    public interface ISequence 
    {
	    int Length { get; }
        string Substring(int offset, int length);
	}
}