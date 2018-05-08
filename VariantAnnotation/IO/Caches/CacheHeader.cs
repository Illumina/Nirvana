using System.IO;
using System.Text;

namespace VariantAnnotation.IO.Caches
{
    public sealed class CacheHeader : Header
    {
        public readonly TranscriptCacheCustomHeader Custom;

        public CacheHeader(Header header, TranscriptCacheCustomHeader customHeader) : base(header.Identifier,
            header.SchemaVersion, header.DataVersion, header.Source, header.CreationTimeTicks,
            header.Assembly)
        {
            Custom = customHeader;
        }

        public new void Write(BinaryWriter writer)
        {
            base.Write(writer);
            Custom.Write(writer);
        }

        public static CacheHeader Read(Stream stream)
        {
            CacheHeader header;

            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                var baseHeader   = Read(reader);
                var customHeader = TranscriptCacheCustomHeader.Read(reader);
                header           = new CacheHeader(baseHeader, customHeader);
            }

            return header;
        }
    }
}
