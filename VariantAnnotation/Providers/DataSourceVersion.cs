using System;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;

namespace VariantAnnotation.Providers
{
	public sealed class DataSourceVersion : IDataSourceVersion, IEquatable<DataSourceVersion>, ISerializable
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
	        var name = reader.ReadAsciiString();
	        var version = reader.ReadAsciiString();
	        var releaseDateTicks = reader.ReadOptInt64();
	        var description = reader.ReadAsciiString();
	        return new DataSourceVersion(name, version, releaseDateTicks, description);
	    }

        public void Write(IExtendedBinaryWriter writer)
		{
			writer.WriteOptAscii(Name);
			writer.WriteOptAscii(Version);
			writer.WriteOpt(ReleaseDateTicks);
			writer.WriteOptAscii(Description);
		}
		
		/// <summary>
		/// returns the release date
		/// </summary>
		private string GetReleaseDate()
		{
			return new DateTime(ReleaseDateTicks, DateTimeKind.Utc).ToString("yyyy-MM-dd");
		}

		public override string ToString()
		{
			return "dataSource=" + Name + ",version:" + Version + ",release date:" + GetReleaseDate();
		}

		/// <summary>
		/// serializes this object to a textual JSON representation via the
		/// string builder.
		/// </summary>
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

	    public bool Equals(DataSourceVersion other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return string.Equals(Name, other.Name) && string.Equals(Description, other.Description) &&
	               string.Equals(Version, other.Version) && ReleaseDateTicks == other.ReleaseDateTicks;
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            var hashCode = (Name != null ? Name.GetHashCode() : 0);
	            hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
	            hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
	            hashCode = (hashCode * 397) ^ ReleaseDateTicks.GetHashCode();
	            return hashCode;
	        }
	    }
	}
}