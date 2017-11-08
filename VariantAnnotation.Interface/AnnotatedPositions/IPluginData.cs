using System.Collections.Generic;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IPluginData
    {
        string PluginName { get; }
        Dictionary<string, List<string>> PluginAnnotation { get; }
    }
}