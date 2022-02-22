using System.IO;
using System.Text;
using Genome;
using Intervals;
using IO;

namespace ReferenceSequence.Creation
{
    internal sealed class ReferenceSequence
    {
        private readonly byte[]     _buffer;
        private readonly Interval[] _maskedEntries;
        private readonly Band[]     _cytogeneticBands;
        private readonly int        _sequenceOffset;
        private readonly int        _numBases;

        internal ReferenceSequence(byte[] buffer, Interval[] maskedEntries, Band[] cytogeneticBands,
            int sequenceOffset, int numBases)
        {
            _buffer           = buffer;
            _maskedEntries    = maskedEntries;
            _cytogeneticBands = cytogeneticBands;
            _sequenceOffset   = sequenceOffset;
            _numBases         = numBases;
        }

        internal ReferenceBuffer GetReferenceBuffer(ushort refIndex)
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
                writer.WriteOpt(maskedEntry.Start);
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