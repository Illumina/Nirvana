using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Providers
{
    public sealed class DataSourceVersion : IDataSourceVersion, ISerializable
    {
        public string Name { get; }
        public string Description { get; }
        public string Version { get; }
        public long ReleaseDateTicks { get; }

        public DataSourceVersion(string name, string version, long releaseDateTicks, string description = null)
        {
            Name             = name;
            Description      = description;
            Version          = version;
            Description      = description;
            ReleaseDateTicks = releaseDateTicks;
        }

        public static IDataSourceVersion Read(ExtendedBinaryReader reader)
        {
            var name             = reader.ReadAsciiString();
            var version          = reader.ReadAsciiString();
            var releaseDateTicks = reader.ReadOptInt64();
            var description      = reader.ReadAsciiString();
            return new DataSourceVersion(name, version, releaseDateTicks, description);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(Name);
            writer.WriteOptAscii(Version);
            writer.WriteOpt(ReleaseDateTicks);
            writer.WriteOptAscii(Description);
        }

        private string GetReleaseDate() => Date.GetDate(ReleaseDateTicks);

        public override string ToString() => "dataSource=" + Name + ",version:" + Version + ",release date:" + GetReleaseDate();

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("name", Name);
            jsonObject.AddStringValue("version", Version);
            if (Description != null) jsonObject.AddStringValue("description", Description.Trim());
            if (ReleaseDateTicks != 0) jsonObject.AddStringValue("releaseDate", GetReleaseDate());
            sb.Append(JsonObject.CloseBrace);
        }
    }

    internal sealed class DataSourceVersionComparer : EqualityComparer<IDataSourceVersion>
    {
        public override bool Equals(IDataSourceVersion x, IDataSourceVersion y)
        {
            return string.Equals(x.Name, y.Name) &&
                   string.Equals(x.Description, y.Description) &&
                   string.Equals(x.Version, y.Version) &&
                   x.ReleaseDateTicks == y.ReleaseDateTicks;
        }

        public override int GetHashCode(IDataSourceVersion x)
        {
            unchecked
            {
                var hashCode = x.Name.GetHashCode();
                if (x.Description != null) hashCode = (hashCode * 397) ^ x.Description.GetHashCode();
                if (x.Version != null) hashCode = (hashCode * 397) ^ x.Version.GetHashCode();
                hashCode = (hashCode * 397) ^ x.ReleaseDateTicks.GetHashCode();
                return hashCode;
            }
        }
    }
}