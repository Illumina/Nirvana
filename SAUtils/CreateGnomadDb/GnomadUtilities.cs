using System;
using System.Collections.Generic;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.CreateGnomadDb
{
    public static class GnomadUtilities
    {
        public static Dictionary<(string refAllele, string altAllele), GnomadItem> GetMergedItems(Dictionary<(string refAllele, string altAllele), GnomadItem> genomeItems, Dictionary<(string refAllele, string altAllele), GnomadItem> exomeItems)
        {
            if (genomeItems == null) return exomeItems;
            if (exomeItems == null) return genomeItems;

            var mergedItems = new Dictionary<(string refAllele, string altAllele), GnomadItem>();
            // take care of the genomeItems and merge if needed
            foreach (var (key, value) in genomeItems)
            {
                mergedItems.Add(key, exomeItems.TryGetValue(key, out var exomeValue) ? MergeItems(value, exomeValue) : value);

                exomeItems.Remove(key);
            }

            foreach (var (key, value) in exomeItems)
            {
                mergedItems.Add(key, value);
            }

            return mergedItems;
        }

        public static GnomadItem GetNormalizedItem(GnomadItem item, ISequenceProvider sequenceProvider)
        {
            var (alignedPos, alignedRef, alignedAlt) =
                VariantUtils.TrimAndLeftAlign(item.Position, item.RefAllele, item.AltAllele, sequenceProvider.Sequence);

            if (item.Position == alignedPos && item.RefAllele == alignedRef && item.AltAllele == alignedAlt)
                return item;

            return new GnomadItem(
                item.Chromosome,
                alignedPos,
                alignedRef,
                alignedAlt,
                item.Depth,
                item.AllAlleleNumber,
                item.AfrAlleleNumber,
                item.AmrAlleleNumber,
                item.EasAlleleNumber,
                item.FinAlleleNumber,
                item.NfeAlleleNumber,
                item.OthAlleleNumber,
                item.AsjAlleleNumber,
                item.SasAlleleNumber,
                item.MaleAlleleNumber,
                item.FemaleAlleleNumber,
                item.AllAlleleCount,
                item.AfrAlleleCount,
                item.AmrAlleleCount,
                item.EasAlleleCount,
                item.FinAlleleCount,
                item.NfeAlleleCount,
                item.OthAlleleCount,
                item.AsjAlleleCount,
                item.SasAlleleCount,
                item.MaleAlleleCount,
                item.FemaleAlleleCount,
                item.AllHomCount,
                item.AfrHomCount,
                item.AmrHomCount,
                item.EasHomCount,
                item.FinHomCount,
                item.NfeHomCount,
                item.OthHomCount,
                item.AsjHomCount,
                item.SasHomCount,
                item.MaleHomCount,
                item.FemaleHomCount,
                //controls
                item.ControlsAllAlleleNumber,
                item.ControlsAllAlleleCount,
                
                item.HasFailedFilters,
                item.IsLowComplexityRegion,
                item.DataType)
            ;
        }

        public static GnomadItem MergeItems(GnomadItem item1, GnomadItem item2)
        {
            if (item1.Chromosome.Index != item2.Chromosome.Index
               || item1.Position != item2.Position
               || item1.RefAllele != item2.RefAllele
               || item1.AltAllele != item2.AltAllele)
                throw new DataMisalignedException($"Trying to merge unequal variants at {item1.Chromosome.UcscName}:{item1.Position} and {item2.Chromosome.UcscName}:{item2.Position}");

            if (item1.DataType == item2.DataType)
                throw new DataMisalignedException($"Trying to merge different data types at {item1.Chromosome.UcscName}:{item1.Position}");

            return new GnomadItem(item1.Chromosome,
                item1.Position,
                item1.RefAllele,
                item1.AltAllele,
                SaParseUtilities.Add(item1.Depth, item2.Depth),
                SaParseUtilities.Add(item1.AllAlleleNumber, item2.AllAlleleNumber),
                SaParseUtilities.Add(item1.AfrAlleleNumber, item2.AfrAlleleNumber),
                SaParseUtilities.Add(item1.AmrAlleleNumber, item2.AmrAlleleNumber),
                SaParseUtilities.Add(item1.EasAlleleNumber, item2.EasAlleleNumber),
                SaParseUtilities.Add(item1.FinAlleleNumber, item2.FinAlleleNumber),
                SaParseUtilities.Add(item1.NfeAlleleNumber, item2.NfeAlleleNumber),
                SaParseUtilities.Add(item1.OthAlleleNumber, item2.OthAlleleNumber),
                SaParseUtilities.Add(item1.AsjAlleleNumber, item2.AsjAlleleNumber),
                SaParseUtilities.Add(item1.SasAlleleNumber, item2.SasAlleleNumber),
                SaParseUtilities.Add(item1.MaleAlleleNumber, item2.MaleAlleleNumber),
                SaParseUtilities.Add(item1.FemaleAlleleNumber, item2.FemaleAlleleNumber),
                SaParseUtilities.Add(item1.AllAlleleCount, item2.AllAlleleCount),
                SaParseUtilities.Add(item1.AfrAlleleCount, item2.AfrAlleleCount),
                SaParseUtilities.Add(item1.AmrAlleleCount, item2.AmrAlleleCount),
                SaParseUtilities.Add(item1.EasAlleleCount, item2.EasAlleleCount),
                SaParseUtilities.Add(item1.FinAlleleCount, item2.FinAlleleCount),
                SaParseUtilities.Add(item1.NfeAlleleCount, item2.NfeAlleleCount),
                SaParseUtilities.Add(item1.OthAlleleCount, item2.OthAlleleCount),
                SaParseUtilities.Add(item1.AsjAlleleCount, item2.AsjAlleleCount),
                SaParseUtilities.Add(item1.SasAlleleCount, item2.SasAlleleCount),
                SaParseUtilities.Add(item1.MaleAlleleCount, item2.MaleAlleleCount),
                SaParseUtilities.Add(item1.FemaleAlleleCount, item2.FemaleAlleleCount),
                SaParseUtilities.Add(item1.AllHomCount, item2.AllHomCount),
                SaParseUtilities.Add(item1.AfrHomCount, item2.AfrHomCount),
                SaParseUtilities.Add(item1.AmrHomCount, item2.AmrHomCount),
                SaParseUtilities.Add(item1.EasHomCount, item2.EasHomCount),
                SaParseUtilities.Add(item1.FinHomCount, item2.FinHomCount),
                SaParseUtilities.Add(item1.NfeHomCount, item2.NfeHomCount),
                SaParseUtilities.Add(item1.OthHomCount, item2.OthHomCount),
                SaParseUtilities.Add(item1.AsjHomCount, item2.AsjHomCount),
                SaParseUtilities.Add(item1.SasHomCount, item2.SasHomCount),
                SaParseUtilities.Add(item1.MaleHomCount, item2.MaleHomCount),
                SaParseUtilities.Add(item1.FemaleHomCount, item2.FemaleHomCount),
                //control
                SaParseUtilities.Add(item1.ControlsAllAlleleNumber, item2.ControlsAllAlleleNumber),
                SaParseUtilities.Add(item1.ControlsAllAlleleCount, item2.ControlsAllAlleleCount),
                
                item1.HasFailedFilters || item2.HasFailedFilters,
                item1.IsLowComplexityRegion || item2.IsLowComplexityRegion,
                item1.DataType
                );
        }
    }
}