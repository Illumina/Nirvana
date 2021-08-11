using System;
using System.Collections.Generic;
using System.Text;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.GeneFusions.SA
{
    public sealed class GeneFusionSourceCollection : IEquatable<GeneFusionSourceCollection>
    {
        private readonly bool               _isPseudogenePair;
        private readonly bool               _isParalogPair;
        private readonly bool               _isReadthrough;
        private readonly GeneFusionSource[] _germlineSources;
        private readonly GeneFusionSource[] _somaticSources;

        private const int PseudogeneMask  = 1;
        private const int ParalogMask     = 2;
        private const int ReadthroughMask = 4;

        public GeneFusionSourceCollection(bool isPseudogenePair, bool isParalogPair, bool isReadthrough, GeneFusionSource[] germlineSources,
            GeneFusionSource[] somaticSources)
        {
            _isPseudogenePair = isPseudogenePair;
            _isParalogPair    = isParalogPair;
            _isReadthrough    = isReadthrough;
            _germlineSources  = germlineSources;
            _somaticSources   = somaticSources;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(GetFlags());
            WriteSourceGroup(writer, _germlineSources);
            WriteSourceGroup(writer, _somaticSources);
        }

        private byte GetFlags()
        {
            byte flags                   = 0;
            if (_isPseudogenePair) flags |= PseudogeneMask;
            if (_isParalogPair) flags    |= ParalogMask;
            if (_isReadthrough) flags    |= ReadthroughMask;
            return flags;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
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
            byte flags            = SpanBufferBinaryReader.ReadByte(ref byteSpan);
            bool isPseudogenePair = (flags & PseudogeneMask)  != 0;
            bool isParalogPair    = (flags & ParalogMask)     != 0;
            bool isReadthrough    = (flags & ReadthroughMask) != 0;

            GeneFusionSource[] germlineSources = ReadSources(ref byteSpan);
            GeneFusionSource[] somaticSources  = ReadSources(ref byteSpan);
            return new GeneFusionSourceCollection(isPseudogenePair, isParalogPair, isReadthrough, germlineSources, somaticSources);
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
        public string GetJsonEntry(IGeneFusionPair geneFusionPair, uint[] oncogeneKeys)
        {
            StringBuilder sb         = StringBuilderPool.Get();
            var           jsonObject = new JsonObject(sb);
            var           entries    = new List<string>();

            AddGenes(geneFusionPair, oncogeneKeys, jsonObject);
            if (_germlineSources != null) AddGeneFusionSource("germlineSources", _germlineSources, entries, jsonObject);
            if (_somaticSources  != null) AddGeneFusionSource("somaticSources",  _somaticSources,  entries, jsonObject);
            return StringBuilderPool.GetStringAndReturn(sb);
        }

        private void AddGenes(IGeneFusionPair geneFusionPair, uint[] oncogeneKeys, JsonObject jsonObject)
        {
            jsonObject.StartObjectWithKey("genes");
            AddGene("first",  geneFusionPair.FirstGeneKey,  geneFusionPair.FirstGeneSymbol,  oncogeneKeys, jsonObject);
            AddGene("second", geneFusionPair.SecondGeneKey, geneFusionPair.SecondGeneSymbol, oncogeneKeys, jsonObject);

            jsonObject.AddBoolValue("isParalogPair",    _isParalogPair);
            jsonObject.AddBoolValue("isPseudogenePair", _isPseudogenePair);
            jsonObject.AddBoolValue("isReadthrough",    _isReadthrough);
            jsonObject.EndObject();
        }

        private static void AddGene(string key, uint geneKey, string geneSymbol, uint[] oncogeneKeys, JsonObject jsonObject)
        {
            jsonObject.StartObjectWithKey(key);
            jsonObject.AddStringValue("hgnc", geneSymbol);

            bool isOncogene = Array.BinarySearch(oncogeneKeys, geneKey) >= 0;
            jsonObject.AddBoolValue("isOncogene", isOncogene);

            jsonObject.EndObject();
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private static void AddGeneFusionSource(string description, GeneFusionSource[] sources, List<string> entries, JsonObject jsonObject)
        {
            entries.Clear();
            foreach (GeneFusionSource source in sources)
            {
                string sourceString = GeneFusionSourceUtilities.Convert(source);
                if (sourceString != null) entries.Add(sourceString);
            }
            jsonObject.AddStringValues(description, entries);
        }

        public bool Equals(GeneFusionSourceCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return _isPseudogenePair == other._isPseudogenePair        &&
                   _isParalogPair    == other._isParalogPair           &&
                   _isReadthrough    == other._isReadthrough           &&
                   _germlineSources.ArrayEqual(other._germlineSources) &&
                   _somaticSources.ArrayEqual(other._somaticSources);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(_isPseudogenePair);
            hashCode.Add(_isParalogPair);
            hashCode.Add(_isReadthrough);
            
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