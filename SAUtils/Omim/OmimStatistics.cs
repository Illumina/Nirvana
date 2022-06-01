using System.Text;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.IO;

namespace SAUtils.Omim;

public class OmimStatistics
{
    public uint                      TotalItems            = 0;
    public uint                      TotalPhenotypes       = 0;
    public CounterDictionary<string> PhenotypeMappings     = new();
    public CounterDictionary<string> PhenotypeInheritances = new();

    public void Add(OmimItem omimItem)
    {
        TotalItems++;

        foreach (OmimItem.Phenotype phenotype in omimItem.Phenotypes)
        {
            TotalPhenotypes++;
            PhenotypeMappings.Add(phenotype.Mapping.ToString());
            foreach (string inheritance in phenotype.Inheritance)
            {
                PhenotypeInheritances.Add(inheritance);
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = StringBuilderPool.Get();
        var jo = new JsonObject(sb);
        sb.Append(JsonObject.OpenBrace);

        jo.AddUIntValue("totalItems",      TotalItems);
        jo.AddUIntValue("totalPhenotypes", TotalPhenotypes);
        jo.AddObjectValue("mappings",     PhenotypeMappings);
        jo.AddObjectValue("inheritances", PhenotypeInheritances);

        sb.Append(JsonObject.CloseBrace);

        return StringBuilderPool.GetStringAndReturn(sb);
    }
}