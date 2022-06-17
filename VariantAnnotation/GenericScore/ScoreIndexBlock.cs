using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ScoreIndexBlock
    {
        public readonly long FilePosition;
        public readonly int  BytesWritten;

        public ScoreIndexBlock(long filePosition, int bytesWritten)
        {
            FilePosition = filePosition;
            BytesWritten = bytesWritten;
        }

        /// <summary>
        /// Deserialize the instance from reader stream
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ScoreIndexBlock Read(ExtendedBinaryReader reader)
        {
            long filePosition = reader.ReadOptInt64();
            int  bytesWritten = reader.ReadOptInt32();

            return new ScoreIndexBlock(filePosition, bytesWritten);
        }

        /// <summary>
        /// Serialize the instance to writer stream
        /// </summary>
        /// <param name="writer"></param>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(FilePosition);
            writer.WriteOpt(BytesWritten);
        }
    }
}