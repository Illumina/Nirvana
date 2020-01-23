using System;
using System.Collections.Generic;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using SAUtils.Schema;
using VariantAnnotation.SA;

namespace SAUtils.Custom
{
    public static class ParserUtilities
    {

        public static string ParseTitle(string line)
        {
            CheckPrefix(line, "#title", "first");
            string firstCol = line.OptimizedSplit('\t')[0];
            (_, string jsonTag) = firstCol.OptimizedKeyValue();

            if (jsonTag == null)
                throw new UserErrorException("Please provide the title in the format: #title=titleValue.");

            if (CheckJsonTagConflict(jsonTag))
                throw new UserErrorException($"{jsonTag} is a reserved supplementary annotation tag in Nirvana. Please use a different value.");
            return jsonTag;
        }

        public static GenomeAssembly ParseGenomeAssembly(string line, HashSet<GenomeAssembly> allowedGenomeAssemblies)
        {
            CheckPrefix(line, "#assembly", "second");
            string firstCol = line.OptimizedSplit('\t')[0];
            (_, string assemblyString) = firstCol.OptimizedKeyValue();

            if (assemblyString == null)
                throw new UserErrorException("Please provide the genome assembly in the format: #assembly=genomeAssembly.");

            var assembly = GenomeAssemblyHelper.Convert(assemblyString);
            if (!allowedGenomeAssemblies.Contains(assembly))
                throw new UserErrorException("Only GRCh37 and GRCh38 are accepted for genome assembly.");

            return assembly;
        }

        public static (bool MatchByAllele, bool IsArray, SaJsonValueType PrimaryType) ParseMatchVariantsBy(string line)
        {
            CheckPrefix(line, "#matchVariantsBy", "third");
            string firstCol = line.OptimizedSplit('\t')[0];
            (_, string matchBy) = firstCol.OptimizedKeyValue();

            bool matchByAllele;
            bool isArray;
            SaJsonValueType primaryType;
            switch (matchBy)
            {
                case null:
                    throw new UserErrorException("Please provide the genome assembly in the format: #matchVariantsBy=allele.");
                case "allele":
                    matchByAllele = true;
                    isArray = false;
                    primaryType = SaJsonValueType.Object;
                    break;
                case "position":
                    primaryType = SaJsonValueType.ObjectArray;
                    matchByAllele = false;
                    isArray = true;
                    break;
                default:
                    throw new UserErrorException("matchVariantsBy tag has to be either \'allele\' or \'position\'");
            }

            return (matchByAllele, isArray, primaryType);
        }

        public static string[] ParseTags(string line, string prefix, int numRequiredCols, string rowNumber)
        {
            CheckPrefix(line, prefix, rowNumber);

            var tags = line.OptimizedSplit('\t');
            if (tags.Length < numRequiredCols)
                throw new UserErrorException($"At least {numRequiredCols} columns required. Please note that the columns should be separated by tab.");

            return tags;
        }


        public static CustomAnnotationCategories[] ParseCategories(string line, int numRequiredColumns, int numAnnotationColumns, Action<string, string>[] annotationValidators, string rowNumber)
        {
            CheckPrefix(line, "#categories", rowNumber);
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != numRequiredColumns + numAnnotationColumns) throw new UserErrorException("#categories row must have the same number of columns as the header row with column names.");

