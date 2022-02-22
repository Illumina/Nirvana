using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloud;
using CommandLine.Utilities;
using IO;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;

namespace VariantAnnotation
{
    public static class ProviderUtilities
    {
        public static Annotator GetAnnotator(IAnnotationProvider taProvider, ReferenceSequenceProvider sequenceProvider,
            ISaAnnotationProvider saProvider, IAnnotationProvider conservationProvider,
            IGeneAnnotationProvider geneAnnotationProviders)
        {
            return new Annotator(taProvider, sequenceProvider, saProvider, conservationProvider,
                geneAnnotationProviders);
        }
        
        public static PsaProvider GetPsaProvider(List<(string dataFile, string indexFile)> dataAndIndexFiles)
        {
            if (dataAndIndexFiles == null || dataAndIndexFiles.Count==0) return null;

            var psaReaders = new List<PsaReader>(dataAndIndexFiles.Count);
            foreach ((string psaPath, string idxPath) in dataAndIndexFiles)
            {
                psaReaders.Add(new PsaReader(PersistentStreamUtils.GetReadStream(psaPath), PersistentStreamUtils.GetReadStream(idxPath)));
            }

            return new PsaProvider(psaReaders.OrderBy(x=>x.Header.JsonKey).ToArray());
        }

        public static ReferenceSequenceProvider GetSequenceProvider(string compressedReferencePath)
        {
             return new ReferenceSequenceProvider(PersistentStreamUtils.GetReadStream(compressedReferencePath));
        }

        public static IAnnotationProvider GetConservationProvider(IEnumerable<(string dataFile, string indexFile)> dataAndIndexFiles)
        {
            if (dataAndIndexFiles == null) return null;

            foreach ((string dataFile, string indexFile) in dataAndIndexFiles)
            {
                if (dataFile.EndsWith(SaCommon.PhylopFileSuffix))
                    return new ConservationScoreProvider(PersistentStreamUtils.GetReadStream(dataFile), PersistentStreamUtils.GetReadStream(indexFile));
            }

            return null;
        }

        public static IRefMinorProvider GetRefMinorProvider(IEnumerable<(string dataFile, string indexFile)> dataAndIndexFiles)
        {
            if (dataAndIndexFiles == null) return null;

            foreach ((string dataFile, string indexFile) in dataAndIndexFiles)
            {
                if (dataFile.EndsWith(SaCommon.RefMinorFileSuffix))
                    return new RefMinorProvider(PersistentStreamUtils.GetReadStream(dataFile), PersistentStreamUtils.GetReadStream(indexFile));
            }

            return null;
        }

        public static IGeneAnnotationProvider GetGeneAnnotationProvider(IEnumerable<(string dataFile, string indexFile)> dataAndIndexFiles)
        {
            if (dataAndIndexFiles == null) return null;
            var ngaFiles = new List<string>();
            foreach ((string dataFile, string _) in dataAndIndexFiles)
            {
                if (dataFile.EndsWith(SaCommon.NgaFileSuffix))
                    ngaFiles.Add(dataFile);
            }
            return ngaFiles.Count > 0? new GeneAnnotationProvider(PersistentStreamUtils.GetStreams(ngaFiles)): null;
        }

        public static ISaAnnotationProvider GetNsaProvider(IEnumerable<(string dataFile, string indexFile)> dataAndIndexFiles, IS3Client s3Client, List<S3Path> annotationsInS3)
        {
            if (dataAndIndexFiles == null && annotationsInS3 == null) return null;

            var nsaReaders = new List<INsaReader>();
            var nsiReaders = new List<INsiReader>();

            if (dataAndIndexFiles != null) GetSaReaders(dataAndIndexFiles, nsaReaders, nsiReaders);

            if (annotationsInS3 != null) GetSaReadersFromS3(s3Client, annotationsInS3, nsaReaders, nsiReaders);

            if (nsaReaders.Count <= 0 && nsiReaders.Count <= 0) return null;

            nsaReaders.Sort((a, b) => string.Compare(a.JsonKey, b.JsonKey, StringComparison.Ordinal));
            nsiReaders.Sort((a, b) => string.Compare(a.JsonKey, b.JsonKey, StringComparison.Ordinal));
            return new NsaProvider(nsaReaders.ToArray(), nsiReaders.ToArray());
        }

