using System;
using System.IO;
using Intervals;

namespace CacheUtils.GFF
{
    public sealed class GffWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        public GffWriter(StreamWriter writer) => _writer = writer;

        public void Dispose() => _writer.Dispose();

        private void WriteRequiredFields(IInterval interval, IRequiredFields fields, string feature)
        {
            char strand = fields.OnReverseStrand ? '-' : '+';
            _writer.Write($"{fields.UcscName}\t{fields.Source}\t{feature}\t{interval.Start}\t{interval.End}\t.\t{strand}\t.\t");
        }

        private static bool NotEmpty(string s) => !string.IsNullOrEmpty(s);

        private void WriteGeneralAttributes(IGeneralAttributes attribs)
        {
            if (NotEmpty(attribs.GeneId))       _writer.Write($"gene_id \"{attribs.GeneId}\"; ");
            if (NotEmpty(attribs.GeneSymbol))   _writer.Write($"gene_name \"{attribs.GeneSymbol}\"; ");
            if (NotEmpty(attribs.TranscriptId)) _writer.Write($"transcript_id \"{attribs.TranscriptId}\"; ");

            _writer.Write($"transcript_type \"{attribs.BioType}\"; ");
            if (attribs.IsCanonical) _writer.Write("tag \"canonical\"; ");

            if (NotEmpty(attribs.ProteinId)) _writer.Write($"protein_id \"{attribs.ProteinId}\"; ");
        }

        public void WriteGene(IGffGene gene, IRequiredFields requiredFields, int internalGeneId)
        {
            WriteRequiredFields(gene, requiredFields, "gene");
            if (!string.IsNullOrEmpty(gene.Id)) _writer.Write($"gene_id \"{gene.Id}\"; ");
            if (!string.IsNullOrEmpty(gene.EntrezGeneId)) _writer.Write($"entrez_gene_id \"{gene.EntrezGeneId}\"; ");
            if (!string.IsNullOrEmpty(gene.EnsemblGeneId)) _writer.Write($"ensembl_gene_id \"{gene.EnsemblGeneId}\"; ");
            if (!string.IsNullOrEmpty(gene.Symbol)) _writer.Write($"gene_name \"{gene.Symbol}\"; ");
            WriteInternalGeneId(internalGeneId);
        }

        private void WriteInternalGeneId(int geneId) => _writer.WriteLine($"internal_gene_id \"{geneId}\"; ");

        public void WriteTranscript(IInterval interval, IRequiredFields requiredFields, IGeneralAttributes attribs)
        {
            WriteRequiredFields(interval, requiredFields, "transcript");
            WriteGeneralAttributes(attribs);
            WriteInternalGeneId(attribs.InternalGeneId);
        }

        public void WriteExonicRegion(IInterval interval, IRequiredFields requiredFields, IGeneralAttributes attribs,
            ushort exonNumber, string feature)
        {
            WriteRequiredFields(interval, requiredFields, feature);
            WriteGeneralAttributes(attribs);
            _writer.Write($"exon_number {exonNumber}; ");
            WriteInternalGeneId(attribs.InternalGeneId);
        }
    }
}
