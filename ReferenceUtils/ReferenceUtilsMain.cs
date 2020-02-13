using System.Collections.Generic;
using CommandLine.Builders;
using ReferenceUtils.Commands.CreateReference;
using ReferenceUtils.Commands.CreateSubstring;
using VariantAnnotation.Interface;

namespace ReferenceUtils
{
    internal static class ReferenceUtilsMain
    {
        private static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["create"]    = new TopLevelOption("creates a full reference file",      CreateReferenceMain.Run),
                ["substring"] = new TopLevelOption("creates a reference substring file", CreateReferenceSubstring.Run)
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