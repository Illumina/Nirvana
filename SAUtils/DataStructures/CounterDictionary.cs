using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures;

public sealed class CounterDictionary<TKey> : Dictionary<TKey, uint>, IJsonSerializer
{
    public uint Total;

    public void Add(TKey key)
    {
        Total++;

        if (TryGetValue(key, out uint _))
        {
            this[key]++;
            return;
        }

        this[key] = 1;
    }

    public void SerializeJson(StringBuilder sb)
    {
        var jo = new JsonObject(sb);
        sb.Append(JsonObject.OpenBrace);
        jo.AddUIntValue("count", Total);
        foreach ((TKey key, uint count) in this)
        {
            jo.AddUIntValue(key.ToString(), count);
        }

        sb.Append(JsonObject.CloseBrace);
    }
}