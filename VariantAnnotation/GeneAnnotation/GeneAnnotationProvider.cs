﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using VariantAnnotation.NSA;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotationProvider : IGeneAnnotationProvider
    {
	    public string Name { get; }
        public GenomeAssembly Assembly => GenomeAssembly.Unknown;
        public IEnumerable<IDataSourceVersion> DataSourceVersions => _ngaReaders.Select(x => x.Version);

        private readonly List<NgaReader> _ngaReaders;

        public string Annotate(string geneName)
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("name", geneName);

            var hasAnnotation = false;
            foreach (var ngaReader in _ngaReaders)
            {
                string jsonString = ngaReader.GetAnnotation(geneName);
                jsonObject.AddStringValue(ngaReader.JsonKey, jsonString, false);
                if (!string.IsNullOrEmpty(jsonString)) hasAnnotation = true;
            }

            if (!hasAnnotation)
            {
                StringBuilderPool.GetStringAndReturn(sb);
                return null;
            }

            sb.Append(JsonObject.CloseBrace);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public GeneAnnotationProvider(IEnumerable<Stream> dbStreams)
        {
            Name        = "Gene annotation provider";
            _ngaReaders = new List<NgaReader>();

            foreach (var dbStream in dbStreams) _ngaReaders.Add(NgaReader.Read(dbStream));
        }

        public void Dispose() {}
    }
}