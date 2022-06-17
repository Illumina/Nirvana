using System.Collections.Generic;

namespace SAUtils.ClinGen
{
    public static class Data
    {
        public static Dictionary<int, string> ScoreToDescription { get; } = new Dictionary<int, string>
        {
            {-1, "Not yet evaluated"},
            {0, "no evidence to suggest that dosage sensitivity is associated with clinical phenotype"},
            {1, "little evidence suggesting dosage sensitivity is associated with clinical phenotype"},
            {2, "emerging evidence suggesting dosage sensitivity is associated with clinical phenotype"},
            {3, "sufficient evidence suggesting dosage sensitivity is associated with clinical phenotype"},
            {30, "gene associated with autosomal recessive phenotype"},
            {40, "dosage sensitivity unlikely"}
        };
    }
}