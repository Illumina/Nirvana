using System;
using System.Collections.Generic;
using System.IO;
using Intervals;
using OptimizedCore;
using VariantAnnotation.Utilities;

namespace CacheUtils.Genbank
{
    public sealed class GenbankReader : IDisposable
    {
        private readonly StreamReader _reader;

        // ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/mRNA_Prot/human.*.rna.gbff.gz

        private const string LocusTag      = "LOCUS";
        private const string FeaturesTag   = "FEATURES";
        private const string OriginTag     = "ORIGIN";
        private const string TerminatorTag = "//";

        private const string GeneFeatureTag = "gene";
        private const string CdsFeatureTag  = "CDS";
        private const string ExonFeatureTag = "exon";

        private const string ProteinIdTag  = "/protein_id=";
        private const string GeneIdTag     = "/db_xref=\"GeneID:";
        private const string GeneSymbolTag = "/gene=";

        private const int FeatureColumnLength = 21;

        public GenbankReader(StreamReader reader) => _reader = reader;

        public GenbankEntry GetGenbankEntry()
        {
            // assert that the record starts with LOCUS
            if (!HasLocus()) return null;

            (string transcriptId, byte transcriptVersion) = ParseHeader();
            var featureData = ParseFeatures();
            ParseOrigin();

            var exons = featureData.Exons.Count == 0 ? null : featureData.Exons.ToArray();

            return transcriptId == null
                ? null
                : new GenbankEntry(transcriptId, transcriptVersion, featureData.ProteinId, featureData.ProteinVersion,
                    featureData.GeneId, featureData.GeneSymbol, featureData.CodingRegion, exons);
        }

        private void ParseOrigin()
        {
            string line;
            do
            {
                line = GetNextLine();
            } while (line != null);
        }

        private string GetNextLine()
        {
            string line = _reader.ReadLine();
            if (line == null || line.StartsWith(TerminatorTag)) return null;
            return line;
        }

        private FeatureData ParseFeatures()
        {
            var featureState = FeaturesState.Unknown;
            var featureData = new FeatureData();

            while (true)
            {
                string line = GetNextLine();
                if (line == null || line.StartsWith(OriginTag)) break;

                bool isNewState;
                (featureState, isNewState) = GetFeatureState(line, featureState);
                string info = line.Substring(FeatureColumnLength);

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (featureState)
                {
                    case FeaturesState.Gene:
                        ParseGeneFeature(info, featureData);
                        break;
                    case FeaturesState.Cds:
                        ParseCdsFeature(isNewState, featureData, info);
                        break;
                    case FeaturesState.Exon:
                        ParseExonFeature(isNewState, featureData, info);
                        break;
                }
            }

            return featureData;
        }

        private static void ParseExonFeature(bool isNewState, FeatureData featureData, string info)
        {
            if (isNewState) featureData.Exons.Add(GetInterval(info));
        }

        private static void ParseCdsFeature(bool isNewState, FeatureData featureData, string info)
        {
            if (isNewState) featureData.CodingRegion = GetInterval(info);
            if (info.StartsWith(ProteinIdTag)) ParseProteinId(featureData, info);
        }

        private static void ParseGeneFeature(string info, FeatureData featureData)
        {
            if (info.StartsWith(GeneIdTag)) featureData.GeneId = ParseGeneId(info);
            if (info.StartsWith(GeneSymbolTag)) featureData.GeneSymbol = ParseGeneSymbol(info);
        }

        private (string TranscriptId, byte TranscriptVersion) ParseHeader()
        {
            const string versionTag = "VERSION";
            string transcriptId     = null;
            byte transcriptVersion  = 0;

            while (true)
            {
                string line = GetNextLine();
                if (line == null || line.StartsWith(FeaturesTag)) break;
                if (line.StartsWith(versionTag)) (transcriptId, transcriptVersion) = ParseVersion(line);
            }

            return (transcriptId, transcriptVersion);
        }

        private static string ParseGeneSymbol(string info) => info.Substring(GeneSymbolTag.Length).Trim('"');
        private static string ParseGeneId(string info)     => info.Substring(GeneIdTag.Length).Trim('"');

        private static void ParseProteinId(FeatureData featureData, string info)
        {
            string rawId = info.Substring(ProteinIdTag.Length).Trim('"');
            (featureData.ProteinId, featureData.ProteinVersion) = FormatUtilities.SplitVersion(rawId);
        }

        private static IInterval GetInterval(string info)
        {
            if (info.StartsWith("join")) return GetJoinInterval(info);

            var coordinates = info.Split("..");
            if (coordinates.Length != 2) throw new InvalidDataException("Expected two coordinates in the exon feature line.");

            int start = int.Parse(coordinates[0].TrimStart('<'));
            int end   = int.Parse(coordinates[1].TrimStart('>'));
            return new Interval(start, end);
        }

        private static IInterval GetJoinInterval(string info)
        {
            var cols  = info.Substring(5, info.Length - 6).OptimizedSplit(',');
            int start = int.Parse(cols[0].Split("..")[0]);
            int end   = int.Parse(cols[1].Split("..")[1]);
            return new Interval(start, end);
        }

        private static (FeaturesState State, bool IsNewState) GetFeatureState(string line, FeaturesState featureState)
        {
            string label = line.Substring(0, FeatureColumnLength).Trim();
            if (string.IsNullOrEmpty(label)) return (featureState, false);

            if (label.StartsWith(GeneFeatureTag)) return (FeaturesState.Gene, true);
            if (label.StartsWith(ExonFeatureTag)) return (FeaturesState.Exon, true);
            return label.StartsWith(CdsFeatureTag) ? (FeaturesState.Cds, true) : (FeaturesState.Unknown, true);
        }

        private bool HasLocus()
        {
            string line = _reader.ReadLine();
            return line != null && line.StartsWith(LocusTag);
        }

        private static (string TranscriptId, byte TranscriptVersion) ParseVersion(string line)
        {
            string accession = line.Substring(12).Trim();
            return FormatUtilities.SplitVersion(accession);
        }

        public void Dispose() => _reader.Dispose();

        private sealed class FeatureData
        {
            public string ProteinId;
            public byte ProteinVersion;
            public string GeneId;
            public string GeneSymbol;
            public IInterval CodingRegion;
            public readonly List<IInterval> Exons = new List<IInterval>();
        }
    }
}
