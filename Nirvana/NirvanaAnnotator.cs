using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using NDesk.Options;
using VariantAnnotation;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace Nirvana
{
    public class NirvanaAnnotator : AbstractCommandLineHandler
    {
        private NirvanaAnnotator(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {
        }

        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in");
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

            HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out");

            if (ConfigurationSettings.LimitReferenceNoCallsToTranscripts)
                ConfigurationSettings.EnableReferenceNoCalls = true;
        }

        protected override void ProgramExecution()
        {
            var processedReferences  = new HashSet<string>();
            string previousReference = null;

            Console.WriteLine("Running Nirvana on {0}:", Path.GetFileName(ConfigurationSettings.VcfPath));

            var outputVcfPath      = ConfigurationSettings.OutputFileName + ".vcf.gz";
            var outputGvcfPath     = ConfigurationSettings.OutputFileName + ".genome.vcf.gz";
            var outputVariantsPath = ConfigurationSettings.OutputFileName + ".json.gz";

            var jsonCreationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // parse the vcf header
            var reader = new LiteVcfReader(ConfigurationSettings.VcfPath);

            var booleanArguments = new List<string>();
            if (ConfigurationSettings.EnableReferenceNoCalls)                                     booleanArguments.Add(AnnotatorInfoCommon.ReferenceNoCall);
            if (ConfigurationSettings.LimitReferenceNoCallsToTranscripts)                         booleanArguments.Add(AnnotatorInfoCommon.TranscriptOnlyRefNoCall);
            if (ConfigurationSettings.ForceMitochondrialAnnotation || reader.IsRcrsMitochondrion) booleanArguments.Add(AnnotatorInfoCommon.EnableMitochondrialAnnotation);
            if (ConfigurationSettings.ReportAllSvOverlappingTranscripts)                          booleanArguments.Add(AnnotatorInfoCommon.ReportAllSvOverlappingTranscripts);
			if (ConfigurationSettings.EnableLoftee)                                               booleanArguments.Add(AnnotatorInfoCommon.EnableLoftee);

            var annotatorInfo = new AnnotatorInfo(reader.SampleNames, booleanArguments);
            var annotatorPaths = new AnnotatorPath(ConfigurationSettings.InputCachePrefix, ConfigurationSettings.CompressedReferencePath, ConfigurationSettings.SupplementaryAnnotationDirectory, ConfigurationSettings.CustomAnnotationDirectories, ConfigurationSettings.CustomIntervalDirectories);
            var annotator      = new AnnotationSourceFactory().CreateAnnotationSource(annotatorInfo, annotatorPaths);

            // sanity check: make sure we have annotations
            if (annotator == null)
            {
                throw new GeneralException("Unable to perform annotation because no annotation sources could be created");
            }

            using (var unifiedJsonWriter = new UnifiedJsonWriter(outputVariantsPath, jsonCreationTime, annotator.GetDataVersion(), annotator.GetDataSourceVersions(), annotator.GetGenomeAssembly(), reader.SampleNames))
            using (var vcfWriter         = ConfigurationSettings.Vcf  ? new LiteVcfWriter(outputVcfPath, reader.HeaderLines, annotator.GetDataVersion(), annotator.GetDataSourceVersions()) : null)
            using (var gvcfWriter        = ConfigurationSettings.Gvcf ? new LiteVcfWriter(outputGvcfPath, reader.HeaderLines, annotator.GetDataVersion(), annotator.GetDataSourceVersions()) : null)
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

                            var annotatedVariant = annotator.Annotate(vcfVariant) ;

                            gvcfWriter?.Write(vcfVariant, annotatedVariant);

                            if (annotatedVariant?.AnnotatedAlternateAlleles == null || !annotatedVariant.AnnotatedAlternateAlleles.Any()) continue;

                            if (annotatedVariant.AnnotatedAlternateAlleles.First().IsReference)
                            {
                                if (annotatedVariant.AnnotatedAlternateAlleles.First().IsReferenceNoCall || annotatedVariant.AnnotatedAlternateAlleles.First().IsReferenceMinor)
                                {
                                    unifiedJsonWriter.Write(annotatedVariant.ToString());
                                }

                                if (annotatedVariant.AnnotatedAlternateAlleles.First().IsReferenceMinor)
                                {
                                    vcfWriter?.Write(vcfVariant, annotatedVariant);
                                }

                                continue;
                            }

                            unifiedJsonWriter.Write(annotatedVariant.ToString());
                            vcfWriter?.Write(vcfVariant, annotatedVariant);
                        }

                        var omimAnnotations = new List<string>();
                        annotator.AddGeneLevelAnnotation(omimAnnotations);
                        var annotionOutput = UnifiedJson.GetGeneAnnotation(omimAnnotations, "omim");
                        unifiedJsonWriter.Write(annotionOutput);

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
