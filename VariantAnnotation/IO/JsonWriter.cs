using System.Collections.Generic;
using System.IO;
using System.Text;
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

        private static StreamWriter GetProperWriter(Stream jsonStream) => jsonStream is BlockGZipStream stream
            ? new BgzipTextWriter(stream)
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
            
            BeginSection(JasixCommons.HeaderSectionTag);

            var sb         = StringBuilderPool.Get();
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

            _writer.Write(StringBuilderPool.GetStringAndReturn(sb));
            if(_bgzipTextWriter != null) EndSection(JasixCommons.HeaderSectionTag);
        }

        public void Dispose()
        {
            WriteFooter();
            _writer?.Flush();
            _jasixIndexCreator?.Flush();
            if (_leaveOpen) return;
            _writer?.Dispose();
            _jasixIndexCreator?.Dispose();
        }
        
        // due to the flush, the end of a section will point to the next to last block for a section.
        // e.g. if positions start at block 2 and end at block 10, blocks 2..9 contains positions. 
        private void BeginSection(string section)
        {
            if (_bgzipTextWriter == null) return;
            _bgzipTextWriter.Flush();
            _jasixIndexCreator.BeginSection(section, _bgzipTextWriter.Position);
        }

        private void EndSection(string section)
        {
            if (_bgzipTextWriter == null) return;
            _bgzipTextWriter.Flush();
            _jasixIndexCreator.EndSection(section, _bgzipTextWriter.Position);
        }


        public void WritePosition(IPosition position, string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;
            _jasixIndexCreator?.Add(position, _bgzipTextWriter.Position);
            if (_firstEntry)
            {
                BeginSection(JasixCommons.PositionsSectionTag);
            }
            else _writer.WriteLine(",");

            _firstEntry = false;
            _writer.Write(entry);
        }
        
        public void WritePosition(IPosition position, StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return;
            _jasixIndexCreator?.Add(position, _bgzipTextWriter.Position);
            if (_firstEntry)
            {
                BeginSection(JasixCommons.PositionsSectionTag);
            }
            else _writer.WriteLine(",");

            _firstEntry = false;
            _writer.Write(sb);
        }

        public void WriteGenes(IEnumerable<string> annotatedGenes)
        {
            _positionFieldClosed = true;
            EndSection(JasixCommons.PositionsSectionTag);
            
            _writer.Write("\n]");

            if (annotatedGenes == null) return;
            _writer.Write($",\"{JasixCommons.GenesSectionTag}\":[\n");
            BeginSection(JasixCommons.GenesSectionTag);

            var sb = StringBuilderPool.Get();
            var firstGeneEntry = true;

            foreach (string jsonString in annotatedGenes)
            {
                if (!firstGeneEntry) sb.Append(",\n");
                sb.Append(jsonString);
                firstGeneEntry = false;
            }

            var json = StringBuilderPool.GetStringAndReturn(sb);
            _writer.Write(json);

            EndSection(JasixCommons.GenesSectionTag);
            
            _writer.WriteLine();
            _writer.Write("]");
        }

        private void WriteFooter()
        {
            if (!_positionFieldClosed)
            {
                EndSection(JasixCommons.PositionsSectionTag);

                _writer.WriteLine();
                _writer.Write("]");
            }
            _writer.WriteLine("}");
        }
    }
}