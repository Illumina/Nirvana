using System.Collections.Generic;
using System.IO;
using Compression.FileHandling;
using IO;
using Jasix;
using Jasix.DataStructures;
using OptimizedCore;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.IO
{
    public sealed class JsonWriter : IJsonWriter
    {
        private readonly StreamWriter _writer;
        private bool _firstEntry;
        private bool _positionFieldClosed;

        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly OnTheFlyIndexCreator _jasixIndexCreator;

        public JsonWriter(StreamWriter writer, string jasixFileName, string annotator, string creationTime, string vepDataVersion,
            List<IDataSourceVersion> dataSourceVersions, string genomeAssembly, string[] sampleNames)
        {
            _writer              = writer;
            _writer.NewLine      = "\n";
            _firstEntry          = true;
            _positionFieldClosed = false;

            _bgzipTextWriter = writer as BgzipTextWriter;

            _jasixIndexCreator = _bgzipTextWriter != null
                ? new OnTheFlyIndexCreator(FileUtilities.GetCreateStream(jasixFileName))
                : null;

            WriteHeader(annotator, creationTime, genomeAssembly, JsonCommon.SchemaVersion, vepDataVersion,
                dataSourceVersions, sampleNames);
        }

        private void WriteHeader(string annotator, string creationTime, string genomeAssembly, int schemaVersion,
            string vepDataVersion, List<IDataSourceVersion> dataSourceVersions, string[] sampleNames)
        {
            _jasixIndexCreator?.BeginSection(JasixCommons.HeaderSectionTag, _bgzipTextWriter.Position);

            var sb         = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append($"{{\"{JasixCommons.HeaderSectionTag}\":{{");
            jsonObject.AddStringValue("annotator", annotator);
            jsonObject.AddStringValue("creationTime", creationTime);
            jsonObject.AddStringValue("genomeAssembly", genomeAssembly);
            jsonObject.AddIntValue("schemaVersion", schemaVersion);
            jsonObject.AddStringValue("dataVersion", vepDataVersion);

            jsonObject.AddObjectValues("dataSources", dataSourceVersions);

            if (sampleNames != null) jsonObject.AddStringValues("samples", sampleNames);
            sb.Append($"}},\"{JasixCommons.PositionsSectionTag}\":[\n");

            _writer.Write(StringBuilderCache.GetStringAndRelease(sb));
            _writer.Flush();
            _jasixIndexCreator?.EndSection(JasixCommons.HeaderSectionTag, _bgzipTextWriter.Position - 1);
        }

        public void Dispose()
        {
            WriteFooter();
            _jasixIndexCreator?.Dispose();
            _writer.Dispose();
        }

        public void WriteJsonEntry(IPosition position, string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;
            _jasixIndexCreator?.Add(position, _bgzipTextWriter.Position);
            if (!_firstEntry) _writer.WriteLine(",");
            else _jasixIndexCreator?.BeginSection(JasixCommons.PositionsSectionTag, _bgzipTextWriter.Position);

            _firstEntry = false;
            _writer.Write(entry);
        }

        public void WriteAnnotatedGenes(IEnumerable<IAnnotatedGene> annotatedGenes)
        {
            _positionFieldClosed = true;
            _writer.Flush();
            _jasixIndexCreator?.EndSection(JasixCommons.PositionsSectionTag, _bgzipTextWriter.Position - 1);

            _writer.WriteLine();
            _writer.Write($"],\"{JasixCommons.GenesSectionTag}\":[\n");
            _writer.Flush();

            _jasixIndexCreator?.BeginSection(JasixCommons.GenesSectionTag, _bgzipTextWriter.Position);

            var sb             = StringBuilderCache.Acquire();
            var firstGeneEntry = true;

            foreach (IAnnotatedGene annotatedGene in annotatedGenes)
            {
                sb.Clear();
                annotatedGene.SerializeJson(sb);
                if (!firstGeneEntry) _writer.WriteLine(",");
                firstGeneEntry = false;
                _writer.Write(sb.ToString());
            }

            _writer.Flush();
            _jasixIndexCreator?.EndSection(JasixCommons.GenesSectionTag, _bgzipTextWriter.Position - 1);

            StringBuilderCache.GetStringAndRelease(sb);
            _writer.WriteLine();
            _writer.Write("]");
        }

        private void WriteFooter()
        {
            if (!_positionFieldClosed)
            {
                _writer.WriteLine();
                _writer.Write("]");
            }
            _writer.WriteLine("}");
        }
    }
}