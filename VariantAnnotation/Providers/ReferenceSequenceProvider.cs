﻿using System.Collections.Generic;
using System.IO;
using Genome;
using Intervals;
using ReferenceSequence.IO;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Providers
{
    public sealed class ReferenceSequenceProvider : ISequenceProvider
    {
        public Dictionary<string, Chromosome> RefNameToChromosome  => _sequenceReader.RefNameToChromosome;
        public Dictionary<ushort, Chromosome> RefIndexToChromosome => _sequenceReader.RefIndexToChromosome;
        public GenomeAssembly                   Assembly             => _sequenceReader.Assembly;
        public string                           Name                 => "Reference sequence provider";
        public IEnumerable<IDataSourceVersion>  DataSourceVersions   => null;

        public ISequence Sequence { get; }

        private ushort _currentChromosomeIndex = 65534; // guaranteed to be updated
        private readonly CompressedSequenceReader _sequenceReader;

        public ReferenceSequenceProvider(Stream stream)
        {
            _sequenceReader = new CompressedSequenceReader(stream);
            Sequence        = _sequenceReader.Sequence;
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null) return;

            annotatedPosition.CytogeneticBand = Sequence.CytogeneticBands.Find(annotatedPosition.Position.Chromosome, annotatedPosition.Position.Start,
                annotatedPosition.Position.End);

            // we don't want HGVS g. nomenclature for structural variants or STRs
            if (annotatedPosition.Position.HasStructuralVariant || annotatedPosition.Position.HasShortTandemRepeat) return;
            
            string refSeqAccession = annotatedPosition.Position.Chromosome.RefSeqAccession;
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                annotatedVariant.HgvsgNotation = HgvsgNotation.GetNotation(refSeqAccession, annotatedVariant.Variant, Sequence, new Interval(0, Sequence.Length));
            }
        }

        public void PreLoad(Chromosome chromosome, List<int> positions)
        {
            throw new System.NotImplementedException();
        }

        public void LoadChromosome(Chromosome chromosome)
        {
            if (chromosome.Index == _currentChromosomeIndex) return;
            _sequenceReader.GetCompressedSequence(chromosome);
            _currentChromosomeIndex = chromosome.Index;
        }

        public void Dispose() => _sequenceReader?.Dispose();
    }
}