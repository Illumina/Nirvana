using System.Collections.Generic;
using Genome;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Gene : IGene
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public bool OnReverseStrand { get; }
        public string Symbol { get; }
        public ICompactId EntrezGeneId { get; }
        public ICompactId EnsemblId { get; }
        public int HgncId { get; }

        public Gene(IChromosome chromosome, int start, int end, bool onReverseStrand, string symbol, int hgncId,
            CompactId entrezGeneId, CompactId ensemblId)
        {
            OnReverseStrand = onReverseStrand;
            Symbol          = symbol;
            HgncId          = hgncId;
            EntrezGeneId    = entrezGeneId;
            EnsemblId       = ensemblId;
            Start           = start;
            End             = end;
	        Chromosome		= chromosome;
        }

        public static IGene Read(IBufferedBinaryReader reader, IDictionary<ushort, IChromosome> indexToChromosome)
        {
            ushort referenceIndex = reader.ReadOptUInt16();
            int start             = reader.ReadOptInt32();
            int end               = reader.ReadOptInt32();
            bool onReverseStrand  = reader.ReadBoolean();
            string symbol         = reader.ReadAsciiString();
            int hgncId            = reader.ReadOptInt32();
            var entrezId          = CompactId.Read(reader);
            var ensemblId         = CompactId.Read(reader);

            return new Gene(indexToChromosome[referenceIndex], start, end, onReverseStrand, symbol, hgncId, entrezId, ensemblId);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.Write(OnReverseStrand);
            writer.WriteOptAscii(Symbol);
            writer.WriteOpt(HgncId);
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            EntrezGeneId.Write(writer);
            EnsemblId.Write(writer);
        }
    }
}
