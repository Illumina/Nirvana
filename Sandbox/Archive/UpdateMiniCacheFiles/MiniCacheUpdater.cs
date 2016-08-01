using System;
using System.IO;
using System.Text.RegularExpressions;
using Illumina.ErrorHandling.Exceptions;
using Illumina.VariantAnnotation.FileHandling;

namespace UpdateMiniCacheFiles
{
    public class MiniCacheUpdaterMain
    {
        #region members

        private readonly Regex _transcriptRegex     = new Regex("^(.+?)_(UF_)?(chr[^_]+)_(\\D+)(\\d+)\\.ndb",                    RegexOptions.Compiled);
        private readonly Regex _regulatoryRegex     = new Regex("^(.+?)_(chr[^_]+)_(\\D+)(\\d+)_reg\\.ndb",                      RegexOptions.Compiled);
        private readonly Regex _positionRegex       = new Regex("^(chr[^_]+)_(\\d+)_([^_]+)_([^_]+)_(UF_)?(\\D+)(\\d+)_pos\\.ndb", RegexOptions.Compiled);
        private readonly Regex _rangedPositionRegex = new Regex("^(chr[^_]+)_(\\d+)_(\\d+)_(\\D+)(\\d+)_pos\\.ndb",              RegexOptions.Compiled);

        private readonly string _rootCacheDirectory;
        private readonly string _rootUnfilteredCacheDirectory;
        private readonly ushort _desiredVepVersion;

        #endregion

        // constructor
        public MiniCacheUpdaterMain(string rootCacheDirectory, string rootUnfilteredCacheDirectory, ushort desiredVepVersion, string inputCompressedReferencePath)
        {
            _rootCacheDirectory           = rootCacheDirectory;
            _rootUnfilteredCacheDirectory = rootUnfilteredCacheDirectory;
            _desiredVepVersion            = desiredVepVersion;

            // ReSharper disable once UnusedVariable
            var compressedSequenceReader = new CompressedSequenceReader(inputCompressedReferencePath);
        }

        /// <summary>
        /// updates the mini-cache files in the specified directory
        /// </summary>
        public void Process(string inputDirectory)
        {
            var ndbFiles = Directory.GetFiles(inputDirectory, "*.ndb");

            // sanity check: make sure we have some mini-cache files
            if (ndbFiles == null)
            {
                throw new UserErrorException("The specified directory (" + inputDirectory + ") does not contain any ndb files.");
            }

            foreach (var ndbFile in ndbFiles) UpdateMiniCache(ndbFile, _desiredVepVersion);
        }

        /// <summary>
        /// updates a mini-cache file
        /// </summary>
        private void UpdateMiniCache(string miniCachePath, int desiredVepVersion)
        {
            string miniCacheFilename = Path.GetFileName(miniCachePath);
            if (miniCacheFilename == null) return;

            // grab the header
            var header = NirvanaDatabaseReader.GetHeader(miniCachePath, false);
            if (header == null) return;

            // return if it looks like we have already upgraded the file
            if ((header.DataVersion   == NirvanaDatabaseCommon.DataVersion)   &&
                (header.SchemaVersion == NirvanaDatabaseCommon.SchemaVersion) &&
                (header.VepVersion    == desiredVepVersion))
                return;

            Console.Write("- checking {0}... ", miniCacheFilename);

            // check if this is a normal transcript mini-cache file
            var match = _transcriptRegex.Match(miniCacheFilename);
            if (match.Success)
            {
                var updater = new TranscriptUpdater(_rootCacheDirectory, _rootUnfilteredCacheDirectory, _desiredVepVersion);
                if (updater.Update(miniCachePath, match) == UpdateStatus.Updated)
                {
                    Console.WriteLine("updated.");
                    return;
                }

                Console.WriteLine("entry not found.");
                return;
            }

            // check if this is a regulatory
            match = _regulatoryRegex.Match(miniCacheFilename);
            if (match.Success)
            {
                var updater = new RegulatoryUpdater(_rootCacheDirectory, _rootUnfilteredCacheDirectory, _desiredVepVersion);
                if (updater.Update(miniCachePath, match) == UpdateStatus.Updated)
                {
                    Console.WriteLine("updated.");
                    return;
                }

                Console.WriteLine("entry not found.");
                return;
            }

            // check if this is a position
            match = _positionRegex.Match(miniCacheFilename);
            if (match.Success)
            {
                var updater = new PositionUpdater(_rootCacheDirectory, _rootUnfilteredCacheDirectory, _desiredVepVersion);
                if (updater.Update(miniCachePath, match) == UpdateStatus.Updated)
                {
                    Console.WriteLine("updated.");
                    return;
                }

                Console.WriteLine("entry not found.");
                return;
            }

            // check if this is a ranged position
            match = _rangedPositionRegex.Match(miniCacheFilename);
            if (match.Success)
            {
                var updater = new PositionRangeUpdater(_rootCacheDirectory, _rootUnfilteredCacheDirectory, _desiredVepVersion);
                if (updater.Update(miniCachePath, match) == UpdateStatus.Updated)
                {
                    Console.WriteLine("updated.");
                    return;
                }

                Console.WriteLine("entry not found.");
                return;
            }

            Console.WriteLine("all regexs failed.");
        }
    }
}
