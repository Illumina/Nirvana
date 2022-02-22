using System.IO;
using Genome;

namespace Cache.Utilities;

public static class CacheFileUtilities
{
    private const string FileSuffix  = ".ndb";
    private const string IndexSuffix = ".idx";

    public static string GetGenomeSymbolsPath(string cacheDir) => Path.Combine(cacheDir, "GeneSymbols" + FileSuffix);

    public static string[] GetCacheFiles(string cacheDir, GenomeAssembly assembly) =>
        Directory.GetFiles(cacheDir, $"{assembly}.*.ndb");
}