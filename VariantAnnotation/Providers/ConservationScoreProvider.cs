﻿using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.PhyloP;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class ConservationScoreProvider : IAnnotationProvider
    {
		private readonly PhylopReader _phylopReader;
		private string _currentUcscReferenceName;


		public string Name { get; }
		public GenomeAssembly Assembly { get; }
		public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

		public ConservationScoreProvider(IEnumerable<string> dirPaths)
		{
            Name                      = "Conservation score provider";
            _phylopReader             = new PhylopReader(dirPaths);
            Assembly                  = _phylopReader.GenomeAssembly;
            DataSourceVersions        = _phylopReader.DataSourceVersions;
            _currentUcscReferenceName = "";
        }

		public void Annotate(IAnnotatedPosition annotatedPosition)
		{
			if (_currentUcscReferenceName != annotatedPosition.Position.Chromosome.UcscName)
				LoadChromosome(annotatedPosition.Position.Chromosome);

			foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
			{
				if (annotatedVariant.Variant.Type != VariantType.SNV) continue;
				annotatedVariant.PhylopScore = _phylopReader.GetScore(annotatedVariant.Variant.Start);
			}
		}

		private void LoadChromosome(IChromosome chromosome)
		{
			_currentUcscReferenceName = chromosome.UcscName;
			_phylopReader.LoadChromosome(chromosome.UcscName);
		}
	}
}