using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling
{
    public static class NirvanaDatabaseCommon
    {
        #region members

        public const uint GuardInt = 4041327495;
        public const string Header = "NirvanaDB";

        // increment the schema version when the file structures are updated
        // N.B. we only need to regenerate unit tests when the schema version is incremented
        // e.g. adding a new feature like regulatory features
        public const ushort SchemaVersion = 17;

        // increment the data version when the contents are updated
        // e.g. a bug is fixed in SIFT parsing or if transcripts are filtered differently
        public const ushort DataVersion = 22;

		#endregion

		/// <summary>
		/// checks the Nirvana cache directory to ensure that the directory only contains files for one
		/// cache schema (8), one VEP version (79), and one genome assembly (GRCh37)
		/// </summary>
		public static void CheckDirectoryIntegrity(string cacheDir, List<DataSourceVersion> dataSourceVersions, out CacheDirectory cacheDirectory)
		{
            var refSeqsToPaths = new Dictionary<string, string>();

            if (cacheDir == null)
		    {
		        cacheDirectory = new CacheDirectory(refSeqsToPaths, DataVersion, 84, GenomeAssembly.GRCh37);
		        return;
		    }

            var observedGenomeAssemblies      = new HashSet<GenomeAssembly>();
            var observedTranscriptDataSources = new HashSet<TranscriptDataSource>();
            var observedSchemaVersions        = new HashSet<ushort>();
            var observedDataVersions          = new HashSet<ushort>();
            var observedVepVersions           = new HashSet<ushort>();            

            long earliestCreationTime = long.MaxValue;

            long vepReleaseTicks = 0;

            // grab the header data from each cache file
            foreach (var ndbPath in Directory.GetFiles(cacheDir, "*.ndb"))
            {
                var header = NirvanaDatabaseReader.GetHeader(ndbPath);
                if (header == null) continue;

                observedGenomeAssemblies.Add(header.GenomeAssembly);
                observedTranscriptDataSources.Add(header.TranscriptDataSource);
                observedSchemaVersions.Add(header.SchemaVersion);
                observedDataVersions.Add(header.DataVersion);
                observedVepVersions.Add(header.VepVersion);
                refSeqsToPaths[header.ReferenceSequenceName] = ndbPath;

                if (vepReleaseTicks == 0) vepReleaseTicks = header.VepReleaseTicks;

                if (header.CreationTimeTicks < earliestCreationTime) earliestCreationTime = header.CreationTimeTicks;
            }

            // sanity check: no references were found
            if ((refSeqsToPaths.Count == 0) || (observedGenomeAssemblies.Count == 0) || (observedSchemaVersions.Count == 0))
            {
                throw new UserErrorException($"Unable to find any cache files in the following directory: {cacheDir}");
            }

            // sanity check: more than one cache schema version found
            if (observedSchemaVersions.Count > 1)
            {
                throw new UserErrorException($"Found more than one cache schema version represented in the following directory: {cacheDir}");
            }

            // sanity check: more than one cache data version found
            if (observedDataVersions.Count > 1)
            {
                throw new UserErrorException($"Found more than one cache data version represented in the following directory: {cacheDir}");
            }

            // sanity check: more than one VEP version found
            if (observedVepVersions.Count > 1)
            {
                throw new UserErrorException($"Found more than one VEP version represented in the following directory: {cacheDir}");
            }

            // sanity check: more than one genome assembly found
            if (observedGenomeAssemblies.Count > 1)
            {
                throw new UserErrorException($"Found more than one genome assembly represented in the following directory: {cacheDir}");
            }


            // sanity check: more than one set of data source versions found
            if (observedTranscriptDataSources.Count > 1)
            {
                throw new UserErrorException($"Found more than one transcript data source represented in the following directory: {cacheDir}");
            }

            cacheDirectory = new CacheDirectory(refSeqsToPaths, observedDataVersions.First(), observedVepVersions.First(),
                observedGenomeAssemblies.First());

            var dataSourceVersion = new DataSourceVersion("VEP", observedVepVersions.First().ToString(), vepReleaseTicks, observedTranscriptDataSources.First().ToString());
            dataSourceVersions.Add(dataSourceVersion);
        }
    }

    public class CacheDirectory
    {
        #region members

        public readonly Dictionary<string, string> RefSeqsToPaths;
        private readonly ushort _dataVersion;
        private readonly ushort _vepVersion;
        public readonly GenomeAssembly GenomeAssembly;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public CacheDirectory(Dictionary<string, string> refSeqsToPaths, ushort dataVersion, ushort vepVersion,
            GenomeAssembly genomeAssembly)
        {
            RefSeqsToPaths = refSeqsToPaths;
            _dataVersion   = dataVersion;
            _vepVersion    = vepVersion;
            GenomeAssembly = genomeAssembly;
        }

        /// <summary>
        /// returns the VEP version and the data version for this directory
        /// </summary>
        public string GetVepVersion(ushort saDataVersion)
        {
            return _vepVersion.ToString() + '.' + _dataVersion + '.' + saDataVersion;
        }
    }

    public class NirvanaDatabaseHeader
    {
        public string ReferenceSequenceName              { get; }
        public long CreationTimeTicks                    { get; }
        public long VepReleaseTicks                      { get; }
        public ushort VepVersion                         { get; }
        public ushort SchemaVersion                      { get; }
        public ushort DataVersion                        { get; }
        public GenomeAssembly GenomeAssembly             { get; }
        public TranscriptDataSource TranscriptDataSource { get; }

        // constructor
        public NirvanaDatabaseHeader(string referenceSequenceName, long creationTimeTicks, long vepReleaseTicks, ushort vepVersion,
            ushort schemaVersion, ushort dataVersion, GenomeAssembly genomeAssembly, TranscriptDataSource transcriptDataSource)
        {
            ReferenceSequenceName = referenceSequenceName;
            CreationTimeTicks     = creationTimeTicks;
            VepReleaseTicks       = vepReleaseTicks;
            VepVersion            = vepVersion;
            SchemaVersion         = schemaVersion;
            DataVersion           = dataVersion;
            GenomeAssembly        = genomeAssembly;
            TranscriptDataSource  = transcriptDataSource;
        }

        public override string ToString()
        {
            return
                $"{ReferenceSequenceName}, VEP: {VepVersion}, Schema: {SchemaVersion}, Data: {DataVersion}, GA: {GenomeAssembly}, DS: {TranscriptDataSource}";
        }
    }
}

