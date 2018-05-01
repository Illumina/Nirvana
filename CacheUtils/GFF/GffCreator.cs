using System;
using System.Collections.Generic;
using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.GFF
{
    public sealed class GffCreator
    {
        private readonly IDictionary<IGene, int> _geneToInternalId;
        private readonly GffWriter _writer;
        private readonly HashSet<int> _observedGenes;

        public GffCreator(GffWriter writer, IDictionary<IGene, int> geneToInternalId)
        {
            _writer           = writer;
            _geneToInternalId = geneToInternalId;
            _observedGenes    = new HashSet<int>();
        }

        public void Create(IEnumerable<IntervalArray<ITranscript>> transcriptIntervalArrays)
        {
            Console.Write("- writing GFF entries... ");
            foreach (var transcriptArray in transcriptIntervalArrays)
            {
                if (transcriptArray == null) continue;
                foreach (var interval in transcriptArray.Array) Write(interval.Value);
            }
            Console.WriteLine("finished.");
        }

        private void Write(ITranscript transcript)
        {
            var requiredFields = GetRequiredFields(transcript);
            var attribs        = GetGeneralAttributes(transcript);

            WriteGene(transcript.Gene, requiredFields, attribs.GeneId, attribs.InternalGeneId);
            WriteTranscript(transcript, requiredFields, attribs);

            var exons        = transcript.TranscriptRegions.GetExons();
            var codingRegion = transcript.Translation?.CodingRegion;

            foreach (var exon in exons) WriteExon(exon, requiredFields, attribs, codingRegion);
        }

        private void WriteTranscript(IInterval interval, IRequiredFields requiredFields, IGeneralAttributes attribs) =>
            _writer.WriteTranscript(interval, requiredFields, attribs);

        private void WriteGene(IGene gene, IRequiredFields requiredFields, string geneId, int internalGeneId)
        {
            if (_observedGenes.Contains(internalGeneId)) return;

            _observedGenes.Add(internalGeneId);
            var gffGene = GetGene(gene, geneId);
            _writer.WriteGene(gffGene, requiredFields, internalGeneId);
        }

        private void WriteExon(ITranscriptRegion exon, IRequiredFields requiredFields, IGeneralAttributes attribs,
            IInterval codingRegion)
        {
            _writer.WriteExonicRegion(exon, requiredFields, attribs, exon.Id, "exon");
            WriteCds(codingRegion, exon, requiredFields, attribs);
            WriteUtr(codingRegion, exon, requiredFields, attribs);
        }

        private void WriteUtr(IInterval codingRegion, ITranscriptRegion exon, IRequiredFields requiredFields,
            IGeneralAttributes attribs)
        {
            if (!GffUtilities.HasUtr(codingRegion, exon)) return;
            if (exon.Start < codingRegion.Start) Write5PrimeUtr(codingRegion, exon, requiredFields, attribs);
            if (exon.End > codingRegion.End) Write3PrimeUtr(codingRegion, exon, requiredFields, attribs);
        }

        private void Write5PrimeUtr(IInterval codingRegion, ITranscriptRegion exon, IRequiredFields requiredFields,
            IGeneralAttributes attribs)
        {
            int utrEnd = codingRegion.Start - 1;
            if (utrEnd > exon.End) utrEnd = exon.End;
            _writer.WriteExonicRegion(new Interval(exon.Start, utrEnd), requiredFields, attribs, exon.Id, "UTR");
        }

        private void Write3PrimeUtr(IInterval codingRegion, ITranscriptRegion exon, IRequiredFields requiredFields,
            IGeneralAttributes attribs)
        {
            int utrStart = codingRegion.End + 1;
            if (utrStart < exon.Start) utrStart = exon.Start;
            _writer.WriteExonicRegion(new Interval(utrStart, exon.End), requiredFields, attribs, exon.Id, "UTR");
        }

        private void WriteCds(IInterval codingRegion, ITranscriptRegion exon, IRequiredFields requiredFields, IGeneralAttributes attribs)
        {
            if (!GffUtilities.HasCds(codingRegion, exon)) return;
            var cds = GffUtilities.GetCdsCoordinates(codingRegion, exon);
            _writer.WriteExonicRegion(cds, requiredFields, attribs, exon.Id, "CDS");
        }

        private static IGffGene GetGene(IGene gene, string id) => new GffGene(gene.Start, gene.End, id,
            gene.EntrezGeneId.WithVersion, gene.EnsemblId.WithVersion, gene.Symbol);

        private static IRequiredFields GetRequiredFields(ITranscript transcript)
        {
            string source = transcript.Source.ToString();
            return new RequiredFields(transcript.Chromosome.UcscName, source, transcript.Gene.OnReverseStrand);
        }

        private IGeneralAttributes GetGeneralAttributes(ITranscript transcript)
        {
            string bioType     = AnnotatedTranscript.GetBioType(transcript.BioType);
            int internalGeneId = _geneToInternalId[transcript.Gene];
            string geneId      = transcript.Source == Source.Ensembl
                ? transcript.Gene.EnsemblId.WithVersion
                : transcript.Gene.EntrezGeneId.WithVersion;

            return new GeneralAttributes(geneId, transcript.Gene.Symbol, transcript.Id.WithVersion,
                transcript.Translation?.ProteinId?.WithVersion, bioType, transcript.IsCanonical, internalGeneId);
        }
    }
}
