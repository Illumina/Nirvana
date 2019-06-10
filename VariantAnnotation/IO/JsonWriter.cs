using System.Collections.Generic;
using System.IO;
using Compression.FileHandling;
using Jasix;
using Jasix.DataStructures;
using OptimizedCore;
using VariantAnnotation.Interface;
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
        private readonly bool _leaveOpen;

        private readonly BgzipTextWriter _bgzipTextWriter;
        private readonly OnTheFlyIndexCreator _jasixIndexCreator;

        private JsonWriter(Stream jsonStream, Stream indexStream, string annotator, string creationTime, string vepDataVersion,
            List<IDataSourceVersion> dataSourceVersions, string genomeAssembly, string[] sampleNames, bool leaveOpen) : this(GetProperWriter(jsonStream), indexStream, annotator, creationTime, vepDataVersion, dataSourceVersions, genomeAssembly, sampleNames, leaveOpen)
        {
        }

        public JsonWriter(Stream jsonStream, Stream indexStream, IAnnotationResources annotationResources, string creationTime, string[] sampleNames, bool leaveOpen) : this(jsonStream, indexStream, annotationResources.AnnotatorVersionTag, creationTime, annotationResources.VepDataVersion, annotationResources.DataSourceVersions, annotationResources.SequenceProvider.Assembly.ToString(), sampleNames, leaveOpen)
        {
        }

        private static StreamWriter GetProperWriter(Stream jsonStream) => jsonStream is BlockGZipStream
            ? new BgzipTextWriter((BlockGZipStream)jsonStream)
            : new StreamWriter(jsonStream);

        public JsonWriter(StreamWriter writer, Stream indexStream, string annotator, string creationTime, string vepDataVersion,
            List<IDataSourceVersion> dataSourceVersions, string genomeAssembly, string[] sampleNames, bool leaveOpen)
        {
            _writer              = writer;
            _writer.NewLine      = "\n";
            _firstEntry          = true;
            _positionFieldClosed = false;
            _leaveOpen           = leaveOpen;

            _bgzipTextWriter = writer as BgzipTextWriter;

            _jasixIndexCreator = _bgzipTextWriter != null
                ? new OnTheFlyIndexCreator(indexStream)
                : null;

            WriteHeader(annotator, creationTime, genomeAssembly, JsonCommon.SchemaVersion, vepDataVersion,
                dataSourceVersions, sampleNames);
        }


        private void WriteHeader(string annotator, string creationTime, string genomeAssembly, int schemaVersion,
            string vepDataVersion, IEnumerable<IDataSourceVersion> dataSourceVersions, string[] sampleNames)
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
            _writer.Flush();
            if (_leaveOpen) return;
            _writer.Dispose();
            _jasixIndexCreator?.Dispose();
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

        public void WriteAnnotatedGenes(IEnumerable<string> annotatedGenes)
        {
            _positionFieldClosed = true;
            _writer.Flush();
            _jasixIndexCreator?.EndSection(JasixCommons.PositionsSectionTag, _bgzipTextWriter.Position - 1);

            _writer.Write("\n]");

            if (annotatedGenes == null) return;
            _writer.Write($",\"{JasixCommons.GenesSectionTag}\":[\n");
            _writer.Flush();

            _jasixIndexCreator?.BeginSection(JasixCommons.GenesSectionTag, _bgzipTextWriter.Position);

            var sb             = StringBuilderCache.Acquire();
            var firstGeneEntry = true;

            foreach (string jsonString in annotatedGenes)
            {
                if (!firstGeneEntry) sb.Append(",\n");
                sb.Append(jsonString);
                firstGeneEntry = false;
            }

            _writer.Write(sb.ToString());
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