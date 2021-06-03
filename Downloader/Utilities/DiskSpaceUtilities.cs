using System;
using System.IO;
using CommandLine.Utilities;

namespace Downloader.Utilities
{
    public static class DiskSpaceUtilities
    {
        public static void CheckAvailableDiskSpace(string outputDirectory, long numBytesToDownload)
        {
            string    absolutePath = GetAbsolutePath(outputDirectory);
            DriveInfo driveInfo    = GetDriveWithLongestCommonPrefix(absolutePath);

            // skip available disk space checking if we can't figure out which drive is being used 
            if (driveInfo == null) return;

            long numAvailableBytes = driveInfo.AvailableFreeSpace;
            if (numBytesToDownload <= numAvailableBytes) return;

            string neededSpace    = MemoryUtilities.ToHumanReadable(numBytesToDownload);
            string availableSpace = MemoryUtilities.ToHumanReadable(numAvailableBytes);

            ConsoleEmbellishments.PrintError("Not enough disk space available");
            Console.WriteLine($" in {absolutePath}. Need: {neededSpace}, available: {availableSpace}");
            Environment.Exit(1);
        }

        private static string GetAbsolutePath(string directoryPath)
        {
            var    directoryInfo = new DirectoryInfo(directoryPath);
            string absolutePath  = directoryInfo.FullName;

            // the absolute path in Windows doesn't always provide the drive letter in uppercase
            // this is benign on Linux since the root is always /
            string root = directoryInfo.Root.ToString().ToUpperInvariant();
            return root + absolutePath.Substring(root.Length);
        }

        private static DriveInfo GetDriveWithLongestCommonPrefix(string absolutePath)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            var       maxPrefixLength = 0;
            DriveInfo maxPrefixDrive  = null;

            foreach (DriveInfo d in allDrives)
            {
                // Windows drive letters are always in uppercase
                if (!d.IsReady || !absolutePath.StartsWith(d.Name) || d.Name.Length <= maxPrefixLength) continue;
                maxPrefixLength = d.Name.Length;
                maxPrefixDrive  = d;
            }

            return maxPrefixDrive;
        }
    }
}