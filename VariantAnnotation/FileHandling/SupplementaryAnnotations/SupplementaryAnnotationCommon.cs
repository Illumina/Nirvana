using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public static class SupplementaryAnnotationCommon
    {
        #region members

        public const uint GuardInt = 4041327495;

        public const string DataHeader    = "NirvanaData";
        public const ushort DataVersion   = 36;
        public const ushort SchemaVersion = 17;

        public const string IndexHeader  = "NirvanaDataIndex";
        public const ushort IndexVersion = 1;

        #endregion

        public static IEnumerable<IDataSourceVersion> GetDataSourceVersions(string saDir)
        {
            var dataSourceVersions = new List<IDataSourceVersion>();
            if (saDir == null) return dataSourceVersions;

            var header = GetSaHeader(saDir);
            if (header == null) return dataSourceVersions;

            dataSourceVersions.AddRange(header.DataSourceVersions);
            return dataSourceVersions;
        }

        private static SupplementaryAnnotationHeader GetSaHeader(string saDir)
        {
            var saFiles = Directory.GetFiles(saDir, "*.nsa");
            if (saFiles == null) throw new UserErrorException($"Unable to find any supplementary annotation files in the following directory: {saDir}");

            long intervalsPosition;
			return saFiles.Length>0 ? SupplementaryAnnotationReader.GetHeader(saFiles.First(), out intervalsPosition) : null;
        }

        public static GenomeAssembly GetGenomeAssembly(string saDir)
        {
            if (saDir == null) return GenomeAssembly.Unknown;

            var header = GetSaHeader(saDir);
            return header?.GenomeAssembly ?? GenomeAssembly.Unknown;
        }

        /// <summary>
        /// checks the supplementary annotation directory to ensure that the directory only contains files for one
        /// cache schema (8), one VEP version (79), and one genome assembly (GRCh37)
        /// </summary>
        public static void CheckDirectoryIntegrity(string saDir, List<DataSourceVersion> mainDataSourceVersions, out SupplementaryAnnotationDirectory saDirectory)
        {
			
			// sanity check: make sure the directory is set
			if (string.IsNullOrEmpty(saDir))
			{
				saDirectory = null;
				return;
			}

            var observedDataVersions = new HashSet<ushort>();
            var refSeqsToPaths       = new Dictionary<string, string>();
			var observedGenomeAssemblies = new HashSet<GenomeAssembly>();

            var earliestCreationTime = long.MaxValue;

            var hasDataSourceVersions            = false;
            var foundDifferentDataSourceVersions = false;
            var dataSourceVersions                = new List<DataSourceVersion>();

            // grab the header data from each cache file
            foreach (var saPath in Directory.GetFiles(saDir, "*.nsa"))
            {
                long intervalsPosition;
                var header = SupplementaryAnnotationReader.GetHeader(saPath, out intervalsPosition);
                if (header == null) continue;

                observedDataVersions.Add(header.DataVersion);
                refSeqsToPaths[header.ReferenceSequenceName] = saPath;
	            observedGenomeAssemblies.Add(header.GenomeAssembly);

                if (header.CreationTimeTicks < earliestCreationTime) earliestCreationTime = header.CreationTimeTicks;

                if (hasDataSourceVersions)
                {
                    if (!dataSourceVersions.SequenceEqual(header.DataSourceVersions))
                    {
                        foundDifferentDataSourceVersions = true;
                    }
                }
                else
                {
                    dataSourceVersions    = header.DataSourceVersions;
                    hasDataSourceVersions = true;
                }
            }

            // sanity check: no references were found
            if (refSeqsToPaths.Count == 0 || observedDataVersions.Count == 0)
            {
                throw new UserErrorException($"Unable to find any supplementary annotation files in the following directory: {saDir}");
            }

            // sanity check: more than one cache data version found
            if (observedDataVersions.Count > 1)
            {
                throw new UserErrorException($"Found more than one cache data version represented in the following directory: {saDir}");
            }

            // sanity check: make sure all of the files have the same data source versions
            if (foundDifferentDataSourceVersions)
            {
                throw new UserErrorException($"Found more than one set of data source versions represented in the following directory: {saDir}");
            }

	        if (observedGenomeAssemblies.Count > 1)
	        {
		        throw new UserErrorException($"Found more than one set of Genome Assemblies represented in the following directory: {saDir}");
	        }
		
            saDirectory = new SupplementaryAnnotationDirectory(observedDataVersions.First(),observedGenomeAssemblies.First());
            mainDataSourceVersions.AddRange(dataSourceVersions);
			
        }
	    /// <summary>
	    /// check if the section guard is in place
	    /// </summary>
	    public static void CheckGuard(BinaryReader reader)
	    {
		    var observedGuard = reader.ReadUInt32();
		    if (observedGuard != GuardInt)
		    {
			    throw new GeneralException($"Expected a guard integer ({GuardInt}), but found another value: ({observedGuard})");
		    }
	    }

        private static SupplementaryAnnotationReader GetReader(string saDir, string ucscReferenceName)
        {
            if (string.IsNullOrEmpty(saDir)) return null;

            var saPath = Path.Combine(saDir, ucscReferenceName + ".nsa");
            return !File.Exists(saPath)
                ? null
                : new SupplementaryAnnotationReader(saPath);
        }

        public static List<ISupplementaryAnnotationReader> GetReaders(IEnumerable<string> saDirs, string ucscReferenceName)
        {
            var readers = new List<ISupplementaryAnnotationReader>();
            if (saDirs == null) return readers;

            readers.AddRange(saDirs.Select(dir => GetReader(dir, ucscReferenceName)).Where(reader => reader != null));
            return readers;
        }
    }


	public sealed class DataSourceVersion : IDataSourceVersion, IJsonSerializer, IEquatable<DataSourceVersion>
    {
		#region members

		public string Name { get; }
		public string Description { get; }
		public string Version { get; }
		public long ReleaseDateTicks { get; }

		// public readonly string Name;
		// public readonly string Description;
		// public readonly string Version;
		// public readonly long ReleaseDateTicks;

		private readonly int _hashCode;

		#endregion

		// constructor
		public DataSourceVersion(string name, string version, long releaseDateTicks, string description = null)
        {
            Name             = name;
            Description      = description;
            Version          = version;
            Description      = description;
			ReleaseDateTicks = releaseDateTicks;

			_hashCode = Name.GetHashCode() ^ Version.GetHashCode() ^ ReleaseDateTicks.GetHashCode();
        }

        internal DataSourceVersion(ExtendedBinaryReader reader) 
	    {
			Name             = reader.ReadAsciiString();
			Version          = reader.ReadAsciiString();
			ReleaseDateTicks = reader.ReadOptInt64();
			Description      = reader.ReadAsciiString();

		    _hashCode = Name.GetHashCode() ^ Version.GetHashCode() ^ ReleaseDateTicks.GetHashCode();
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
        private string GetReleaseDate() => Date.GetDate(ReleaseDateTicks);
		
		public override string ToString()
        {
			//This is used by the vcf writer and description is not needed.
            return "dataSource=" + Name + ",version:" + Version + ",release date:" + GetReleaseDate();
        }

        /// <summary>
        /// deserializes a data source version from the specified reader
        /// </summary>
        
	    public static DataSourceVersion Read(ExtendedBinaryReader reader)
	    {
			return new DataSourceVersion(reader);
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
            if (Description      != null) jsonObject.AddStringValue("description", Description.Trim());
            if (ReleaseDateTicks != 0)    jsonObject.AddStringValue("releaseDate", GetReleaseDate());
            sb.Append(JsonObject.CloseBrace);
        }

        /// <summary>
        /// writes the data source version to the specified writer
        /// </summary>
        internal void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(Name);
            writer.WriteOptAscii(Version);
            writer.WriteOpt(ReleaseDateTicks);
			writer.WriteOptAscii(Description);
        }
    }

    public sealed class SupplementaryAnnotationDirectory
    {
        #region members

        public ushort DataVersion { get; private set; }

        public GenomeAssembly GenomeAssembly { get; private set; }	

        #endregion

        // constructor
        public SupplementaryAnnotationDirectory(ushort dataVersion,GenomeAssembly genomeAssembly)
        {
            DataVersion    = dataVersion;
            GenomeAssembly = genomeAssembly;
        }
    }

    public sealed class SupplementaryAnnotationHeader
    {
        public string ReferenceSequenceName                { get; }
        public long CreationTimeTicks                      { get; }
        public ushort DataVersion                          { get; }
        public List<DataSourceVersion> DataSourceVersions  { get; }
		public GenomeAssembly GenomeAssembly { get; }

        // constructor
        public SupplementaryAnnotationHeader(string referenceSequenceName, long creationTimeTicks, ushort dataVersion, 
            List<DataSourceVersion> dataSourceVersions, GenomeAssembly genomeAssembly)
        {
            ReferenceSequenceName = referenceSequenceName;
            CreationTimeTicks     = creationTimeTicks;
            DataVersion           = dataVersion;
            DataSourceVersions    = dataSourceVersions;
            GenomeAssembly        = genomeAssembly;
        }
    }
}
