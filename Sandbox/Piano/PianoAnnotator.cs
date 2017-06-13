using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace Piano
{
    sealed class PianoAnnotator : AbstractCommandLineHandler
    {
        #region members

        private const string OutHeader =
            "#Chrom\tPos\tRefAllele\tAltAllele\tGeneSymbol\tGeneId\tTranscriptID\tProteinID\tProteinPos\tUpstream\tAAchange\tDownstream\tConsequences";

        #endregion

        private PianoAnnotator(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {
        }

        static void Main(string[] args)
        {
            var ops = new OptionSet
            {
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
                    "force-mt",
                    "forces to annotate mitochondria variants",
                    v => ConfigurationSettings.ForceMitochondrialAnnotation = v != null
                }
            };

            var commandLineExample = "-i <vcf path> -d <cache dir> -r <ref path> -o <base output filename>";

            var piano = new PianoAnnotator("Annotates a set of variants", ops, commandLineExample, Constants.Authors);
            piano.Execute(args);
        }

        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.VcfPath, "vcf", "--in");
            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "compressed reference sequence", "--ref");
            HasRequiredParameter(ConfigurationSettings.InputCachePrefix, "cache prefix", "--cache");
            HasRequiredParameter(ConfigurationSettings.OutputFileName, "output file stub", "--out");
        }

        protected override void ProgramExecution()
        {
            var processedReferences = new HashSet<string>();
            string previousReference = null;

            Console.WriteLine("Running Nirvana on {0}:", Path.GetFileName(ConfigurationSettings.VcfPath));

            var outputFilePath = ConfigurationSettings.OutputFileName + ".txt.gz";
            var annotationCreationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var reader = new LiteVcfReader(ConfigurationSettings.VcfPath);

            var compressedSequence = new CompressedSequence();
            var compressedSequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(ConfigurationSettings.CompressedReferencePath), compressedSequence);
            var transcriptCacheStream = new FileStream(CacheConstants.TranscriptPath(ConfigurationSettings.InputCachePrefix),
                FileMode.Open, FileAccess.Read, FileShare.Read);

            var annotator = new PianoAnnotationSource(transcriptCacheStream, compressedSequenceReader);

            if (ConfigurationSettings.ForceMitochondrialAnnotation || reader.IsRcrsMitochondrion)
                annotator.EnableMitochondrialAnnotation();

            // sanity check: make sure we have annotations
            if (annotator == null)
            {
                throw new GeneralException("Unable to perform annotation because no annotation sources could be created");
            }

            using (var writer = GZipUtilities.GetStreamWriter(outputFilePath))
            {

                WriteHeader(writer, annotationCreationTime);
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
                            throw new FileNotSortedException(
                                "The current input vcf file is not sorted. Please sort the vcf file before running variant annotation using a tool like vcf-sort in vcftools.");
                        }
                        if (!processedReferences.Contains(currentReference))
                        {
                            processedReferences.Add(currentReference);
                        }
                        previousReference = currentReference;

                        var annotatedVariant = annotator.Annotate(vcfVariant);

                        writer.Write(annotatedVariant.ToString());
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

        private static void WriteHeader(StreamWriter writer, string annotationTime)
        {
            writer.WriteLine("##Source=Piano");
            writer.WriteLine($"##CreationTime={annotationTime}");
            writer.WriteLine(OutHeader);
        }

        private static IVariant CreateVcfVariant(string vcfLine, bool isGatkGenomeVcf)
        {
            var fields = vcfLine.Split('\t');
            return fields.Length < VcfCommon.MinNumColumns ? null : new VcfVariant(fields, vcfLine, isGatkGenomeVcf);
        }
    }
}