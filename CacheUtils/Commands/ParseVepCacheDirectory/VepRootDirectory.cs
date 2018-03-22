using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public sealed class VepRootDirectory
    {
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;

        public VepRootDirectory(IDictionary<string, IChromosome> refNameToChromosome)
        {
            _refNameToChromosome = refNameToChromosome;
        }

        public Dictionary<ushort, string> GetRefIndexToVepDir(string dirPath)
        {
            var vepDirectories = Directory.GetDirectories(dirPath);
            var referenceDict  = new Dictionary<ushort, string>();

            foreach (string dir in vepDirectories)
            {
                string referenceName = Path.GetFileName(dir);
                var chromosome    = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, referenceName);
                if (chromosome.IsEmpty()) continue;
                
                referenceDict[chromosome.Index] = dir;
            }

            return referenceDict;
        }

        public static IEnumerable<string> GetSortedFiles(IEnumerable<string> filePaths)
        {
            var sortedPaths = new SortedDictionary<int, string>();

            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName == null) continue;

                int hyphenPos = fileName.IndexOf("-", StringComparison.Ordinal);
                if (hyphenPos == -1) throw new InvalidDataException($"Could not find the hyphen in: [{fileName}]");

                int position = int.Parse(fileName.Substring(0, hyphenPos));
                sortedPaths[position] = filePath;
            }

            return sortedPaths.Values.ToArray();
        }
    }
}
