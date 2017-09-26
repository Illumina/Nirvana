using System;
using System.IO;
using ErrorHandling;
using SAUtils.TsvWriters;
using CommonUtilities;

namespace SAUtils.GeneScoresTsv
{
    public class GeneScoreTsvCreator
    {
        private StreamReader _reader;
        private GeneAnnotationTsvWriter _writer;

        public const string PliTag = "pLI";
        public const string PnullTag = "pNull";
        public const string PrecTag = "pRec";

        public GeneScoreTsvCreator(StreamReader reader, GeneAnnotationTsvWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public ExitCodes Create()
        {
            bool isFirstLine = true;
            using (_reader)
            using (_writer)
            {
                string line;
                while ((line=_reader.ReadLine())!=null)
                {
                    if (isFirstLine)
                    {
                        var indices = GetColumnIndices(line);
                        if (indices.pLiIndex < 0 && indices.pRecIndex < 0 && indices.pNullIndex < 0)
                            return ExitCodes.InvalidData;

                        isFirstLine = false;
                    }

                    var scores = GetScores(line);
                }

            }
            
            return ExitCodes.Success;
        }

        private (string pLi, string pRec, string pNull) GetScores(string line)
        {
            var tabIndices = line.AllIndicesOf('\t');
            var pLi = line.GetSlice()
        }

        private (int pLiIndex, int pRecIndex, int pNullIndex) GetColumnIndices(string line)
        {
            var cols = line.Split("\t");

            var pLiIndex = Array.IndexOf(cols, PliTag);
            var pNullIndex = Array.IndexOf(cols, PnullTag);
            var pRecIndex = Array.IndexOf(cols, PrecTag);

            return (pLiIndex, pRecIndex, pNullIndex);
        }
    }
}