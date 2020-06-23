using System.Collections.Generic;
using System.IO;
using Cloud;
using Cloud.Messages;
using IO;
using VariantAnnotation.ProteinConservation;
using VariantAnnotation.SA;

namespace Nirvana
{
    public sealed class AnnotationFiles
    {
        public List<(string Nsa, string Idx)> NsaFiles { get; } = new List<(string, string)>();
        public List<string> NsiFiles { get; } = new List<string>();
        public List<string> NgaFiles { get; } = new List<string>();
        public (string Npd, string Idx) ConservationFile { get; private set; }
        public string LowComplexityRegionFile { get; private set; }
        public string ProteinConservationFile { get; private set; }
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
                    case SaCommon.IntervalFileSuffix:
                        NsiFiles.Add(filePath);
                        break;
                    case SaCommon.GeneFileSuffix:
                        NgaFiles.Add(filePath);
                        break;
                    case SaCommon.PhylopFileSuffix:
                        ConservationFile = (filePath, filePath + SaCommon.IndexSufix);
                        break;
                    case ProteinConservationCommon.FileSuffix:
                        ProteinConservationFile = filePath;
                        break;
                    case SaCommon.LcrFileSuffix:
                        LowComplexityRegionFile = filePath;
                        break;
                    case SaCommon.RefMinorFileSuffix:
                        RefMinorFile = (filePath, filePath + SaCommon.IndexSufix);
                        break;
                }
            }
        }

        public void AddFiles(SaUrls saUrls)
        {
            switch (saUrls.SaType)
            {
                case CustomSaType.Nsa:
                    NsaFiles.Add((saUrls.nsaUrl, saUrls.idxUrl));
                    break;
                case CustomSaType.Nsi:
                    NsiFiles.Add(saUrls.nsiUrl);
                    break;
                case CustomSaType.Nga:
                    NgaFiles.Add(saUrls.ngaUrl);
                    break;
                default:
                    throw new InvalidDataException("Unknown custom SA type.");
            }
        }

        private static IEnumerable<string> GetFiles(string directoryOrManifestFilePath)
        {
            if (HttpUtilities.IsUrl(directoryOrManifestFilePath))
            {
                using (var reader = new StreamReader(PersistentStreamUtils.GetReadStream(directoryOrManifestFilePath)))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return LambdaUrlHelper.GetBaseUrl() + line;
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