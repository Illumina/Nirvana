using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using IO.StreamSourceCollection;
using OptimizedCore;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using VariantAnnotation.NSA;
using VariantAnnotation.SA;

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
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("name", geneName);

            bool hasAnnotation = false;
            foreach (NgaReader ngaReader in _ngaReaders)
            {
                var jsonString = ngaReader.GetAnnotation(geneName);
                jsonObject.AddStringValue(ngaReader.JsonKey, jsonString, false);
                if (!string.IsNullOrEmpty(jsonString)) hasAnnotation = true;
            }

            if (!hasAnnotation) return null;

            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public GeneAnnotationProvider(IStreamSourceCollection[] annotationStreamSourceCollections)
        {
            Name = "Gene annotation provider";
            _ngaReaders = new List<NgaReader>();
            foreach (var collection in annotationStreamSourceCollections)
            {
                foreach (var streamSource in collection.GetStreamSources(SaCommon.NgaFileSuffix))
                {
                    _ngaReaders.Add(new NgaReader(streamSource.GetStream()));
                }
            }
        }
        
    }
}