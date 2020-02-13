using System;
using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;

namespace SAUtils.ClinGen
{
    public sealed class GeneDiseaseValidityParser: IDisposable
    {
        private readonly Stream _stream;
        private Dictionary<int, string> _hgncIdToSymbols;

        private readonly HashSet<int> _unknownIds = new HashSet<int>();
        private readonly HashSet<string> _classificationSet = new HashSet<string>
        {
            "no reported evidence",
            "disputed",
            "limited",
            "moderate",
            "definitive",
            "strong",
            "refuted"
        };

        public GeneDiseaseValidityParser(Stream stream, Dictionary<int, string> hgncIdToSymbols)
        {
            _stream = stream;
            _hgncIdToSymbols = hgncIdToSymbols;
        }
        public void Dispose()
        {
            _stream?.Dispose();
        }

        public Dictionary<string, List<ISuppGeneItem>> GetItems()
        {
            var geneAnnotations = new Dictionary<string, Dictionary<string,GeneDiseaseValidityItem>>();

            using (var reader = new StreamReader(_stream))
            {
                string line;
                bool isComments = true;
                bool isHeaderLine = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isComments)
                    {
                        //the header starts with a bunch of '+' signs
                        if (!line.StartsWith("++++")) continue;
                        isComments = false;
                        isHeaderLine = true;
                        continue;
                    }

                    if (isHeaderLine)
                    {
                        ParseHeaderLine(line);
                        isHeaderLine = false;
                        line = reader.ReadLine();//reading end of header line
                        if (line.StartsWith("++++")) continue;
                    }

                    if (MissingIndices()) throw new InvalidDataException("Column indices not set!!");
                    var geneAnnotation = GetAnnotationItem(line);
                    if(geneAnnotation == null) continue;

                    if (geneAnnotations.TryGetValue(geneAnnotation.GeneSymbol, out var annotations))
                        AddLatest(annotations, geneAnnotation);
                    else geneAnnotations.Add(geneAnnotation.GeneSymbol, new Dictionary<string, GeneDiseaseValidityItem> {{geneAnnotation.DiseaseId, geneAnnotation}});

                }
            }

            Console.WriteLine($"Number of geneIds missing from the cache:{_unknownIds.Count} ({100.0*_unknownIds.Count/_hgncIdToSymbols.Count}%)");
            
            return GetLatestAnnotations(geneAnnotations);
        }

        private static Dictionary<string, List<ISuppGeneItem>> GetLatestAnnotations(Dictionary<string, Dictionary<string, GeneDiseaseValidityItem>> annotationByDiseaseIds)
        {
            var latestAnnotations = new Dictionary<string, List<ISuppGeneItem>>();
            foreach (var annotation in annotationByDiseaseIds)
            {
                var geneAnnotation = new List<ISuppGeneItem>();
                foreach (var geneAnno in annotation.Value.Values)
                {
                    geneAnnotation.Add(geneAnno);
                }

                latestAnnotations.Add(annotation.Key, geneAnnotation);
            }

            return latestAnnotations;
        }

        private void AddLatest(Dictionary<string, GeneDiseaseValidityItem> annotations, GeneDiseaseValidityItem geneAnnotation)
        {
            if(!annotations.TryGetValue(geneAnnotation.DiseaseId, out var diseaseItem)) annotations.Add(geneAnnotation.DiseaseId, geneAnnotation);
            else
            {
                if (diseaseItem.CompareDate(geneAnnotation) < 0) annotations[geneAnnotation.DiseaseId] = geneAnnotation;
            }
        }


        private GeneDiseaseValidityItem GetAnnotationItem(string line)
        {
            var cols = line.OptimizedSplit('\t');

            var geneId = int.Parse(cols[_geneIdIndex].OptimizedSplit(':')[1]);
            if (!_hgncIdToSymbols.TryGetValue(geneId, out var geneSymbol))
            {
                _unknownIds.Add(geneId);
                return null;
            }

            var disease = cols[_diseaseIndex].Trim('\"');
            var diseaseId = cols[_diseaseIdIndex];
            var classification = cols[_classificationIndex].ToLower();
            if (!_classificationSet.Contains(classification))
            {
                throw new InvalidDataException($"Unknown classification found: {classification}");
            }
        
            var classificationDate = cols[_classificationDateIndex].OptimizedSplit('T')[0];//2018-06-07T14:37:47.175Z

            return new GeneDiseaseValidityItem(geneSymbol, diseaseId, disease, classification, classificationDate);
        }

        private int _geneIdIndex = -1;
        private int _diseaseIdIndex = -1;
        private int _diseaseIndex = -1;
        private int _classificationIndex = -1;
        private int _classificationDateIndex = -1;

        private const string GeneIdTag = "GENE ID (HGNC)";
        private const string DiseaseTag = "DISEASE LABEL";
        private const string DiseaseIdTag = "DISEASE ID (MONDO)";
        private const string ClassificationTag = "CLASSIFICATION";
        private const string ClassificationDateTag = "CLASSIFICATION DATE";

        private bool MissingIndices()
        {
            return _geneIdIndex            == -1 ||
                   _diseaseIdIndex         == -1 ||
                   _diseaseIndex           == -1 ||
                   _classificationIndex    == -1 ||
                   _classificationDateIndex== -1;
        }

        private void ParseHeaderLine(string line)
        {
            var cols = line.OptimizedSplit('\t');

            _geneIdIndex             = Array.IndexOf(cols, GeneIdTag);
            _diseaseIndex            = Array.IndexOf(cols, DiseaseTag);
            _diseaseIdIndex          = Array.IndexOf(cols, DiseaseIdTag);
            _classificationIndex     = Array.IndexOf(cols, ClassificationTag);
            _classificationDateIndex = Array.IndexOf(cols, ClassificationDateTag);
        }

    }
}