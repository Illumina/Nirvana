using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.FileHandling;
using Genome;

namespace Tabix
{
    public static class Reader
    {
        public static Index Read(BinaryReader reader, IDictionary<string, IChromosome> refNameToChromosome)
        {
            int magic = reader.ReadInt32();
            if (magic != Constants.TabixMagic) throw new InvalidDataException("This does not seem to be a tabix file. Did you use a GZipStream?");

            int numReferenceSequences       = reader.ReadInt32();
            int format                      = reader.ReadInt32();
            int sequenceNameIndex           = reader.ReadInt32() - 1;
            int sequenceBeginIndex          = reader.ReadInt32() - 1;
            int sequenceEndIndex            = reader.ReadInt32() - 1;
            var commentChar                 = (char)reader.ReadInt32();
            int numLinesToSkip              = reader.ReadInt32();
            int concatenatedSequenceNameLen = reader.ReadInt32();
            var concatenatedNames           = reader.ReadBytes(concatenatedSequenceNameLen);

            var referenceSequenceNames = GetReferenceSequenceNames(concatenatedNames, numReferenceSequences);
            var referenceSequences     = new ReferenceSequence[numReferenceSequences];
            var refIndexToTabixIndex   = new Dictionary<ushort, ushort>(numReferenceSequences);

            for (ushort i = 0; i < numReferenceSequences; i++)
            {
                string chromosomeName = referenceSequenceNames[i];
                var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, chromosomeName);
                referenceSequences[i] = ReadReferenceSequence(reader, chromosome);
                refIndexToTabixIndex[chromosome.Index] = i;
            }

            return new Index(format, sequenceNameIndex, sequenceBeginIndex, sequenceEndIndex, commentChar,
                numLinesToSkip, referenceSequences, refIndexToTabixIndex);
        }

        public static Index GetTabixIndex(Stream tabixStream, IDictionary<string, IChromosome> refNameToChromosome)
        {
            using (var binaryReader = new BinaryReader(new BlockGZipStream(tabixStream, CompressionMode.Decompress)))
            {
                return Read(binaryReader, refNameToChromosome);
            }
        }

        private static string[] GetReferenceSequenceNames(byte[] concatenatedBytes, int numRefSeqs)
        {
            var refSeqNames = new string[numRefSeqs];
            var nullIndexes = GetNullIndexes(concatenatedBytes, numRefSeqs);
            var startIndex = 0;

            var index = 0;
            foreach (int nullIndex in nullIndexes)
            {
                refSeqNames[index++] = Encoding.ASCII.GetString(concatenatedBytes, startIndex, nullIndex - startIndex);
                startIndex = nullIndex + 1;
            }

            return refSeqNames;
        }

        private static IEnumerable<int> GetNullIndexes(IReadOnlyList<byte> bytes, int numRefSeqs)
        {
            var nullPositions = new int[numRefSeqs];
            var index = 0;
            for (var pos = 0; pos < bytes.Count; pos++) if (bytes[pos] == 0) nullPositions[index++] = pos;
            return nullPositions;
        }

        private static ReferenceSequence ReadReferenceSequence(BinaryReader reader, IChromosome chromosome)
        {
            int numBins = reader.ReadInt32();
            var idToChunks = new Dictionary<int, Interval[]>();

            for (var i = 0; i < numBins; i++)
            {
                (int id, var chunks) = ReadBin(reader);
                idToChunks[id] = chunks;
            }

            int numLinearFileOffsets = reader.ReadInt32();
            var linearFileOffsets    = new ulong[numLinearFileOffsets];

            for (var i = 0; i < numLinearFileOffsets; i++)
            {
                linearFileOffsets[i] = reader.ReadUInt64();
            }

            return new ReferenceSequence(chromosome, idToChunks, linearFileOffsets);
        }

        private static (int Id, Interval[] Chunks) ReadBin(BinaryReader reader)
        {
            int id        = reader.ReadInt32();
            int numChunks = reader.ReadInt32();

            var chunks = new Interval[numChunks];
            for (var i = 0; i < numChunks; i++) chunks[i] = ReadChunk(reader);

            return (id, chunks);
        }

        private static Interval ReadChunk(BinaryReader reader)
        {
            ulong begin = reader.ReadUInt64();
            ulong end   = reader.ReadUInt64();
            return new Interval(begin, end);
        }
    }
}
