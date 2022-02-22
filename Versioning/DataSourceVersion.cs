using System;
using System.Text;
using IO;
using JSON;

namespace Versioning;

public sealed record DataSourceVersion
    (string Name, string? Description, string Version, long ReleaseDateTicks) : IDataSourceVersion, ISerializable
{
    public static DataSourceVersion Read(ExtendedBinaryReader reader)
    {
        string name             = reader.ReadAsciiString();
        string version          = reader.ReadAsciiString();
        long   releaseDateTicks = reader.ReadOptInt64();
        string description      = reader.ReadAsciiString();
        return new DataSourceVersion(name, description, version, releaseDateTicks);
    }

    public void Write(IExtendedBinaryWriter writer)
    {
        writer.WriteOptAscii(Name);
        writer.WriteOptAscii(Version);
        writer.WriteOpt(ReleaseDateTicks);
        writer.WriteOptAscii(Description);
    }

    private string GetReleaseDate() => new DateTime(ReleaseDateTicks).ToString("yyyy-MM-dd");

    public override string ToString() =>
        $"dataSource={Name},version:{Version},release date:{GetReleaseDate()}";

    public void SerializeJson(StringBuilder sb)
    {
        var jsonObject = new JsonObject(sb);

        sb.Append(JsonObject.OpenBrace);
        jsonObject.AddStringValue("name", Name);
        jsonObject.AddStringValue("version", Version);
        if (Description      != null) jsonObject.AddStringValue("description", Description.Trim());
        if (ReleaseDateTicks != 0) jsonObject.AddStringValue("releaseDate", GetReleaseDate());
        sb.Append(JsonObject.CloseBrace);
    }
}