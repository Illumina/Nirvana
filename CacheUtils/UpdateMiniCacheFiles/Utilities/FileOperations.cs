using System;
using System.Collections.Generic;
using System.IO;

namespace CacheUtils.UpdateMiniCacheFiles.Utilities
{
    public static class FileOperations
    {
        public static void Delete(IEnumerable<string> fileList)
        {
            foreach (var filePath in fileList) if (File.Exists(filePath)) File.Delete(filePath);
        }

        public static void RemoveExtension(IEnumerable<string> fileList)
        {
            foreach (var filePath in fileList)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"- skipping deletion of {filePath}");
                    continue;
                }

                var newPath = GetFullPathWithoutExtension(filePath);
                if (newPath == null) continue;

                if (File.Exists(newPath)) File.Delete(newPath);
                File.Move(filePath, newPath);
            }
        }

        public static string GetFullPathWithoutExtension(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (directory == null) return null;

            var filenameStub = Path.GetFileNameWithoutExtension(path);
            if (filenameStub == null) return null;

            return Path.Combine(directory, filenameStub);
        }
    }
}
