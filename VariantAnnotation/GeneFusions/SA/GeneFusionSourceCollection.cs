using System;
using System.Collections.Generic;
using System.Text;
using IO;
using OptimizedCore;
using VariantAnnotation.IO;

namespace VariantAnnotation.GeneFusions.SA
{
    public sealed class GeneFusionSourceCollection : IEquatable<GeneFusionSourceCollection>
    {
        private readonly GeneFusionSource[] _relationships;
        private readonly GeneFusionSource[] _germlineSources;
        private readonly GeneFusionSource[] _somaticSources;

        public GeneFusionSourceCollection(GeneFusionSource[] relationships, GeneFusionSource[] germlineSources, GeneFusionSource[] somaticSources)
        {
            _relationships   = relationships;
            _germlineSources = germlineSources;
            _somaticSources  = somaticSources;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            WriteSourceGroup(writer, _relationships);
            WriteSourceGroup(writer, _germlineSources);
            WriteSourceGroup(writer, _somaticSources);
        }

        private static void WriteSourceGroup(ExtendedBinaryWriter writer, GeneFusionSource[] sources)
        {
            if (sources == null)
            {
                writer.Write((byte)0);
                return;
            }

            writer.WriteOpt(sources.Length);
            foreach (GeneFusionSource source in sources) writer.Write((byte) source);
        }

        public static GeneFusionSourceCollection Read(ref ReadOnlySpan<byte> byteSpan)
        {
            GeneFusionSource[] relationships   = ReadSources(ref byteSpan);
            GeneFusionSource[] germlineSources = ReadSources(ref byteSpan);
            GeneFusionSource[] somaticSources  = ReadSources(ref byteSpan);
            return new GeneFusionSourceCollection(relationships, germlineSources, somaticSources);
        }

        private static GeneFusionSource[] ReadSources(ref ReadOnlySpan<byte> byteSpan)
        {
            int numSources = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
            if (numSources == 0) return null;

            var sources = new GeneFusionSource[numSources];

            for (var i = 0; i < numSources; i++)
            {
                sources[i] = (GeneFusionSource) SpanBufferBinaryReader.ReadByte(ref byteSpan);
            }

            return sources;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public string GetJsonEntry(string[] geneSymbols)
        {
            StringBuilder sb         = StringBuilderCache.Acquire();
            var           jsonObject = new JsonObject(sb);
            var           entries    = new List<string>();

            jsonObject.AddStringValues("genes", geneSymbols);
            if (_relationships   != null) AddGeneFusionSource("relationships",   _relationships,   entries, jsonObject);
            if (_germlineSources != null) AddGeneFusionSource("germlineSources", _germlineSources, entries, jsonObject);
            if (_somaticSources  != null) AddGeneFusionSource("somaticSources",  _somaticSources,  entries, jsonObject);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private static void AddGeneFusionSource(string description, GeneFusionSource[] sources, List<string> entries, JsonObject jsonObject)
        {
            entries.Clear();
            foreach (GeneFusionSource source in sources) entries.Add(GeneFusionSourceUtilities.Convert(source));
            jsonObject.AddStringValues(description, entries);
        }

        public bool Equals(GeneFusionSourceCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return _relationships.ArrayEqual(other._relationships)     &&
                   _germlineSources.ArrayEqual(other._germlineSources) &&
                   _somaticSources.ArrayEqual(other._somaticSources);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            
            if (_relationships != null)
                foreach (GeneFusionSource source in _relationships)
                    hashCode.Add((byte) source);
            
            if (_germlineSources != null)
                foreach (GeneFusionSource source in _germlineSources)
                    hashCode.Add((byte) source);
            
            if (_somaticSources != null)
                foreach (GeneFusionSource source in _somaticSources)
                    hashCode.Add((byte) source);
            
            return hashCode.ToHashCode();
        }
    }
}