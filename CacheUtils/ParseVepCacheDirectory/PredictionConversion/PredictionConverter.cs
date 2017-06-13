using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.FileHandling.PredictionCache;

namespace CacheUtils.ParseVepCacheDirectory.PredictionConversion
{
    public sealed class PredictionConverter
    {
        private readonly ushort _numReferenceSeqs;
        private static readonly long CurrentTimeTicks = DateTime.Now.Ticks;

        /// <summary>
        /// constructor
        /// </summary>
        public PredictionConverter(ushort numReferenceSeqs)
        {
            _numReferenceSeqs = numReferenceSeqs;
        }

        public void Convert(string outputPath, string description, GlobalImportCommon.FileType fileType)
        {
            var inputPath = outputPath.Replace(".dat", ".dat.tmp");

            using (var reader = new TempPredictionReader(inputPath, description, fileType))
            {
                Console.Write($"- loading {description}... ");
                var tempPredictions = Load(reader);
                Console.WriteLine("finished.");                

                Console.Write($"- creating {description} LUT... ");
                var oldLut = TempPrediction.CreateLookupTable(tempPredictions);
                var newLut = TempPrediction.ConvertLookupTable(oldLut);
                Console.WriteLine("finished.");

                Console.Write($"- converting {description} matrices... ");
                var predictionsPerRef = TempPrediction.ConvertMatrices(tempPredictions, oldLut, newLut, _numReferenceSeqs);
                Console.WriteLine("finished.");

                tempPredictions.Clear();

                var header = PredictionCacheHeader.GetHeader(CurrentTimeTicks, reader.Header.GenomeAssembly, _numReferenceSeqs);
                
                Console.Write($"- writing to {Path.GetFileName(outputPath)}... ");
                using (var writer = new PredictionCacheWriter(outputPath, header))
                {
                    writer.Write(newLut, predictionsPerRef);
                }
                Console.WriteLine("finished.");
            }
        }

        /// <summary>
        /// loads the items from the VEP reader
        /// </summary>
        private static List<TempPrediction> Load(TempPredictionReader reader)
        {
            var values = new List<TempPrediction>();

            while (true)
            {
                var value = reader.Next();
                if (value == null) break;
                values.Add(value);
            }

            return values;
        }
    }
}
