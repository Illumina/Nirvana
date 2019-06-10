using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers.DGV
{
    public sealed class DgvReader: IDisposable
    {
        #region members

        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        #endregion

        // constructor
        public DgvReader(StreamReader reader, IDictionary<string, IChromosome> refChromDict)
        {
            _reader = reader;
            _refChromDict = refChromDict;
        }

        /// <summary>
        /// returns a ClinVar object given the vcf line
        /// </summary>
        public static DgvItem ExtractDgvItem(string line, IDictionary<string, IChromosome> refChromDict)
        {
            var cols = line.OptimizedSplit('\t');
            if (cols.Length < 8) return null;

            string id = cols[0];
            string chromosomeName = cols[1];

            if (!refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome = refChromDict[chromosomeName];

            int start = int.Parse(cols[2]);
            int end = int.Parse(cols[3]);
            string variantType = cols[4];
            string variantSubType = cols[5];
            int sampleSize = int.Parse(cols[14]);
            int observedGains = cols[15] == "" ? 0 : int.Parse(cols[15]);
            int observedLosses = cols[16] == "" ? 0 : int.Parse(cols[16]);

            var seqAltType = SaParseUtilities.GetSequenceAlterationType(variantType, variantSubType);

            return new DgvItem(id, chromosome, start, end, sampleSize, observedGains, observedLosses, seqAltType);
        }



        public IEnumerable<DgvItem> GetItems()
        {
            using (var reader = _reader)
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

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}