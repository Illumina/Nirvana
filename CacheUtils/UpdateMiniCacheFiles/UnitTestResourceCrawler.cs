using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Utilities;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Transcript;

namespace CacheUtils.UpdateMiniCacheFiles
{
    internal sealed class UnitTestResourceCrawler
    {
        #region members

        private readonly ushort _newVepVersion;
        private readonly string _cacheRoot;
        private readonly string _referenceDir;

        private int _numOutdatedFiles;
        private int _numCurrentFiles;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public UnitTestResourceCrawler(string cacheRoot, string referenceDir, ushort newVepVersion)
        {
            _cacheRoot     = cacheRoot;
            _referenceDir  = referenceDir;
            _newVepVersion = newVepVersion;
        }

        /// <summary>
        /// updates the mini-cache files in the specified directory
        /// </summary>
        public void Process(string cacheRootDir)
        {
            var genomeAssemblyDirs = GetGenomeAssemblyDirs(cacheRootDir);
            foreach (var dir in genomeAssemblyDirs) ProcessGenomeAssemblyDir(dir);
        }

        private void ProcessGenomeAssemblyDir(string gaDir)
        {
            var genomeAssembly         = Path.GetFileName(gaDir);
            var compressedSequencePath = GetCompressedSequencePath(_referenceDir, genomeAssembly);
            var renamer                = ChromosomeRenamer.GetChromosomeRenamer(FileUtilities.GetReadStream(compressedSequencePath));
            var cacheFiles             = GetCacheFiles(gaDir, renamer);
            var transcriptDataSources  = GetTranscriptDataSources(cacheFiles);

            Console.WriteLine("GenomeAssembly dir: {0}", gaDir);
            foreach (var ds in transcriptDataSources)
            {
                ProcessTranscriptDataSource(cacheFiles, genomeAssembly, ds);
                Console.WriteLine();
            }
        }

        private void ProcessTranscriptDataSource(IEnumerable<CacheFile> cacheFiles, string genomeAssembly, TranscriptDataSource ds)
        {
            _numOutdatedFiles = 0;
            _numCurrentFiles = 0;

            var bundle = GetDataBundle(genomeAssembly, ds);

            if (bundle == null)
            {
                Console.WriteLine("- skipping transcript data source: {0}", ds);
                return;
            }

            Console.WriteLine("- Transcript data source: {0}", ds);

            foreach (var cacheFile in cacheFiles.OrderBy(x => x.ReferenceIndex))
            {
                if (cacheFile.TranscriptDataSource != ds) continue;
                ProcessCacheFile(cacheFile, bundle);                
            }

            if (_numOutdatedFiles == 0 && _numCurrentFiles > 0)
            {
                Console.WriteLine("  - All {0} files are already up-to-date.", _numCurrentFiles);
            }
        }

        private DataBundle GetDataBundle(string genomeAssembly, TranscriptDataSource ds)
        {
            var compressedSequencePath = GetCompressedSequencePath(_referenceDir, genomeAssembly);
            var cachePrefix            = GetCachePrefix(_cacheRoot, genomeAssembly, ds, _newVepVersion);

            var sequence = new CompressedSequence();

            var bundle = new DataBundle
            {
                Sequence       = sequence,
                SequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedSequencePath), sequence),
                Cache          = CacheUtilities.LoadCache(cachePrefix),
                SiftReader     = CacheUtilities.GetPredictionReader(CacheConstants.SiftPath(cachePrefix)),
                PolyPhenReader = CacheUtilities.GetPredictionReader(CacheConstants.PolyPhenPath(cachePrefix))
            };

            if (bundle.Cache == null) return null;

