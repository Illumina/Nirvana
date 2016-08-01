using System;
using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Illumina.VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;
using Xunit.Sdk;

namespace NirvanaUnitTests.SupplementaryAnnotations
{
    [Collection("GRCh37 collection")]
    public sealed class CreateSupplementaryDatabaseTests : IDisposable
	{
		#region members

		private readonly string _tempDir;
		private bool _isDisposed;
		private readonly SupplementaryAnnotationReader _supplementaryAnnotationReaderChr1;
		private readonly SupplementaryAnnotationReader _supplementaryAnnotationReaderChr3;
		private readonly SupplementaryAnnotationReader _supplementaryAnnotationReaderChr4;

		#endregion

		// constructor
		public CreateSupplementaryDatabaseTests()
		{
			_tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			if (!Directory.Exists(_tempDir)) Directory.CreateDirectory(_tempDir);

			List<string> customAnnotationFiles = new List<string>()
			{
				@"D:\Projects\Nirvana\Sandbox\NirvanaUnitTests\Resources\missingLastVariantHgmd.vcf"
			};

		    // ReSharper disable once UnusedVariable
			var supplementaryDatabaseCreator =
				   new CreateSupplementaryDatabase.CreateSupplementaryDatabase(_tempDir, null, null, null, null, null, null, null, null, null, customAnnotationFiles);

			string supplementaryAnnotationPathChr1 = Path.Combine(_tempDir, "chr1" + ".nsa");
			_supplementaryAnnotationReaderChr1 =
				new SupplementaryAnnotationReader(supplementaryAnnotationPathChr1);

			string supplementaryAnnotationPathChr3 = Path.Combine(_tempDir, "chr3" + ".nsa");
			_supplementaryAnnotationReaderChr3 =
				new SupplementaryAnnotationReader(supplementaryAnnotationPathChr3);

			string supplementaryAnnotationPathChr4 = Path.Combine(_tempDir, "chr4" + ".nsa");
			_supplementaryAnnotationReaderChr4 =
				new SupplementaryAnnotationReader(supplementaryAnnotationPathChr4);
		}

		[Fact(Skip = "does not work on all machines")]
		public void FirstVariantsCreatedCorrectly()
		{
			// chr1	899318	.	CCT	C	.	.	CLASS=DM?;MUT=ALT;GENE=KLHL17;STRAND=+;DNA=NM_198317.2:c.1375_1376delCT;PHEN=Schizophrenia;ACC=CD142720
			
			var sa = _supplementaryAnnotationReaderChr1.GetAnnotation(899319);

			Assert.NotNull(sa);
			Assert.Equal("HGMD",sa.CustomItems[0].AnnotationType);
			Assert.Equal("2", sa.CustomItems[0].SaAltAllele);
			Assert.Equal(true,sa.CustomItems[0].IsPositional);

			// chr3	361508	.	C	T	.	.	CLASS=DP;MUT=ALT;GENE=CHL1;STRAND=+;DNA=NM_006614.3:c.49C>T;PROT=NP_006605.2:p.L17F;DB=rs2272522;PHEN=Schizophrenia_association_with;ACC=CM023348
			sa = _supplementaryAnnotationReaderChr3.GetAnnotation(361508);

			Assert.NotNull(sa);
			Assert.Equal("HGMD", sa.CustomItems[0].AnnotationType);
			Assert.Equal("T", sa.CustomItems[0].SaAltAllele);
			Assert.Equal(true, sa.CustomItems[0].IsPositional);
		}

		[Fact(Skip = "does not work on all machines")]
		public void LastVariantsCreatedCorrectly()
		{
			// chr1	949696	.	C	CG	.	.	CLASS=DM;MUT=ALT;GENE=ISG15;STRAND=+;DNA=NM_005101.3:c.339dupG;PHEN=Mycobacterial_disease_mendelian_susceptibility_to;ACC=CI128669
			var sa = _supplementaryAnnotationReaderChr1.GetAnnotation(949697);
			Assert.NotNull(sa);

			Assert.Equal("HGMD", sa.CustomItems[0].AnnotationType);
			Assert.Equal("iG", sa.CustomItems[0].SaAltAllele);
			Assert.Equal(true, sa.CustomItems[0].IsPositional);
		}

		[Fact(Skip = "does not work on all machines")]
		public void InsertionDeletionCreatedCorrectly()
		{
			// chr4    619535.CCCGCC  CGAGGACGGCCTGCGA.   .CLASS = DM; MUT = ALT; GENE = PDE6B; STRAND = +; DNA = NM_000283.3:c.121_125delCCGCCinsGAGGACGGCCTGCGA; PHEN = Retinitis_pigmentosa_autosomal_recessive; ACC = CX148735
			var sa = _supplementaryAnnotationReaderChr4.GetAnnotation(619536);
			Assert.NotNull(sa);

			Assert.Equal("HGMD", sa.CustomItems[0].AnnotationType);
			Assert.Equal("5GAGGACGGCCTGCGA", sa.CustomItems[0].SaAltAllele);
			Assert.Equal(true, sa.CustomItems[0].IsPositional);
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_supplementaryAnnotationReaderChr1.Dispose();
				_supplementaryAnnotationReaderChr3.Dispose();
				_supplementaryAnnotationReaderChr4.Dispose();

				if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
				_isDisposed = true;
			}
		}
	}
}
