using System;
using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace ExtractHgncIds
{
    public class HgncExtractor
    {
        /// <summary>
        /// extracts the HGNC ids from the Nirvana cache files
        /// </summary>
        public void DumpIds(string cacheDirectory, string inputCompressedReferencePath, string outputPath)
        {
            // load the reference
            // ReSharper disable once UnusedVariable
            var compressedSequenceReader = new CompressedSequenceReader(inputCompressedReferencePath);

            var ndbFiles = Directory.GetFiles(cacheDirectory, "*.ndb");

            int numTranscripts = 0;
            int numTranscriptsWithoutGeneSymbol = 0;
            var missingList = new List<string>();
            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;

            using (var writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("#GeneSymbol\tGeneSymbolSource\tTranscriptID\tCanonical\tGeneID\tHgncID\tTranscriptLength\tCdsLength\tUcscRefName\tEnsemblRefName");

                foreach (var ndbFile in ndbFiles)
                {
                    var dataStore = new NirvanaDataStore();
                    var transcriptIntervalTree = new IntervalTree<Transcript>();

                    Console.Write("Processing {0}... ", Path.GetFileName(ndbFile));

                    var ucscRefName    = Path.GetFileNameWithoutExtension(ndbFile);
                    var ensemblRefName = chromosomeRenamer.GetEnsemblReferenceName(ucscRefName);

                    using (var reader = new NirvanaDatabaseReader(ndbFile))
                    {
                        reader.PopulateData(dataStore, transcriptIntervalTree);

                        numTranscripts += dataStore.Transcripts.Count;

                        // dump out the HGNC symbols
                        foreach (var transcript in dataStore.Transcripts)
                        {
                            int cdsLength = (transcript.CompDnaCodingStart == -1) || (transcript.CompDnaCodingEnd == -1) ? 
                                0 : transcript.CompDnaCodingEnd - transcript.CompDnaCodingStart + 1;

                            int transcriptLength = transcript.End - transcript.Start + 1;

                            writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}", transcript.GeneSymbol,
                                transcript.GeneSymbolSource, transcript.StableId, transcript.IsCanonical ? "YES" : "",
                                transcript.GeneStableId, transcript.HgncId, transcriptLength, cdsLength, ucscRefName, ensemblRefName);

                            if (string.IsNullOrEmpty(transcript.GeneSymbol))
                            {
                                numTranscriptsWithoutGeneSymbol++;
                                missingList.Add($"- GeneSymbol: {transcript.GeneSymbol}, GeneSymbolSource: {transcript.GeneSymbolSource}, StableId: {transcript.StableId}, GeneStableId: {transcript.GeneStableId}");
                            }                                
                        }
                    }

                    Console.WriteLine("finished.");
                }

                // display the missing transcripts
                if (missingList.Count > 0)
                {
                    Console.WriteLine("\nTranscripts missing gene symbols:");
                    foreach (var missing in missingList) Console.WriteLine(missing);

                    double percentWithoutGeneSymbol = numTranscriptsWithoutGeneSymbol/(double) numTranscripts*100.0;

                    Console.WriteLine("\n# of transcripts without gene symbol: {0} / {1} ({2:0.0} %)",
                        numTranscriptsWithoutGeneSymbol, numTranscripts, percentWithoutGeneSymbol);
                }
                else
                {
                    Console.WriteLine("All transcripts have gene symbols");
                }
            }
        }
    }
}
