namespace VariantAnnotation.Interface.Providers
{
    public interface ITranscriptAnnotationProvider : IAnnotationProvider
    {
        ushort VepVersion { get; }
    }
}
