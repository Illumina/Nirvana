using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAUtils.CreateOmimTsv
{
    public sealed class OmimReader : IDisposable
    {
        private readonly Stream _stream;

        private int _mimNumberCol       = -1;
        private int _hgncCol            = -1;
        private int _geneDescriptionCol = -1;
        private int _phenotypeCol       = -1;
        private int _entrezGeneIdCol    = -1;
        private int _ensemblGeneIdCol   = -1;

        public OmimReader(Stream stream) => _stream = stream;

        public void AddOmimEntries(Dictionary<int, OmimImportEntry> omimEntries)
        {
            using (var reader = new StreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (IsHeader(line))
                    {
                        ParseHeader(line);
                        continue;
                    }

                    if (IsCommentLine(line)) continue;

                    var contents      = line.Split('\t');
                    var mimNumber     = Convert.ToInt32(contents[_mimNumberCol]);
                    var geneSymbol    = contents[_hgncCol];
                    var description   = _geneDescriptionCol >= 0 ? contents[_geneDescriptionCol].Replace(@"\\'", @"'") : null;
                    var phenotypeInfo = _phenotypeCol >= 0 ? contents[_phenotypeCol].Replace(@",,", @",") : null;
                    var entrezGeneId  = _entrezGeneIdCol >= 0 ? contents[_entrezGeneIdCol] : null;
                    var ensemblGeneId = _ensemblGeneIdCol >= 0 ? contents[_ensemblGeneIdCol] : null;

                    if (string.IsNullOrEmpty(geneSymbol) || omimEntries.ContainsKey(mimNumber)) continue;

                    omimEntries[mimNumber] = new OmimImportEntry(mimNumber, geneSymbol, description, phenotypeInfo,
                        entrezGeneId, ensemblGeneId);
                }
            }
        }

        private void ParseHeader(string line)
        {
            line = line.Trim('#').Trim(' ');
            var colNames = line.Split('\t').Select(x => x.Trim(' ')).ToList();

            for (var index = 0; index < colNames.Count; index++)
            {
                var colname = colNames[index].ToLower();

                if (colname == "mim number")
                {
                    _mimNumberCol = index;
                }
                else if (colname == "gene name")
                {
                    _geneDescriptionCol = index;
                }
                else if (colname == "approved symbol" || colname.StartsWith("approved gene symbol"))
                {
                    _hgncCol = index;
                }
                else if (colname == "phenotypes")
                {
                    _phenotypeCol = index;
                }
                else if (colname == "entrez gene id" || colname == "entrez gene id (ncbi)")
                {
                    _entrezGeneIdCol = index;
                }
                else if (colname == "ensembl gene id" || colname == "ensembl gene id (ensembl)")
                {
                    _ensemblGeneIdCol = index;
                }
            }
        }

        private static bool IsHeader(string line) => line.StartsWith("# Chromosome\t") || line.StartsWith("# MIM Number\t");

        private static bool IsCommentLine(string line) => line.StartsWith("#");

        public void Dispose() => _stream.Dispose();
    }
}