using System;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;

namespace VariantAnnotation.Providers
{
	public sealed class DataSourceVersion : IDataSourceVersion, IEquatable<DataSourceVersion>, ISerializable
	{
		#region members

		public string Name { get; }
		public string Description { get; }
		public string Version { get; }
		public long ReleaseDateTicks { get; }

		private readonly int _hashCode;

		#endregion

		/// <summary>
		/// constructor
		/// </summary>
		public DataSourceVersion(string name, string version, long releaseDateTicks, string description = null)
		{
			Name             = name;
			Description      = description;
			Version          = version;
			Description      = description;
			ReleaseDateTicks = releaseDateTicks;

			_hashCode = Name.GetHashCode() ^ Version.GetHashCode() ^ ReleaseDateTicks.GetHashCode();
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
		
		#region Equality Overrides

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override bool Equals(object obj)
		{
			// If parameter cannot be cast to DataSourceVersion return false:
			var other = obj as DataSourceVersion;
			if ((object)other == null) return false;

			// Return true if the fields match:
			return this == other;
		}

		bool IEquatable<DataSourceVersion>.Equals(DataSourceVersion other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		private bool Equals(DataSourceVersion other)
		{
			return this == other;
		}

		public static bool operator ==(DataSourceVersion a, DataSourceVersion b)
		{
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(a, b)) return true;

			// If one is null, but not both, return false.
			if ((object)a == null || (object)b == null) return false;

			return a.Name == b.Name &&
			       a.Version == b.Version &&
			       a.ReleaseDateTicks == b.ReleaseDateTicks;
		}

		public static bool operator !=(DataSourceVersion a, DataSourceVersion b)
		{
			return !(a == b);
		}

		#endregion

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

		
	}
}