namespace Genome
{
    public interface ISequence 
    {
	    int Length { get; }
        Band[] CytogeneticBands { get; }
        string Substring(int offset, int length);
	}
}