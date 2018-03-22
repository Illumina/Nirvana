using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.Interface;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    // making this class a disposable is not recommneded for the following reasons
    // multiple threads access different parts of a iTSV file simultaneously. So having one stream doesn't work.
    // instead, each thread is handed an enumerator which has its own stream that it disposes upon use
    public sealed class SaMiscellaniesReader : ITsvReader
    {
        public SaHeader SaHeader => null;
        public IEnumerable<string> RefNames => _refNameOffsets.Keys;
        private readonly string _fileName;
        private readonly Dictionary<string, long> _refNameOffsets;

        public SaMiscellaniesReader(string fileName)
        {
            _fileName = fileName;
            using (var tsvIndex = new TsvIndex(new BinaryReader(FileUtilities.GetReadStream(_fileName + TsvIndex.FileExtension))))
            {
                _refNameOffsets = tsvIndex.TagPositions;
            }
        }

        public IEnumerable<SaMiscellanies> GetItems(string refName)
        {
            if (!_refNameOffsets.ContainsKey(refName)) yield break;

            var offset = _refNameOffsets[refName];

            using (var reader = GZipUtilities.GetAppropriateStreamReader(_fileName))
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

        private static SaMiscellanies ExtractItem(string line)
        {
            var columns = line.Split('\t');
            return columns.Length < 3 ? null : new SaMiscellanies(InterimSaCommon.RefMinorTag, columns[0], Convert.ToInt32(columns[1]), columns[2]);
        }
        
        
    }
}