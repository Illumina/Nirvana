using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using VariantAnnotation.Caches;
using VariantAnnotation.ProteinConservation;
using VariantAnnotation.Providers;

namespace SAUtils.AAConservation
{
    public sealed class ProteinConservationWriter:IDisposable
    {
        private readonly Stream _stream;
        private readonly Stream _transcriptGroupStream;
        private readonly GenomeAssembly _assembly;
        private readonly ExtendedBinaryWriter _writer;
        private readonly DataSourceVersion _version;

        private readonly TranscriptCacheData _transcriptCacheData;
        //some transcripts have multiple locations in the genome and may have conflicting scores
        // so, we need to load them up and check for duplicates and resolve them.
        

        public ProteinConservationWriter(Stream stream, Stream groupStream, TranscriptCacheData transcriptData, DataSourceVersion version)
        {
            _stream              = stream;
            _transcriptGroupStream = groupStream;
            _writer              = new ExtendedBinaryWriter(_stream);
            _transcriptCacheData = transcriptData;
            _version             = version;
            
        }

        public void Write(IEnumerable<ProteinConservationItem> items)
        {
            if (items == null) return;
            _writer.WriteOpt(ProteinConservationCommon.SchemaVersion);
            _writer.Write((byte) _assembly);
            _version.Write(_writer);
            
            var alignedProteinsAndScores = GetProteinWithUniqueScores(items);
            var nirvanaProteins = new HashSet<string>(_transcriptCacheData.PeptideSeqs);
            CheckProteinSetOverlap(alignedProteinsAndScores, nirvanaProteins);
            
            var transcriptScores = new Dictionary<string, byte[]>();
            //protein sequence -> transcript ids mapping
            var transcriptGroupsByProtein = new Dictionary<string, List<string>>(alignedProteinsAndScores.Count);
            foreach (var protein in alignedProteinsAndScores.Keys)
            {
                transcriptGroupsByProtein.Add(protein, new List<string>());
            }
            foreach (var transcriptIntervalArray in _transcriptCacheData.TranscriptIntervalArrays)
            {
                if (transcriptIntervalArray == null) continue;//may happen since for GRCh38 decoy contigs, there may be none
                foreach (var transcriptInterval in transcriptIntervalArray.Array)
                {
                    var transcript = transcriptInterval.Value;
                    if(transcript.Translation == null) continue;
                    var peptideSeq = transcript.Translation.PeptideSeq;
                    if(!alignedProteinsAndScores.TryGetValue(transcript.Translation.PeptideSeq, out var scores)) continue;

                    transcriptScores.TryAdd(transcript.Id.WithVersion, scores);
                    transcriptGroupsByProtein[peptideSeq].Add(transcript.Id.WithVersion);
                }
            }
            
            foreach (var (transcriptId, scores) in transcriptScores)
            {
                var transcriptScore = new TranscriptConservationScores(transcriptId, scores);
                transcriptScore.Write(_writer);
            }

            WriteTranscriptGroups(transcriptGroupsByProtein);

            Console.WriteLine($"Recorded conservation scores for {transcriptScores.Count} transcripts.");
            //writing an empty item to indicate end of records
            var endOfRecordItem = TranscriptConservationScores.GetEmptyItem();
            endOfRecordItem.Write(_writer);
        }

        private void WriteTranscriptGroups(Dictionary<string, List<string>> transcriptGroupsByProtein)
        {
            using (var writer = new StreamWriter(_transcriptGroupStream))
            {
                var ensemblIds = new List<string>();
                var refseqIds = new List<string>();
                writer.WriteLine("#EnsemblIds\tRefSeqIds\tPeptide sequence");
                foreach (var (protein,ids) in transcriptGroupsByProtein)
                {
                    if(ids.Count == 0) continue;
                    ensemblIds.Clear();
                    refseqIds.Clear();
                    foreach (var id in ids)
                    {
                        if(id.StartsWith("ENST")) ensemblIds.Add(id);
                        else refseqIds.Add(id);
                    }
                    writer.WriteLine($"{string.Join(',',ensemblIds)}\t{string.Join(',',refseqIds)}\t{protein}");
                }
            }
        }

        private void CheckProteinSetOverlap(Dictionary<string, byte[]> proteinAndScores, HashSet<string> nirvanaProteins)
        {
            var count = 0;
            foreach (var protein in proteinAndScores.Keys)
            {
                if (nirvanaProteins.Contains(protein)) count++;
            }

            Console.WriteLine($"{count} aligned proteins were also in Nirvana cache");
        }

        private static Dictionary<string, byte[]> GetProteinWithUniqueScores(IEnumerable<ProteinConservationItem> items)
        {
            var proteinAndScores = new Dictionary<string, byte[]>();
            var multiAlignProteins = new HashSet<string>();
            var proteinCount            = 0;
            foreach (var item in items)
            {
                if (proteinAndScores.TryAdd(item.ProteinSequence, item.Scores)) proteinCount++;
                else
                {
                    if (item.Chromosome == "chrX" || item.Chromosome == "X")
                    {
                        proteinAndScores[item.ProteinSequence] = item.Scores;
                    }

                    if (!item.Scores.SequenceEqual(proteinAndScores[item.ProteinSequence]))
                        multiAlignProteins.Add(item.ProteinSequence);
                }
            }

            foreach (var protein in multiAlignProteins)
            {
                proteinAndScores.Remove(protein);
            }

            Console.WriteLine($"Found {proteinCount} proteins with unique scores.");
            return proteinAndScores;
        }

        public void Dispose()=>_writer?.Dispose();
        
    }
}