namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IPluginData
    {
        string Name { get; }
        //Dictionary<string, List<string>> PluginAnnotation { get; }
        string GetJsonString();
    }
}