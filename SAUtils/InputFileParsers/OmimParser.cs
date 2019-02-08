using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.Omim;

namespace SAUtils.InputFileParsers
{
    namespace SAUtils.CreateOmimTsv
    {
        public sealed class OmimParser : IDisposable
        {
            private readonly StreamReader _reader;
            private readonly GeneSymbolUpdater _geneSymbolUpdater;
            private int _mimNumberCol = -1;
            private int _hgncCol = -1;
            private int _geneDescriptionCol = -1;
            private int _phenotypeCol = -1;
            private int _entrezGeneIdCol = -1;
            private int _ensemblGeneIdCol = -1;

            public OmimParser(StreamReader reader, GeneSymbolUpdater geneSymbolUpdater)
            {
                _reader = reader;
                _geneSymbolUpdater = geneSymbolUpdater;
            }

            public IEnumerable<OmimItem> GetItems()
            {
                using (_reader)
                {
                    string line;
                    while ((line = _reader.ReadLine()) != null)
                    {
                        if (IsHeader(line))
                        {
                            ParseHeader(line);
                            continue;
                        }

                        if (IsCommentLine(line)) continue;

                        var result = ExtractItem(line);
                        if (result==null) continue;

                        yield return result;
                    }
                }
            }

            private OmimItem ExtractItem(string line) //(int MimNumber, string GeneSymbol, string Description, string PhenotypeInfo, string EntrezGeneId, string EnsemblGeneId)
            {
                var contents = line.OptimizedSplit('\t');

                var mimNumber = Convert.ToInt32(contents[_mimNumberCol]);
                var geneSymbol = contents[_hgncCol];
                var description = _geneDescriptionCol >= 0 ? contents[_geneDescriptionCol].Replace(@"\\'", @"'") : null;
                var phenotypeInfo = _phenotypeCol >= 0 ? contents[_phenotypeCol].Replace(@",,", @",") : null;
                var entrezGeneId = _entrezGeneIdCol >= 0 ? contents[_entrezGeneIdCol] : null;
                var ensemblGeneId = _ensemblGeneIdCol >= 0 ? contents[_ensemblGeneIdCol] : null;

                var phenotypes = OmimUtilities.Parse(phenotypeInfo);
                if (string.IsNullOrEmpty(geneSymbol)) return null;
                var updatedGeneSymbol = _geneSymbolUpdater.UpdateGeneSymbol(geneSymbol, ensemblGeneId, entrezGeneId);
                
                return string.IsNullOrEmpty(updatedGeneSymbol) ? null : new OmimItem(updatedGeneSymbol, description, mimNumber, phenotypes);
            }

            private void ParseHeader(string line)
            {
                line = line.Trim('#').Trim(' ');
                var colNames = line.OptimizedSplit('\t').Select(x => x.Trim(' ')).ToList();

                for (var index = 0; index < colNames.Count; index++)
                {
                    var colname = colNames[index].ToLower();

                    switch (colname)
                    {
                        case "mim number":
                            _mimNumberCol = index;
                            break;
                        case "gene name":
                            _geneDescriptionCol = index;
                            break;
                        case "approved symbol":
                        case "approved gene symbol (hgnc)":
                            _hgncCol = index;
                            break;
                        case "phenotypes":
                            _phenotypeCol = index;
                            break;
                        case "entrez gene id":
                        case "entrez gene id (ncbi)":
                            _entrezGeneIdCol = index;
                            break;
                        case "ensembl gene id":
                        case "ensembl gene id (ensembl)":
                            _ensemblGeneIdCol = index;
                            break;
                    }
                }
            }

            private static bool IsHeader(string line) => line.StartsWith("# Chromosome\t") || line.StartsWith("# MIM Number\t");

            private static bool IsCommentLine(string line) => line.OptimizedStartsWith('#');

            public void Dispose() => _reader?.Dispose();
        }
    }
}