using System.Collections.Generic;
using System.IO;
using Cloud;
using IO;
using VariantAnnotation.SA;

namespace Nirvana
{
    public sealed class AnnotationFiles
    {
        public List<(string Nsa, string Idx)> NsaFiles { get; } = new List<(string, string)>();
        public List<string> NsiFiles { get; } = new List<string>();
        public List<string> NgaFiles { get; } = new List<string>();
        public (string Npd, string Idx) ConservationFile { get; private set; }
        public (string Rma, string Idx) RefMinorFile { get; private set; }

        public void AddFiles(string saDirectoryPath)
        {
            foreach (string filePath in GetFiles(saDirectoryPath))
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (filePath.GetFileSuffix(true))
                {
                    case SaCommon.SaFileSuffix:
                        NsaFiles.Add((filePath, filePath + SaCommon.IndexSufix));
                        break;
                    case SaCommon.SiFileSuffix:
                        NsiFiles.Add(filePath);
                        break;
                    case SaCommon.NgaFileSuffix:
                        NgaFiles.Add(filePath);
                        break;
                    case SaCommon.PhylopFileSuffix:
                        ConservationFile = (filePath, filePath + SaCommon.IndexSufix);
                        break;
                    case SaCommon.RefMinorFileSuffix:
                        RefMinorFile = (filePath, filePath + SaCommon.IndexSufix);
                        break;
                }
            }
        }

        public void AddFiles(SaUrls saUrls)
        {
            if (saUrls.IsNsa())
                NsaFiles.Add((saUrls.nsaUrl, saUrls.idxUrl));
            else
                NsiFiles.Add(saUrls.nsiUrl);
        }

        private static IEnumerable<string> GetFiles(string directoryOrManifestFilePath)
        {
            if (ConnectUtilities.IsHttpLocation(directoryOrManifestFilePath))
            {
                using (var reader = new StreamReader(PersistentStreamUtils.GetReadStream(directoryOrManifestFilePath)))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return NirvanaHelper.S3Url + line;
                    }
                }
            }

            else
            {
                foreach (string file in Directory.GetFiles(directoryOrManifestFilePath))
                    yield return file;
            }
        }
    }
}