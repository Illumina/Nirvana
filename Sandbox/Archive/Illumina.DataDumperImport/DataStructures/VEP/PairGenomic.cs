using System.Collections.Generic;
using Illumina.VariantAnnotation.DataStructures;

namespace Illumina.DataDumperImport.DataStructures.VEP
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

            if (mapperPair.From.Id == MapperUnitType.Genomic)
            {
                genomicMappingUnit = mapperPair.From;
                cdnaMappingUnit    = mapperPair.To;
            }
            else
            {
                genomicMappingUnit = mapperPair.To;
                cdnaMappingUnit    = mapperPair.From;
            }

            return new CdnaCoordinateMap(
                new AnnotationInterval(genomicMappingUnit.Start, genomicMappingUnit.End),
                new AnnotationInterval(cdnaMappingUnit.Start, cdnaMappingUnit.End));
        }
    }
}
