using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;

namespace VariantAnnotation.Providers
{
    public sealed class ReferenceSequenceProvider : ISequenceProvider
    {
	    public string Name { get; }
	    public GenomeAssembly GenomeAssembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        public ushort NumRefSeqs { get; }

        public ISequence Sequence { get; }

        private readonly IDictionary<string, IChromosome> _chromosomeDictionary;
        private readonly IDictionary<ushort, IChromosome> _chromosomeIndexDictionary;
        public IDictionary<string, IChromosome> GetChromosomeDictionary() => _chromosomeDictionary;
        public IDictionary<ushort, IChromosome> GetChromosomeIndexDictionary() => _chromosomeIndexDictionary;

        private readonly CytogeneticBands _cytogeneticBands;
        private int _currentReferenceIndex;
        private readonly CompressedSequenceReader _sequenceReader;

        public ReferenceSequenceProvider(Stream stream)
        {
	        Name = "Reference sequence provider";
            _sequenceReader            = new CompressedSequenceReader(stream);
            Sequence                   = _sequenceReader.Sequence;
            NumRefSeqs                 = _sequenceReader.NumRefSeqs;
            _cytogeneticBands          = new CytogeneticBands(_sequenceReader.CytogeneticBands);
            _chromosomeDictionary      = new Dictionary<string, IChromosome>();
            _chromosomeIndexDictionary = new Dictionary<ushort, IChromosome>();
            GenomeAssembly             = _sequenceReader.Assembly;
            DataSourceVersions         = null;
            _currentReferenceIndex     = -1;

            AddReferenceMetadata(_sequenceReader.Metadata);
        }

        /// <summary>
        /// returns true if the specified reference sequence is in the standard reference sequences and in VEP
        /// </summary>
        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            LoadChromosome(annotatedPosition.Position.Chromosome);
            if (annotatedPosition.AnnotatedVariants == null) return;

            annotatedPosition.CytogeneticBand = _cytogeneticBands.GetCytogeneticBand(annotatedPosition.Position.Chromosome, annotatedPosition.Position.Start,
                annotatedPosition.Position.End);
        }

        private void LoadChromosome(IChromosome chromosome)
        {
            var refIndex = chromosome.Index;
            if (refIndex == _currentReferenceIndex) return;

            _sequenceReader.GetCompressedSequence(chromosome);
            _currentReferenceIndex = refIndex;
        }

        private void AddReferenceMetadata(ReferenceMetadata[] refMetadataList)
        {
            ushort index = 0;

            _chromosomeDictionary.Clear();

            foreach (var refMetadata in refMetadataList)
            {
                AddReferenceName(refMetadata.EnsemblName, refMetadata.UcscName, index);
                index++;
            }
        }

        /// <summary>
        /// adds a Ensembl/UCSC reference name pair to the current dictionary
        /// </summary>
        private void AddReferenceName(string ensemblReferenceName, string ucscReferenceName, ushort refIndex)
        {
            var isUcscEmpty    = string.IsNullOrEmpty(ucscReferenceName);
            var isEnsemblEmpty = string.IsNullOrEmpty(ensemblReferenceName);

            // sanity check: make sure we have at least one reference name
            if (isUcscEmpty && isEnsemblEmpty) return;

            if (isUcscEmpty)    ucscReferenceName    = ensemblReferenceName;
            if (isEnsemblEmpty) ensemblReferenceName = ucscReferenceName;

            var chromosome = new Chromosome(ucscReferenceName, ensemblReferenceName, refIndex);

            _chromosomeDictionary[ucscReferenceName]    = chromosome;
            _chromosomeDictionary[ensemblReferenceName] = chromosome;
            _chromosomeIndexDictionary[refIndex]        = chromosome;
        }
    }
}