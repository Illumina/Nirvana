using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace CreateGeneSymbolsAndSynonyms
{
    public class GeneSymbolParser
    {
        #region members

        private const int HumanTaxonomicId = 9606;

        private readonly SortedDictionary<int, GeneInfo> _geneSymbols;
        private readonly Regex _hgncRegex;

        #endregion

        // constructor
        public GeneSymbolParser()
        {
            _hgncRegex   = new Regex("HGNC:HGNC:(\\d+)", RegexOptions.Compiled);
            _geneSymbols = new SortedDictionary<int, GeneInfo>();
        }

        /// <summary>
        /// loads the NCBI gene_info file
        /// </summary>
        public void LoadGeneInfo(string geneInfoPath)
        {
            const int numExpectedCols = 15;

            Console.Write("- loading gene_info data... ");

            using (var reader = GZipUtilities.GetAppropriateStreamReader(geneInfoPath))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    // skip comments
                    if (line.StartsWith("#")) continue;

                    var cols = line.Split('\t');
                    if (cols.Length != numExpectedCols)
                    {
                        throw new GeneralException(
                            $"Expected {numExpectedCols} columns, but found {cols.Length}: [{line}]");
                    }

                    // skip entries where we can't convert the taxonomic ID
                    int taxId;
                    if (!int.TryParse(cols[0], out taxId)) continue;
                    if (taxId != HumanTaxonomicId) continue;

                    // convert the geneID
                    int geneId = GetGeneId(cols[1]);

                    // we're going to use symbol rather than the one from the nomenclature authority
                    var symbol     = cols[2];
                    var synonyms   = FixNullValue(cols[4]);
                    var dbXRefs    = cols[5];
                    var hgncSymbol = FixNullValue(cols[10]);

                    // get the HGNC id
                    var hgncId = GetHgncId(dbXRefs);

                    // get the nomenclature source
                    GeneSymbolSource geneSymbolSource = GeneSymbolSource.NCBI;
                    if((hgncId != null) && (symbol == hgncSymbol)) geneSymbolSource = GeneSymbolSource.HGNC;

                    // add to the gene symbols list
                    GeneInfo geneInfo;
                    if (_geneSymbols.TryGetValue(geneId, out geneInfo))
                    {
                        throw new GeneralException("Found a conflicting geneID in gene_info: " + line);
                    }

                    geneInfo = new GeneInfo
                    {
                        GeneID           = geneId,
                        GeneSymbol       = symbol,
                        GeneSymbolSource = geneSymbolSource,
                        HgncID           = hgncId,
                        Synonyms         = synonyms
                    };

                    _geneSymbols[geneId] = geneInfo;
                }
            }

            Console.WriteLine("{0} genes loaded.", _geneSymbols.Count);
        }

        /// <summary>
        /// Converts the string representation of the gene ID to an integer
        /// </summary>
        private int GetGeneId(string geneIdString)
        {
            int geneId;
            if (!int.TryParse(geneIdString, out geneId))
            {
                throw new GeneralException($"Unable to convert the gene ID to an integer: [{geneIdString}]");
            }

            return geneId;
        }

        /// <summary>
        /// searches the cross-references to identify the HGNC id
        /// </summary>
        private int? GetHgncId(string dbCrossRefs)
        {
            var match = _hgncRegex.Match(dbCrossRefs);
            if (match.Success) return int.Parse(match.Groups[1].Value);
            return null;
        }

        /// <summary>
        /// removes the dashes that denote null values
        /// </summary>
        private string FixNullValue(string s)
        {
            return s == "-" ? null : s;
        }

        /// <summary>
        /// loads the NCBI gene2refseq file
        /// </summary>
        public void LoadGene2RefSeq(string gene2RefSeqPath)
        {
            const int numExpectedCols = 16;

            Console.Write("- loading gene2refseq data... ");

            using (var reader = GZipUtilities.GetAppropriateStreamReader(gene2RefSeqPath))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    // skip comments
                    if (line.StartsWith("#")) continue;

                    var cols = line.Split('\t');
                    if (cols.Length != numExpectedCols)
                    {
                        throw new GeneralException(
                            $"Expected {numExpectedCols} columns, but found {cols.Length}: [{line}]");
                    }

                    // skip entries where we can't convert the taxonomic ID
                    int taxId;
                    if (!int.TryParse(cols[0], out taxId)) continue;
                    if (taxId != HumanTaxonomicId) continue;

                    // convert the geneID
                    int geneId = GetGeneId(cols[1]);

                    // skip entries with null
                    var refSeqAccession = FixNullValue(cols[3]);
                    if (refSeqAccession == null) continue;

                    // add to the gene symbols list
                    GeneInfo geneInfo;
                    if (_geneSymbols.TryGetValue(geneId, out geneInfo))
                    {
                        geneInfo.RefSeqAccessions.Add(refSeqAccession);
                    }
                    else
                    {
                        Console.WriteLine("Could not find the entry in the list: " + line);
                        Environment.Exit(1);
                    }
                }
            }

            int numAccessions = _geneSymbols.Values.Sum(geneInfo => geneInfo.RefSeqAccessions.Count);

            Console.WriteLine("{0} accessions added.", numAccessions);
        }

        /// <summary>
        /// writes the gene symbols to an output file
        /// </summary>
        public void WriteGeneSymbols(string geneSymbolsPath)
        {
            Console.Write("- writing gene symbols... ");

            using (var writer = new StreamWriter(FileUtilities.GetCreateStream(geneSymbolsPath)))
            {
                writer.WriteLine("#GeneID\tSymbol\tSymbolSource\tHgncID\tSynonyms\tAccessions");
                foreach (var geneInfo in _geneSymbols.Values)
                {
                    writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", geneInfo.GeneID, geneInfo.GeneSymbol, geneInfo.GeneSymbolSource, geneInfo.HgncID, geneInfo.Synonyms, string.Join("|", geneInfo.RefSeqAccessions));
                }
            }

            Console.WriteLine("finished.");
        }
    }
}
