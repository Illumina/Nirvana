using System.Collections.Generic;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

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
        public int MimNumber { get; }

        /// <summary>
        /// constructor
        /// </summary>
        internal Gene(IChromosome chromosome, int start, int end, bool onReverseStrand, string symbol, int hgncId,
            CompactId entrezGeneId, CompactId ensemblId, int mimNumber)
        {
            OnReverseStrand = onReverseStrand;
            Symbol          = symbol;
            HgncId          = hgncId;
            EntrezGeneId    = entrezGeneId;
            EnsemblId       = ensemblId;
            MimNumber       = mimNumber;
            Start           = start;
            End             = end;
	        Chromosome		= chromosome;
        }

        /// <summary>
        /// reads the gene data from the binary reader
        /// </summary>
        public static IGene Read(IExtendedBinaryReader reader, IDictionary<ushort, IChromosome> indexToChromosome)
        {
            ushort referenceIndex = reader.ReadUInt16();
            int start             = reader.ReadOptInt32();
            int end               = reader.ReadOptInt32();
            bool onReverseStrand  = reader.ReadBoolean();
            string symbol         = reader.ReadAsciiString();
            int hgncId            = reader.ReadOptInt32();
            var entrezId          = CompactId.Read(reader);
            var ensemblId         = CompactId.Read(reader);
            int mimNumber         = reader.ReadOptInt32();

            return new Gene(indexToChromosome[referenceIndex], start, end, onReverseStrand, symbol, hgncId, entrezId, ensemblId, mimNumber);
        }

        /// <summary>
        /// writes the gene data to the binary writer
        /// </summary>
        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.Write(OnReverseStrand);
            writer.WriteOptAscii(Symbol);
            writer.WriteOpt(HgncId);
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            EntrezGeneId.Write(writer);
            EnsemblId.Write(writer);
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
            writer.WriteOpt(MimNumber);
        }
    }
}
