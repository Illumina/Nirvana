namespace VariantAnnotation.AnnotatedPositions.AminoAcids
{
    public static class AminoAcidCommon
    {
        public const char StopCodon = '*';

        public static readonly AminoAcid StandardAminoAcids;
        public static readonly AminoAcid MitochondrialAminoAcids;

        static AminoAcidCommon()
        {
            AminoAcidEntry[] standardEntries =
            {
                new AminoAcidEntry(4276545, 'K'), // AAA
                new AminoAcidEntry(4276547, 'N'), // AAC
                new AminoAcidEntry(4276551, 'K'), // AAG
                new AminoAcidEntry(4276564, 'N'), // AAT
                new AminoAcidEntry(4277057, 'T'), // ACA
                new AminoAcidEntry(4277059, 'T'), // ACC
                new AminoAcidEntry(4277063, 'T'), // ACG
                new AminoAcidEntry(4277076, 'T'), // ACT
                new AminoAcidEntry(4278081, 'R'), // AGA
                new AminoAcidEntry(4278083, 'S'), // AGC
                new AminoAcidEntry(4278087, 'R'), // AGG
                new AminoAcidEntry(4278100, 'S'), // AGT
                new AminoAcidEntry(4281409, 'I'), // ATA
                new AminoAcidEntry(4281411, 'I'), // ATC
                new AminoAcidEntry(4281415, 'M'), // ATG
                new AminoAcidEntry(4281428, 'I'), // ATT
                new AminoAcidEntry(4407617, 'Q'), // CAA
                new AminoAcidEntry(4407619, 'H'), // CAC
                new AminoAcidEntry(4407623, 'Q'), // CAG
                new AminoAcidEntry(4407636, 'H'), // CAT
                new AminoAcidEntry(4408129, 'P'), // CCA
                new AminoAcidEntry(4408131, 'P'), // CCC
                new AminoAcidEntry(4408135, 'P'), // CCG
                new AminoAcidEntry(4408148, 'P'), // CCT
                new AminoAcidEntry(4409153, 'R'), // CGA
                new AminoAcidEntry(4409155, 'R'), // CGC
                new AminoAcidEntry(4409159, 'R'), // CGG
                new AminoAcidEntry(4409172, 'R'), // CGT
                new AminoAcidEntry(4412481, 'L'), // CTA
                new AminoAcidEntry(4412483, 'L'), // CTC
                new AminoAcidEntry(4412487, 'L'), // CTG
                new AminoAcidEntry(4412500, 'L'), // CTT
                new AminoAcidEntry(4669761, 'E'), // GAA
                new AminoAcidEntry(4669763, 'D'), // GAC
                new AminoAcidEntry(4669767, 'E'), // GAG
                new AminoAcidEntry(4669780, 'D'), // GAT
                new AminoAcidEntry(4670273, 'A'), // GCA
                new AminoAcidEntry(4670275, 'A'), // GCC
                new AminoAcidEntry(4670279, 'A'), // GCG
                new AminoAcidEntry(4670292, 'A'), // GCT
                new AminoAcidEntry(4671297, 'G'), // GGA
                new AminoAcidEntry(4671299, 'G'), // GGC
                new AminoAcidEntry(4671303, 'G'), // GGG
                new AminoAcidEntry(4671316, 'G'), // GGT
                new AminoAcidEntry(4674625, 'V'), // GTA
                new AminoAcidEntry(4674627, 'V'), // GTC
                new AminoAcidEntry(4674631, 'V'), // GTG
                new AminoAcidEntry(4674644, 'V'), // GTT
                new AminoAcidEntry(5521729, '*'), // TAA
                new AminoAcidEntry(5521731, 'Y'), // TAC
                new AminoAcidEntry(5521735, '*'), // TAG
                new AminoAcidEntry(5521748, 'Y'), // TAT
                new AminoAcidEntry(5522241, 'S'), // TCA
                new AminoAcidEntry(5522243, 'S'), // TCC
                new AminoAcidEntry(5522247, 'S'), // TCG
                new AminoAcidEntry(5522260, 'S'), // TCT
                new AminoAcidEntry(5523265, '*'), // TGA
                new AminoAcidEntry(5523267, 'C'), // TGC
                new AminoAcidEntry(5523271, 'W'), // TGG
                new AminoAcidEntry(5523284, 'C'), // TGT
                new AminoAcidEntry(5526593, 'L'), // TTA
                new AminoAcidEntry(5526595, 'F'), // TTC
                new AminoAcidEntry(5526599, 'L'), // TTG
                new AminoAcidEntry(5526612, 'F')  // TTT
            };

            StandardAminoAcids = new AminoAcid(standardEntries);

            AminoAcidEntry[] mitochondrialEntries =
            {
                new AminoAcidEntry(4276545, 'K'), // AAA
                new AminoAcidEntry(4276547, 'N'), // AAC
                new AminoAcidEntry(4276551, 'K'), // AAG
                new AminoAcidEntry(4276564, 'N'), // AAT
                new AminoAcidEntry(4277057, 'T'), // ACA
                new AminoAcidEntry(4277059, 'T'), // ACC
                new AminoAcidEntry(4277063, 'T'), // ACG
                new AminoAcidEntry(4277076, 'T'), // ACT
                new AminoAcidEntry(4278081, '*'), // AGA - R to *
                new AminoAcidEntry(4278083, 'S'), // AGC
                new AminoAcidEntry(4278087, '*'), // AGG - R to *
                new AminoAcidEntry(4278100, 'S'), // AGT
                new AminoAcidEntry(4281409, 'M'), // ATA - I to M
                new AminoAcidEntry(4281411, 'I'), // ATC
                new AminoAcidEntry(4281415, 'M'), // ATG
                new AminoAcidEntry(4281428, 'I'), // ATT
                new AminoAcidEntry(4407617, 'Q'), // CAA
                new AminoAcidEntry(4407619, 'H'), // CAC
                new AminoAcidEntry(4407623, 'Q'), // CAG
                new AminoAcidEntry(4407636, 'H'), // CAT
                new AminoAcidEntry(4408129, 'P'), // CCA
                new AminoAcidEntry(4408131, 'P'), // CCC
                new AminoAcidEntry(4408135, 'P'), // CCG
                new AminoAcidEntry(4408148, 'P'), // CCT
                new AminoAcidEntry(4409153, 'R'), // CGA
                new AminoAcidEntry(4409155, 'R'), // CGC
                new AminoAcidEntry(4409159, 'R'), // CGG
                new AminoAcidEntry(4409172, 'R'), // CGT
                new AminoAcidEntry(4412481, 'L'), // CTA
                new AminoAcidEntry(4412483, 'L'), // CTC
                new AminoAcidEntry(4412487, 'L'), // CTG
                new AminoAcidEntry(4412500, 'L'), // CTT
                new AminoAcidEntry(4669761, 'E'), // GAA
                new AminoAcidEntry(4669763, 'D'), // GAC
                new AminoAcidEntry(4669767, 'E'), // GAG
                new AminoAcidEntry(4669780, 'D'), // GAT
                new AminoAcidEntry(4670273, 'A'), // GCA
                new AminoAcidEntry(4670275, 'A'), // GCC
                new AminoAcidEntry(4670279, 'A'), // GCG
                new AminoAcidEntry(4670292, 'A'), // GCT
                new AminoAcidEntry(4671297, 'G'), // GGA
                new AminoAcidEntry(4671299, 'G'), // GGC
                new AminoAcidEntry(4671303, 'G'), // GGG
                new AminoAcidEntry(4671316, 'G'), // GGT
                new AminoAcidEntry(4674625, 'V'), // GTA
                new AminoAcidEntry(4674627, 'V'), // GTC
                new AminoAcidEntry(4674631, 'V'), // GTG
                new AminoAcidEntry(4674644, 'V'), // GTT
                new AminoAcidEntry(5521729, '*'), // TAA
                new AminoAcidEntry(5521731, 'Y'), // TAC
                new AminoAcidEntry(5521735, '*'), // TAG
                new AminoAcidEntry(5521748, 'Y'), // TAT
                new AminoAcidEntry(5522241, 'S'), // TCA
                new AminoAcidEntry(5522243, 'S'), // TCC
                new AminoAcidEntry(5522247, 'S'), // TCG
                new AminoAcidEntry(5522260, 'S'), // TCT
                new AminoAcidEntry(5523265, 'W'), // TGA - * to W
                new AminoAcidEntry(5523267, 'C'), // TGC
                new AminoAcidEntry(5523271, 'W'), // TGG
                new AminoAcidEntry(5523284, 'C'), // TGT
                new AminoAcidEntry(5526593, 'L'), // TTA
                new AminoAcidEntry(5526595, 'F'), // TTC
                new AminoAcidEntry(5526599, 'L'), // TTG
                new AminoAcidEntry(5526612, 'F')  // TTT
            };

            MitochondrialAminoAcids = new AminoAcid(mitochondrialEntries);
        }
    }
}