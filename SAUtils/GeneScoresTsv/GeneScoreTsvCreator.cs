using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrorHandling;
using SAUtils.TsvWriters;
using VariantAnnotation.IO;

namespace SAUtils.GeneScoresTsv
{
    public sealed class GeneScoreTsvCreator
    {
        private readonly StreamReader _reader;
        private readonly GeneAnnotationTsvWriter _writer;

        private const string GeneTag  = "gene";
        private const string PliTag   = "pLI";
        private const string PrecTag  = "pRec";
        private const string PnullTag = "pNull";

        private int _geneIndex  = -1;
        private int _pliIndex   = -1;
        private int _precIndex  = -1;
        private int _pnullIndex = -1;

        public GeneScoreTsvCreator(StreamReader reader, GeneAnnotationTsvWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public ExitCodes Create()
        {
            bool isFirstLine = true;

            var genes = new List<(string gene, string pLi, string pRec, string pNull)>();
            using (_reader)
            using (_writer)
            {
                string line;
                while ((line=_reader.ReadLine())!=null)
                {
                    if (isFirstLine)
                    {
                        GetColumnIndices(line);
                        if (_geneIndex < 0)
                        {
                            Console.WriteLine("gene column not found");
                            return ExitCodes.InvalidData;
                        }
                        if (_pliIndex < 0)
                        {
                            Console.WriteLine("pLI column not found");
                            return ExitCodes.InvalidData;
                        }
                        if (_precIndex < 0)
                        {
                            Console.WriteLine("pRec column not found");
                            return ExitCodes.InvalidData;
                        }
                        if (_pnullIndex < 0)
                        {
                            Console.WriteLine("pNull column not found");
                            return ExitCodes.InvalidData;
                        }

                        isFirstLine = false;

                    }
                    else
                        genes.Add(GetGeneAndScores(line));
                    
                }

                foreach (var geneScores in genes.OrderBy(x=>x.gene))
                {
                    WriteScores(geneScores.gene, geneScores.pLi, geneScores.pRec, geneScores.pNull);
                }

            }
            
            return ExitCodes.Success;
        }

        private void WriteScores(string gene, string pLi, string pRec, string pNull)
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("pLi", pLi, false);
            jsonObject.AddStringValue("pRec", pRec, false);
            jsonObject.AddStringValue("pNull", pNull, false);
            sb.Append(JsonObject.CloseBrace);

            _writer.AddEntry(gene, new List<string>(){sb.ToString()});
        }

        private (string gene, string pLi, string pRec, string pNull) GetGeneAndScores(string line)
        {
            var cols  = line.Split('\t');
            var gene  = cols[_geneIndex];
            var pLi   = cols[_pliIndex];
            var pRec  = cols[_precIndex];
            var pNull = cols[_pnullIndex];

            return (gene, pLi, pRec, pNull);
        }

        private void GetColumnIndices(string line)
        {
            var cols = line.Split("\t");

            _geneIndex  = Array.IndexOf(cols, GeneTag);
            _pliIndex   = Array.IndexOf(cols, PliTag);
            _pnullIndex = Array.IndexOf(cols, PnullTag);
            _precIndex  = Array.IndexOf(cols, PrecTag);
        }
    }
}