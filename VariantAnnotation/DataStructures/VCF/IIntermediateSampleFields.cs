namespace VariantAnnotation.DataStructures.VCF
{
    interface IIntermediateSampleFields
    {
        FormatIndices FormatIndices { get; }
        string[] SampleColumns { get; }
        int? Tir { get; }
        int? Tar { get; }
        int? TotalAlleleCount { get; }
        string VcfRefAllele { get; }
        // ReSharper disable InconsistentNaming
        int? ACount { get; }
        int? CCount { get; }
        int? GCount { get; }
        int? TCount { get; }
        // ReSharper disable UnusedMemberInSuper.Global
        int? MajorChromosomeCount { get; }
        int? CopyNumber { get; }
        // ReSharper restore UnusedMemberInSuper.Global
        int? NR { get; }
        int? NV { get; }
        string[] AltAlleles { get; }
        // ReSharper restore InconsistentNaming
		string RepeatNumber { get; }
        string RepeatNumberSpan { get; }
    }
}
