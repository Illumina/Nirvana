using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public sealed class MitoMapDisease
    {
        private readonly Dictionary<string, List<string>> _mitomapDiseaseAnnotation = new Dictionary<string, List<string>>();

        public MitoMapDisease(FileInfo diseaseAnnotationFile)
        {
            using (var reader = new StreamReader(diseaseAnnotationFile.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith('#')) continue;
                    var info = line.Split('\t');
                    var disease = info.Length == 2 ? info[1].Split(';').ToList() : new List<string>();
                    _mitomapDiseaseAnnotation.Add(info[0], disease);
                }
            }
        }

        public List<string> GetDisease(string mitomapDiseaseString)
        {
            if (!_mitomapDiseaseAnnotation.ContainsKey(mitomapDiseaseString))
                //throw new Exception($"MITOMAP disease description hasn't been curated: {mitomapDiseaseString}");
            {
                Console.WriteLine($"MITOMAP disease description hasn't been curated: {mitomapDiseaseString}");
                return null;
            }
            return _mitomapDiseaseAnnotation[mitomapDiseaseString];
        }
    }
}