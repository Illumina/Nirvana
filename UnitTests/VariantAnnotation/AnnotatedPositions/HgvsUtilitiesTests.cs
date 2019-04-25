using Genome;
using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsUtilitiesTests
    {
        [Fact]
        public void ShiftAndRotateAlleles_Rotated()
        {
            int observedStart            = 98;
            string observedRefAminoAcids = "YYAKEV";
            string observedAltAminoAcids = "Y";

            HgvsUtilities.ShiftAndRotateAlleles(ref observedStart, ref observedRefAminoAcids, ref observedAltAminoAcids,
                "MHYCVLSAFLILHLVTVALSLSTCSTLDMDQFMRKRIEAIRGQILSKLKLTSPPEDYPEPEEVPPEVISIYNSTRDLLQEKASRRAAACERERSDEEYYAKEVYKIDMPPFFPSENAIPPTFYRPYFRIVRFDVSAMEKNASNLVKAEFRVFRLQNPKARVPEQRIELYQILKSKDLTSPTQRYIDSKVVKTRAEGEWLSFDVTDAVHEWLHHKDRNLGFKISLHCPCCTFVPSNNYIIPNKSEELEARFAGIDGTSTYTSGDQKTIKSTRKKNSGKTPHLLLMLLPSYRLESQQTNRRKKRALDAAYCFRNVQDNCCLRPLYIDFKRDLGWKWIHEPKGYNANFCAGACPYLWSSDTQHSRVLSLYNTINPEASASPCCVSQDLEPLTILYYIGKTPKIEQLSNMIVKSCKCS");

            Assert.Equal(100,     observedStart);
            Assert.Equal("AKEVY", observedRefAminoAcids);
            Assert.Equal("",      observedAltAminoAcids);
        }

        [Fact]
        public void Rotate3Prime_Identity_Insertion()
        {
            const int expectedStart            = 46;
            const string expectedRefAminoAcids = "";
            const string expectedAltAminoAcids = "A";

            var observedResult = HgvsUtilities.Rotate3Prime("", "A", 44,
                "MAAQVAPAAASSLGNPPPPPPSELKKAEQQQREEAGGEAAAAAAAERGEMKAAAGQESEGPAVGPPQPLGKELQDGAESNGGGGGGGAGSGGGPGAEPDLKNSNGNAGPRPALNNNLTEPPGGGGGGSSDGVGAPPHSAAAALPPPAYGFGQPYGRSPSAVAAAAAAVFHQQHGGQQSPGLAALQSGGGGGLEPYAGPQQNSHDHGFPNHQYNSYYPNRSAYPPPAPAYALSSPRGGTPGSGAAAAAGSKPPPSSSASASSSSSSFAQQRFGAMGGGGPSAAGGGTPQPTATPTLNQLLTSPSSARGYQGYPGGDYSGGPQDGGAGKGPADMASQCWGAAAAAAAAAAASGGAQQRSHHAPMSPGSSGGGGQPLARTPQPSSPMDQMGKMRPQPYGGTNPYSQQQGPPSGPQQGHGYPGQPYGSQTPQRYPMTMQGRAQSAMGGLSYTQQIPPYGQQGPSGYGQQGQTPYYNQQSPHPQQQQPPYSQQPPSQTPHAQPSYQQQPQSQPPQLQSSQPPYSQQPSQPPHQQSPAPYPSQQSTTQQHPQSQPPYSQPQAQSPYQQQQPQQPAPSTLSQQAAYPQPQSQQSQQTAYSQQRFPPPQELSQDSFGSQASSAPSMTSSKGGQEDMNLSLQSRPSSLPDLSGSIDDLPMGTEGALSPGVSTSGISSSQGEQSNPAQSPFSPHTSPHLPGIRGPSPSPVGSPASVAQSRSGPLSPAAVPGNQMPPRPPSGQSDSIMHPSMNQSSIAQDRGYMQRNPQMPQYSSPQPGSALSPRQPSGGQIHTGMGSYQQNSMGSYGPQGGQYGPQGGYPRQPNYNALPNANYPSAGMAGGINPMGAGGQMHGQPGIPPYGTLPPGRMSHASMGNRPYGPNMANMPPQVGSGMCPPPGGMNRKTQETAVAMHVAANSIQNRPPGYPNMNQGGMMGTGPPYGQGINSMAGMINPQGPPYSMGGTMANNSAGMAASPEMMGLGDVKLTPATKMNNKADGTPKTESKSKKSSSSTTTNEKITKLYELGGEPERKMWVDRYLAFTEEKAMGMTNLPAVGRKPLDLYRLYVSVKEIGGLTQVNKNKKWRELATNLNVGTSSSAASSLKKQYIQCLYAFECKIERGEDPPPDIFAAADSKKSQPKIQPPSPAGSGSMQGPQTPQSTSSSMAEGGDLKPPTPASTPHSQIPPLPGMSRSNSVGIQDAFNDGSDSTFQKRNSMTPNPGYQPSMNTSDMMGRMSYEPNKDPYGSMRKAPGSDPFMSSGQGPNGGMGDPYSRAAGPGLGNVAMGPRQHYPYGGPYDRVRTEPGIGPEGNMSTGAPQPNLMPSNPDSGMYSPSRYPPQQQQQQQQRHDSYGNQFSTQGTPSGSPFPSQQTTMYQQQQQNYKRPMDGTYGPPAKRHEGEMYSVPYSTGQGQPQQQQLPPAQPQPASQQQAAQPSPQQDVYNQYGNAYPATATAATERRPAGGPQNQFPFQFGRDRVSAPPGTNAQQNMPPQMMGGPIQASAEVAQQGTMWQGRNDMTYNYANRQSTGSAPQGPAYHGVNRTDEMLHTDQRANHEGSWPSHGTRQPPYGPSAPVPPMTRPPPSNYQPPPSMQNHIPQVSSPAPLPRPMENRTSPSKSPFLHSGMKMQKAGPPVPASHIAPAPVQPPMIRRDITFPPGSVEATQPVLKQRRRLTMKDIGTPEAWRVMMSLKSGLLAESTWALDTINILLYDDNSIMTFNLSQLPGLLELLVEYFRRCLIEIFGILKEYEVGDPGQRTLLDPGRFSKVSSPAPMEGGEEEEELLGPKLEEEEEEEVVENDEEIAFSGKDKPASENSEEKLISKFDKLPVKIVQKNDPFVVDCSDKLGRVQEFDSGLLHWRIGGGDTTEHIQTHFESKTELLPSRPHAPCPPAPRKHVTTAEGTPGTTDQEGPPPDGPPEKRITATMDDMLSTRSSTLTEDGAKSSEAIKESSKFPFGISPAQSHRNIKILEDEPHSKDETPLCTLLDWQDSLAKRCVCVSNTIRSLSFVPGNDFEMSKHPGLLLILGKLILLHHKHPERKQAPLTYEKEEEQDQGVSCNKVEWWWDCLEMLRENTLVTLANISGQLDLSPYPESICLPVLDGLLHWAVCPSAEAQDPFSTLGPNAVLSPQRLVLETLSKLSIQDNNVDLILATPPFSRLEKLYSTMVRFLSDRKNPVCREMAVVLLANLAQGDSLAARAIAVQKGSIGNLLGFLEDSLAATQFQQSQASLLHMQNPPFEPTSVDMMRRAARALLALAKVDENHSEFTLYESRLLDISVSPLMNSLVSQVICDVLFLIGQS");

            Assert.Equal(expectedStart,         observedResult.Start);
            Assert.Equal(expectedRefAminoAcids, observedResult.RefAminoAcids);
            Assert.Equal(expectedAltAminoAcids, observedResult.AltAminoAcids);
        }

        [Fact]
        public void Rotate3Prime_Identity_Deletion()
        {
            const int expectedStart            = 530;
            const string expectedRefAminoAcids = "A";
            const string expectedAltAminoAcids = "";

            var observedResult = HgvsUtilities.Rotate3Prime("A", "", 529,
                "MEAAAGGRGCFQPHPGLQKTLEQFHLSSMSSLGGPAAFSARWAQEAYKKESAKEAGAAAVPAPVPAATEPPPVLHLPAIQPPPPVLPGPFFMPSDRSTERCETVLEGETISCFVVGGEKRLCLPQILNSVLRDFSLQQINAVCDELHIYCSRCTADQLEILKVMGILPFSAPSCGLITKTDAERLCNALLYGGAYPPPCKKELAASLALGLELSERSVRVYHECFGKCKGLLVPELYSSPSAACIQCLDCRLMYPPHKFVVHSHKALENRTCHWGFDSANWRAYILLSQDYTGKEEQARLGRCLDDVKEKFDYGNKYKRRVPRVSSEPPASIRPKTDDTSSQSPAPSEKDKPSSWLRTLAGSSNKSLGCVHPRQRLSAFRPWSPAVSASEKELSPHLPALIRDSFYSYKSFETAVAPNVALAPPAQQKVVSSPPCAAAVSRAPEPLATCTQPRKRKLTVDTPGAPETLAPVAAPEEDKDSEAEVEVESREEFTSSLSSLSSPSFTSSSSAKDLGSPGARALPSAVPDAAAPADAPSGLEAELEHLRQALEGGLDTKEAKEKFLHEVVKMRVKQEEKLSAALQAKRSLHQELEFLRVAKKEKLREATEAKRNLRKEIERLRAENEKKMKEANESRLRLKRELEQARQARVCDKGCEAGRLRAKYSAQIEDLQVKLQHAEADREQLRADLLREREAREHLEKVVKELQEQLWPRARPEAAGSEGAAELEP");

            Assert.Equal(expectedStart,         observedResult.Start);
            Assert.Equal(expectedRefAminoAcids, observedResult.RefAminoAcids);
            Assert.Equal(expectedAltAminoAcids, observedResult.AltAminoAcids);
        }

        [Fact]
        public void Rotate3Prime_Identity_WithNullAminoAcids()
        {
            const int expectedStart            = 55;
            const string expectedRefAminoAcids = "Q";
            const string expectedAltAminoAcids = "*";

            var observedResult = HgvsUtilities.Rotate3Prime(expectedRefAminoAcids, expectedAltAminoAcids, expectedStart,
                "MGWDLTVKMLAGNEFQVSLSSSMSVSELKAQITQKIGVHAFQQRLAVHPSGVALQDRVPLASQGLGPGSTVLLVVDKCDEPLSILVRNNKGRSSTYEVRLTQTVAHLKQQVSGLEGVQDDLFWLTFEGKPLEDQLPLGEYGLKPLSTVFMNLRLRGGGTEPGGRS");

            Assert.Equal(expectedStart,         observedResult.Start);
            Assert.Equal(expectedRefAminoAcids, observedResult.RefAminoAcids);
            Assert.Equal(expectedAltAminoAcids, observedResult.AltAminoAcids);
        }

        [Fact]
        public void IsAminoAcidDuplicate_True()
        {
            var observedResult = HgvsUtilities.IsAminoAcidDuplicate(85, "P",
                "MEAAAGGRGCFQPHPGLQKTLEQFHLSSMSSLGGPAAFSARWAQEAYKKESAKEAGAAAVPAPVPAATEPPPVLHLPAIQPPPPVLPGPFFMPSDRSTERCETVLEGETISCFVVGGEKRLCLPQILNSVLRDFSLQQINAVCDELHIYCSRCTADQLEILKVMGILPFSAPSCGLITKTDAERLCNALLYGGAYPPPCKKELAASLALGLELSERSVRVYHECFGKCKGLLVPELYSSPSAACIQCLDCRLMYPPHKFVVHSHKALENRTCHWGFDSANWRAYILLSQDYTGKEEQARLGRCLDDVKEKFDYGNKYKRRVPRVSSEPPASIRPKTDDTSSQSPAPSEKDKPSSWLRTLAGSSNKSLGCVHPRQRLSAFRPWSPAVSASEKELSPHLPALIRDSFYSYKSFETAVAPNVALAPPAQQKVVSSPPCAAAVSRAPEPLATCTQPRKRKLTVDTPGAPETLAPVAAPEEDKDSEAEVEVESREEFTSSLSSLSSPSFTSSSSAKDLGSPGARALPSAVPDAAAPADAPSGLEAELEHLRQALEGGLDTKEAKEKFLHEVVKMRVKQEEKLSAALQAKRSLHQELEFLRVAKKEKLREATEAKRNLRKEIERLRAENEKKMKEANESRLRLKRELEQARQARVCDKGCEAGRLRAKYSAQIEDLQVKLQHAEADREQLRADLLREREAREHLEKVVKELQEQLWPRARPEAAGSEGAAELEP");
            Assert.True(observedResult);
        }

        [Fact]
        public void IsAminoAcidDuplicate_False()
        {
            var observedResult = HgvsUtilities.IsAminoAcidDuplicate(307, "*RX",
                "MHYDGHVRFDLPPQGSVLARNVSTRSCPPRTSPAVDLEEEEEESSVDGKGDRKSTGLKLSKKKARRRHTDDPSKECFTLKFDLNVDIETEIVPAMKKKSLGEVLLPVFERKGIALGKVDIYLDQSNTPLSLTFEAYRFGGHYLRVKAPAKPGDEGKVEQGMKDSKSLSLPILRPAGTGPPALERVDAQSRRESLDILAPGRRRKNMSEFLGEASIPGQEPPTPSSCSLPSGSSGSTNTGDSWKNRAASRFSGFFSSGPSTSAFGREVDKMEQLEGKLHTYSLFGLPRLPRGLRFDHDSWEEEYDEDEDEDNACLRLEDSWRELIDGHEKLTRRQCHQQEAVWELLHTEASYIRKLRVIINLFLCCLLNLQESGLLCEVEAERLFSNIPEIAQLHRRLWASVMAPVLEKARRTRALLQPGDFLKGFKMFGSLFKPYIRYCMEEEGCMEYMRGLLRDNDLFRAYITWAEKHPQCQRLKLSDMLAKPHQRLTKYPLLLKSVLRKTEEPRAKEAVVAMIGSVERFIHHVNACMRQRQERQRLAAVVSRIDAYEVVESSSDEVDKLLKEFLHLDLTAPIPGASPEETRQLLLEGSLRMKEGKDSKMDVYCFLFTDLLLVTKAVKKAERTRVIRPPLLVDKIVCRELRDPGSFLLIYLNEFHSAVGAYTFQASGQALCRGWVDTIYNAQNQLQQLRAQEPPGSQQPLQSLEEEEDEQEEEEEEEEEEEEGEDSGTSAASSPTIMRKSSGSPDSQHCASDGSTETLAMVVVEPGDTLSSPEFDSGPFSSQSDETSLSTTASSATPTSELLPLGPVDGRSCSMDSAYGTLSPTSLQDFVAPGPMAELVPRAPESPRVPSPPPSPRLRRRTPVQLLSCPPHLLKSKSEASLLQLLAGAGTHGTPSAPSRSLSELCLAVPAPGIRTQGSPQEAGPSWDCRGAPSPGSGPGLVGCLAGEPAGSHRKRCGDLPSGASPRVQPEPPPGVSAQHRKLTLAQLYRIRTTLLLNSTLTASEV");
            Assert.False(observedResult);
        }

        [Fact]
        public void IsAminoAcidDuplicate_False_WhenAminoAcidsNull()
        {
            var observedResult = HgvsUtilities.IsAminoAcidDuplicate(307, null, null);
            Assert.False(observedResult);
        }

        [Fact]
        public void IsAminoAcidDuplicate_False_StartEqualToAminoAcidLength()
        {
            var observedResult = HgvsUtilities.IsAminoAcidDuplicate(3, "ABC", "DEF");
            Assert.False(observedResult);
        }

        [Fact]
        public void GetNumAminoAcidsUntilStopCodon_FirstAminoAcidIsStop()
        {
            const int expectedResult = -1;
            var observedResult =
                HgvsUtilities.GetNumAminoAcidsUntilStopCodon(
                    "RHRNRNTQTETNTETQRHRNTQKHRNKHRDTETHRNTETNTETQKHTETQKQTQRHRNTQKHTDRNKHRNTETQKYRNTQKHRNKHRDTETQKHSDAETQQHKHRNTETHRNRNTETNTETQTHRHRETQKHTETLKHSGRCPGCRGSIA",
                    "RHRNRNTQTETNTETQRHRNTQKHRNKHRDTETHRNTQKHRNKHRDTETHRNTETNTETQKHTETHRQKQTQKHRDTEIQKHTETQKQTQRHRDTETQRRRNTATQTQKHRNTQKQKHRNKHRDTDTQTQRNTETHRNTETQWAVSRLQRLHRC",
                    37, true);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetNumAminoAcidsUntilStopCodon_FoundExtraAminoAcids()
        {
            const int expectedResult = 38;
            var observedResult =
                HgvsUtilities.GetNumAminoAcidsUntilStopCodon(
                    "MLAEPFNWHVEYAHSGDVLGPSGLPASPGAPGTCLHNPAGSNWGPG*EVLMAGTVPAVPG*SGEGSQF*LPWSCSDSPQAGSRAHGQGPGIPLLPQGHGTQSLCRAQGSVPAAEPVPPTEDGRGLSGPEQGHRGTAPARRPPGGWQDLLLLSATLRL*RHCPQHQQ*LPARPGRLAAGCP*ETLWP*PLLSPAAGHRTRDLFPGGGDVPGGPLLRQAGCGADPAFPRPEPQGFGRPLEPAPCGAGGDHHERDRGC*RPHQLL*GVRCHSRRPPEPSDGGPHPRGHGAAPQCQQCGGCAAAQASGLPGAAGPAEGQCRRGPVPVLQ*AGAQRAAEARQLPQPDADLRSRPSAH*QPSLGGRAFHPDVWQSLGRESGLRSDLVQEPGLLCAERKALGRGAEPVPAPAARLPHRPWRPGQPCRAGQQEPVQALPALQLEGNGGTTWAPPFRQPSVRLLRLQPCAGAGRPLRPLIPYLPWPEEFLHHHRELAGLLELLDPSAGEPGP*GPTPLPWRS*EWPSVGL*VQ*RPVVLFPAAAGAAGARARAGPNAQ*LPGPQGQVPREPAGLRLVRGADL*AG**HRGHRCASRAALWPVPPGTVGGLEPRGPVPAAGHGALWLPLGPHAPVAG*RALRPTLRPGVQLPGPAGPPAAARPDPLLQHPPHACAPAAAALRRGGPGRPDLCQGDLSPGGQQQLRPGAAGHGCRRAHSLPLPTVTHWPGWRAAGRADHEPLPATPPHEP*PASHQPRQEGGSPGHGQDEA*DHHAGEPGGP*AQEVAHLGCPAALRPAGVVHGLRRLPGMRAPVPLARFAPVLSFARVFPPFSAPPPAQRALALQNLLSHSQAPERAGQALSRCL*PAALCIGG*MQKQGRNGVCS*EASNSGQERSLKKRPPAVTHSYQPAQHGMAPKLRRSQEET*RGGLRLIREGFLGEVILELAPGEHSEHDW*TEGCRGAQGSTLPRAKQGHWGLS*DPEGVKPLLPQLPLLLEPLHI*PLALLFTASTCSRLPSLSPPSWLCSRNSRLLPVSLLFFRLHL*RMRADNRNTVAKTRLWKGFQKSFFFFN*KKYLQR*ALAMLPRLVSNSWAQAILPSQPPRVLGLQV*ATAPSPRNLSAVWSSISHLMTCSAWGGGVSFPQLPQGGPLPSAAPLSC*PSSRKHTGCR*SGHSRDPQFKRVISISGDSRMGVSALNSPSCFTRKDPVKSPTEVTAH*RGERWSIE*HWAIQAALLPPDRS*ASLAGGLPTAFSGARLAGDGAAARPSLPAPW*PRGFLSAGLSCYLSLHHELSA*DWGSKRVSSQ*A*VGDCDLEKPWASNTCFSEAPKEGSDILFKNTTKQNSQDMCSFVCSVSHNLRLGDGTLG*GRFFCLASPHLPLALWIRQI*TF*RILREGFLG*GSMAKSVSLWTVYTSRRWI*RNPGFHFQCQSETCSQAGALVHTTYSGHQQQPRPDRASLFFFFETESLSPRLEPSGEILAHYNLHLPGSGNSRA*ASRVAATTGAGQHACLIFVF**RQGFTMLPRLVSNS*AQAVYPPQPPKVLGLQA*ATAPSQNICFYTQRAPLVRTEPRCPEPGSRPPGAQHLSFYT*WAGSGEDRESWWKFHSWPRGGALAPHCRLLTAPIPAAPVPDFISLLSPRVPGPSTLPSVLQEPTPLQLQHQGERGLHMPKYPCRMKGRPALDVPFLNNSHCRRV*DVLF*LSPASDAPPICAEWVWECG*GSKCQRSTFQNQVPSANHVGKVQTWRCPCASAPTHPFSFSCVRKEKFSEPSRLVAFKLQTMICSKKRAFHHKSVHLFTTVFQAGFIKKFLTLE",
                    "MLAEPFNWHPGMWNMLIVAMCLALLGCLQAQELQGHVSIILLGATGDLAKKYLWQGLFQLYLDEAGRGHSFSFHGAALTAPKQGQELMAKALESLSCPKDMAPSHCAEHKDQFLQLSQYRQLKTAEDYQALNKDIEAQLQHAGLREAGRIFYFSVPPFAYEDIARNINSSCRPGPGAWLRVVLEKPFGHDHFSAQQLATELGTFFQEEEMYRVDHYLGKQAVAQILPFRDQNRKALDGLWNRHHVERVEIIMKETVDAEGRTSFYEEYGVIRDVLQNHLTEVLTLVAMELPHNVSSAEAVLRHKLQVFQALRGLQRGSAVVGQYQSYSEQVRRELQKPDSFHSLTPTFAAVLVHIDNLRWEGVPFILMSGKALDERVGYARILFKNQACCVQSEKHWAAAQSQCLPRQLVFHIGHGDLGSPAVLVSRNLFRPSLPSSWKEMEGPPGLRLFGSPLSDYYAYSPVRERDAHSVLLSHIFHGRKNFFITTENLLASWNFWTPLLESLAHKAPRLYPGGAENGRLLDFEFSSGRLFFSQQQPEQLVPGPGPAPMPSDFQVLRAKYRESPLVSAWSEELISKLANDIEATAVRAVRRFGQFHLALSGGSSPVALFQQLATAHYGFPWAHTHLWLVDERCVPLSDPESNFQGLQAHLLQHVRIPYYNIHPMPVHLQQRLCAEEDQGAQIYAREISALVANSSFDLVLLGMGADGHTASLFPQSPTGLDGEQLVVLTTSPSQPHRRMSLSLPLINRAKKVAVLVMGRMKREITTLVSRVGHEPKKWPISGVLPHSGQLVWYMDYDAFLG",
                    9, true);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetChangesAfterFrameshift_AfterFrameshift()
        {
            var observedResult = HgvsUtilities.GetChangesAfterFrameshift(4, "MABCDEFGHIIIKL", "MABCEFGH*");

            Assert.Equal(5,   observedResult.Start);
            Assert.Equal('D', observedResult.RefAminoAcid);
            Assert.Equal('E', observedResult.AltAminoAcid);
        }

        [Fact]
        public void GetChangesAfterFrameshift_AtEndAfterFrameshift()
        {
            var observedResult = HgvsUtilities.GetChangesAfterFrameshift(4, "MABCDEFGHIIIKL", "MABCDEFGHIIIKLL*");

            Assert.Equal(15,  observedResult.Start);
            Assert.Equal('*', observedResult.RefAminoAcid);
            Assert.Equal('L', observedResult.AltAminoAcid);
        }

        [Fact]
        public void GetChangesAfterFrameshift_WhenStopRetained()
        {
            var observedResult = HgvsUtilities.GetChangesAfterFrameshift(4, "MABCDEFGHIIIKL", "MABCDEFGHIIIKL*");

            Assert.Equal(15,  observedResult.Start);
            Assert.Equal('*', observedResult.RefAminoAcid);
            Assert.Equal('*', observedResult.AltAminoAcid);
        }

        [Fact]
        public void GetChangesAfterFrameshift_FirstAminoAcidIsStop()
        {
            var observedResult = HgvsUtilities.GetChangesAfterFrameshift(4, "MABCDEFGHIIIKL", "MABCDEFGHIIIKL*");

            Assert.Equal(15,  observedResult.Start);
            Assert.Equal('*', observedResult.RefAminoAcid);
            Assert.Equal('*', observedResult.AltAminoAcid);
        }

        [Fact]
        public void GetAltPeptideSequence_Genomic()
        {
            var refSequence                  = GetGenomicRefSequence();
            var transcript                   = GetGenomicTranscript();
            const int cdsBegin               = 112;
            const int cdsEnd                 = 121;
            const string transcriptAltAllele = "";

            const string expectedResult = "RHRNRNTQTETNTETQRHRNTQKHRNKHRDTETHRNTETNTETQKHTETQKQTQRHRNTQKHTDRNKHRNTETQKYRNTQKHRNKHRDTETQKHSDAETQQHKHRNTETHRNRNTETNTETQTHRHRETQKHTETLKHSGRCPGCRGSIA";
            var observedResult =
                HgvsUtilities.GetAltPeptideSequence(refSequence, cdsBegin, cdsEnd, transcriptAltAllele, transcript,
                    false);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_RltL_Reverse()
        {
            var transcript = HgvsCodingNomenclatureTests.GetReverseTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 137619, 1);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(2, po.Offset);
            Assert.Equal(1759, po.Position);
            Assert.Equal("*909+2", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_ReqL_Reverse()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Intron, 10, 108901173, 108918171, 422, 423)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(108813927, 108941437, 129, 1613, 1485));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(108810721);
            transcript.SetupGet(x => x.End).Returns(108918171);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);

            var po = HgvsUtilities.GetCdnaPositionOffset(transcript.Object, 108909672, 0);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(8500, po.Offset);
            Assert.Equal(422, po.Position);
            Assert.Equal("294+8500", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_LltR_Reverse()
        {
            var transcript = HgvsCodingNomenclatureTests.GetReverseTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 136000, 1);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(-198, po.Offset);
            Assert.Equal(1760, po.Position);
            Assert.Equal("*910-198", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_LeqR_Reverse()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 134901, 135802, 1760, 2661),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 135803, 137619, 1759, 1760),
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 137620, 139379, 1, 1759)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(138530, 139309, 71, 850, 780));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(134901);
            transcript.SetupGet(x => x.End).Returns(139379);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);

            var po = HgvsUtilities.GetCdnaPositionOffset(transcript.Object, 136711, 1);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(909, po.Offset);
            Assert.Equal(1759, po.Position);
            Assert.Equal("*909+909", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Gap_LeftSide_Forward()
        {
            var transcript = GetForwardGapTranscript();
            var po         = HgvsUtilities.GetCdnaPositionOffset(transcript, 1101, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(100, po.Position);
            Assert.Equal("50", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Gap_RightSide_Forward()
        {
            var transcript = GetForwardGapTranscript();
            var po         = HgvsUtilities.GetCdnaPositionOffset(transcript, 1102, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(101, po.Position);
            Assert.Equal("51", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Gap_LeftSide_Reverse()
        {
            var transcript = GetReverseGapTranscript();
            var po         = HgvsUtilities.GetCdnaPositionOffset(transcript, 1102, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(201, po.Position);
            Assert.Equal("151", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Gap_RightSide_Reverse()
        {
            var transcript = GetReverseGapTranscript();
            var po         = HgvsUtilities.GetCdnaPositionOffset(transcript, 1103, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(200, po.Position);
            Assert.Equal("150", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_RltL_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 1262210, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(-6, po.Offset);
            Assert.Equal(337, po.Position);
            Assert.Equal("-75-6", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_LltR_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 1260583, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(101, po.Offset);
            Assert.Equal(336, po.Position);
            Assert.Equal("-76+101", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Intron_LeqR_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 1261349, 1);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(867, po.Offset);
            Assert.Equal(336, po.Position);
            Assert.Equal("-76+867", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Exon_Forward()
        {
            var transcript = HgvsCodingNomenclatureTests.GetForwardTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 1262627, 4);

            Assert.NotNull(po);
            Assert.False(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(540, po.Position);
            Assert.Equal("129", po.Value);
        }

        [Fact]
        public void GetCdnaPositionOffset_Exon_Reverse()
        {
            var transcript = HgvsCodingNomenclatureTests.GetReverseTranscript();
            var po = HgvsUtilities.GetCdnaPositionOffset(transcript, 137721, 2);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(1659, po.Position);
            Assert.Equal("*809", po.Value);
        }
        //temp skipping to run smoke tests
        //[Fact]
        //public void GetCdnaPositionOffset_RnaEdits()
        //{
        //    var transcript = GetRnaEditTranscript();
        //    var positionOffset = HgvsUtilities.GetCdnaPositionOffset(transcript, 51135987, 20);

        //    Assert.NotNull(positionOffset);
        //    Assert.False(positionOffset.HasStopCodonNotation);
        //    Assert.Equal(0, positionOffset.Offset);
        //    Assert.Equal(1343, positionOffset.Position);
        //    Assert.Equal("1343", positionOffset.Value);
        //}

        [Fact]
        public void GetCdnaPositionOffset_Gap_Forward_ReturnNull()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Gap, 1, 134901, 135802, 1760, 2661)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(138530, 139309, 71, 850, 780));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(134901);
            transcript.SetupGet(x => x.End).Returns(139379);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);

            var po = HgvsUtilities.GetCdnaPositionOffset(transcript.Object, 135001, 0);

            Assert.NotNull(po);
            Assert.True(po.HasStopCodonNotation);
            Assert.Equal(0, po.Offset);
            Assert.Equal(1760, po.Position);
            Assert.Equal("*910", po.Value);
        }

        private static ISequence GetGenomicRefSequence()
        {
            return new SimpleSequence(
                "AGACACAGAAACAGAAACACACAGACAGAAACAAACACAGAGACACAGAGACACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAAACACACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAAACACACAGAAACACACAGACAGAAACAAACACAGAAACACAGAGACACAGAAATACAGAAACACACAGAAACACAGAAACAAACACAGAGACACAGAGACACAGAAACACAGCGACGCAGAAACACAGCAACACAAACACAGAAACACAGAAACACACAGAAACAGAAACACAGAAACAAACACAGAGACACAGACACACAGACACAGAGAAACACAGAAACACACAGAAACACTGAAACACAGTGGGCGGTGTCCAGGCTGCAGAGGCTCCATCGCTGT",
                2258580);
        }

        private static ITranscript GetGenomicTranscript()
        {
            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 2258581, 2259042, 1, 462)
            };

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.TranscriptRegions).Returns(transcriptRegions);
            transcript.SetupGet(x => x.StartExonPhase).Returns(0);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.Translation.CodingRegion.CdnaStart).Returns(1);
            return transcript.Object;
        }

        private static ITranscript GetReverseGapTranscript()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 1001, 1100, 201, 300),
                new TranscriptRegion(TranscriptRegionType.Gap,    1, 1101, 1103, 200, 201),
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 1104, 1203, 101, 200),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 1204, 1303, 100, 101),
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 1304, 1403, 1, 100)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(1051, 1353, 51, 250, 200));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(1001);
            transcript.SetupGet(x => x.End).Returns(1403);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);

            return transcript.Object;
        }

        private static ITranscript GetForwardGapTranscript()
        {
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 1001, 1100, 1, 100),
                new TranscriptRegion(TranscriptRegionType.Gap,    1, 1101, 1103, 100, 101),
                new TranscriptRegion(TranscriptRegionType.Exon,   1, 1104, 1203, 101, 200),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 1204, 1303, 200, 201),
                new TranscriptRegion(TranscriptRegionType.Exon,   2, 1304, 1403, 201, 300)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(1051, 1353, 51, 250, 200));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(1001);
            transcript.SetupGet(x => x.End).Returns(1403);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);

            return transcript.Object;
        }

        private static ITranscript GetRnaEditTranscript()
        {
            // NM_033517.1, SHANK3
            var regions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 51113070, 51113132, 1, 63),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 51113133, 51113475, 63, 64),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 51113476, 51113679, 64, 267),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 51113680, 51115049, 267, 268),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 51115050, 51115121, 268, 339),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 51115122, 51117012, 339, 340),
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 51117013, 51117121, 340, 448),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 51117122, 51117196, 448, 449),
                new TranscriptRegion(TranscriptRegionType.Exon, 5, 51117197, 51117348, 449, 600),
                new TranscriptRegion(TranscriptRegionType.Intron, 5, 51117349, 51117446, 600, 601),
                new TranscriptRegion(TranscriptRegionType.Exon, 6, 51117447, 51117614, 601, 768),
                new TranscriptRegion(TranscriptRegionType.Intron, 6, 51117615, 51117739, 768, 769),
                new TranscriptRegion(TranscriptRegionType.Exon, 7, 51117740, 51117856, 769, 885),
                new TranscriptRegion(TranscriptRegionType.Intron, 7, 51117857, 51121767, 885, 886),
                new TranscriptRegion(TranscriptRegionType.Exon, 8, 51121768, 51121845, 886, 963),
                new TranscriptRegion(TranscriptRegionType.Intron, 8, 51121846, 51123012, 963, 964),
                new TranscriptRegion(TranscriptRegionType.Exon, 9, 51123013, 51123079, 964, 1030),
                new TranscriptRegion(TranscriptRegionType.Intron, 9, 51123080, 51133202, 1030, 1031),
                new TranscriptRegion(TranscriptRegionType.Exon, 10, 51133203, 51133474, 1031, 1302),
                new TranscriptRegion(TranscriptRegionType.Intron, 10, 51133475, 51135984, 1302, 1342),
                new TranscriptRegion(TranscriptRegionType.Exon, 11, 51135985, 51135989, 1342, 1346),
                new TranscriptRegion(TranscriptRegionType.Gap, 11, 51135990, 51135991, 1346, 1347),
                new TranscriptRegion(TranscriptRegionType.Exon, 11, 51135992, 51136143, 1347, 1498),
                new TranscriptRegion(TranscriptRegionType.Intron, 11, 51136144, 51137117, 1498, 1499),
                new TranscriptRegion(TranscriptRegionType.Exon, 12, 51137118, 51137231, 1499, 1612),
                new TranscriptRegion(TranscriptRegionType.Intron, 12, 51137232, 51142287, 1612, 1613),
                new TranscriptRegion(TranscriptRegionType.Exon, 13, 51142288, 51142363, 1613, 1688),
                new TranscriptRegion(TranscriptRegionType.Intron, 13, 51142364, 51142593, 1688, 1689),
                new TranscriptRegion(TranscriptRegionType.Exon, 14, 51142594, 51142676, 1689, 1771),
                new TranscriptRegion(TranscriptRegionType.Intron, 14, 51142677, 51143165, 1771, 1772),
                new TranscriptRegion(TranscriptRegionType.Exon, 15, 51143166, 51143290, 1772, 1896),
                new TranscriptRegion(TranscriptRegionType.Intron, 15, 51143291, 51143391, 1896, 1897),
                new TranscriptRegion(TranscriptRegionType.Exon, 16, 51143392, 51143524, 1897, 2029),
                new TranscriptRegion(TranscriptRegionType.Intron, 16, 51143525, 51144499, 2029, 2030),
                new TranscriptRegion(TranscriptRegionType.Exon, 17, 51144500, 51144580, 2030, 2110),
                new TranscriptRegion(TranscriptRegionType.Intron, 17, 51144581, 51150042, 2110, 2111),
                new TranscriptRegion(TranscriptRegionType.Exon, 18, 51150043, 51150066, 2111, 2134),
                new TranscriptRegion(TranscriptRegionType.Intron, 18, 51150067, 51153344, 2134, 2135),
                new TranscriptRegion(TranscriptRegionType.Exon, 19, 51153345, 51153475, 2135, 2265),
                new TranscriptRegion(TranscriptRegionType.Intron, 19, 51153476, 51154096, 2265, 2266),
                new TranscriptRegion(TranscriptRegionType.Exon, 20, 51154097, 51154181, 2266, 2350),
                new TranscriptRegion(TranscriptRegionType.Intron, 20, 51154182, 51158611, 2350, 2351),
                new TranscriptRegion(TranscriptRegionType.Exon, 21, 51158612, 51160865, 2351, 4604),
                new TranscriptRegion(TranscriptRegionType.Intron, 21, 51160866, 51169148, 4604, 4605),
                new TranscriptRegion(TranscriptRegionType.Exon, 22, 51169149, 51171640, 4605, 7096)
            };

            var edits = new IRnaEdit[]
            {
                new RnaEdit(1303, 1302, "AGCCCGAGCGGGCCCGGCGGCCCCGGCCCCGCGCCCGGC"),
                new RnaEdit(1304, 1304, "C"),
                new RnaEdit(1308, 1309, ""),
                new RnaEdit(7060, 7059, "AAAAAAAAAAAAAAAAA")
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(51113070, 51169740, 1, 5196, 5157));

            var transcript = new Mock<ITranscript>();
            transcript.SetupGet(x => x.Start).Returns(51113070);
            transcript.SetupGet(x => x.End).Returns(51169740);
            transcript.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            transcript.SetupGet(x => x.TranscriptRegions).Returns(regions);
            transcript.SetupGet(x => x.RnaEdits).Returns(edits);
            transcript.SetupGet(x => x.Translation).Returns(translation.Object);

            return transcript.Object;
        }
    }
}
