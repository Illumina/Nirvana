using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.PhyloP;
using VariantAnnotation.SA;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class ConservationScoreProvider : IAnnotationProvider
    {
		private readonly NpdReader _phylopReader;


		public string Name { get; }
		public GenomeAssembly Assembly { get; }
		public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

		public ConservationScoreProvider(IEnumerable<string> dirPaths)
		{
            Name = "Conservation score provider";

		    foreach (string saDir in dirPaths)
		    {
		        var phylopFiles = Directory.GetFiles(saDir, "*"+SaCommon.PhylopFileSuffix);
		        if (phylopFiles.Length > 0)
		        {
		            var npdFile = phylopFiles[0];
		            var npdIndexFile = npdFile + SaCommon.IndexSufix;
                    _phylopReader = new NpdReader(FileUtilities.GetReadStream(npdFile), FileUtilities.GetReadStream(npdIndexFile));
		            break;//we can have only one phylop database
		        }
		    }

		    Assembly                  = _phylopReader.Assembly;
            DataSourceVersions        = new []{_phylopReader.Version};
        }

		public void Annotate(IAnnotatedPosition annotatedPosition)
		{
			foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
			{
				if (annotatedVariant.Variant.Type != VariantType.SNV) continue;
				annotatedVariant.PhylopScore = _phylopReader.GetAnnotation(annotatedPosition.Position.Chromosome, annotatedVariant.Variant.Start);
			}
		}

        public void PreLoad(IChromosome chromosome, List<int> positions)
        {
            throw new NotImplementedException();
        }

        
	}
}