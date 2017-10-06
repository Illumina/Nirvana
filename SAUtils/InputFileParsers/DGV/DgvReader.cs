using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.DGV
{
    public sealed class DgvReader
    {
        #region members

        private readonly FileInfo _dgvFileInfo;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        #endregion

        // constructor
        public DgvReader(FileInfo dgvFileInfo, IDictionary<string, IChromosome> refChromDict)
        {
            _dgvFileInfo = dgvFileInfo;
            _refChromDict = refChromDict;
        }

        /// <summary>
        /// returns a ClinVar object given the vcf line
        /// </summary>
        public static DgvItem ExtractDgvItem(string line, IDictionary<string, IChromosome> refChromDict)
        {
            var cols = line.Split('\t');
            if (cols.Length < 8) return null;

            var id = cols[0];
            var chromosomeName = cols[1];
            if (!refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome = refChromDict[chromosomeName];

            var start = int.Parse(cols[2]);
            var end = int.Parse(cols[3]);
            var variantType = cols[4];
            var variantSubType = cols[5];
            var sampleSize = int.Parse(cols[14]);
            var observedGains = cols[15] == "" ? 0 : int.Parse(cols[15]);
            var observedLosses = cols[16] == "" ? 0 : int.Parse(cols[16]);

            var seqAltType = SaParseUtilities.GetSequenceAlterationType(variantType, variantSubType);

            return new DgvItem(id, chromosome, start, end, sampleSize, observedGains, observedLosses, seqAltType);
        }



        public IEnumerable<DgvItem> GetDgvItems()
        {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_dgvFileInfo.FullName))
            {
                while (true)
                {
                    // grab the next line
                    string line = reader.ReadLine();
                    if (line == null) break;

                    // skip header and empty lines
                    if (string.IsNullOrWhiteSpace(line) || IsDgvHeader(line)) continue;
                    var dgvItem = ExtractDgvItem(line, _refChromDict);
                    if (dgvItem == null) continue;
                    yield return dgvItem;
                }
            }
        }

        private static bool IsDgvHeader(string line)
        {
            return line.StartsWith("variantaccession");
        }
    }
}