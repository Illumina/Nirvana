using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using VariantAnnotation;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace Nirvana
{
    public class NirvanaAnnotator : AbstractCommandLineHandler
    {
        private readonly VcfConversion _conversion = new VcfConversion();

        private NirvanaAnnotator(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {}

        protected override void ProgramExecution()
        {
            if (!Console.IsOutputRedirected) Console.WriteLine("Running Nirvana on {0}:", GetFileName());

            var outputVcfPath      = ConfigurationSettings.OutputFileName + ".vcf.gz";
            var outputGvcfPath     = ConfigurationSettings.OutputFileName + ".genome.vcf.gz";
            var outputVariantsPath = ConfigurationSettings.OutputFileName + ".json.gz";

            using (var reader = GetVcfReader())
            {
                var booleanArguments = SetAnnotationFlags(reader.IsRcrsMitochondrion);
                var annotator        = GetAnnotator(reader.SampleNames, booleanArguments);

                if (annotator == null)
                {
                    throw new InvalidOperationException("Unable to perform annotation because no annotation sources could be created");
                }

                using (var jsonStreamWriter = GetJsonStreamWriter(outputVariantsPath))
                using (var jsonWriter       = GetJsonWriter(jsonStreamWriter, Date.GetTimeStamp, annotator, reader))
                using (var vcfWriter        = ConfigurationSettings.Vcf  ? new LiteVcfWriter(outputVcfPath, reader.HeaderLines, annotator.GetDataVersion(), annotator.GetDataSourceVersions())  : null)
                using (var gvcfWriter       = ConfigurationSettings.Gvcf ? new LiteVcfWriter(outputGvcfPath, reader.HeaderLines, annotator.GetDataVersion(), annotator.GetDataSourceVersions()) : null)
                {
                    string vcfLine = null;
                    var checker = new SortedVcfChecker();

                    try
                    {
                        while (true)
                        {
                            vcfLine = reader.ReadLine();
                            if (vcfLine == null) break;

                            var fields = vcfLine.Split('\t');
                            if (fields.Length < VcfCommon.MinNumColumns) continue;

                            var variant = new VcfVariant(fields, vcfLine, reader.IsGatkGenomeVcf);
                            checker.CheckVcfOrder(variant.ReferenceName);

                            var annotatedVariant = annotator.Annotate(variant);

                            var vcfOutput = GetVcfString(variant, annotatedVariant);
                            WriteOutput(vcfOutput, annotatedVariant, vcfWriter, gvcfWriter, jsonWriter);
                        }

                        WriteOmim(annotator, jsonWriter);
                    }
                    catch (Exception e)
                    {
                        // embed the vcf line
                        e.Data["VcfLine"] = vcfLine;
                        throw;
                    }
                }

                annotator.FinalizeMetrics();
            }
        }

        private static StreamWriter GetJsonStreamWriter(string outputPath)
        {
            return ConfigurationSettings.OutputFileName == "-"
                ? new StreamWriter(Console.OpenStandardOutput())
                : GZipUtilities.GetStreamWriter(outputPath);
        }

        private static UnifiedJsonWriter GetJsonWriter(StreamWriter streamWriter, string jsonCreationTime, IAnnotationSource annotator, LiteVcfReader reader)
        {
            return new UnifiedJsonWriter(streamWriter, jsonCreationTime, annotator.GetDataVersion(), annotator.GetDataSourceVersions(), annotator.GetGenomeAssembly(), reader.SampleNames);
        }

        private static string GetFileName()
        {
            return Console.IsInputRedirected ? "stdin" : Path.GetFileName(ConfigurationSettings.VcfPath);
        }

        private LiteVcfReader GetVcfReader()
        {
            var useStdInput = ConfigurationSettings.VcfPath == "-";

            var peekStream =
                new PeekStream(useStdInput
                    ? Console.OpenStandardInput()
                    : FileUtilities.GetReadStream(ConfigurationSettings.VcfPath));

            return new LiteVcfReader(GZipUtilities.GetAppropriateStream(peekStream));
        }

        private string GetVcfString(IVariant variant, IAnnotatedVariant annotatedVariant)
        {
            return ConfigurationSettings.Vcf || ConfigurationSettings.Gvcf
                ? _conversion.Convert(variant, annotatedVariant)
                : null;
        }

        private static IAnnotationSource GetAnnotator(string[] sampleNames, List<string> booleanArguments)
        {
            var annotatorInfo  = new AnnotatorInfo(sampleNames, booleanArguments);
            var annotatorPaths = new AnnotatorPath(ConfigurationSettings.InputCachePrefix,
                ConfigurationSettings.CompressedReferencePath, ConfigurationSettings.SupplementaryAnnotationDirectory,
                ConfigurationSettings.CustomAnnotationDirectories, ConfigurationSettings.CustomIntervalDirectories);
            return new AnnotationSourceFactory().CreateAnnotationSource(annotatorInfo, annotatorPaths);
        }

        private static List<string> SetAnnotationFlags(bool usingRcrsMitochondrion)
        {
            var booleanArguments = new List<string>();
            if (ConfigurationSettings.EnableReferenceNoCalls)                                 booleanArguments.Add(AnnotatorInfoCommon.ReferenceNoCall);
            if (ConfigurationSettings.LimitReferenceNoCallsToTranscripts)                     booleanArguments.Add(AnnotatorInfoCommon.TranscriptOnlyRefNoCall);
            if (ConfigurationSettings.ForceMitochondrialAnnotation || usingRcrsMitochondrion) booleanArguments.Add(AnnotatorInfoCommon.EnableMitochondrialAnnotation);
            if (ConfigurationSettings.ReportAllSvOverlappingTranscripts)                      booleanArguments.Add(AnnotatorInfoCommon.ReportAllSvOverlappingTranscripts);
            if (ConfigurationSettings.EnableLoftee)                                           booleanArguments.Add(AnnotatorInfoCommon.EnableLoftee);
            return booleanArguments;
        }

        private static void WriteOmim(IAnnotationSource annotator, UnifiedJsonWriter unifiedJsonWriter)
        {
            var omimAnnotations = new List<string>();
            annotator.AddGeneLevelAnnotation(omimAnnotations);
            var annotionOutput = UnifiedJson.GetGeneAnnotation(omimAnnotations, "omim");
            unifiedJsonWriter.Write(annotionOutput);
        }

        private void WriteOutput(string vcfOutput, IAnnotatedVariant annotatedVariant, LiteVcfWriter vcfWriter,
            LiteVcfWriter gvcfWriter, UnifiedJsonWriter jsonWriter)
        {
            if (ConfigurationSettings.Gvcf) WriteGvcf(vcfOutput, gvcfWriter);

            if (annotatedVariant?.AnnotatedAlternateAlleles == null ||
                annotatedVariant.AnnotatedAlternateAlleles.Count == 0) return;

            var firstAllele = annotatedVariant.AnnotatedAlternateAlleles[0];
            WriteJson(annotatedVariant, firstAllele, jsonWriter);
            if (ConfigurationSettings.Vcf) WriteVcf(vcfOutput, firstAllele, vcfWriter);
        }

        private static void WriteVcf(string output, IAnnotatedAlternateAllele firstAllele,
            LiteVcfWriter writer)
        {
            if (firstAllele.IsReference && !firstAllele.IsReferenceMinor) return;
            writer.Write(output);
        }

        private static void WriteGvcf(string output, LiteVcfWriter writer)
        {
            writer.Write(output);
        }

        private void WriteJson(IAnnotatedVariant annotatedVariant, IAnnotatedAlternateAllele firstAllele,
            UnifiedJsonWriter writer)
        {
            if (firstAllele.IsReference && !firstAllele.IsReferenceMinor && !firstAllele.IsReferenceNoCall) return;
            writer.Write(annotatedVariant.ToString());
        }

        protected override void ValidateCommandLine()
        {
            if (ConfigurationSettings.VcfPath != "-")
            {
                CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in");
            }

            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "compressed reference sequence", "--ref");
            CheckInputFilenameExists(CacheConstants.TranscriptPath(ConfigurationSettings.InputCachePrefix), "transcript cache", "--cache");
            CheckInputFilenameExists(CacheConstants.SiftPath(ConfigurationSettings.InputCachePrefix), "SIFT cache", "--cache");
            CheckInputFilenameExists(CacheConstants.PolyPhenPath(ConfigurationSettings.InputCachePrefix), "PolyPhen cache", "--cache");
            CheckDirectoryExists(ConfigurationSettings.SupplementaryAnnotationDirectory, "supplementary annotation", "--sd", false);
            foreach (var customAnnotationDirectory in ConfigurationSettings.CustomAnnotationDirectories)
            {
                CheckDirectoryExists(customAnnotationDirectory, "custom annotation", "--ca", false);
            }

            foreach (var customAnnotationDirectory in ConfigurationSettings.CustomIntervalDirectories)
            {
                CheckDirectoryExists(customAnnotationDirectory, "custom interval", "--ci", false);
            }

            // if we're using stdout, it doesn't make sense to output the VCF and gVCF
            if (ConfigurationSettings.OutputFileName == "-")
            {
                ConfigurationSettings.Vcf        = false;
                ConfigurationSettings.Gvcf       = false;
                PerformanceMetrics.DisableOutput = true;
            }

            HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out");

            if (ConfigurationSettings.LimitReferenceNoCallsToTranscripts)
                ConfigurationSettings.EnableReferenceNoCalls = true;
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "ca=",
                    "input custom annotation {directory}",
                    v => ConfigurationSettings.CustomAnnotationDirectories.Add(v)
                },
                {
                    "ci=",
                    "input custom intervals {directory}",
                    v => ConfigurationSettings.CustomIntervalDirectories.Add(v)
                },
                {
                    "cache|c=",
                    "input cache {prefix}",
                    v => ConfigurationSettings.InputCachePrefix = v
                },
                {
                    "in|i=",
                    "input VCF {path}",
                    v => ConfigurationSettings.VcfPath = v
                },
                {
                    "nc",
                    "enables reference no-calls",
                    v => ConfigurationSettings.EnableReferenceNoCalls = v != null
                },
                {
                    "loftee",
                    "enables loftee",
                    v => ConfigurationSettings.EnableLoftee = v != null
                },
                {
                    "gvcf",
                    "enables genome vcf output",
                    v => ConfigurationSettings.Gvcf = v != null
                },
                {
                    "vcf",
                    "enables vcf output",
                    v => ConfigurationSettings.Vcf = v != null
                },
                {
                    "out|o=",
                    "output {file path} ",
                    v => ConfigurationSettings.OutputFileName = v
                },
                {
                    "ref|r=",
                    "input compressed reference sequence {path}",
                    v => ConfigurationSettings.CompressedReferencePath = v
                },
                {
                    "sd=",
                    "input supplementary annotation {directory}",
                    v => ConfigurationSettings.SupplementaryAnnotationDirectory = v
                },
                {
                    "transcript-nc",
                    "limits reference no-calls to transcripts",
                    v => ConfigurationSettings.LimitReferenceNoCallsToTranscripts = v != null
                },
                {
                    "force-mt",
                    "forces to annotate mitochondria variants",
                    v => ConfigurationSettings.ForceMitochondrialAnnotation = v != null
                },
                {
                    "verbose-transcripts",
                    "reports all overlapping transcripts for structural variants",
                    v => ConfigurationSettings.ReportAllSvOverlappingTranscripts = v != null
                }
            };

            var commandLineExample = "-i <vcf path> -c <cache prefix> --sd <sa dir> -r <ref path> -o <base output filename>";

            var nirvana = new NirvanaAnnotator("Annotates a set of variants", ops, commandLineExample, Constants.Authors);
            nirvana.Execute(args);

            return nirvana.ExitCode;
        }
    }
}