            var categories = new CustomAnnotationCategories[numAnnotationColumns];
            for (var i = 0; i < numAnnotationColumns; i++)
            {
                switch (splits[i + numRequiredColumns].ToLower())
                {
                    case "allelecount":
                        categories[i] = CustomAnnotationCategories.AlleleCount;
                        break;
                    case "allelenumber":
                        categories[i] = CustomAnnotationCategories.AlleleNumber;
                        break;
                    case "allelefrequency":
                        categories[i] = CustomAnnotationCategories.AlleleFrequency;
                        break;
                    case "homozygouscount":
                        categories[i] = CustomAnnotationCategories.HomozygousCount;
                        break;
                    case "prediction":
                        categories[i] = CustomAnnotationCategories.Prediction;
                        annotationValidators[i] = AllowedValues.ValidatePredictionValue;
                        break;
                    case "filter":
                        categories[i] = CustomAnnotationCategories.Filter;
                        annotationValidators[i] = AllowedValues.ValidateFilterValue;
                        break;
                    case "identifier":
                        categories[i] = CustomAnnotationCategories.Identifier;
                        annotationValidators[i] = AllowedValues.ValidateIdentifierValue;
                        break;
                    case "description":
                        categories[i] = CustomAnnotationCategories.Description;
                        annotationValidators[i] = AllowedValues.ValidateDescriptionValue;
                        break;
                    case ".":
                    case "":
                        categories[i] = CustomAnnotationCategories.Unknown;
                        break;
                    default:
                        throw new UserErrorException($"Invalid category value: {splits[i + numRequiredColumns]}");
                }
            }

            return categories;
        }

        public static string[] ParseDescriptions(string line, int numRequiredColumns, int numAnnotationColumns, string rowNumber)
        {
            CheckPrefix(line,"#descriptions", rowNumber);
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != numRequiredColumns + numAnnotationColumns) throw new UserErrorException("#descriptions row must have the same number of columns as the header row with column names");

            var descriptions = new string[numAnnotationColumns];
            for (var i = 0; i < numAnnotationColumns; i++)
            {
                if (splits[i + numRequiredColumns] == "." || splits[i + numRequiredColumns] == "") descriptions[i] = null;
                else descriptions[i] = splits[i + numRequiredColumns];
            }

            return descriptions;
        }

        public static SaJsonValueType[] ParseTypes(string line, int numRequiredColumns, int numAnnotationColumns, string rowNumber)
        {
            CheckPrefix(line, "#type", rowNumber);
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != numRequiredColumns + numAnnotationColumns) throw new UserErrorException("#types row must have the same number of columns as the header row with column names");

            var valueTypes = new SaJsonValueType[numAnnotationColumns];
            for (var i = 0; i < numAnnotationColumns; i++)
            {
                switch (splits[i + numRequiredColumns].ToLower())
                {
                    case "bool":
                        valueTypes[i] = SaJsonValueType.Bool;
                        break;
                    case "string":
                        valueTypes[i] = SaJsonValueType.String;
                        break;
                    case "number":
                        valueTypes[i] = SaJsonValueType.Number;
                        break;
                    default:
                        throw new UserErrorException("Invalid value for type column. Valid values are bool, string and number.");
                }
            }

            return valueTypes;
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        internal static void CheckPrefix(string line, string prefix, string rowNumber)
        {
            if (line != null && !line.StartsWith(prefix))
                throw new UserErrorException($"The TSV file is required to start with {prefix} in the {rowNumber} row.");
        }

        private static bool CheckJsonTagConflict(string value)
        {
            return value.Equals(SaCommon.DbsnpTag)
                   || value.Equals(SaCommon.GlobalAlleleTag)
                   || value.Equals(SaCommon.AncestralAlleleTag)
                   || value.Equals(SaCommon.ClinGenTag)
                   || value.Equals(SaCommon.ClinvarTag)
                   || value.Equals(SaCommon.CosmicTag)
                   || value.Equals(SaCommon.CosmicCnvTag)
                   || value.Equals(SaCommon.DgvTag)
                   || value.Equals(SaCommon.ExacScoreTag)
                   || value.Equals(SaCommon.GnomadTag)
                   || value.Equals(SaCommon.GnomadExomeTag)
                   || value.Equals(SaCommon.MitoMapTag)
                   || value.Equals(SaCommon.OmimTag)
                   || value.Equals(SaCommon.OneKgenTag)
                   || value.Equals(SaCommon.OnekSvTag)
                   || value.Equals(SaCommon.PhylopTag)
                   || value.Equals(SaCommon.RefMinorTag)
                   || value.Equals(SaCommon.TopMedTag);
        }
    }
}