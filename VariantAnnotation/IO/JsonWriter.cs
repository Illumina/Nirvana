using System.Collections.Generic;
using System.IO;
using CommonUtilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.IO
{
    public sealed class JsonWriter : IJsonWriter
    {
        private readonly StreamWriter _writer;
        private bool _firstEntry;
        private bool _positionFieldClosed;
        public string Header { get; private set; }
        
        public JsonWriter(StreamWriter writer, string annotator, string creationTime, string vepDataVersion,
            List<IDataSourceVersion> dataSourceVersions, string genomeAssembly, string[] sampleNames)
        {
            _writer         = writer;
            _writer.NewLine = "\n";
            _firstEntry     = true;
            _positionFieldClosed = false;

            WriteHeader(annotator, creationTime, genomeAssembly, JsonCommon.SchemaVersion, vepDataVersion,
                dataSourceVersions, sampleNames);
        }

        private void WriteHeader(string annotator, string creationTime, string genomeAssembly, int schemaVersion,
            string vepDataVersion, List<IDataSourceVersion> dataSourceVersions, string[] sampleNames)
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append("{\"header\":{");
            jsonObject.AddStringValue("annotator", annotator);
            jsonObject.AddStringValue("creationTime", creationTime);
            jsonObject.AddStringValue("genomeAssembly", genomeAssembly);
            jsonObject.AddIntValue("schemaVersion", schemaVersion);
            jsonObject.AddStringValue("dataVersion", vepDataVersion);

            jsonObject.AddObjectValues("dataSources", dataSourceVersions);

            if (sampleNames != null) jsonObject.AddStringValues("samples", sampleNames);
            sb.Append("},\"positions\":[\n");

            Header = StringBuilderCache.GetStringAndRelease(sb);
            _writer.Write(Header);
        }

        public void Dispose()
        {
            WriteFooter();
            _writer.Dispose();
        }

        public void WriteJsonEntry(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;
            if (!_firstEntry) _writer.WriteLine(",");
            _firstEntry = false;
            _writer.Write(entry);
        }

        public void WriteAnnotatedGenes(string data)
        {
            _writer.WriteLine();
            _writer.Write("],");
            _positionFieldClosed = true;
            _writer.Write(data);
        }

        /// <summary>
        /// write the footer
        /// </summary>
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