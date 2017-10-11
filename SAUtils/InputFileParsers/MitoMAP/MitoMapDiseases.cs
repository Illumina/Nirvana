using System.Collections.Generic;
using System.Linq;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public static class MitoMapDiseases
    {
        public static Dictionary<string, string> MitoMapDiseaseAbbreviations = new Dictionary<string, string>
        {
            {"AD", "Alzeimer's Disease"},
            {"ADPD", "Alzeimer's Disease and Parkinsons's Disease"},
            {"MMC", "Maternal Myopathy and Cardiomyopathy"},
            {"CPEO", "Chronic Progressive External Ophthalmoplegia"},
            {"DM", "Diabetes Mellitus"},
            {"DMDF", "Diabetes Mellitus + Deafness"},
            {"DEAF", "Maternally inherited Deafness or aminoglycoside-induced Deafness"},
            {"PEM", "Progressive encephalopathy"},
            {"EXIT", "Exercise Intolerance"},
            {"AMDF", "Ataxia, Myoclonus and Deafness"},
            {"AMD", "Age-Related Macular Degeneration"},
            {"AMegL", "Acute Megakaryoblastic Leukemia"},
            {"CIPO", "Chronic Intestinal Pseudoobstruction with myopathy and Ophthalmoplegia"},
            {"DEMCHO", "Dementia and Chorea"},
            {"ESOC", "Epilepsy, Strokes, Optic atrophy, & Cognitive decline"},
            {"FBSN", "Familial Bilateral Striatal Necrosis"},
            {"FICP", "Fatal Infantile Cardiomyopathy Plus, a MELAS-associated cardiomyopathy"},
            {"FSGS", "Focal Segmental Glomerulosclerosis"},
            {"GER", "Gastrointestinal Reflux"},
            {"KSS", "Kearns Sayre Syndrome"},
            {"LD", "Leigh Disease"},
            {"LS", "Leigh Syndrome"},
            {"LDYT", "Leber's hereditary optic neuropathy and Dystonia"},
            {"LHON", "Leber Hereditary Optic Neuropathy"},
            {"LIMM", "Lethal Infantile Mitochondrial Myopathy"},
            {"LONGEVITY", "Long life"},
            {"LVNC", "Left Ventricular Noncompaction"},
            {"MDM", "Myopathy and Diabetes Mellitus"},
            {"MELAS", "Mitochondrial Encephalomyopathy, Lactic Acidosis, and Stroke-like episodes"},
            {"MEPR", "Myoclonic Epilepsy and Psychomotor Regression"},
            {"MERME", "MERRF/MELAS overlap disease"},
            {"MERRF", "Myoclonic Epilepsy and Ragged Red Muscle Fibers"},
            {"MHCM", "Maternally inherited Hypertrophic CardioMyopathy"},
            {"MI", "Myocardial Infarction"},
            {"MICM", "Maternally Inherited Cardiomyopathy"},
            {"MIDD", "Maternally Inherited Diabetes and Deafness"},
            {"MILS", "Maternally Inherited Leigh Syndrome"},
            {"MEc", "Mitochondrial Encephalocardiomyopathy"},
            {"MEm", "Mitochondrial Encephalomyopathy"},
            {"MM", "Mitochondrial Myopathy"},
            {"MMC", "Maternal Myopathy and Cardiomyopathy"},
            {"NAION", "Nonarteritic Anterior Ischemic Optic Neuropathy"},
            {"NARP", "Neurogenic muscle weakness, Ataxia, and Retinitis Pigmentosa"},
            {"NIDDM", "Non-Insulin Dependent Diabetes Mellitus"},
            {"NRTI-PN", "Antiretroviral Therapy-Associated Peripheral Neuropathy"},
            {"OAT", "Oligoasthenoteratozoospermia"},
            {"PEG", "Pseudoexfoliation Glaucoma"},
            {"PEM", "Progressive Encephalopathy"},
            {"PME", "Progressive Myoclonus Epilepsy"},
            {"POAG", "Primary Open Angle Glaucoma"},
            {"RTT", "Rett Syndrome"},
            {"SIDS", "Sudden Infant Death Syndrome"},
            {"SNHL", "SensoriNeural Hearing Loss"}
        };

        public static List<string> ParseDiseaseInfo(string diseaseInfo)
        {
            return new List<string>() { diseaseInfo };
            // throw new System.NotImplementedException();
            //diseaseInfo.Split(' ').Select(x => x.TrimEnd(',')).Select(x => MitoMapDiseaseAbbreviations.ContainsKey(x) ? ";" + x + ";" : " " + x + " ");
        }
    }
}