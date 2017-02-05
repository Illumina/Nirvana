using System.Collections.Generic;
using VariantAnnotation.DataStructures;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class PairGenomic
    {
        public List<MapperPair> Genomic = null;

        /// <summary>
        /// converts a mapper pair to a cDNA coordinate map
        /// </summary>
        public static CdnaCoordinateMap ConvertMapperPair(MapperPair mapperPair)
        {
            MapperUnit genomicMappingUnit;
            MapperUnit cdnaMappingUnit;

            if (mapperPair.From.ID == MapperUnitType.Genomic)
            {
                genomicMappingUnit = mapperPair.From;
                cdnaMappingUnit    = mapperPair.To;
            }
            else
            {
                genomicMappingUnit = mapperPair.To;
                cdnaMappingUnit    = mapperPair.From;
            }

            return new CdnaCoordinateMap(genomicMappingUnit.Start, genomicMappingUnit.End, cdnaMappingUnit.Start,
                cdnaMappingUnit.End);
        }
    }
}
