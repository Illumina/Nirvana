using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IO;
using VariantAnnotation;
using CommandLine.Utilities;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace Nirvana
{
    public static class ProviderUtilities
    {
        public static IAnnotator GetAnnotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider, IAnnotationProvider saProviders, IAnnotationProvider conservationProvider, IGeneAnnotationProvider geneAnnotationProviders, IEnumerable<IPlugin> plugins = null)
        {
            return new Annotator(taProvider, sequenceProvider, saProviders, conservationProvider, geneAnnotationProviders, plugins);
        }

        public static ISequenceProvider GetSequenceProvider(string compressedReferencePath)
        {
            return new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReferencePath));
        }

        public static IAnnotationProvider GetConservationProvider(IEnumerable<string> dirPaths)
        {
            if (dirPaths == null) return null;
            dirPaths = dirPaths.ToList();
            return dirPaths.All(x => Directory.GetFiles(x, "*.npd").Length == 0) ? null : new ConservationScoreProvider(dirPaths);
        }

        
        public static IAnnotationProvider GetNsaProvider(List<string> supplementaryAnnotationDirectories)
        {
            if (supplementaryAnnotationDirectories == null || supplementaryAnnotationDirectories.Count == 0)
                return null;
            var nsaReaders = new List<INsaReader>();
            var nsiReaders = new List<INsiReader>();
            foreach (string directory in supplementaryAnnotationDirectories)
            {
                if (directory.StartsWith("http"))
                {
                    var benchmark= new Benchmark();
                    Console.Write("Loading indexes from S3.....");
                    //assuming this is the Nirvana S3 bucket that contains NSA files for dbsnp, 1kg, gnomad, topmed
                    var onekGenStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/1000%20Genomes%20Project_Phase%203%20v5a.nsa");
                    var onekGenIndexStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/1000%20Genomes%20Project_Phase%203%20v5a.nsa.idx");
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(onekGenStream), onekGenIndexStream));

                    var clinvarStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/ClinVar_20180129.nsa");
                    var clinvarIndexStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/ClinVar_20180129.nsa.idx");
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(clinvarStream), clinvarIndexStream));

                    var dbsnpStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/dbSNP_150.nsa");
                    var dbsnpIndexStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/dbSNP_150.nsa.idx");
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(dbsnpStream), dbsnpIndexStream));

                    var gnomadStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/gnomAD_2.0.2.nsa");
                    var gnomadIndexStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/gnomAD_2.0.2.nsa.idx");
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(gnomadStream), gnomadIndexStream));

                    var gnomadExomeStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/gnomAD_exome_2.0.2.nsa");
                    var gnomadExomeIndexStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/gnomAD_exome_2.0.2.nsa.idx");
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(gnomadExomeStream), gnomadExomeIndexStream));

                    var topmedStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/TOPMed_freeze_5.nsa");
                    var topmedIndexStream = new HttpFileStream("https://illumina-annotation.s3.amazonaws.com/Test/CachableSa/TOPMed_freeze_5.nsa.idx");
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(topmedStream), topmedIndexStream));

                    Console.WriteLine($"{Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");
                    continue;
                }

                foreach (var nsaFile in Directory.GetFiles(directory, "*"+SaCommon.SaFileSuffix))
                {
                    nsaReaders.Add(new NsaReader(new ExtendedBinaryReader(FileUtilities.GetReadStream(Path.Combine(directory, nsaFile))),
                        FileUtilities.GetReadStream(Path.Combine(directory, nsaFile + SaCommon.IndexSufix))));
                }

                foreach (var nsiFile in Directory.GetFiles(directory, "*"+SaCommon.SiFileSuffix))
                {
                    nsiReaders.Add(new NsiReader(FileUtilities.GetReadStream(Path.Combine(directory, nsiFile))));
                }
            }

            if (nsaReaders.Count > 0 || nsiReaders.Count > 0)
                return new NsaProvider(nsaReaders.ToArray(), nsiReaders.ToArray());
            return null;
        }

        public static ITranscriptAnnotationProvider GetTranscriptAnnotationProvider(string path, ISequenceProvider sequenceProvider)
        {
            var benchmark = new Benchmark();
            var provider = new TranscriptAnnotationProvider(path, sequenceProvider);
            var wallTimeSpan = benchmark.GetElapsedTime();
            Console.WriteLine("Cache Time: {0} ms", wallTimeSpan.TotalMilliseconds);
			return provider;
        }

        public static IRefMinorProvider GetRefMinorProvider(List<string> supplementaryAnnotationDirectories)
        {
            return supplementaryAnnotationDirectories == null || supplementaryAnnotationDirectories.Count == 0
                ? null
                : new RefMinorProvider(supplementaryAnnotationDirectories);
        }

        public static IGeneAnnotationProvider GetGeneAnnotationProvider(List<string> supplementaryAnnotationDirectories)
        {
            return supplementaryAnnotationDirectories == null || supplementaryAnnotationDirectories.Count == 0
                ? null
                : new GeneAnnotationProvider(supplementaryAnnotationDirectories);
            
        }
    }
}