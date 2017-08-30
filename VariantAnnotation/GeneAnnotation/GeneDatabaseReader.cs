using ErrorHandling.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.GeneAnnotation
{
    public class GeneDatabaseReader
    {
        private readonly string _geneDatabaseFile;
        private readonly ExtendedBinaryReader _reader;
        public GenomeAssembly GenomeAssembly;
        private long _creationTime;
        public List<IDataSourceVersion> DataSourceVersions;
        private bool _isDisposed;


        public GeneDatabaseReader(string geneDatabaseFile)
        {
            // open the database file
            _geneDatabaseFile = geneDatabaseFile;
            _reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(geneDatabaseFile));
            DataSourceVersions = new List<IDataSourceVersion>();
            ReadHeader();
        }

        private void ReadHeader()
        {
            var header = _reader.ReadString();
            if (header != SupplementaryAnnotationCommon.DataHeader)
                throw new FormatException("Unrecognized header in this database");

            var dataVersion = _reader.ReadUInt16();
            if (dataVersion != SupplementaryAnnotationCommon.DataVersion)
                throw new UserErrorException(
                    $"Gene database data version mismatch. Expected {SupplementaryAnnotationCommon.DataVersion}, observed {dataVersion}");

            var schema = _reader.ReadUInt16();
            if (schema != SupplementaryAnnotationCommon.SchemaVersion)
                throw new UserErrorException(
                    $"Gene database schema mismatch. Expected {SupplementaryAnnotationCommon.SchemaVersion}, observed {schema}");

            GenomeAssembly = (GenomeAssembly)_reader.ReadByte();

            _creationTime = _reader.ReadInt64();

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
            if (observedGuard != SupplementaryAnnotationCommon.GuardInt)
            {
                throw new UserErrorException($"Expected a guard integer ({SupplementaryAnnotationCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }
    }
}
