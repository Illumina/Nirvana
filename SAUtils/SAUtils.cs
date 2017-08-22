using System.Collections.Generic;
using CommandLine.Builders;
using SAUtils.CreateIntermediateTsvs;
using SAUtils.CreateOmimDatabase;
using SAUtils.ExtractMiniSa;
using SAUtils.MergeInterimTsvs;
using VariantAnnotation.Interface;

namespace SAUtils
{
    public class SaUtils 
    {

        public static int Main(string[] args)
        {
            var ops = new Dictionary<string, TopLevelOption>
            {
                ["createOMIM"] = new TopLevelOption("create omim database", CreateOmimDatabaseMain.Run),
                ["createSA"] = new TopLevelOption("create Nirvana supplementary annotation database", MergeIntermediateTsvsMain.Run),
                ["createTSV"] = new TopLevelOption("create intermediate tsv file for supplementary annotation", CreateIntermediateTsvsMain.Run),
                ["extractMiniSA"] = new TopLevelOption("extracts mini SA", ExtractMiniSaMain.Run),

            };


            var exitCode = new TopLevelAppBuilder(args,ops)
                .Parse()
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Utilities focused on supplementary annotation")
                .ShowErrors().Execute();
            return (int) exitCode;
        }







    }
}
