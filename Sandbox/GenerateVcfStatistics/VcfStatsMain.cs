using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace GenerateVcfStatistics
{
    static class VcfStatsMain
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("USAGE: {0} <reference path> <input vcf file>", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
                Environment.Exit(1);
            }

            var referencePath = args[0];
            var vcfPath       = args[1];

            if (!File.Exists(vcfPath))
            {
                Console.WriteLine($"ERROR: {vcfPath} does not exist.");
                Environment.Exit(1);
            }

            if (!File.Exists(referencePath))
            {
                Console.WriteLine($"ERROR: {referencePath} does not exist.");
                Environment.Exit(1);
            }

            var renamer = ChromosomeRenamer.GetChromosomeRenamer(FileUtilities.GetReadStream(referencePath));
            var vid     = new VID();

            var counts = new Dictionary<VariantType, int>();

            using (var reader = new LiteVcfReader(vcfPath))
            {
                while (true)
                {
                    var vcfLine = reader.ReadLine();
                    if (vcfLine == null) break;

                    if (vcfLine.StartsWith("#")) continue;

                    VcfVariant vcfVariant = null;

                    try
                    {
                        vcfVariant = CreateVcfVariant(vcfLine);                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ERROR: Could not parse the VCF line:\n{vcfLine}");
                        Console.WriteLine(e.Message);
                        Environment.Exit(1);
                    }

                    if (vcfVariant == null) continue;
                    var variant = new VariantFeature(vcfVariant, renamer, vid);

                    if (variant.IsReference) continue;

                    variant.AssignAlternateAlleles();

                    bool hasMnv = false;
                    foreach (var altAllele in variant.AlternateAlleles)
                    {
                        AddVariantType(altAllele.NirvanaVariantType, counts);
                        if (altAllele.NirvanaVariantType == VariantType.MNV) hasMnv = true;
                    }

                    if (hasMnv) Console.WriteLine(vcfLine);
                }
            }

            const int keyFieldLength = 15;
            const int valueFieldLength = 9;

            Console.WriteLine("VariantType counts:");
            Console.WriteLine($"{new string('-', keyFieldLength)} {new string('-', valueFieldLength)}");

            foreach (var kvp in counts.OrderBy(x => x.Key.ToString()))
            {
                var spaceLeft = keyFieldLength - kvp.Key.ToString().Length - 1;
                var filler = new string(' ', spaceLeft);
                Console.WriteLine($"{kvp.Key}:{filler} {kvp.Value,9:N0}");
            }
        }

        private static void AddVariantType(VariantType variantType, Dictionary<VariantType, int> counts)
        {
            int count;
            if (counts.TryGetValue(variantType, out count)) counts[variantType] = count + 1;
            else counts[variantType] = 1;
        }

        private static VcfVariant CreateVcfVariant(string vcfLine)
        {
            var fields = vcfLine.Split('\t');
            return fields.Length < VcfCommon.MinNumColumns ? null : new VcfVariant(fields, vcfLine, false);
        }
    }
}
