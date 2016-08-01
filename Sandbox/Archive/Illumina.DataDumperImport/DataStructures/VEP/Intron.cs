namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class Intron
    {
        public int Start;
        public int End;

        public Slice Slice = null;

        /// <summary>
        /// converts the current VEP exon into a Nirvana exon 
        /// </summary>
        public VariantAnnotation.DataStructures.Intron Convert()
        {
            return new VariantAnnotation.DataStructures.Intron(Start, End);
        }
    }
}
