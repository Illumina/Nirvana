using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.IntermediateIO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using System;
using System.Collections.Generic;
using CacheUtils.Commands.Download;
using CacheUtils.Genbank;
using CacheUtils.Logger;
using Genome;
using IO;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Logger;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public static class ParseVepCacheDirectoryMain
    {
        private static string _inputVepDirectory;
        private static string _inputReferencePath;

        private static string _outputStub;

        private static string _vepReleaseDate;
        private static string _genomeAssembly;
        private static string _transcriptSource;
        private static ushort _vepVersion;

        private static ExitCodes ProgramExecution()
        {
            var logger = new ConsoleLogger();

            var transcriptSource = GetSource(_transcriptSource);
            var sequenceReader   = new CompressedSequenceReader(FileUtilities.GetReadStream(_inputReferencePath));
            var vepRootDirectory = new VepRootDirectory(sequenceReader.RefNameToChromosome);
            var refIndexToVepDir = vepRootDirectory.GetRefIndexToVepDir(_inputVepDirectory);

            var genomeAssembly   = GenomeAssemblyHelper.Convert(_genomeAssembly);
            long vepReleaseTicks = DateTime.Parse(_vepReleaseDate).Ticks;
            var idToGenbank      = GetIdToGenbank(logger, genomeAssembly, transcriptSource);

            // =========================
            // create the pre-cache file
            // =========================

            // process each VEP directory
            int numRefSeqs = sequenceReader.NumRefSeqs;            
            var header     = new IntermediateIoHeader(_vepVersion, vepReleaseTicks, transcriptSource, genomeAssembly, numRefSeqs);

            string siftPath       = _outputStub + ".sift.gz";
            string polyphenPath   = _outputStub + ".polyphen.gz";
            string transcriptPath = _outputStub + ".transcripts.gz";
            string regulatoryPath = _outputStub + ".regulatory.gz";

            using (var mergeLogger            = new TranscriptMergerLogger(FileUtilities.GetCreateStream(_outputStub + ".merge_transcripts.log")))
            using (var siftWriter             = new PredictionWriter(GZipUtilities.GetStreamWriter(siftPath), header, IntermediateIoCommon.FileType.Sift))
            using (var polyphenWriter         = new PredictionWriter(GZipUtilities.GetStreamWriter(polyphenPath), header, IntermediateIoCommon.FileType.Polyphen))
            using (var transcriptWriter       = new MutableTranscriptWriter(GZipUtilities.GetStreamWriter(transcriptPath), header))
            using (var regulatoryRegionWriter = new RegulatoryRegionWriter(GZipUtilities.GetStreamWriter(regulatoryPath), header))
            {
                var converter           = new VepCacheParser(transcriptSource);
                var emptyPredictionDict = new Dictionary<string, List<int>>();

                for (ushort refIndex = 0; refIndex < numRefSeqs; refIndex++)
                {
                    var chromosome = sequenceReader.RefIndexToChromosome[refIndex];

                    if (!refIndexToVepDir.TryGetValue(refIndex, out string vepSubDir))
                    {
                        siftWriter.Write(chromosome, emptyPredictionDict);
                        polyphenWriter.Write(chromosome, emptyPredictionDict);
                        continue;
                    }

                    Console.WriteLine("Parsing reference sequence [{0}]:", chromosome.UcscName);

                    var rawData                 = converter.ParseDumpDirectory(chromosome, vepSubDir);
                    var mergedTranscripts       = TranscriptMerger.Merge(mergeLogger, rawData.Transcripts, idToGenbank);
                    var mergedRegulatoryRegions = RegulatoryRegionMerger.Merge(rawData.RegulatoryRegions);

                    int numRawTranscripts    = rawData.Transcripts.Count;
                    int numMergedTranscripts = mergedTranscripts.Count;
                    Console.WriteLine($"- # merged transcripts: {numMergedTranscripts}, # total transcripts: {numRawTranscripts}");

                    WriteTranscripts(transcriptWriter, mergedTranscripts);
                    WriteRegulatoryRegions(regulatoryRegionWriter, mergedRegulatoryRegions);
                    WritePredictions(siftWriter, mergedTranscripts, x => x.SiftData, chromosome);
                    WritePredictions(polyphenWriter, mergedTranscripts, x => x.PolyphenData, chromosome);
                }
            }

            Console.WriteLine("\n{0} directories processed.", refIndexToVepDir.Count);

            return ExitCodes.Success;
        }

        private static Dictionary<string, GenbankEntry> GetIdToGenbank(ILogger logger, GenomeAssembly assembly, Source source)
        {
            if (assembly != GenomeAssembly.GRCh37 || source != Source.RefSeq) return null;

            logger.Write("- loading the intermediate Genbank file... ");

            Dictionary<string, GenbankEntry> genbankDict;
            using (var reader = new IntermediateIO.GenbankReader(GZipUtilities.GetAppropriateReadStream(ExternalFiles.GenbankFilePath)))
            {
                genbankDict = reader.GetIdToGenbank();
            }

            logger.WriteLine($"{genbankDict.Count} entries loaded.");
            return genbankDict;
        }

        private static void WriteRegulatoryRegions(RegulatoryRegionWriter writer, IEnumerable<IRegulatoryRegion> regulatoryRegions)
        {
            foreach (var regulatoryRegion in regulatoryRegions) writer.Write(regulatoryRegion);
        }

        private static void WriteTranscripts(MutableTranscriptWriter writer, IEnumerable<MutableTranscript> transcripts)
        {
            foreach (var transcript in transcripts) writer.Write(transcript);
        }

        private static void WritePredictions(PredictionWriter writer, IReadOnlyList<MutableTranscript> transcripts,
            Func<MutableTranscript, string> predictionFunc, IChromosome chromosome)
        {
            var predictionDict = new Dictionary<string, List<int>>(StringComparer.Ordinal);

            for (var transcriptIndex = 0; transcriptIndex < transcripts.Count; transcriptIndex++)
            {
                var transcript        = transcripts[transcriptIndex];
                string predictionData = predictionFunc(transcript);
                if (predictionData == null) continue;

                if (predictionDict.TryGetValue(predictionData, out var transcriptIdList)) transcriptIdList.Add(transcriptIndex);
                else predictionDict[predictionData] = new List<int> { transcriptIndex };
            }

            writer.Write(chromosome, predictionDict);
        }

        private static Source GetSource(string source)
        {
            source = source.ToLower();
            if (source.StartsWith("ensembl")) return Source.Ensembl;
            if (source.StartsWith("refseq")) return Source.RefSeq;
            return source.StartsWith("both") ? Source.BothRefSeqAndEnsembl : Source.None;
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "date=",
                    "VEP release {date}",
                    v => _vepReleaseDate = v
                },
                {
                    "source|s=",
                    "transcript {source}",
                    v => _transcriptSource = v
                },
                {
                    "ga=",
                    "genome assembly {version}",
                    v => _genomeAssembly = v
                },
                {
                    "in|i=",
                    "input VEP {directory}",
                    v => _inputVepDirectory = v
                },
                {
                    "out|o=",
                    "output filename {stub}",
                    v => _outputStub = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => _inputReferencePath = v
                },
                {
                    "vep=",
                    "VEP {version}",
                    (ushort v) => _vepVersion = v
                }
            };

            string commandLineExample = $"{command} --in <VEP directory> --out <Nirvana pre-cache file> --vep <VEP version>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .CheckDirectoryExists(_inputVepDirectory, "VEP", "--in")
                .CheckInputFilenameExists(_inputReferencePath, "compressed reference sequence", "--ref")
                .HasRequiredParameter(_outputStub, "output stub", "--out")
                .HasRequiredParameter(_vepVersion, "VEP version", "--vep")
                .HasRequiredParameter(_genomeAssembly, "genome assembly", "--ga")
                .HasRequiredDate(_vepReleaseDate, "VEP release date", "--date")
                .HasRequiredParameter(_transcriptSource, "transcript source", "--source")
                .SkipBanner()
                .ShowHelpMenu("Converts *deserialized* VEP cache files to a Nirvana pre-cache file.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