        private static void GetSaReadersFromS3(IS3Client s3Client, List<S3Path> annotationsInS3, List<INsaReader> nsaReaders, List<INsiReader> nsiReaders)
        {
            foreach (var annotation in annotationsInS3)
            {
                if (annotation.path.EndsWith(SaCommon.SaFileSuffix))
                    nsaReaders.Add(GetNsaReader(
                        PersistentStreamUtils.GetS3ReadStream(s3Client, annotation.bucketName, annotation.path, 0),
                        PersistentStreamUtils.GetS3ReadStream(s3Client, annotation.bucketName,
                            annotation.path + SaCommon.IndexSuffix, 0)));
                else
                {
                    nsiReaders.Add(GetNsiReader(
                        PersistentStreamUtils.GetS3ReadStream(s3Client, annotation.bucketName, annotation.path, 0)));
                }
            }
        }

        private static void GetSaReaders(IEnumerable<(string dataFile, string indexFile)> dataAndIndexFiles, List<INsaReader> nsaReaders, List<INsiReader> nsiReaders)
        {
            foreach ((string dataFile, string indexFile) in dataAndIndexFiles)
            {
                if (dataFile.EndsWith(SaCommon.SaFileSuffix))
                    nsaReaders.Add(
                        GetNsaReader(PersistentStreamUtils.GetReadStream(dataFile),
                        PersistentStreamUtils.GetReadStream(indexFile))
                        );
                if (dataFile.EndsWith(SaCommon.SiFileSuffix))
                    nsiReaders.Add(GetNsiReader(PersistentStreamUtils.GetReadStream(dataFile)));
            }
        }

        public static List<(string dataFile, string indexFile)> GetSaDataAndIndexPaths(string saDirectoryPath) => ConnectUtilities.IsHttpLocation(saDirectoryPath) ? GetSaPathsFromManifest(saDirectoryPath) : GetLocalSaPaths(saDirectoryPath);

        private static List<(string dataFile, string indexFile)> GetSaPathsFromManifest(string saDirectoryPath)
        {
            var baseUrl = NirvanaHelper.S3Url;
            
            var paths = new List<(string, string)>();
            using (var reader = new StreamReader(PersistentStreamUtils.GetReadStream(saDirectoryPath)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.EndsWith(SaCommon.SiFileSuffix) || line.EndsWith(SaCommon.NgaFileSuffix))
                        paths.Add((baseUrl + line, null));
                    else paths.Add((baseUrl + line, baseUrl + line + SaCommon.IndexSuffix));
                }
            }

            return paths.Count > 0 ? paths : null;
        }

        private static List<(string, string)> GetLocalSaPaths(string saDirectoryPath)
        {
            var paths = new List<(string, string)>();
            foreach (var filePath in Directory.GetFiles(saDirectoryPath))
            {
                if (filePath.EndsWith(SaCommon.SaFileSuffix) || filePath.EndsWith(SaCommon.PhylopFileSuffix) ||
                    filePath.EndsWith(SaCommon.RefMinorFileSuffix))
                    paths.Add((filePath, filePath + SaCommon.IndexSuffix));

                if (filePath.EndsWith(SaCommon.SiFileSuffix) || filePath.EndsWith(SaCommon.NgaFileSuffix))
                    paths.Add((filePath, null));
                //skip files with all other extensions
            }

            return paths.Count>0? paths:null;
        }

        public static List<(string, string)> GetPsaPaths(string saDirectoryPath)
        {
            var paths = new List<(string, string)>();
            foreach (var filePath in Directory.GetFiles(saDirectoryPath))
            {
                if (filePath.EndsWith(SaCommon.PsaFileSuffix))
                    paths.Add((filePath, filePath + SaCommon.IndexSuffix));
            }

            return paths.Count >0? paths:null;
        }

        public static TranscriptAnnotationProvider GetTranscriptAnnotationProvider(string cacheDir,
            ISequenceProvider sequenceProvider, PsaProvider psaProvider)
         {
            var benchmark = new Benchmark();
            var provider = new TranscriptAnnotationProvider(cacheDir, sequenceProvider, psaProvider);
            Console.WriteLine("Cache Time: {0}\n", Benchmark.ToHumanReadable(benchmark.GetElapsedTime()));
            return provider;
        }

        private static NsaReader GetNsaReader(Stream dataStream, Stream indexStream) =>
            new NsaReader(new ExtendedBinaryReader(dataStream), indexStream);

        private static NsiReader GetNsiReader(Stream stream) => new NsiReader(stream);
    }
}