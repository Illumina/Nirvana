namespace SAUtils.MitoHeteroplasmy
{
    //{
    //"ad": [1],
    //"allele_type": "alt",
    //"vrf": [0.004273504273504274],
    //"vrf_stats": {
    //    "kurtosis": 241.00408163265314,
    //    "max": 0.0042735042735042739,
    //    "mean": 1.7371968591480788e-05,
    //    "min": 0.0,
    //    "nobs": 246,
    //    "skewness": 15.588588185998535,
    //    "stdev": 0.00027246868079629845,
    //    "variance": 7.4239182014875175e-08
    //}
    //}

    public sealed class PositionStats
    {
        public AlleleStats A_C;
        public AlleleStats A_G;
        public AlleleStats A_T;

        public AlleleStats C_A;
        public AlleleStats C_G;
        public AlleleStats C_T;

        public AlleleStats G_C;
        public AlleleStats G_A;
        public AlleleStats G_T;

        public AlleleStats T_C;
        public AlleleStats T_G;
        public AlleleStats T_A;

    }
    public class AlleleStats
    {
        public int[] ad;
        public double[] vrf;
        public VrfStats vrf_stats;
    }

    public class VrfStats
    {
        public double mean;
        public double stdev;
    }
}