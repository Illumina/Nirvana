using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using CacheUtils.CombineAndUpdateGenes.FileHandling;
using CacheUtils.CreateCache.Algorithms;
using CacheUtils.CreateCache.FileHandling;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;
using VD = VariantAnnotation.DataStructures;
using VepCombinedGeneReader = CacheUtils.CreateCache.FileHandling.VepCombinedGeneReader;

namespace CacheUtils.CreateCache
{
    public sealed class NirvanaDatabaseCreator
    {
        #region members

        private readonly VepTranscriptReader _transcriptReader;
        private readonly IVepReader<VD.RegulatoryElement> _regulatoryReader;
        private readonly IVepReader<MutableGene> _geneReader;
        private readonly IVepReader<MutableGene> _mergedGeneReader;
        private readonly IVepReader<VD.SimpleInterval> _intronReader;
        private readonly IVepReader<VD.SimpleInterval> _microRnaReader;
        private readonly IVepReader<string> _peptideReader;

        private readonly List<VD.Transcript> _transcripts;
        private readonly List<VD.RegulatoryElement> _regulatoryElements;
        private readonly List<MutableGene> _genes;
        private readonly List<MutableGene> _mergedGenes;
        private readonly List<VD.SimpleInterval> _introns;
        private readonly List<VD.SimpleInterval> _microRnas;
        private readonly List<string> _peptideSeqs;

        private readonly long _currentTimeTicks = DateTime.Now.Ticks;

        private readonly ChromosomeRenamer _renamer;
        private bool _hasData;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public NirvanaDatabaseCreator(VepTranscriptReader transcriptReader, VepRegulatoryReader regulatoryReader,
            VepGeneReader geneReader, VepCombinedGeneReader mergedGeneReader, VepSimpleIntervalReader intronReader,
            VepSimpleIntervalReader mirnaReader, VepSequenceReader peptideReader, ChromosomeRenamer renamer)
        {
            _transcriptReader = transcriptReader;
            _regulatoryReader = regulatoryReader;
            _geneReader       = geneReader;
            _mergedGeneReader = mergedGeneReader;
            _intronReader     = intronReader;
            _microRnaReader   = mirnaReader;
            _peptideReader    = peptideReader;
            _renamer          = renamer;

            _transcripts        = new List<VD.Transcript>();
            _regulatoryElements = new List<VD.RegulatoryElement>();
            _genes              = new List<MutableGene>();
            _mergedGenes        = new List<MutableGene>();
            _introns            = new List<VD.SimpleInterval>();
            _microRnas          = new List<VD.SimpleInterval>();
            _peptideSeqs        = new List<string>();
        }

        /// <summary>
        /// loads the intermediate annotation data
        /// </summary>
        public void LoadData()
        {
            Console.Write("- loading intermediate annotation data... ");
            var loadBenchmark = new Benchmark();

            Load(_regulatoryReader, _regulatoryElements);
            Load(_geneReader, _genes);
            Load(_mergedGeneReader, _mergedGenes);
            Load(_intronReader, _introns);
            Load(_microRnaReader, _microRnas);
            Load(_peptideReader, _peptideSeqs);

            var geneForest = CreateGeneForest(_mergedGenes, _renamer.NumRefSeqs);
            _transcriptReader.AddLists(_introns, _microRnas, _peptideSeqs, geneForest);

            while (true)
            {
                var transcript = _transcriptReader.GetLightTranscript();

                if (transcript == null) break;
                _transcripts.Add(transcript);
            }

            Console.WriteLine("{0}", Benchmark.ToHumanReadable(loadBenchmark.GetElapsedTime()));
            _hasData = true;
        }

        private static IIntervalForest<MutableGene> CreateGeneForest(List<MutableGene> genes, int numRefSeqs)
        {
            if (genes == null) return new NullIntervalSearch<MutableGene>();

            var intervalLists = new List<IntervalArray<MutableGene>.Interval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<IntervalArray<MutableGene>.Interval>();

            foreach (var transcript in genes)
            {
                intervalLists[transcript.ReferenceIndex].Add(
                    new IntervalArray<MutableGene>.Interval(transcript.Start, transcript.End, transcript));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<MutableGene>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                refIntervalArrays[i] = new IntervalArray<MutableGene>(intervalLists[i].ToArray());
            }

            return new IntervalForest<MutableGene>(refIntervalArrays);
        }

        /// <summary>
        /// creates the global database
        /// </summary>
        public void CreateTranscriptCacheFile(string outputPrefix)
        {
            if (!_hasData) throw new GeneralException("Data was not loaded before running CreateTranscriptCacheFile");

            Console.Write("- creating transcript cache file... ");
            var createBenchmark = new Benchmark();

            var globalOutputPath = CacheConstants.TranscriptPath(outputPrefix);

            var customHeader = new GlobalCustomHeader(_transcriptReader.Header.VepReleaseTicks,
                _transcriptReader.Header.VepVersion);

            var header = new FileHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, _transcriptReader.Header.TranscriptSource, _currentTimeTicks, _transcriptReader.Header.GenomeAssembly, customHeader);

            var genes = ConvertGenes();

            using (var writer = new GlobalCacheWriter(globalOutputPath, header))
            {
                var cache = new VD.GlobalCache(header, _transcripts.ToArray(), _regulatoryElements.ToArray(),
                    genes, _introns.ToArray(), _microRnas.ToArray(), _peptideSeqs.ToArray());

                writer.Write(cache);
            }

            Console.WriteLine("{0}", Benchmark.ToHumanReadable(createBenchmark.GetElapsedTime()));
        }

        private VD.Gene[] ConvertGenes()
        {
            var genes = new VD.Gene[_mergedGenes.Count];
            for (int i = 0; i < _mergedGenes.Count; i++) genes[i] = _mergedGenes[i].ToGene();
            return genes;
        }

        /// <summary>
        /// loads the items from the VEP reader
        /// </summary>
        private static void Load<T>(IVepReader<T> reader, List<T> values)
        {
            while (true)
            {
                var value = reader.Next();
                if (value == null) break;
                values.Add(value);
            }
        }

        /// <summary>
        /// copies the protein function prediction files to the output folder
        /// </summary>
        public void CopyPredictionCacheFile(string description, string inputPredictionPath, string outputPredictionPath)
        {
            Console.Write($"- copying {description} prediction cache... ");
            var copyBenchmark = new Benchmark();
            File.Copy(inputPredictionPath, outputPredictionPath, true);
            Console.WriteLine("{0}", Benchmark.ToHumanReadable(copyBenchmark.GetElapsedTime()));
        }

        public void MarkCanonicalTranscripts(string lrgPath)
        {
            // sanity check: make sure we're only doing this for RefSeq
            if (_transcriptReader.Header.TranscriptSource != VD.TranscriptDataSource.RefSeq) return;

            Console.Write("- marking canonical transcripts... ");
            var canonical = new CanonicalTranscriptMarker(lrgPath);
            int numCanonicalFlagsChanged = canonical.MarkTranscripts(_transcripts);
            Console.WriteLine($"{numCanonicalFlagsChanged} canonical transcripts marked.");
        }
    }
}
