using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SAUtils.InputFileParsers.DGV
{
    public class DgvReader : IEnumerable<DgvItem>
    {
        #region members

        private readonly FileInfo _dgvFileInfo;

        #endregion

        #region IEnumerable implementation

        public IEnumerator<DgvItem> GetEnumerator()
        {
            return GetDgvItems().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        // constructor
        public DgvReader(FileInfo dgvFileInfo)
        {
            _dgvFileInfo = dgvFileInfo;
        }

        /// <summary>
        /// returns a ClinVar object given the vcf line
        /// </summary>
        internal static DgvItem ExtractDgvItem(string line)
        {
            var cols = line.Split('\t');
            if (cols.Length < 8) return null;

            var id = cols[0];
            var chromosome = cols[1];
            var start = int.Parse(cols[2]);
            var end = int.Parse(cols[3]);
            var variantType = cols[4];
            var variantSubType = cols[5];
            var sampleSize = int.Parse(cols[14]);
            var observedGains = cols[15] == "" ? 0 : int.Parse(cols[15]);
            var observedLosses = cols[16] == "" ? 0 : int.Parse(cols[16]);

            var seqAltType = SequenceAlterationUtilities.GetSequenceAlteration(variantType, variantSubType);

            return new DgvItem(id, chromosome, start, end, sampleSize, observedGains, observedLosses, seqAltType);
        }



        /// <summary>
        /// Parses a ClinVar file and return an enumeration object containing all the ClinVar objects
        /// that have been extracted
        /// </summary>
        private IEnumerable<DgvItem> GetDgvItems()
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
                    var dgvItem = ExtractDgvItem(line);
                    if (dgvItem == null) continue;
                    yield return dgvItem;
                }
            }
        }

        private bool IsDgvHeader(string line)
        {
            return line.StartsWith("variantaccession");
        }
    }
}