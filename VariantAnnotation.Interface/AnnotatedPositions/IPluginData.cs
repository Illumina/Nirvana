namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IPluginData
    {
        string Name { get; }
        string GetJsonString();
    }
}