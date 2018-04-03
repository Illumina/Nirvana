using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
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
    }
}
