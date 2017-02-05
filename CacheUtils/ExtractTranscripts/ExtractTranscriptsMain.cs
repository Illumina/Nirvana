using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.UpdateMiniCacheFiles;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Updaters;
using CacheUtils.UpdateMiniCacheFiles.Utilities;
using ErrorHandling.Exceptions;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;

namespace CacheUtils.ExtractTranscripts
{
    sealed class ExtractTranscriptsMain : AbstractCommandLineHandler
    {
        #region members
        
        private bool _useTranscriptUpdater;
        private bool _useMultiTranscriptUpdater;
        private bool _usePositionUpdater;
        private bool _usePositionRangeUpdater;

        #endregion

        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "aa=",
                    "alternate {allele}",
                    v => ConfigurationSettings.AlternateAllele = v
                },
                {
                    "in|i=",
                    "input cache {prefix}",
                    v => ConfigurationSettings.InputPrefix = v
                },
                {
                    "name|n=",
                    "reference {name}",
                    v => ConfigurationSettings.ReferenceName = v
                },
                {
                    "out|o=",
                    "output Nirvana cache {file}",
                    v => ConfigurationSettings.OutputDirectory = v
                },
                {
                    "pos|p=",
                    "reference {position}",
                    (int v) => ConfigurationSettings.ReferencePosition = v
                },
                {
                    "endpos=",
                    "reference end {position}",
                    (int v) => ConfigurationSettings.ReferenceEndPosition = v
                },
                {
                    "ra=",
                    "reference {allele}",
                    v => ConfigurationSettings.ReferenceAllele = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => ConfigurationSettings.InputReferencePath = v
                },
                {
                    "transcript|t=",
                    "transcript {ID}",
                    v => ConfigurationSettings.TranscriptIds.Add(v)
                }
            };

            var commandLineExample = $"{command}\n"+
                                     "       --in <prefix> --out <dir> -r <ref path> -t <ID>\n" +
                                     "       --in <prefix> --out <dir> -r <ref path> --chr <name> -p <pos> --ra <base> --aa <base>\n" +
                                     "       --in <prefix> --out <dir> -r <ref path> --chr <name> -p <pos> --endpos <pos>\n";

            var extractor = new ExtractTranscriptsMain("Extracts transcripts from Nirvana cache files.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        // constructor
        private ExtractTranscriptsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            _useTranscriptUpdater = ConfigurationSettings.TranscriptIds.Count == 1;

            _useMultiTranscriptUpdater = ConfigurationSettings.TranscriptIds.Count > 1;

            _usePositionUpdater = !string.IsNullOrEmpty(ConfigurationSettings.ReferenceName) &&
                                  !string.IsNullOrEmpty(ConfigurationSettings.ReferenceAllele) &&
                                  !string.IsNullOrEmpty(ConfigurationSettings.AlternateAllele) &&
                                  ConfigurationSettings.ReferencePosition != -1;

            _usePositionRangeUpdater = !string.IsNullOrEmpty(ConfigurationSettings.ReferenceName) &&
                                       ConfigurationSettings.ReferencePosition != -1 &&
                                       ConfigurationSettings.ReferenceEndPosition != -1;

            HasRequiredParameter(ConfigurationSettings.InputPrefix, "Nirvana cache", "--in");
			CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference sequence", "--ref");
			CheckDirectoryExists(ConfigurationSettings.OutputDirectory, "output cache", "--out");

            // make sure we have exactly one option selected
            int numSelectedOptions = 0;
            if (_useTranscriptUpdater)      numSelectedOptions++;
            if (_useMultiTranscriptUpdater) numSelectedOptions++;
            if (_usePositionUpdater)        numSelectedOptions++;
            if (_usePositionRangeUpdater)   numSelectedOptions++;

            HasOnlyOneOption(numSelectedOptions, "[--transcript], [--chr, --pos, --ra, --aa], or [--chr, --pos, --endpos]");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var bundle = GetDataBundle(ConfigurationSettings.InputReferencePath, ConfigurationSettings.InputPrefix);
            var header = bundle.Cache.Header.Custom as GlobalCustomHeader;
            if (header == null) throw new InvalidCastException("Unable to cast the custom header as a GlobalCustomHeader");

            IUpdater updater         = null;
            var outputFiles          = new List<string>();
            var transcriptDataSource = bundle.Cache.Header.TranscriptSource.ToString();

            if (_useTranscriptUpdater)
            {
                var transcripts = GetDesiredTranscripts(bundle, ConfigurationSettings.TranscriptIds);
                if (transcripts.Count == 0) throw new UserErrorException($"Unable to find the desired transcript: {ConfigurationSettings.TranscriptIds}");

                var transcriptId = ConfigurationSettings.TranscriptIds.First();
                var transcript   = transcripts[0];

                updater = new TranscriptUpdater(transcriptId, transcript.ReferenceIndex, transcriptDataSource);
            }
            else if (_useMultiTranscriptUpdater)
            {
                var transcripts = GetDesiredTranscripts(bundle, ConfigurationSettings.TranscriptIds);
                if (transcripts.Count < 2) throw new UserErrorException($"Unable to find two or more transcripts: {ConfigurationSettings.TranscriptIds}");

                var ids = new List<string>();
                ids.AddRange(ConfigurationSettings.TranscriptIds);

                updater = new MultiTranscriptUpdater(ids, transcriptDataSource);
            }
            else if (_usePositionUpdater)
            {
                var refIndex = bundle.Sequence.Renamer.GetReferenceIndex(ConfigurationSettings.ReferenceName);
                updater = new PositionUpdater(refIndex, ConfigurationSettings.ReferencePosition,
                    ConfigurationSettings.ReferenceAllele, ConfigurationSettings.AlternateAllele, transcriptDataSource);
            }
            else if (_usePositionRangeUpdater)
            {
                var refIndex = bundle.Sequence.Renamer.GetReferenceIndex(ConfigurationSettings.ReferenceName);
                updater = new PositionRangeUpdater(refIndex, ConfigurationSettings.ReferencePosition,
                    ConfigurationSettings.ReferenceEndPosition, transcriptDataSource);
            }

            if (updater == null) throw new NullReferenceException("The IUpdater is null.");

            var status = updater.Update(bundle, ConfigurationSettings.OutputDirectory, header.VepVersion, outputFiles);

            UnitTestResourceCrawler.CleanupFiles(status, new List<string>(), outputFiles);

            if (status != UpdateStatus.Current)
            {
                throw new UserErrorException($"Unable to create the mini-cache file. Status: {status}");
            }

            Console.WriteLine();
            Console.WriteLine("- created the following files:");
            foreach (var path in outputFiles) Console.WriteLine(Path.GetFileNameWithoutExtension(path));
        }

        private static List<Transcript> GetDesiredTranscripts(DataBundle bundle, HashSet<string> transcriptIds)
        {
            return bundle.Cache.Transcripts.Where(transcript => transcriptIds.Contains(transcript.Id.ToString())).ToList();
        }

        private static DataBundle GetDataBundle(string compressedSequencePath, string cachePrefix)
        {
            Console.Write("- loading global cache and reference sequence... ");
            var sequence = new CompressedSequence();

            var bundle = new DataBundle
            {
                Sequence       = sequence,
                SequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedSequencePath), sequence),
                Cache          = CacheUtilities.LoadCache(cachePrefix),
                SiftReader     = CacheUtilities.GetPredictionReader(CacheConstants.SiftPath(cachePrefix)),
                PolyPhenReader = CacheUtilities.GetPredictionReader(CacheConstants.PolyPhenPath(cachePrefix))
            };

            bundle.TranscriptForest = CacheUtilities.GetIntervalForest(bundle.Cache.Transcripts, bundle.Sequence.Renamer.NumRefSeqs);

            Console.WriteLine("finished.");

            return bundle.Cache == null ? null : bundle;
        }
    }
}
