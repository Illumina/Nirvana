using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures.VEP;
using VariantAnnotation.DataStructures.Transcript;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class ImportDataStore
    {
        #region members

        public readonly List<VEP.Transcript> Transcripts               = new List<VEP.Transcript>();
        public readonly List<RegulatoryFeature> RegulatoryFeatures = new List<RegulatoryFeature>();

        public ushort CurrentReferenceIndex;
        public static TranscriptDataSource TranscriptSource;

        #endregion

        /// <summary>
        /// clears the temporary data
        /// </summary>
        public void Clear()
        {
            Transcripts.Clear();
            RegulatoryFeatures.Clear();
        }

        /// <summary>
        /// copies the transcripts and regulatory features from another datastore
        /// </summary>
        public void CopyDataFrom(ImportDataStore other)
        {
            Transcripts.AddRange(other.Transcripts);
            RegulatoryFeatures.AddRange(other.RegulatoryFeatures);
        }

        /// <summary>
        /// returns a string representation of the import data store
        /// </summary>
        public override string ToString()
        {
            return $"{Transcripts.Count} transcripts, {RegulatoryFeatures.Count} regulatory";
        }
    }
}
