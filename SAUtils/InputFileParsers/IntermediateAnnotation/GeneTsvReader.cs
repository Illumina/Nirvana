using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.Interface;
using VariantAnnotation.IO;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.GeneAnnotation;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    public sealed class GeneTsvReader:ITsvReader
    {
        private readonly StreamReader _reader;

        public SaHeader SaHeader { get; }
        public IEnumerable<string> RefNames => null;

        private string _name;
        private string _genomeAssembly;
        private string _version;
        private string _releaseDate;
        private string _description;
        private string _keyName;
        private bool _isArray;
        private const int GeneIndex = 0;
        private const int JsonStringIndex = 1;
        private const int MinNoOfColumns = 2;


        public GeneTsvReader(string fileName)
        {
            _reader = GZipUtilities.GetAppropriateStreamReader(fileName); 
            SaHeader = ReadHeader(_reader);
        }

        private SaHeader ReadHeader(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Skip empty lines.
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (reader.Peek() != '#') break;

                ParseHeaderLine(line);
            }

            if (string.IsNullOrEmpty(_genomeAssembly))
                _genomeAssembly = "";

            if (!string.IsNullOrEmpty(_name) 
             && !string.IsNullOrEmpty(_version) && !string.IsNullOrEmpty(_releaseDate) 
             && !string.IsNullOrEmpty(_keyName))
                return new SaHeader(_name, _genomeAssembly, _version, _releaseDate, _description);
            Console.WriteLine($"Insufficient version information for {_name}");
            return null;
        }


        public IEnumerable<IAnnotatedGene> GetItems()
        {
            var annotatedGenes = new List<IAnnotatedGene>();
            string line;
            //getting to the chromosome
            while ((line = _reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var annotationItem = ExtractItem(line);
                if (annotationItem == null) continue;
                annotatedGenes.Add(annotationItem);
            }

            return annotatedGenes.Count == 0 ? null : annotatedGenes;
        }


        private IAnnotatedGene ExtractItem(string line)
        {
            var columns = line.Split('\t');
            if (columns.Length < MinNoOfColumns)
                throw new InvalidDataException("Line contains too few columns:\n" + line);

            var geneSymbol = columns[GeneIndex];
            var jsonStrings = columns.Skip(JsonStringIndex).ToArray();
            return new AnnotatedGene(geneSymbol, new IGeneAnnotationSource[] { new GeneAnnotationSource(_keyName, jsonStrings, _isArray) });
        }

        private void ParseHeaderLine(string line)
        {
            var words = line.Split('=');
            if (words.Length < 2) return;

            var key = words[0];
            var value = words[1];

            switch (key)
            {
                case "#name":
                    _name = value;
                    break;
                case "#assembly":
                    _genomeAssembly = value;
                    break;
                case "#version":
                    _version = value;
                    break;
                case "#releaseDate":
                    _releaseDate = value;
                    break;
                case "#description":
                    _description = value;
                    break;
                case "#keyName":
                    _keyName = value;
                    break;
                case "#isArray":
                    _isArray = bool.Parse(value);
                    break;
                case "#schemaVerstion":
                    var schemaVersion = int.Parse(value);
                    if (schemaVersion != JsonCommon.SupplementarySchemaVersion)
                        throw new InvalidDataException($"Expected Schema version:{JsonCommon.SupplementarySchemaVersion}, oberved: {value}");
                    break;
            }
        }


        public void Dispose()
        {
            _reader.Dispose();
        }
        
    }
}

