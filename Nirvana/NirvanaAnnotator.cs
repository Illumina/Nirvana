using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using NDesk.Options;
using VariantAnnotation.CommandLine;

namespace Nirvana
{
    public class NirvanaAnnotator : AbstractCommandLineHandler
    {
        #region members

        private int _numVariants;
        private int _numReferencePositions;

        #endregion

        private NirvanaAnnotator(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {
        }

        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in");
            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "compressed reference sequence", "--ref");
            CheckDirectoryExists(ConfigurationSettings.CacheDirectory, "cache", "--dir");
            CheckDirectoryExists(ConfigurationSettings.SupplementaryAnnotationDirectory, "supplementary annotation", "--sd", false);
            foreach (var customAnnotationDirectory in ConfigurationSettings.CustomAnnotationDirectories)
            {
                CheckDirectoryExists(customAnnotationDirectory, "custom annotation", "--ca", false);
            }

            foreach (var customAnnotationDirectory in ConfigurationSettings.CustomIntervalDirectories)
            {
                CheckDirectoryExists(customAnnotationDirectory, "custom interval", "--ci", false);
            }

            HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out");

            if (ConfigurationSettings.LimitReferenceNoCallsToTranscripts)
                ConfigurationSettings.EnableReferenceNoCalls = true;
        }

        protected override void ProgramExecution()
        {
            var processedReferences = new HashSet<string>();
            string previousReference = null;

            Console.WriteLine("Running Nirvana on {0}:", Path.GetFileName(ConfigurationSettings.VcfPath));

            var outputVcfPath = ConfigurationSettings.OutputFileName + ".vcf.gz";
            var outputGvcfPath = ConfigurationSettings.OutputFileName + ".genome.vcf.gz";
            var outputVariantsPath = ConfigurationSettings.OutputFileName + ".json.gz";

            var jsonCreationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			// parse the vcf header
			var reader = new LiteVcfReader(ConfigurationSettings.VcfPath);

            var booleanArguments = new List<string>();
            if (ConfigurationSettings.EnableReferenceNoCalls) booleanArguments.Add(AnnotatorInfoCommon.ReferenceNoCall);
            if (ConfigurationSettings.LimitReferenceNoCallsToTranscripts) booleanArguments.Add(AnnotatorInfoCommon.TranscriptOnlyRefNoCall);
			if (ConfigurationSettings.ForceMitochondrialAnnotation || reader.IsRcrsMitochondrion) booleanArguments.Add(AnnotatorInfoCommon.EnableMitochondrialAnnotation);

            var annotatorInfo  = new AnnotatorInfo(reader.SampleNames, booleanArguments);
            var annotatorPaths = new AnnotatorPaths(ConfigurationSettings.CacheDirectory, ConfigurationSettings.CompressedReferencePath, ConfigurationSettings.SupplementaryAnnotationDirectory, ConfigurationSettings.CustomAnnotationDirectories, ConfigurationSettings.CustomIntervalDirectories);
            var annotator      = new AnnotationSourceFactory().CreateAnnotationSource(annotatorInfo, annotatorPaths);

            // sanity check: make sure we have annotations
            if (annotator == null)
            {
                throw new GeneralException("Unable to perform annotation because no annotation sources could be created");
            }

            using (var unifiedJsonWriter = new UnifiedJsonWriter(outputVariantsPath, jsonCreationTime, annotator.GetDataVersion(), annotator.GetDataSourceVersions(), reader.SampleNames))
            using (var vcfWriter         = ConfigurationSettings.Vcf ?  new LiteVcfWriter(outputVcfPath, reader.HeaderLines, annotator.GetDataVersion(), annotator.GetDataSourceVersions()):null)
            using (var gvcfWriter        = ConfigurationSettings.Gvcf ? new LiteVcfWriter(outputGvcfPath, reader.HeaderLines, annotator.GetDataVersion(), annotator.GetDataSourceVersions()):null)
            {
                {
                    string vcfLine = null;

                    try
                    {
                        while ((vcfLine = reader.ReadLine()) != null)
                        {
                            var vcfVariant = CreateVcfVariant(vcfLine, reader.IsGatkGenomeVcf);

                            // check if the vcf is sorted
                            if (vcfVariant == null) continue;

                            UpdateStatistics(vcfVariant);

                            var currentReference = vcfVariant.ReferenceName;
                            if (currentReference != previousReference && processedReferences.Contains(currentReference))
                            {
                                throw new FileNotSortedException("The current input vcf file is not sorted. Please sort the vcf file before running variant annotation using a tool like vcf-sort in vcftools.");
                            }
                            if (!processedReferences.Contains(currentReference))
                            {
                                processedReferences.Add(currentReference);
                            }
                            previousReference = currentReference;

                            var annotatedVariant = annotator.Annotate(vcfVariant) as UnifiedJson;

                            gvcfWriter?.Write(vcfVariant, annotatedVariant);

                            if (annotatedVariant?.JsonVariants == null || annotatedVariant.JsonVariants.Count <= 0) continue;

                            if (annotatedVariant.JsonVariants[0].IsReference)
                            {
                                if (annotatedVariant.JsonVariants[0].IsReferenceNoCall || annotatedVariant.JsonVariants[0].IsReferenceMinor)
                                {
                                    unifiedJsonWriter.Write(annotatedVariant.ToString());
                                }

                                if (annotatedVariant.JsonVariants[0].IsReferenceMinor)
                                {
                                    vcfWriter?.Write(vcfVariant, annotatedVariant);
                                }

                                continue;
                            }

                            unifiedJsonWriter.Write(annotatedVariant.ToString());
                            vcfWriter?.Write(vcfVariant, annotatedVariant);
                        }
                    }
                    catch (Exception e)
                    {
                        // embed the vcf line
                        e.Data["VcfLine"] = vcfLine;
                        throw;
                    }
                }
            }

            annotator.FinalizeMetrics();
        }

