using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;

namespace AminoAcidAligner
{
    public static class ExonToTranscript
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Aggregate exon alignments into transcript alignments");

            if (args.Length != 3)
            {
                Console.WriteLine("usage: dotnet AminoAcidAligner.dll [input exon alignment FASTA file] [output transcript alignment file] [output AA conservation scores file]");
                return;
            }

            var exonAlignmentFile = args[0];
            var transcriptAlignmentFile = args[1];
            var conservationScoresFile = args[2];

            using (var reader = GZipUtilities.GetAppropriateStreamReader(exonAlignmentFile))
            using (var writer = GZipUtilities.GetStreamWriter(transcriptAlignmentFile))
            using (var scoresWriter = GZipUtilities.GetStreamWriter(conservationScoresFile))
            {
                var count = CreateTranscriptAlignments(reader, writer, scoresWriter);
                Console.WriteLine($"Created {count} transcript alignments");
            }
        }

        
        /// <summary>
        /// merges multiple exon (amino acid) alignments to create transcript alignment
        /// </summary>
        /// <param name="reader">Stream reader for the input FASTA file with exon alignment</param>
        /// <param name="writer">Stream writer for the output file with transcript alignment</param>
        /// <param name="scoresWriter">Stream writer for the output file with conservation scores(percentage) </param>
        /// <returns>number if transcripts alignments created</returns>
        /// <exception cref="NotImplementedException"></exception>
        private static int CreateTranscriptAlignments(StreamReader reader, StreamWriter writer, StreamWriter scoresWriter)
        {
            string name = null;
            string sequence = null;
            var count = 0;
            AlignmentBuilder alignmentBuilder = null;
            scoresWriter.WriteLine("#Ensembl\tChromosome\tProteinSequence\tPercent Conservation at each AA residue");
            while (((name, sequence)= GetNextSequence(reader)) != (null, null))
            {
                (string transcriptId, string species, string chromosome) = Utilities.ParseSequenceName(name);
                
                if(alignmentBuilder == null) alignmentBuilder = new AlignmentBuilder(transcriptId);
                
                if (alignmentBuilder.TranscriptId != transcriptId)
                {
                    writer.WriteLine(alignmentBuilder.Chromosome);
                    writer.WriteLine(alignmentBuilder.TranscriptId);
                    writer.WriteLine(alignmentBuilder.ToString());
                    scoresWriter.WriteLine(alignmentBuilder.GetScoresLine());
                    alignmentBuilder = new AlignmentBuilder(transcriptId);
                    count++;
                }
                
                alignmentBuilder.Add(transcriptId, species, sequence);
                if (species == "hg38" || species == "hg19") alignmentBuilder.Chromosome = chromosome;

            }

            return count;
        }

        
        private static (string name, string sequence) GetNextSequence(StreamReader reader)
        {
            var name = reader.ReadLine();
            while (name=="")
            {
                name = reader.ReadLine();
            }
            if (name == null) return (null, null);
            if(!name.StartsWith('>')) throw new DataMisalignedException($"FASTQ entry does not start with >. Observed name: {name}");
            var sequence = reader.ReadLine();
            if (sequence == null) throw new DataMisalignedException($"No sequence found for {name}");

            return (name, sequence);
        }
    }
}