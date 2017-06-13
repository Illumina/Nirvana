using System;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Mocks
{
    public class MockReferenceSequence : ICompressedSequence
    {
        public ChromosomeRenamer Renamer { get; set; }
        public ICytogeneticBands CytogeneticBands { get; set; }
        public GenomeAssembly GenomeAssembly { get; set; }

        private readonly string _sequence;
        private readonly int _offset;
        public int NumBases { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public MockReferenceSequence(string s, int offset = 0, ChromosomeRenamer renamer = null)
        {
            _sequence = s;
            _offset   = offset;
            Renamer   = renamer;
            NumBases  = s.Length;
        }

        public void Set(int numBases, byte[] buffer, IIntervalSearch<MaskedEntry> maskedIntervalSearch, int sequenceOffset = 0)
        {
            throw new NotImplementedException();
        }

        public string Substring(int offset, int length)
        {
            return _sequence.Substring(offset - _offset, length);
        }

        public bool Validate(int start, int end, string testSequence)
        {
            throw new NotImplementedException();
        }
    }
}
