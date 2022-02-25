using System;
using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;

namespace SAUtils.ClinGen
{
    public sealed class DosageSensitivityParser:IDisposable
    {
        private readonly Stream _stream;

        private const string GeneSymbolTag = "#Gene Symbol";
        private const string GeneIdTag = "Gene ID";
        private const string HaploInsufficiencyScoreTag = "Haploinsufficiency Score";
        private const string TriploSensitivityScoreTag = "Triplosensitivity Score";

        private int _geneSymbolIndex = -1;
        private int _geneIdIndex = -1;
        private int _haploInsufficiencyScoreIndex = -1;
        private int _triploSensitivityScoreIndex = -1;

        public DosageSensitivityParser(Stream stream)
        {
            _stream = stream;
        }
        public void Dispose()
        {
            _stream?.Dispose();
        }

        public Dictionary<string, List<ISuppGeneItem>> GetItems()
        {
            var geneAnnotations = new Dictionary<string, List<ISuppGeneItem>>();
            var duplicateGenes  = new HashSet<string>();
            
            using (var reader = new StreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                        ParseHeaderLine(line);
                    }
                    else
                    {
                        if (MissingIndices()) throw new InvalidDataException("Column indices not set!!");
                        var geneAnnotation = GetGeneAndScores(line);
                        bool isDuplicate = geneAnnotations.TryAdd(geneAnnotation.GeneSymbol, new List<ISuppGeneItem> { geneAnnotation });
                        if (!isDuplicate)
                        {
                            duplicateGenes.Add(geneAnnotation.GeneSymbol);
                            if (geneAnnotation.GetJsonString() != geneAnnotations[geneAnnotation.GeneSymbol][0].GetJsonString())
                            {
                                Console.WriteLine(geneAnnotation.GetJsonString());
                                Console.WriteLine(geneAnnotations[geneAnnotation.GeneSymbol][0].GetJsonString());
                                throw new DataMisalignedException($"Duplicate gene entries have conflicting informatioin.");
                            }
                        }
                    }
                }
                Console.WriteLine($"WARNING: Duplicate entries found for genes:{string.Join(',', duplicateGenes)}. But the contents were identical.");
            }

            return geneAnnotations;
        }

        private ISuppGeneItem GetGeneAndScores(string line)
        {
            var cols = line.OptimizedSplit('\t');

            var gene    = cols[_geneSymbolIndex];
            if (!int.TryParse(cols[_haploInsufficiencyScoreIndex], out var hiScore)) hiScore = -1;
            if (!int.TryParse(cols[_triploSensitivityScoreIndex], out var tsScore)) tsScore = -1;

            return new DosageSensitivityItem(gene, hiScore, tsScore);
        }

        private bool MissingIndices()
        {
            return _geneSymbolIndex == -1 ||
                   _geneIdIndex == -1 ||
                   _haploInsufficiencyScoreIndex == -1 ||
                   _triploSensitivityScoreIndex == -1;
        }

        private void ParseHeaderLine(string line)
        {
            if (line.StartsWith("#Gene Symbol")) GetColumnIndices(line);
        }
        
        private void GetColumnIndices(string line)
        {
            var cols = line.OptimizedSplit('\t');

            _geneSymbolIndex                    = Array.IndexOf(cols, GeneSymbolTag);
            _geneIdIndex                        = Array.IndexOf(cols, GeneIdTag);
            _haploInsufficiencyScoreIndex       = Array.IndexOf(cols, HaploInsufficiencyScoreTag);
            _triploSensitivityScoreIndex        = Array.IndexOf(cols, TriploSensitivityScoreTag);
        }
    }
}