using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Reference;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace CreateCompressedReference
{
    public sealed class FastaToCompressedConverter
    {
        /// <summary>
        /// converts the FASTA file to a compressed reference file
        /// </summary>
        public void Convert(string inputFastaPath, string inputCytogeneticBandpath, string inputChromosomeNamesPath,
            string outputCompressedPath, GenomeAssembly genomeAssembly)
        {
            Console.Write("- getting reference metadata... ");
            var referenceMetaDataList = GetReferenceMetadata(inputChromosomeNamesPath);
            Console.WriteLine("{0} references found.", referenceMetaDataList.Count);

            var renamer = new ChromosomeRenamer();
            renamer.AddReferenceMetadata(referenceMetaDataList);

            // pre-allocate the cytogenetic bands
            Console.Write("- getting cytogenetic bands... ");
            var cytogeneticBands = GetCytogeneticBands(inputCytogeneticBandpath, renamer);
            Console.WriteLine("finished.\n");

            // parse the reference
            using (var fastaReader = new FastaReader(inputFastaPath))
            {
                using (var writer = new CompressedSequenceWriter(outputCompressedPath, referenceMetaDataList, cytogeneticBands, genomeAssembly))
                {
                    Console.WriteLine("Converting the following reference sequences:");

                    while(true)
                    {
                        var referenceSequence = fastaReader.GetReferenceSequence();
                        if (referenceSequence == null) break;

                        Console.WriteLine("- {0} ({1:n0} bytes)", referenceSequence.Name, referenceSequence.Bases.Length);

                        writer.Write(referenceSequence.Name, referenceSequence.Bases);
                    }
                }
            }

            Console.WriteLine("\nFile size: {0}", new FileInfo(outputCompressedPath).Length);
        }

        /// <summary>
        /// returns a list of reference cytogenetic bands
        /// </summary>
        private static ICytogeneticBands GetCytogeneticBands(string inputCytobandPath, ChromosomeRenamer renamer)
        {
            var numRefSeqs = renamer.NumRefSeqs;

            var bandLists = new List<Band>[numRefSeqs];
            for (int i = 0; i < numRefSeqs; i++) bandLists[i] = new List<Band>();

            using (var reader = new StreamReader(FileUtilities.GetReadStream(inputCytobandPath)))
            {
                while (true)
                {
                    // grab the next line
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    // split the line into columns
                    var cols = line.Split('\t');

                    // sanity check: make sure we have the right number of columns
                    const int expectedNumColumns = 5;

                    if (cols.Length != expectedNumColumns)
                    {
                        throw new GeneralException($"Expected {expectedNumColumns} columns, but found {cols.Length} columns: [{line}]");
                    }

                    // grab the essential values
                    var ucscName = cols[0];
                    var begin    = int.Parse(cols[1]) + 1;
                    var end      = int.Parse(cols[2]);
                    var name     = cols[3];

                    ushort refIndex = renamer.GetReferenceIndex(ucscName);
                    if (refIndex == ChromosomeRenamer.UnknownReferenceIndex) continue;

                    bandLists[refIndex].Add(new Band(begin, end, name));
                }
            }

            // create the band arrays
            var bands = new Band[numRefSeqs][];
            for (int i = 0; i < numRefSeqs; i++) bands[i] = bandLists[i].ToArray();

            return new CytogeneticBands(bands, renamer);
        }

        private static List<ReferenceMetadata> GetReferenceMetadata(string inputPath)
        {
            var referenceMetadataList = new List<ReferenceMetadata>();

            using (var reader = new StreamReader(FileUtilities.GetReadStream(inputPath)))
            {
                while (true)
                {
                    // grab the next line
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    // skip comment lines
                    if (line.StartsWith("#")) continue;

                    // split the line into columns
                    var cols = line.Split('\t');

                    // sanity check: make sure we have the right number of columns
                    const int expectedNumColumns = 3;

                    if (cols.Length != expectedNumColumns)
                    {
                        throw new GeneralException($"Expected {expectedNumColumns} columns, but found {cols.Length} columns: [{line}]");
                    }

                    // grab the essential values
                    var ucscName    = cols[0];
                    var ensemblName = cols[1];
                    var inVep       = cols[2] == "YES";

                    referenceMetadataList.Add(new ReferenceMetadata(ucscName, ensemblName, inVep));
                }
            }

            return referenceMetadataList;
        }
    }
}
