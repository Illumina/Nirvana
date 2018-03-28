using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;

namespace SAUtils.MergeInterimTsvs
{
    public static class ValidatorInSaExtensions
    {
        public static IConsoleAppValidator GetTsvAndIntervalFiles(this IConsoleAppValidator validator, string tsvFilesDirectory, List<string> intermediateFiles, List<string> intervalFiles,List<string> geneTsvFiles, ref string miscFiles)
        {
            if (!Directory.Exists(tsvFilesDirectory)) return validator;
            foreach (var file in Directory.GetFiles(tsvFilesDirectory))
            {
                if (!file.EndsWith(".tsv.gz")) continue;
                if (file.EndsWith(".interval.tsv.gz"))
                {
                    intervalFiles.Add(file);
                    continue;
                }
                if (file.EndsWith(".misc.tsv.gz"))
                {
                    miscFiles = file;
                    continue;
                }
                if (file.EndsWith(".gene.tsv.gz"))
                {
                    geneTsvFiles.Add(file);
                    continue;
                }
                intermediateFiles.Add(file);
            }
            return validator;
        }
    }
}