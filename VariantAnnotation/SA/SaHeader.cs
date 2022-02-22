using Genome;
using IO;
using Versioning;

namespace VariantAnnotation.SA
{
    public sealed record SaHeader(string JsonKey, GenomeAssembly Assembly, IDataSourceVersion Version,
        int SchemaVersion)
    {
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(SaCommon.GuardInt);
            writer.WriteOptAscii(JsonKey);
            writer.Write((byte) Assembly);
            writer.WriteOpt(SchemaVersion);
            Version.Write(writer);
        }

        public static SaHeader Read(ExtendedBinaryReader reader)
        {
            SaCommon.CheckGuardInt(reader, "SaHeader");
            var jsonKey       = reader.ReadAsciiString();
            var assembly      = (GenomeAssembly) reader.ReadByte();
            var schemaVersion = reader.ReadOptInt32();
            var version       = DataSourceVersion.Read(reader);

            return new SaHeader(jsonKey, assembly, version, schemaVersion);
        }

        public string             JsonKey       { get; } = JsonKey;
        public GenomeAssembly     Assembly      { get; } = Assembly;
        public IDataSourceVersion Version       { get; } = Version;
        public int                SchemaVersion { get; } = SchemaVersion;
    }
}