            bundle.TranscriptForest = CacheUtilities.GetIntervalForest(bundle.Cache.Transcripts, bundle.Sequence.Renamer.NumRefSeqs);
            return bundle;
        }

        private static string GetCachePrefix(string cacheRoot, string genomeAssembly, TranscriptDataSource ds, ushort vepVersion)
        {
            return Path.Combine(cacheRoot, genomeAssembly, $"{ds}{vepVersion}");
        }

        private static string GetCompressedSequencePath(string referenceDir, string genomeAssembly)
        {
            var referenceFiles = Directory.GetFiles(referenceDir, "Homo_sapiens.*.Nirvana.dat");

            foreach (var referencePath in referenceFiles)
            {
                if (referencePath.Contains(genomeAssembly)) return referencePath;
            }

            throw new GeneralException($"Unable to find the reference file for {genomeAssembly} in {referenceDir}");
        }

        private void ProcessCacheFile(CacheFile cacheFile, DataBundle bundle)
        {
            if (IsMiniCacheCurrent(cacheFile))
            {
                cacheFile.Status = UpdateStatus.Current;
                _numCurrentFiles++;
                return;
            }

            Console.WriteLine("  - Processing {0}", Path.GetFileName(cacheFile.CachePath));
            _numOutdatedFiles++;

            var outputCacheDir = Path.GetDirectoryName(cacheFile.CachePath);
            if (outputCacheDir == null) return;

            cacheFile.Status = cacheFile.Updater.Update(bundle, outputCacheDir, _newVepVersion, cacheFile.OutputFiles);

            CleanupFiles(cacheFile.Status, cacheFile.InputFiles, cacheFile.OutputFiles);
            UpdateCounters(cacheFile.Status);
        }

        public static void CleanupFiles(UpdateStatus status, IEnumerable<string> inputFiles,
            IEnumerable<string> outputFiles)
        {
            // successful update
            if (status == UpdateStatus.Current)
            {
                FileOperations.Delete(inputFiles);
                FileOperations.RemoveExtension(outputFiles);
                return;
            }

            // failed update
            FileOperations.Delete(outputFiles);
        }

        private void UpdateCounters(UpdateStatus status)
        {
            switch (status)
            {
                case UpdateStatus.Current:
                    _numCurrentFiles++;
                    break;
                case UpdateStatus.NeedsUpdate:
                    _numOutdatedFiles++;
                    break;
            }
        }

        private static List<CacheFile> GetCacheFiles(string gaDir, ChromosomeRenamer renamer)
        {
            var ndbFiles = Directory.GetFiles(gaDir, "*.ndb");
            var cacheFiles = new List<CacheFile>();

            foreach (var ndbPath in ndbFiles)
            {
                var cacheFile = CacheFile.Create(ndbPath, renamer);
                if (cacheFile.Type == MiniCacheType.Unknown) continue;
                cacheFiles.Add(cacheFile);
            }

            return cacheFiles;
        }

        private static IEnumerable<string> GetGenomeAssemblyDirs(string cacheRootDir)
        {
            var dirs  = Directory.GetDirectories(cacheRootDir, "GRCh*");
            var regex = new Regex(@"^grch(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var genomeAssemblyDirs = new List<string>();

            foreach (var dirPath in dirs)
            {
                var dir = Path.GetFileName(dirPath);
                if (dir == null) continue;

                var match = regex.Match(dir);
                if (match.Success) genomeAssemblyDirs.Add(dirPath);
            }

            return genomeAssemblyDirs;
        }

        private static IEnumerable<TranscriptDataSource> GetTranscriptDataSources(IEnumerable<CacheFile> cacheFiles)
        {
            var transcriptDataSources = new HashSet<TranscriptDataSource>();
            foreach (var cacheFile in cacheFiles) transcriptDataSources.Add(cacheFile.TranscriptDataSource);
            return transcriptDataSources;
        }

        private bool IsMiniCacheCurrent(CacheFile cacheFile)
        {
            var header = GlobalCacheReader.GetHeader(cacheFile.CachePath);
            if (header == null) return false;

            var customHeader = GlobalCacheReader.GetCustomHeader(header);

            return header.DataVersion      == CacheConstants.DataVersion   &&
                   header.SchemaVersion    == CacheConstants.SchemaVersion &&
                   customHeader.VepVersion == _newVepVersion;
        }
    }
}
