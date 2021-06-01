// ReSharper disable InconsistentNaming
// ReSharper disable SuggestBaseTypeForParameter

using System.Text.Json;

namespace SAUtils.CosmicGeneFusions.Conversion
{
    public sealed record CosmicGeneFusion(string id, int numSamples, string[] geneSymbols, string hgvsr, CosmicCount[] histologies,
        CosmicCount[] sites, int[] pubMedIds)
    {
        public override string ToString()
        {
            string json = JsonSerializer.Serialize(this);
            return json.Substring(1, json.Length - 2);
        }
    }

    public sealed record CosmicCount(string name, int numSamples);
}