using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvspNotationTests
    {
        [Fact]
        //hgvs example: LRG_199p1:p.Trp24Cys
        public void Missense_substitution()
        {
            Assert.Equal("LRG_199p1:p.(Trp24Cys)", HgvspNotation.GetSubstitutionNotation("LRG_199p1",24, "Trp", "Cys"));
        }

        [Fact]
        //hgvs example: LRG_199p1:p.Trp24Ter 
        public void Nonsense_substitution()
        {
            Assert.Equal("LRG_199p1:p.(Trp24Ter)", HgvspNotation.GetSubstitutionNotation("LRG_199p1", 24,"Trp", "Ter"));
        }

        [Fact]
        //hgvs example: NP_003997.1:p.Cys188=
        public void Silent_substitution()
        {
            Assert.Equal("NP_003997.1:c.XXX(p.(Cys188=))", HgvspNotation.GetSilentNotation("NP_003997.1:c.XXX", 188, "Cys", false));
        }

        [Fact]
        //hgvs example: LRG_199p1:p.(Met1?)
        public void StartLost_due_to_substitution()
        {
            Assert.Equal("LRG_199p1:p.(Met1?)", HgvspNotation.GetSubstitutionNotation("LRG_199p1", 1, "Met","Cys"));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Ala3del)
        public void One_aminoAcid_deletion()
        {
			Assert.Equal("NP_003997.1:p.(Ala3del)", HgvspNotation.GetDeletionNotation("NP_003997.1", 3, 3, "Ala", false));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Ala3_Ser5del)
        public void Multiple_aminoAcid_deletion()
        {
            Assert.Equal("NP_003997.1:p.(Ala3_Ser5del)", HgvspNotation.GetDeletionNotation("NP_003997.1", 3, 5,"AlaLysSer", false));
        }

	    [Fact]
		//p.Trp26Ter
		public void Deletion_gained_stop()
	    {
		    Assert.Equal("NP_003997.1:p.(Trp26Ter)", HgvspNotation.GetDeletionNotation("NP_003997.1", 26, 27, "Trp", true));
		}

	    [Fact]
	    public void Unknown_start_equals_end()
	    {
			Assert.Equal("NP_003997.1:p.(Arg26Cys)", HgvspNotation.GetUnknownNotation("NP_003997.1", 26, 26,  "Arg","Cys"));
	    }

	    [Fact]
	    public void Unknown_start_not_equals_end()
	    {
		    Assert.Equal("NP_003997.1:p.(Arg26_Cys27)", HgvspNotation.GetUnknownNotation("NP_003997.1", 26, 27, "Arg", "Cys"));
	    }

		[Fact]
        // hgvs example:NP_003997.1:p.(Ala3dup)
        public void One_aminoAcid_duplication()
        {
            Assert.Equal("NP_003997.1:p.(Ala3dup)", HgvspNotation.GetDuplicationNotation("NP_003997.1", 3, 3, "Ala"));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Ala3_Ser5dup)
        public void Multiple_aminoAcid_duplication()
        {
            Assert.Equal("NP_003997.1:p.(Ala3_Ser5dup)", HgvspNotation.GetDuplicationNotation("NP_003997.1",3, 5, "AlaLysSer"));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(His4_Gln5insAla)
        public void One_aminoAcid_insertion()
        {
            Assert.Equal("NP_003997.1:p.(His4_Gln5insAla)", HgvspNotation.GetInsertionNotation("NP_003997.1", 4, 5, "Ala","MBCHQDE"));
        }

		[Fact]
		public void Insert_stop_codon()
		{
			Assert.Equal("NP_003997.1:p.(Gln5Ter)", HgvspNotation.GetInsertionNotation("NP_003997.1", 4, 5, "TerAla", "MBCHQDE"));
		}

	    [Fact]
	    public void Insert_past_stop()
	    {
			Assert.Null(HgvspNotation.GetInsertionNotation("NP_003997.1", 8, 9, "TerAla", "MBCHQDE"));
	    }
		
	    [Fact]
        // hgvs example:NP_003997.1:p.(Lys2_Gly3insGlnSerLys)
        public void Multiple_aminoAcid_insertion()
        {
            Assert.Equal("NP_003997.1:p.(Lys2_Gly3insGlnSerLys)", HgvspNotation.GetInsertionNotation("NP_003997.1", 2, 3, "GlnSerLys", "MKGABC"));
        }

	    [Fact]
	    // hgvs example:NP_003997.1:p.(Lys2_Gly3insGlnSerLys)
	    public void Insertion_at_end()
	    {
		    Assert.Equal("NP_003997.1:p.(Cys6_Ter7insGlnSerLys)", HgvspNotation.GetInsertionNotation("NP_003997.1", 6, 7, "GlnSerLys", "MKGABC*"));
	    }

		[Fact]
        // hgvs example:NP_003997.1:p.(Cys28delinsTrpVal)
        public void Del_one_ins_two()
        {
            Assert.Equal("NP_003997.1:p.(Cys28delinsTrpVal)", HgvspNotation.GetDelInsNotation("NP_003997.1", 28, 28, "Cys", "TrpVal"));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Cys28_Lys29delinsTrp)
        public void Del_two_ins_one()
        {
            Assert.Equal("NP_003997.1:p.(Cys28_Lys29delinsTrp)", HgvspNotation.GetDelInsNotation("NP_003997.1", 28, 29, "CysLys", "Trp"));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Pro578_Lys579delinsLeuTer)
        public void Del_two_ins_stop()
        {
            Assert.Equal("NP_003997.1:p.(Pro578_Lys579delinsLeuTer)", HgvspNotation.GetDelInsNotation("NP_003997.1", 578, 579, "ProLys", "LeuTer"));
        }

	    [Fact]
		//Pro578_Lys579 goes to TerLeu
		public void Delins_becomes_substitution_of_Ter()
	    {
		    Assert.Equal("NP_003997.1:p.(Pro578Ter)", HgvspNotation.GetDelInsNotation("NP_003997.1", 578, 579, "ProLys", "TerLeu"));
		}

	    [Fact]
        // hgvs example:NP_003997.1:p.(Arg97ProfsTer23)
        public void Frameshift_with_known_countToStop()
        {
            Assert.Equal("NP_003997.1:p.(Arg97ProfsTer23)", HgvspNotation.GetFrameshiftNotation("NP_003997.1", 97, "Arg", "Pro",23));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Tyr4Ter)
        public void Frameshift_gains_immediate_stop()
        {
            Assert.Equal("NP_003997.1:p.(Tyr4Ter)", HgvspNotation.GetFrameshiftNotation("NP_003997.1", 4, "Tyr", "TerCysIle", -1));
        }

        [Fact]
        // hgvs example:NP_003997.1:p.(Ile327ArgfsTer?)
        public void Frameshift_unknown_countToStop()
        {
            Assert.Equal("NP_003997.1:p.(Ile327ArgfsTer?)", HgvspNotation.GetFrameshiftNotation("NP_003997.1", 327, "Ile", "Arg", -1));
        }

	    [Fact]
	    
	    public void Frameshift_due_to_insertion()
	    {
		    Assert.Equal("NP_003997.1:p.(Cys3ArgfsTer40)", HgvspNotation.GetFrameshiftNotation("NP_003997.1", 3, "Cys", "Arg", 40));
	    }

	    [Fact]
		//NP_001263627.1:p.(Met1?)
		public void Start_lost_start_equals_end()
	    {
		    Assert.Equal("NP_001263627.1:p.(Met1?)", HgvspNotation.GetStartLostNotation("NP_001263627.1", 1, 1, "Met"));
		}

	    [Fact]
	    //NP_001263627.1:p.(Met1?)
	    public void Start_lost_start_not_equals_end()
	    {
		    Assert.Equal("NP_001263627.1:p.(Met1_?3)", HgvspNotation.GetStartLostNotation("NP_001263627.1", 1, 3, "Met"));
	    }

	    [Fact]
		// from varnom: p.Ter110Glnext*17
		public void Stop_lost_with_countToEnd()
	    {
		    Assert.Equal("NP_001263627.1:p.(Ter110GlnextTer17)", HgvspNotation.GetExtensionNotation("NP_001263627.1",110, "Ter", "Gln", 17));
		}

		//p.Ter327Argext*?
		[Fact]
	    public void Stop_lost_without_countToEnd()
	    {
		    Assert.Equal("NP_001263627.1:p.(Ter327ArgextTer?)", HgvspNotation.GetExtensionNotation("NP_001263627.1", 327, "Ter", "Arg", -1));
	    }

	}
}
