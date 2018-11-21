using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.DataDumperImport.Import;
using CacheUtils.DataDumperImport.IO;
using Compression.Utilities;
using Genome;
using IO;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public sealed class VepCacheParser
    {
        private readonly Source _source;
        private readonly TranscriptIdFilter _filter;

        public VepCacheParser(Source source)
        {
            _source = source;
            _filter = new TranscriptIdFilter(source);
        }

        public (List<IRegulatoryRegion> RegulatoryRegions, List<MutableTranscript> Transcripts) ParseDumpDirectory(
            IChromosome chromosome, string dirPath)
        {
            var regulatoryRegions = ParseRegulatoryFiles(chromosome, dirPath);
            var transcripts       = ParseTranscriptFiles(chromosome, dirPath);
            return (regulatoryRegions, transcripts);
        }

        private static List<IRegulatoryRegion> ParseRegulatoryFiles(IChromosome chromosome, string dirPath)
        {
            var regulatoryRegions = new List<IRegulatoryRegion>();
            var files = FileUtilities.GetFileNamesInDir(dirPath, "*_reg_regulatory_regions_data_dumper.txt.gz")
                    .ToArray();

            foreach (string dumpPath in VepRootDirectory.GetSortedFiles(files))
            {
                ParseRegulatoryDumpFile(chromosome, dumpPath, regulatoryRegions);
            }

            return regulatoryRegions;
        }

        private List<MutableTranscript> ParseTranscriptFiles(IChromosome chromosome, string dirPath)
        {
            var transcripts = new List<MutableTranscript>();
            var files = FileUtilities.GetFileNamesInDir(dirPath, "*_transcripts_data_dumper.txt.gz").ToArray();

            foreach (string dumpPath in VepRootDirectory.GetSortedFiles(files))
            {
                ParseTranscriptDumpFile(chromosome, dumpPath, transcripts);
            }

            return transcripts;
        }

        private static void ParseRegulatoryDumpFile(IChromosome chromosome, string filePath,
            ICollection<IRegulatoryRegion> regulatoryRegions)
        {
            Console.WriteLine("- processing {0}", Path.GetFileName(filePath));

            using (var reader = new DataDumperReader(GZipUtilities.GetAppropriateReadStream(filePath)))
            {
                foreach (var ad in reader.GetRootNode().Value.Values)
                {
                    if (!(ad is ObjectKeyValueNode objectKeyValue)) continue;

                    foreach (var featureGroup in objectKeyValue.Value.Values)
                    {
                        switch (featureGroup.Key)
                        {
                            case "MotifFeature":
                                // not used
                                break;
                            case "RegulatoryFeature":
                                ParseRegulatoryRegions(chromosome, featureGroup, regulatoryRegions);
                                break;
                            default:
                                throw new InvalidDataException("Found an unexpected feature group (" + featureGroup.Key + ") in the regulatory regions file.");
                        }
                    }
                }
            }
        }

        private void ParseTranscriptDumpFile(IChromosome chromosome, string filePath,
            ICollection<MutableTranscript> transcripts)
        {
            Console.WriteLine("- processing {0}", Path.GetFileName(filePath));

            using (var reader = new DataDumperReader(GZipUtilities.GetAppropriateReadStream(filePath)))
            {
                foreach (var node in reader.GetRootNode().Value.Values)
                {
                    if (!(node is ListObjectKeyValueNode transcriptNodes)) continue;

                    foreach (var tNode in transcriptNodes.Values)
                    {
                        if (!(tNode is ObjectValueNode transcriptNode)) throw new InvalidOperationException("Expected a transcript object value node, but the current node is not an object value.");
                        if (transcriptNode.Type != "Bio::EnsEMBL::Transcript") throw new InvalidOperationException($"Expected a transcript node, but the current data type is: [{transcriptNode.Type}]");

                        var transcript = ImportTranscript.Parse(transcriptNode, chromosome, _source);
                        if (_filter.Pass(transcript)) transcripts.Add(transcript);
                    }
                }
            }
        }

        private static void ParseRegulatoryRegions(IChromosome chromosome, IImportNode featureGroupNode,
            ICollection<IRegulatoryRegion> regulatoryRegions)
        {
            if (!(featureGroupNode is ListObjectKeyValueNode regulatoryFeatureNodes)) return;

            foreach (var node in regulatoryFeatureNodes.Values)
            {
                if (!(node is ObjectValueNode regulatoryFeatureNode))                         throw new InvalidOperationException("Expected a regulatory region object value node, but the current node is not an object value.");
                if (regulatoryFeatureNode.Type != "Bio::EnsEMBL::Funcgen::RegulatoryFeature") throw new InvalidOperationException($"Expected a regulatory region node, but the current data type is: [{regulatoryFeatureNode.Type}]");

                var regulatoryRegion = ImportRegulatoryFeature.Parse(regulatoryFeatureNode, chromosome);
                regulatoryRegions.Add(regulatoryRegion);
            }
        }
    }
}
