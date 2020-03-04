using System.Collections.Generic;
using CommandLine.Builders;
using ReferenceSequence.Commands;
using VariantAnnotation.Interface;

namespace ReferenceSequence
{
    internal static class ReferenceUtilsMain
    {
        private static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["create"]    = new TopLevelOption("creates a full reference file", CreateReferenceMain.Run),
                ["substring"] = new TopLevelOption("creates a reference substring file", CreateSubstringMain.Run),
                ["testseq"]   = new TopLevelOption("creates a TestSeq_reference.dat file", CreateTestSeqMain.Run)
            };

            var exitCode = new TopLevelAppBuilder(args, ops)
                .Parse()
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Utilities focused on creating the reference files")
                .ShowErrors()
                .Execute();

            return (int)exitCode;
        }
    }
}