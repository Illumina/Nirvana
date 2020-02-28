using System.IO;
using System.Text;
using Genome;
using IO;
using VariantAnnotation.Sequence;

namespace ReferenceUtils.Common
{
    internal sealed class ReferenceSequence
    {
        private readonly ushort _refIndex;
        private byte[] _buffer;
        private MaskedEntry[] _maskedEntries;
        private Band[] _cytogeneticBands;
        private readonly int _sequenceOffset;
        private readonly int _numBases;

        public ReferenceSequence(ushort refIndex, byte[] buffer, MaskedEntry[] maskedEntries, Band[] cytogeneticBands, int sequenceOffset, int numBases)
        {
            _refIndex         = refIndex;
            _buffer           = buffer;
            _maskedEntries    = maskedEntries;
            _cytogeneticBands = cytogeneticBands;
            _sequenceOffset   = sequenceOffset;
            _numBases         = numBases;
        }

        public CompressionBlock GetBlock()
        {
            CompressionBlock block;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    WriteMetadata(writer);
                    WriteBuffer(writer);
                    WriteMaskedEntries(writer);
                    WriteCytogeneticBands(writer);
                    writer.Flush();
                }

                var    numBytes = (int) ms.Position;
                byte[] buffer   = ms.ToArray();

                block = new CompressionBlock(_refIndex, buffer, numBytes);
                block.Compress();
            }

            _buffer           = null;
            _maskedEntries    = null;
            _cytogeneticBands = null;

            return block;
        }

        private void WriteMetadata(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_sequenceOffset);
            writer.WriteOpt(_numBases);
        }

        private void WriteCytogeneticBands(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_cytogeneticBands.Length);

            foreach (var band in _cytogeneticBands)
            {
                writer.WriteOpt(band.Begin);
                writer.WriteOpt(band.End);
                writer.WriteOptAscii(band.Name);
            }
        }

        private void WriteMaskedEntries(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_maskedEntries.Length);

            foreach (var maskedEntry in _maskedEntries)
            {
                writer.WriteOpt(maskedEntry.Begin);
                writer.WriteOpt(maskedEntry.End);
            }
        }

        private void WriteBuffer(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_buffer.Length);
            writer.Write(_buffer);
        }
    }
}