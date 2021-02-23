using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.Custom
{
    public static class CaUtilities
    {
        public static NsaWriter GetNsaWriter(Stream nsaStream, Stream indexStream, VariantAnnotationsParser parser, string dataVersion, ISequenceProvider referenceProvider, out DataSourceVersion version, bool skipRefBaseValidation)
        {
            dataVersion = string.IsNullOrEmpty(parser.Version) ? dataVersion : parser.Version;
            version = new DataSourceVersion(parser.JsonTag, dataVersion, DateTime.Now.Ticks,
                parser.DataSourceDescription);
            return new NsaWriter(
                nsaStream,
                indexStream,
                version,
                referenceProvider,
                parser.JsonTag,
                parser.MatchByAllele, // match by allele
                parser.IsArray, // is array
                SaCommon.SchemaVersion,
                false, // is positional
                skipRefBaseValidation, // skip incorrect ref base
                true // throw error on conflicting entries
            );
        }

        public static NsiWriter GetNsiWriter(Stream nsiStream, DataSourceVersion version, GenomeAssembly assembly, string jsonTag, ReportFor reportFor) => new NsiWriter(nsiStream, version, assembly, jsonTag, reportFor, SaCommon.SchemaVersion);

        public static NgaWriter GetNgaWriter(Stream ngaStream, GeneAnnotationsParser parser, string dataVersion)
        {
            dataVersion = string.IsNullOrEmpty(parser.Version) ? dataVersion : parser.Version;
            var version = new DataSourceVersion(parser.JsonTag, dataVersion, DateTime.Now.Ticks, parser.DataSourceDescription);
            return new NgaWriter(ngaStream, version, parser.JsonTag, SaCommon.SchemaVersion, false);
        }

        public static (string JsonTag, int NsaItemsCount, SaJsonSchema IntervalJsonSchema, List<CustomInterval> Intervals) WriteSmallVariants(VariantAnnotationsParser parser, NsaWriter nsaWriter, StreamWriter schemaWriter)
        {
            int nsaItemsCount = nsaWriter.Write(parser.GetItems());
            schemaWriter.Write(parser.JsonSchema);
            var intervals = parser.GetCustomIntervals();

            if (nsaItemsCount == 0 & intervals == null) throw new UserErrorException(GeneAnnotationsParser.NoValidEntriesErrorMessage);
            return (parser.JsonTag, nsaItemsCount, parser.IntervalJsonSchema, intervals);
        }

        public static string GetInputFileName(string inputFilePath)
        {
            int fileNameIndex = inputFilePath.LastIndexOf(Path.DirectorySeparatorChar);
            return fileNameIndex < 0 ? inputFilePath : inputFilePath.Substring(fileNameIndex + 1);
        }
    }
}