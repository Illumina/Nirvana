using System.IO;

namespace CacheUtils.IntermediateIO
{
    public static class IntermediateIoCommon
    {
        public const string Header = "NirvanaIntermediateIo";

        public enum FileType : byte
        {
            Genbank,
            Polyphen,
            Regulatory,
            Sift,
            Transcript
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public static IntermediateIoHeader ReadHeader(StreamReader reader, FileType expectedType)
        {
            (string id, FileType type, IntermediateIoHeader header) = IntermediateIoHeader.Read(reader);
            if (id != Header || type != expectedType) throw new InvalidDataException("Could not verify the header tag or the file type in the header.");
            return header;
        }
    }
}
