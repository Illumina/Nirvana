using System.Collections.Generic;
using System.IO;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.Interface;

namespace UnitTests.Mocks
{
    public class MockConservationScoreReader : IConservationScoreReader
    {
        public bool IsInitialized { get; }
        private readonly PhylopReader _reader;
        public GenomeAssembly GenomeAssembly => GenomeAssembly.Unknown;
        public IEnumerable<IDataSourceVersion> DataSourceVersions => new List<IDataSourceVersion>();
        public void Clear() => _reader.Clear();

        /// <summary>
        /// constructor
        /// </summary>
        public MockConservationScoreReader(Stream stream)
        {
            _reader = new PhylopReader(stream);
            IsInitialized = _reader != null;
        }

        public string GetScore(int position) => _reader.GetScore(position);

        public void LoadReference(string ucscReferenceName) {}
    }
}
