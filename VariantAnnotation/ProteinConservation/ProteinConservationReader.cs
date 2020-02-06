using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace VariantAnnotation.ProteinConservation
{
    public sealed class ProteinConservationReader:IDisposable
    {
        private readonly Stream _stream;
        public GenomeAssembly Assembly { get; private set; }
        private readonly ExtendedBinaryReader _reader;
        public readonly IDataSourceVersion Version;

        public ProteinConservationReader(Stream stream)
        {
            _stream = stream;
            _reader = new ExtendedBinaryReader(_stream);
            
            var schemaVersion = _reader.ReadOptInt32();
            if(schemaVersion != ProteinConservationCommon.SchemaVersion)
                throw new Exception($"Schema version mismatch found. Observed: {schemaVersion}, expected: {ProteinConservationCommon.SchemaVersion}");
            Assembly = (GenomeAssembly) _reader.ReadByte();
            Version = DataSourceVersion.Read(_reader);
        }

        public IEnumerable<TranscriptConservationScores> GetItems()
        {
            TranscriptConservationScores score;
            while ((score = TranscriptConservationScores.Read(_reader))!=null)
            {
                if (score.IsEmpty()) break;
                yield return score;
            }
            
        }

        public void Dispose() =>_reader?.Dispose(); 
        
    }
}