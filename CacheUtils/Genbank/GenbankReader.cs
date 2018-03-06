using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Utilities;

namespace CacheUtils.Genbank
{
    public sealed class GenbankReader : IDisposable
    {
        private readonly StreamReader _reader;

        // ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/mRNA_Prot/human.*.rna.gbff.gz

        private const string LocusTag      = "LOCUS";
        private const string VersionTag    = "VERSION";
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
            string transcriptId    = null;
            string proteinId       = null;
            string geneId          = null;
            string geneSymbol      = null;
            IInterval codingRegion = null;
            var exons              = new List<IInterval>();
            byte transcriptVersion = 0;
            byte proteinVersion    = 0;

            var currentState = GenbankState.Header;
            var featureState = FeaturesState.Unknown;
            
            // assert that the record starts with LOCUS
            if (!HasLocus()) return null;

            while (true)
            {
                var line = _reader.ReadLine();
                if (line == null || line.StartsWith(TerminatorTag)) break;

                if (line.StartsWith(FeaturesTag)) currentState = GenbankState.Features;
                else if (line.StartsWith(OriginTag)) currentState = GenbankState.Origin;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (currentState)
                {
                    case GenbankState.Header:
                        if (line.StartsWith(VersionTag)) (transcriptId, transcriptVersion) = ParseVersion(line);
                        break;

                    case GenbankState.Features:
                        bool isNewState;
                        (featureState, isNewState) = GetFeatureState(featureState, line);
                        var info    = line.Substring(FeatureColumnLength);

                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (featureState)
                        {
                            case FeaturesState.Gene:
                                if (info.StartsWith(GeneIdTag))     geneId     = ParseGeneId(info);
                                if (info.StartsWith(GeneSymbolTag)) geneSymbol = ParseGeneSymbol(info);
                                break;
                            case FeaturesState.Cds:
                                if (isNewState) codingRegion = GetInterval(info);
                                if (info.StartsWith(ProteinIdTag)) (proteinId, proteinVersion) = ParseProteinId(info);
                                break;
                            case FeaturesState.Exon:
                                if (isNewState) exons.Add(GetInterval(info));
                                break;
                        }
                        break;
                }
            }

            return transcriptId == null
                ? null
                : new GenbankEntry(transcriptId, transcriptVersion, proteinId, proteinVersion, geneId, geneSymbol,
                    codingRegion, exons.Count == 0 ? null : exons.ToArray());
        }

        private static string ParseGeneSymbol(string info) => info.Substring(GeneSymbolTag.Length).Trim('"');
        private static string ParseGeneId(string info)     => info.Substring(GeneIdTag.Length).Trim('"');

        private static (string Id, byte Version) ParseProteinId(string info)
        {
            var rawId = info.Substring(ProteinIdTag.Length).Trim('"');
            return FormatUtilities.SplitVersion(rawId);
        }

        private static IInterval GetInterval(string info)
        {
            if (info.StartsWith("join")) return GetJoinInterval(info);

            var coordinates = info.Split("..");
            if (coordinates.Length != 2) throw new InvalidDataException("Expected two coordinates in the exon feature line.");

            var start = int.Parse(coordinates[0].TrimStart('<'));
            var end   = int.Parse(coordinates[1].TrimStart('>'));
            return new Interval(start, end);
        }

        private static IInterval GetJoinInterval(string info)
        {
            var cols  = info.Substring(5, info.Length - 6).Split(',');
            var start = int.Parse(cols[0].Split("..")[0]);
            var end   = int.Parse(cols[1].Split("..")[1]);
            return new Interval(start, end);
        }

        private static (FeaturesState State, bool IsNewState) GetFeatureState(FeaturesState featureState, string line)
        {
            var label = line.Substring(0, FeatureColumnLength).Trim();
            if (string.IsNullOrEmpty(label)) return (featureState, false);

            if (label.StartsWith(GeneFeatureTag)) return (FeaturesState.Gene, true);
            if (label.StartsWith(ExonFeatureTag)) return (FeaturesState.Exon, true);
            return label.StartsWith(CdsFeatureTag) ? (FeaturesState.Cds, true) : (FeaturesState.Unknown, true);
        }

        private bool HasLocus()
        {
            var line = _reader.ReadLine();
            return line != null && line.StartsWith(LocusTag);
        }

        private static (string TranscriptId, byte TranscriptVersion) ParseVersion(string line)
        {
            var accession = line.Substring(12).Trim();
            return FormatUtilities.SplitVersion(accession);
        }

        public void Dispose() => _reader.Dispose();
    }
}
