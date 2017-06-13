using System;
using System.Collections.Generic;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Utilities;

namespace vcf2vid
{
    class Vcf2Vid : AbstractCommandLineHandler
    {
        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "ref|r=",
                    "input compressed reference sequence {path}",
                    v => ConfigurationSettings.CompressedReferencePath = v
                }
            };

            var commandLineExample = "-r <reference path> < input.vcf";

            var vid = new Vcf2Vid("Parses a VCF and displays tab-delimited VIDs for each alternate allele", ops, commandLineExample, Constants.Authors);
            vid.Execute(args);
            return vid.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private Vcf2Vid(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        {
            DisableConsoleOutput();
        }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "reference", "--ref");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            if (!Console.IsInputRedirected)
            {
                CommandLineUtilities.DisplayBanner(Constants.Authors);
                OutputHelper.WriteLabel("USAGE: ");
                Console.WriteLine("{0} {1}", OutputHelper.GetExecutableName(), "< input.vcf");
                Console.WriteLine("Parses a VCF and displays tab-delimited VIDs for each alternate allele");
                ExitCode = 1;
                return;
            }

            var vid  = new VID();
            var vids = new List<string>();

            using (var refReader  = FileUtilities.GetReadStream(ConfigurationSettings.CompressedReferencePath))
            using (var peekStream = new PeekStream(Console.OpenStandardInput()))
            using (var reader     = new LiteVcfReader(GZipUtilities.GetAppropriateStream(peekStream)))
            {
                var compressedSequence       = new CompressedSequence();
                var compressedSequenceReader = new CompressedSequenceReader(refReader, compressedSequence);
                var dataFileManager          = new DataFileManager(compressedSequenceReader, compressedSequence);

                while (true)
                {
                    string vcfLine = reader.ReadLine();
                    if (vcfLine == null) break;

                    var variant        = CreateVcfVariant(vcfLine);
                    var variantFeature = new VariantFeature(variant, compressedSequence.Renamer, vid);

                    CheckRefNoCall(variantFeature);

                    // load the reference sequence
                    dataFileManager.LoadReference(variantFeature.ReferenceIndex, () => { });

                    variantFeature.AssignAlternateAlleles();
                    
                    if (variantFeature.IsReference && !variantFeature.IsRefNoCall)
                    {
                        var refAllele = new VariantAlternateAllele(variantFeature.VcfReferenceBegin,
                            variantFeature.VcfReferenceEnd, variantFeature.EnsemblReferenceName,
                            variantFeature.VcfRefAllele);
                        var refVid = vid.Create(compressedSequence.Renamer, variantFeature.EnsemblReferenceName,
                            refAllele);
                        Console.WriteLine(refVid);
                    }
                    else
                    {
                        vids.Clear();
                        foreach (var altAllele in variantFeature.AlternateAlleles)
                        {
                            vids.Add(altAllele.VariantId);
                        }

                        Console.WriteLine(string.Join("\t", vids));
                    }
                }
            }
        }

        private static void CheckRefNoCall(VariantFeature variant)
        {
            if (!variant.IsReference) return;
            if (variant.PassFilter()) return;
            variant.IsRefNoCall = true;
        }

        private static VcfVariant CreateVcfVariant(string vcfLine)
        {
            var fields = vcfLine.Split('\t');

            if (fields.Length < VcfCommon.MinNumColumns)
            {
                throw new UserErrorException($"Expected at least {VcfCommon.MinNumColumns} tab-delimited columns in the VCF line, but found only {fields.Length}");
            }

            return new VcfVariant(fields, vcfLine, false);
        }
    }
}