        private static IVariant CreateVcfVariant(string vcfLine, bool isGatkGenomeVcf)
        {
            var fields = vcfLine.Split('\t');
            return fields.Length < VcfCommon.MinNumColumns ? null : new VcfVariant(fields, vcfLine, isGatkGenomeVcf);
        }

        private void UpdateStatistics(IVariant variant)
        {
            if (variant.Fields[VcfCommon.AltIndex] == VcfCommon.NonVariant) _numReferencePositions++;
            else _numVariants++;
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
                    "dir|d=",
                    "input cache {directory}",
                    v => ConfigurationSettings.CacheDirectory = v
                },
                {
                    "et",
                    "enables telemetry",
                    v => ConfigurationSettings.EnableTelemetry = v != null
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
                }
            };

            var commandLineExample = "-i <vcf path> -d <cache dir> --sd <sa dir> -r <ref path> -o <base output filename>";

			var nirvana = new NirvanaAnnotator("Annotates a set of variants", ops, commandLineExample, Constants.Authors);
            nirvana.Execute(args);

            // upload telemetry if we opted-in
            if (ConfigurationSettings.EnableTelemetry)
            {
                var wallTime = Telemetry.GetWallTime(nirvana.WallTimeSpan);
                var peakMemoryUsageGB = nirvana.PeakMemoryUsageBytes / (double)MemoryUtilities.NumBytesInGB;

                var telemetry = Telemetry.PackTelemetry(Path.GetFileName(ConfigurationSettings.VcfPath),
                    nirvana._numVariants, nirvana._numReferencePositions,
                    ConfigurationSettings.CustomAnnotationDirectories.Count,
                    ConfigurationSettings.CustomIntervalDirectories.Count, ConfigurationSettings.EnableReferenceNoCalls,
                    ConfigurationSettings.Gvcf, ConfigurationSettings.Vcf,
                    CommandLineUtilities.InformationalVersion, NirvanaDatabaseCommon.DataVersion,
                    SupplementaryAnnotationCommon.DataVersion, CompressedSequenceCommon.HeaderVersion, wallTime,
                    peakMemoryUsageGB, nirvana.ExitCode);

                Telemetry.Update(telemetry);
            }

            return nirvana.ExitCode;
        }
    }
}
