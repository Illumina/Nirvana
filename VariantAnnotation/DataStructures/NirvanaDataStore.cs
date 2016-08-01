using System.Collections.Generic;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class NirvanaDataStore
    {
        #region members

        public NirvanaDatabaseHeader CacheHeader = null;

        // genes
        public List<Gene> Genes; 

        // transcripts
        public List<Transcript> Transcripts;

        // transcript object
        public List<CdnaCoordinateMap> CdnaCoordinateMaps;
        public List<Exon> Exons;
        public List<Intron> Introns;
        public List<MicroRna> MicroRnas;
        public List<PolyPhen> PolyPhens;
        public List<Sift> Sifts;

        // regulatory features
        public List<RegulatoryFeature> RegulatoryFeatures;

        #endregion

        /// <summary>
        /// clears the data kept in the data store
        /// </summary>
        public void Clear()
        {
            CdnaCoordinateMaps?.Clear();
            Exons?.Clear();
            Genes?.Clear();
            Introns?.Clear();
            Transcripts?.Clear();
            MicroRnas?.Clear();
            PolyPhens?.Clear();
            Sifts?.Clear();
            RegulatoryFeatures?.Clear();
        }
    }
}
