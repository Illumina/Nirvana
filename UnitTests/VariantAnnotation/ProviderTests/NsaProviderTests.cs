﻿using System;
using System.Collections.Generic;
using Genome;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.VariantAnnotation.ProviderTests
{
    public sealed class NsaProviderTests
    {
        private static IAnnotationProvider GetDbSnpProvider()
        {
            var chrom1Pos100Annotations = new List<(string refAllele, string altAllele, string annotation)>
            {
                ("A", "T", "\"rs100\""),
                ("A", "C", "\"rs101\"")
            };

            var dbsnpReader = new Mock<INsaReader>();
            dbsnpReader.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            dbsnpReader.SetupGet(x => x.MatchByAllele).Returns(true);
            dbsnpReader.SetupGet(x => x.IsArray).Returns(true);
            dbsnpReader.SetupGet(x => x.JsonKey).Returns("dbSnp");
            dbsnpReader.SetupGet(x => x.Version)
                .Returns(new DataSourceVersion("dbsnp", "v1", DateTime.Now.Ticks, "dummy db snp"));
            
            //dbsnpReader.SetupSequence(x => x.GetAnnotation(100)).Returns(chrom1Pos100Annotations);
            //List<(string refAllele, string altAllele, string annotation)> annotations=null;
            dbsnpReader.Setup(x =>
                x.GetAnnotation(It.IsAny<int>(), It.IsAny<List<(string refAllele, string altAllele, string annotation)>>() ))
                .Callback((int position, List<(string refAllele, string altAllele, string annotation)> annotations) =>
                {
                    annotations.Clear();
                    annotations.AddRange(chrom1Pos100Annotations);
                });
            var provider = new NsaProvider(new[] {dbsnpReader.Object}, null, null);

            return provider;
        }

        private static IAnnotationProvider GetClinVarProvider()
        {
            var chrom1Pos100Annotations = new List<(string refAllele, string altAllele, string annotation)>
            {
                ("A", "T", "RCV00001"),
                ("A", "C", "RCV00002")
            };

            var clinvarReader = new Mock<INsaReader>();
            clinvarReader.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            clinvarReader.SetupGet(x => x.MatchByAllele).Returns(false);
            clinvarReader.SetupGet(x => x.IsArray).Returns(true);
            clinvarReader.SetupGet(x => x.JsonKey).Returns("clinvar");
            clinvarReader.SetupGet(x => x.Version)
                .Returns(new DataSourceVersion("clinvar", "v1", DateTime.Now.Ticks, "dummy clinvar data"));
            clinvarReader.Setup(x =>
                    x.GetAnnotation(It.IsAny<int>(), It.IsAny<List<(string refAllele, string altAllele, string annotation)>>() ))
                .Callback((int position, List<(string refAllele, string altAllele, string annotation)> annotations) =>
                {
                    annotations.Clear();
                    annotations.AddRange(chrom1Pos100Annotations);
                });

            var provider = new NsaProvider(new[] {clinvarReader.Object}, null, null);

            return provider;
        }

        private static IAnnotatedPosition GetPosition(IChromosome chrom, int start, string refAllele, string[] altAlleles)
        {
            var position = new Mock<IAnnotatedPosition>();
            var annotatedVariants = new List<IAnnotatedVariant>();
            foreach (string altAllele in altAlleles)
            {
                VariantType type = SmallVariantCreator.GetVariantType(refAllele, altAllele);
                int end = start + altAllele.Length - 1;

                annotatedVariants.Add(new AnnotatedVariant(new Variant(chrom, start, end, refAllele, altAllele, type, null, false, false, false,
                    null, AnnotationBehavior.SmallVariants, false)));
            }

            position.SetupGet(x => x.AnnotatedVariants).Returns(annotatedVariants.ToArray);
            return position.Object;
        }


        [Fact]
        public void Annotate_alleleSpecific()
        {
            var provider = GetDbSnpProvider();
            var position = GetPosition(ChromosomeUtilities.Chr1, 100, "A", new []{"T"});

            provider.Annotate(position);
            var jsonString = position.AnnotatedVariants[0].GetJsonString("chr1");
            Assert.Equal("{\"chromosome\":\"chr1\",\"begin\":100,\"end\":100,\"refAllele\":\"A\",\"altAllele\":\"T\",\"variantType\":\"SNV\",\"dbSnp\":[\"rs100\"]}", jsonString);
        }

        [Fact]
        public void Annotate_notAlleleSpecific_isArray()
        {
            var provider = GetClinVarProvider();
            var position = GetPosition(ChromosomeUtilities.Chr1, 100, "A", new[] { "T" });

            provider.Annotate(position);
            var jsonString = position.AnnotatedVariants[0].GetJsonString("chr1");
            Assert.Equal("{\"chromosome\":\"chr1\",\"begin\":100,\"end\":100,\"refAllele\":\"A\",\"altAllele\":\"T\",\"variantType\":\"SNV\",\"clinvar\":[{RCV00001,\"isAlleleSpecific\":true},{RCV00002}]}", jsonString);
        }
    }
}
