using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace InvestigateVcfFields
{
    class VcfInvestigator
    {
        #region members

        private readonly Dictionary<string, ulong> _infoKeys;
        private readonly Dictionary<string, ulong> _genotypeKeys;
        private readonly Dictionary<int, ulong> _altAlleleCounts;

        #endregion

        public VcfInvestigator()
        {
            _infoKeys        = new Dictionary<string, ulong>();
            _genotypeKeys    = new Dictionary<string, ulong>();
            _altAlleleCounts = new Dictionary<int, ulong>();
        }

        public void AnalyzeDirectory(string vcfDirectory)
        {
            var files = new List<string>();
            var gzFiles = Directory.GetFiles(vcfDirectory, "*.vcf.gz", SearchOption.AllDirectories);
            var plainFiles = Directory.GetFiles(vcfDirectory, "*.vcf", SearchOption.AllDirectories);

            files.AddRange(gzFiles);
            files.AddRange(plainFiles);

            foreach(var file in files) AnalyzeVcf(file);
        }

        private void AnalyzeVcf(string vcfPath)
        {
            Console.WriteLine("- analyzing {0}", Path.GetFileName(vcfPath));

            using (var reader = new LiteVcfReader(vcfPath))
            {
                //if (reader.SampleNames.Length > 1)
                //{
                //    Console.WriteLine("- skipping file because it has {0} samples", reader.SampleNames.Length);
                //    return;
                //}

                while (true)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    var cols = line.Split('\t');

                    var altAlleles = cols[VcfCommon.AltIndex];
                    var info       = cols[VcfCommon.InfoIndex];
                    var format     = cols[VcfCommon.FormatIndex];

                    AddAltAlleleCounts(altAlleles);
                    AddInfoCounts(info);
                    AddGenotypeCounts(format);
                }
            }
        }

        private void AddInfoCounts(string infoField)
        {
            var infoDictionary = VcfField.GetKeysAndValues(infoField);

            foreach (var key in infoDictionary.Keys)
            {
                ulong count;
                if (_infoKeys.TryGetValue(key, out count))
                {
                    _infoKeys[key] = count + 1;
                }
                else
                {
                    _infoKeys[key] = 1;
                }
            }
        }

        private void AddGenotypeCounts(string genotypeField)
        {
            var formatFields = genotypeField.Split(':');

            foreach (var key in formatFields)
            {
                ulong count;
                if (_genotypeKeys.TryGetValue(key, out count))
                {
                    _genotypeKeys[key] = count + 1;
                }
                else
                {
                    _genotypeKeys[key] = 1;
                }
            }
        }

        private void AddAltAlleleCounts(string altAlleles)
        {
            var numAlleles = altAlleles.Split(',').Length;

            ulong count;
            if (_altAlleleCounts.TryGetValue(numAlleles, out count))
            {
                _altAlleleCounts[numAlleles] = count + 1;
            }
            else
            {
                _altAlleleCounts[numAlleles] = 1;
            }
        }

        public void DumpData()
        {
            DumpDictionary("Info fields:", _infoKeys);
            DumpDictionary("Genotype fields:", _genotypeKeys);
            DumpAlleleCounts();
        }

        private static void DumpDictionary(string description, Dictionary<string, ulong> dict)
        {
            Console.WriteLine("{0}:", description);
            foreach (var kvp in dict.OrderByDescending(x => x.Value))
            {
                Console.WriteLine("{0}\t{1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine();
        }

        private void DumpAlleleCounts()
        {
            Console.WriteLine("Allele counts:");
            foreach (var kvp in _altAlleleCounts.OrderBy(x => x.Key))
            {
                Console.WriteLine("{0}\t{1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("USAGE: {0} <directory>", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
                Environment.Exit(1);
            }

            var vcfDirectory = args[0];

            var investigator = new VcfInvestigator();
            investigator.AnalyzeDirectory(vcfDirectory);
            investigator.DumpData();
            Console.WriteLine("Finished.");
        }
    }
}
