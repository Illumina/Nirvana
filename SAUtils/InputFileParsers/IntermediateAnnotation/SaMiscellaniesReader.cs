using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    public sealed class SaMiscellaniesReader:IEnumerable<SaMiscellanies>
    {
        private readonly StreamReader _inputFileStreamReader;

        private readonly Dictionary<string, long> _refNameOffsets;
        

        public SaMiscellaniesReader(StreamReader inputFileStreamReader,Stream indexFileStream)
        {

            _inputFileStreamReader = inputFileStreamReader;

            using (var tsvIndex = new TsvIndex(new BinaryReader(indexFileStream)))
            {
                _refNameOffsets = tsvIndex.TagPositions;
            }


            string line;
            while ((line = _inputFileStreamReader.ReadLine()) != null)
            {
                // Skip empty lines.
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!line.StartsWith("#")) break;

                ParseHeaderLine(line);
            }
            
        }

        private void ParseHeaderLine(string line)
        {

        }

        public IEnumerator<SaMiscellanies> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }



        public IEnumerable<SaMiscellanies> GetAnnotationItems(string refName)
        {
            if (!_refNameOffsets.ContainsKey(refName)) yield break;

            var offset = _refNameOffsets[refName];

            using (var reader = _inputFileStreamReader)
            {
                reader.BaseStream.Position = offset;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    // finding desired chromosome. We need this because the GetLocation for GZipStream may return a position a few lines before the start of the chromosome
                    if (line.StartsWith(refName + "\t")) break;
                }
                if (line == null) yield break;
                string lastLine = line;
                do
                {
                    //next chromosome
                    if (!line.StartsWith(refName + "\t")) yield break;

                    var annotationItem = ExtractItem(line);
                    if (annotationItem == null) continue;

                    yield return annotationItem;
                    try
                    {
                        line = reader.ReadLine();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("error while reading line in while loop. Last line read:");
                        Console.WriteLine(lastLine);
                        throw;
                    }
                    lastLine = line;
                } while (line != null);
            }
        }

        private SaMiscellanies ExtractItem(string line)
        {
            var columns = line.Split('\t');
            if (columns.Length < 3) return null;

            return new SaMiscellanies(InterimSaCommon.RefMinorTag, columns[0], Convert.ToInt32(columns[1]), columns[2],
                true);
        }

        public List<string> GetAllRefNames()
        {
            return _refNameOffsets.Keys.ToList();
        }
    }
}