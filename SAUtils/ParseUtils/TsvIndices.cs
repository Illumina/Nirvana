namespace SAUtils.ParseUtils;

public struct TsvIndices
{
    public ushort Chromosome = ushort.MaxValue;
    public ushort Start      = ushort.MaxValue;
    public ushort End        = ushort.MaxValue;
    public ushort VariantId  = ushort.MaxValue;
    public ushort SvType     = ushort.MaxValue;
    public ushort Filters    = ushort.MaxValue;

    public ushort AllAlleleCount    = ushort.MaxValue;
    public ushort AfrAlleleCount    = ushort.MaxValue;
    public ushort AmrAlleleCount    = ushort.MaxValue;
    public ushort EasAlleleCount    = ushort.MaxValue;
    public ushort EurAlleleCount    = ushort.MaxValue;
    public ushort OthAlleleCount    = ushort.MaxValue;
    public ushort FemaleAlleleCount = ushort.MaxValue;
    public ushort MaleAlleleCount   = ushort.MaxValue;

    public ushort AllAlleleFrequency    = ushort.MaxValue;
    public ushort AfrAlleleFrequency    = ushort.MaxValue;
    public ushort AmrAlleleFrequency    = ushort.MaxValue;
    public ushort EasAlleleFrequency    = ushort.MaxValue;
    public ushort EurAlleleFrequency    = ushort.MaxValue;
    public ushort OthAlleleFrequency    = ushort.MaxValue;
    public ushort FemaleAlleleFrequency = ushort.MaxValue;
    public ushort MaleAlleleFrequency   = ushort.MaxValue;

    public ushort AllAlleleNumber    = ushort.MaxValue;
    public ushort AfrAlleleNumber    = ushort.MaxValue;
    public ushort AmrAlleleNumber    = ushort.MaxValue;
    public ushort EasAlleleNumber    = ushort.MaxValue;
    public ushort EurAlleleNumber    = ushort.MaxValue;
    public ushort OthAlleleNumber    = ushort.MaxValue;
    public ushort FemaleAlleleNumber = ushort.MaxValue;
    public ushort MaleAlleleNumber   = ushort.MaxValue;

    public ushort AllHomCount    = ushort.MaxValue;
    public ushort AfrHomCount    = ushort.MaxValue;
    public ushort AmrHomCount    = ushort.MaxValue;
    public ushort EasHomCount    = ushort.MaxValue;
    public ushort EurHomCount    = ushort.MaxValue;
    public ushort OthHomCount    = ushort.MaxValue;
    public ushort FemaleHomCount = ushort.MaxValue;
    public ushort MaleHomCount   = ushort.MaxValue;

    public TsvIndices()
    {
    }
}