using System.IO;
using System.Text;
using Genome;
using IO;
using VariantAnnotation.Sequence;

namespace ReferenceUtils.Common
{
    public sealed class ReferenceSequence
    {
        public readonly  ushort        RefIndex;
        private          byte[]        _buffer;
        private          MaskedEntry[] _maskedEntries;
        private          Band[]        _cytogeneticBands;
        private readonly int           _sequenceOffset;
        private readonly int           _numBases;

        public ReferenceSequence(ushort refIndex, byte[] buffer, MaskedEntry[] maskedEntries, Band[] cytogeneticBands,
            int sequenceOffset, int numBases)
        {
            RefIndex          = refIndex;
            _buffer           = buffer;
            _maskedEntries    = maskedEntries;
            _cytogeneticBands = cytogeneticBands;
            _sequenceOffset   = sequenceOffset;
            _numBases         = numBases;
        }

        public ReferenceBuffer GetReferenceBuffer(ushort refIndex)
        {
            int    bufferSize;
            byte[] buffer;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.Write(ReferenceSequenceCommon.ReferenceStartTag);
                    WriteMetadata(writer);
                    WriteBuffer(writer);
                    WriteMaskedEntries(writer);
                    WriteCytogeneticBands(writer);
                }

                bufferSize = (int) ms.Position;
                buffer     = ms.ToArray();
            }

            return new ReferenceBuffer(refIndex, buffer, bufferSize);
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