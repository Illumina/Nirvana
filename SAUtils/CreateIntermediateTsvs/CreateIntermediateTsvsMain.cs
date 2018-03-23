using System.Collections.Generic;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.CreateIntermediateTsvs
{
    public sealed class CreateIntermediateTsvsMain
    {
        private static string _compressedReference;
        private static string _inputDbSnpFileName;
        private static string _inputCosmicVcfFileName;
        private static string _inputCosmicTsvFileName;
        private static string _inputClinVarFileName;
        private static string _inputClinvarXml;
        private static string _input1000GFileName;
        private static string _inputEvsFile;
        private static string _inputExacFile;
        private static string _inputDgvFile;
        private static string _input1000GSvFileName;
        private static string _inputClinGenFileName;
        private static readonly List<string> InputMitoMapVarFileNames = new List<string>();
        private static readonly List<string> InputMitoMapSvFileNames = new List<string>();
        private static string _outputSupplementaryDirectory;
        private static readonly List<string> CustomAnnotationFiles = new List<string>();
        private static readonly List<string> CustomIntervalFiles = new List<string>();


        private static int NumberOfProvidedInputFiles()
        {
            var count = 0;

            if (_input1000GFileName != null) count++;
            if (_inputClinVarFileName != null) count++;
            if (_inputCosmicVcfFileName != null) count++;
            if (_inputDbSnpFileName != null) count++;
            if (_inputEvsFile != null) count++;
            if (_inputExacFile != null) count++;
            if (_inputDgvFile != null) count++;
            if (_input1000GSvFileName != null) count++;
            if (_inputClinGenFileName != null) count++;

            count += InputMitoMapVarFileNames.Count;
            count += InputMitoMapSvFileNames.Count;
            count += CustomAnnotationFiles.Count;
            count += CustomIntervalFiles.Count;

            return count;
        }

        private static ExitCodes ProgramExecution()
        {
            var interimTsvCreator =
				new CreateIntermediateTsvs(
                    _compressedReference,
                    _outputSupplementaryDirectory,
                    _inputDbSnpFileName,
                    _inputCosmicVcfFileName,
                    _inputCosmicTsvFileName,
                    _inputClinVarFileName,
                    _input1000GFileName,
                    _inputEvsFile,
                    _inputExacFile,
                    _inputDgvFile,
                    _input1000GSvFileName,
                    _inputClinGenFileName,
                    InputMitoMapVarFileNames,
                    InputMitoMapSvFileNames,
                    CustomAnnotationFiles,
                    CustomIntervalFiles
                    );

            interimTsvCreator.CreateTsvs();

            return ExitCodes.Success;
        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "ref|r=",
                     "compressed reference sequence file",
                     v => _compressedReference = v
                 },
                 {
                     "dbs|d=",
                     "input dbSNP vcf.gz file",
                     v => _inputDbSnpFileName = v
                 },
                 {
                     "csm|c=",
                     "input COSMIC vcf file",
                     v => _inputCosmicVcfFileName = v
                 },
                 {
                     "tsv=",
                     "input COSMIC TSV file",
                     v => _inputCosmicTsvFileName = v
                 },
                 {
                     "cvr|V=",
                     "input ClinVar file",
                     v => _inputClinVarFileName= v
                 },
                 {
                     "vcf=",
                     "input ClinVar no known medical importance file",
                     v => _inputClinvarXml= v
                 },
                 {
                     "onek|k=",
                     "input 1000 Genomes AF file",
                     v => _input1000GFileName= v
                 },
                 {
                     "evs|e=",
                     "input EVS file",
                     v => _inputEvsFile= v
                 },
                 {
                     "exac|x=",
                     "input ExAc file",
                     v => _inputExacFile= v
                 },
                 {
                     "dgv|g=",
                     "input Dgv file",
                     v => _inputDgvFile= v
                 },
                 {
                     "cust|t=",
                     "input Custom annotation file",
                     v => CustomAnnotationFiles.Add(v)
                 },
                 {
                     "bed=",
                     "input Custom interval file",
                     v => CustomIntervalFiles.Add(v)
                 },
                {
                    "onekSv|s=",
                    "input 1000 Genomes Structural file",
                    v => _input1000GSvFileName = v
                },
                {
                    "clinGen|l=",
                    "input ClinGen file",
                    v => _inputClinGenFileName = v
                },
				{
			        "mitoVar=",
			        "input MitoMAP variant HTML file",
			        v => InputMitoMapVarFileNames.Add(v)
			    },
			    {
			        "mitoSv=",
			        "input MitoMAP SV HTML file",
			        v => InputMitoMapSvFileNames.Add(v)
			    },
                {
                    "out|o=",
                    "output Nirvana Supplementary directory",
                    v => _outputSupplementaryDirectory = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_compressedReference, "Compressed reference sequence file name", "--ref")
                .HasRequiredParameter(_outputSupplementaryDirectory, "output Supplementary directory", "--out")
                .CheckInputFilenameExists(_inputDbSnpFileName, "input VCF file containing dbSNP scores", "--dbs", false)
                .CheckInputFilenameExists(_inputCosmicVcfFileName, "input unified COSMIC file", "--csm", false)
                .CheckInputFilenameExists(_inputCosmicTsvFileName, "input cosmic tsv file", "--tsv", false)
                .CheckInputFilenameExists(_inputClinVarFileName, "input ClinVar xml file", "--cvr", false)
                .CheckInputFilenameExists(_inputClinvarXml, "no known medical importance vcf file", "--cvr", false)
                .CheckInputFilenameExists(_input1000GFileName, "input 1000 Genomes AF file", "--onek", false)
                .CheckInputFilenameExists(_inputEvsFile, "input EVS file", "--evs", false)
                .CheckInputFilenameExists(_inputExacFile, "input Exac file", "--exac", false)
                .CheckInputFilenameExists(_inputDgvFile, "input DGV file", "--dgv", false)
                .CheckInputFilenameExists(_input1000GSvFileName, "input DGV file", "--onekSv", false)
                .CheckEachFilenameExists(InputMitoMapVarFileNames, "input MitoMap variant file names", "--mitoVar", false)
                .CheckEachFilenameExists(InputMitoMapSvFileNames, "input MitoMap SV file names", "--mitoSv", false)
                .CheckEachFilenameExists(CustomAnnotationFiles, "Custom Annotation file name", "--cust", false)
                .CheckEachFilenameExists(CustomIntervalFiles, "Custom interval file name", "--bed", false)
                .CheckNonZero(NumberOfProvidedInputFiles(), "supplementary data source")
                .SkipBanner()
                .ShowHelpMenu("Reads provided supplementary data files and populates tsv files", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return  exitCode;
		}
    }
}
