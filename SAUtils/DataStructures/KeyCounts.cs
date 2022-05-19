using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures;

public class KeyCounts: IJsonSerializer
{
    public readonly Dictionary<string, int> Counts;

    public KeyCounts(IEnumerable<string> keys)
    {
        Counts = new ();
        foreach (var key in keys)
        {
            Counts[key] = 0;
        }
    }

    public void Increment(string key)
    {
        Counts[key]++;
    }

    public void SerializeJson(StringBuilder sb)
    {
        var jo = new JsonObject(sb);
        sb.Append(JsonObject.OpenBrace);
        foreach (var (key, count) in Counts)
        {
            jo.AddIntValue(key, count);
        }
        
        sb.Append(JsonObject.CloseBrace);
    }
}
