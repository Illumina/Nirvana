using ErrorHandling.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using IO;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.GeneAnnotation
{
    public sealed class GeneDatabaseReader
    {
        private readonly ExtendedBinaryReader _reader;
        public readonly List<IDataSourceVersion> DataSourceVersions;

        public GeneDatabaseReader(Stream geneDatabaseFileStream)
        {
            _reader = new ExtendedBinaryReader(geneDatabaseFileStream);
            DataSourceVersions = new List<IDataSourceVersion>();
            ReadHeader();
        }

        private void ReadHeader()
        {
            var header = _reader.ReadString();
            if (header != SaCommon.DataHeader) throw new FormatException("Unrecognized header in this database");

            _reader.ReadUInt16(); // data version

            var schema = _reader.ReadUInt16();
            if (schema != SaCommon.SchemaVersion) throw new UserErrorException($"Gene database schema mismatch. Expected {SaCommon.SchemaVersion}, observed {schema}");

            _reader.ReadByte(); // genome assembly
            _reader.ReadInt64(); // creation time

            var dataSourseVersionsCount = _reader.ReadOptInt32();

            for (var i = 0; i < dataSourseVersionsCount; i++)
            {
                DataSourceVersions.Add(DataSourceVersion.Read(_reader));
            }

            CheckGuard();
        }

        public IEnumerable<IAnnotatedGene> Read()
        {
            IAnnotatedGene annotatedGene;
            while ((annotatedGene = AnnotatedGene.Read(_reader)) != null)
            {
                yield return annotatedGene;
            }
        }

        private void CheckGuard()
        {
            var observedGuard = _reader.ReadUInt32();
            if (observedGuard != SaCommon.GuardInt)
            {
                throw new UserErrorException($"Expected a guard integer ({SaCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }
    }
}
