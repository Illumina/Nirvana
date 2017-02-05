using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class SupplementaryAnnotationPosition : ISupplementaryAnnotationPosition
    {
        #region members

        public int ReferencePosition { get; }

        public string GlobalMinorAllele { get; set; }
        public string GlobalMinorAlleleFrequency { get; set; }
        public string GlobalMajorAllele { get; set; }
        public string GlobalMajorAlleleFrequency { get; set; }

        public bool IsRefMinorAllele { get; set; }

        public Dictionary<string, AlleleSpecificAnnotation> AlleleSpecificAnnotations { get; }
        public List<CosmicItem> CosmicItems { get; }
        public List<ClinVarItem> ClinVarItems { get; }

        public List<ICustomItem> CustomItems { get; set; }

        #endregion

        // positional 
        public bool ContainsCosmicId(string id)
        {
            return CosmicItems.Any(x => x.ID.Equals(id));
        }

        // constructor
        public SupplementaryAnnotationPosition(int referencePosition = 0)
        {
            ReferencePosition = referencePosition;
            AlleleSpecificAnnotations = new Dictionary<string, AlleleSpecificAnnotation>();
            CosmicItems = new List<CosmicItem>();
            ClinVarItems = new List<ClinVarItem>();
            CustomItems = new List<ICustomItem>();
        }

        public void SetIsAlleleSpecific(string saAltAllele)
        {
            foreach (var cosmicItem in CosmicItems)
                cosmicItem.IsAlleleSpecific = cosmicItem.SaAltAllele == saAltAllele ? "true" : null;

            foreach (var clinVarEntry in ClinVarItems)
                clinVarEntry.IsAlleleSpecific = clinVarEntry.SaAltAllele == saAltAllele ? "true" : null;

            foreach (var customItem in CustomItems)
                customItem.IsAlleleSpecific = customItem.SaAltAllele == saAltAllele ? "true" : null;
        }

        public void AddSaPositionToVariant(IAnnotatedAlternateAllele jsonVariant) 
		{
            jsonVariant.GlobalMinorAllele = GlobalMinorAllele;
            jsonVariant.GlobalMinorAlleleFrequency = GlobalMinorAlleleFrequency;

            // adding cosmic
            foreach (var cosmicItem in CosmicItems) jsonVariant.CosmicEntries.Add(cosmicItem);

            // adding ClinVar
            foreach (var clinVarItem in ClinVarItems) jsonVariant.ClinVarEntries.Add(clinVarItem);

            // adding custom items
            foreach (var customItem in CustomItems)
            {
                // we need to check if a custom annotation is allele specific. If so, we match the alt allele.
                if (!customItem.IsPositional
                    && customItem.SaAltAllele != jsonVariant.SaAltAllele)
                    continue;
                jsonVariant.CustomItems.Add(new CustomItem(jsonVariant.ReferenceName,
                    jsonVariant.ReferenceBegin ?? 0,
                    jsonVariant.RefAllele,
                    jsonVariant.AltAllele,
                    customItem.AnnotationType,
                    customItem.Id,
                    customItem.IsPositional,
                    customItem.StringFields,
                    customItem.BooleanFields,
                    customItem.IsAlleleSpecific));
            }

            // adding allele-specific annotations
            AlleleSpecificAnnotation asa;
            if (!AlleleSpecificAnnotations.TryGetValue(jsonVariant.SaAltAllele, out asa)) return;

            foreach (DataSourceCommon.DataSource dataSource in Enum.GetValues(typeof(DataSourceCommon.DataSource)))
            {
                if (asa.HasDataSource(dataSource))
                    asa.Annotations[DataSourceCommon.GetIndex(dataSource)].AddAnnotationToVariant(jsonVariant);
            }
        }

        public static void Read(ExtendedBinaryReader reader, SupplementaryAnnotationPosition sa)
        {
            sa.GlobalMinorAllele = reader.ReadAsciiString();
            sa.GlobalMinorAlleleFrequency = reader.ReadAsciiString();
            sa.GlobalMajorAllele = reader.ReadAsciiString();
            sa.GlobalMajorAlleleFrequency = reader.ReadAsciiString();

            // read the allele-specific records
            var numAlleles = reader.ReadOptInt32();

            for (var alleleIndex = 0; alleleIndex < numAlleles; alleleIndex++)
            {
                var allele = reader.ReadAsciiString();
                var asa = AlleleSpecificAnnotation.Read(reader);
                sa.AlleleSpecificAnnotations[allele] = asa;
            }

            // read cosmic records
            var numCosmic = reader.ReadOptInt32();
            for (var i = 0; i < numCosmic; i++)
            {
                var cosmicItem = new CosmicItem(reader);
                sa.CosmicItems.Add(cosmicItem);
            }

            // read clinVar items
            var numClinVar = reader.ReadOptInt32();
            for (var i = 0; i < numClinVar; i++)
            {
                var clinVarItem = new ClinVarItem(reader);
                sa.ClinVarItems.Add(clinVarItem);
            }

            // read custom annotation items
            var numCustom = reader.ReadOptInt32();
            for (var i = 0; i < numCustom; i++)
            {
                var customItem = new CustomItem(reader);
                sa.CustomItems.Add(customItem);
            }
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            byte[] annotationBytes;

            // add everything to a memory stream so we can capture the length
            using (var ms = new MemoryStream())
            {
                using (var extendedWriter = new ExtendedBinaryWriter(ms))
                {
                    // write the position-specific records
                    extendedWriter.WriteOptAscii(GlobalMinorAllele);
                    extendedWriter.WriteOptAscii(GlobalMinorAlleleFrequency);
                    extendedWriter.WriteOptAscii(GlobalMajorAllele);
                    extendedWriter.WriteOptAscii(GlobalMajorAlleleFrequency);

                    // write the allele-specific records
                    extendedWriter.WriteOpt(AlleleSpecificAnnotations.Count);

                    foreach (var alleleKvp in AlleleSpecificAnnotations)
                    {
                        // write the allele
                        extendedWriter.WriteOptAscii(alleleKvp.Key);
                        alleleKvp.Value.Write(extendedWriter);
                    }

                    // write the cosmic items
                    extendedWriter.WriteOpt(CosmicItems.Count);
                    foreach (var cosmicItem in CosmicItems)
                    {
                        cosmicItem.Write(extendedWriter);
                    }

                    // writing ClinVar items
                    extendedWriter.WriteOpt(ClinVarItems.Count);
                    foreach (var clinVarItem in ClinVarItems)
                    {
                        clinVarItem.Write(extendedWriter);
                    }

                    // writing custom Annotations
                    extendedWriter.WriteOpt(CustomItems.Count);
                    foreach (var customItem in CustomItems)
                    {
                        customItem.Write(extendedWriter);
                    }
                }

                annotationBytes = ms.ToArray();
            }

            writer.Write(annotationBytes);
        }
    }
}
