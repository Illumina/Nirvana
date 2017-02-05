using System;
using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.PredictionCache;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace CacheUtils.CombineCacheDirectories
{
    public sealed class CacheCombiner
    {
        private readonly string _cachePath1;
        private readonly string _cachePath2;
		private readonly string _prefix1;
		private readonly string _prefix2;
		private readonly string _outPrefix;
		private readonly string _outputCachePath;
	    private GenomeAssembly _genomeAssembly= GenomeAssembly.Unknown;
	    private int _numRefSeq;

	    /// <summary>
        /// constructor
        /// </summary>
        public CacheCombiner(string inputPrefix1, string inputPrefix2, string outputPrefix)
	    {
		    _prefix1 = inputPrefix1;
		    _prefix2 = inputPrefix2;
		    _outPrefix = outputPrefix;
            _cachePath1      = CacheConstants.TranscriptPath(inputPrefix1);
            _cachePath2      = CacheConstants.TranscriptPath(inputPrefix2);
            _outputCachePath = CacheConstants.TranscriptPath(outputPrefix);
        }

        public void Combine()
        {
	        using (var reader1 = new GlobalCacheReader(_cachePath1)) 
	        using (var reader2 = new GlobalCacheReader(_cachePath2))
	        {
				var cache1 = reader1.Read();
				var cache2 = reader2.Read();

				//todo: hardcoding the combined custom header, note that the date is March 2016 on the vep website but we useds to say 2016-04-29
				var customHeader = new GlobalCustomHeader(DateTime.Parse("2016-04-29").Ticks,84);

				var header = new FileHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
				CacheConstants.DataVersion, TranscriptDataSource.BothRefSeqAndEnsembl, DateTime.Now.Ticks, reader1.FileHeader.GenomeAssembly, customHeader);

				CombinePredictionsCaches();

		        using (var writer = new GlobalCacheWriter(_outputCachePath, header))
		        {
			        List<RegulatoryElement> combinedRegElements;
			        List<Gene> combinedGenes;
			        List<SimpleInterval> combinedIntrons;
			        List<SimpleInterval> combinedMirnas;
			        List<string> combinedPeptides;
			        var combinedTranscripts = GetCombinedCacheElements(cache1, cache2, out combinedRegElements, out combinedGenes, out combinedIntrons, out combinedMirnas, out combinedPeptides);

			        var combinedCache = new GlobalCache(header, combinedTranscripts.ToArray(), 
						combinedRegElements.ToArray(), 
						combinedGenes.ToArray(),
						combinedIntrons.ToArray(),
						combinedMirnas.ToArray(),
						combinedPeptides.ToArray()
						);

			        Console.WriteLine("Writing combined cache...");
			        writer.Write(combinedCache);
			        Console.WriteLine("Done");
					
		        }
		        
			}
			
		}

	    private void CombinePredictionsCaches()
	    {
			Console.WriteLine("Writing combined Sift...");

			var mergedSift = GetMergedPredictions(CacheConstants.SiftPath(_prefix1), CacheConstants.SiftPath(_prefix2));
			
		    using (var writer = new PredictionCacheWriter(CacheConstants.SiftPath(_outPrefix), PredictionCacheHeader.GetHeader(DateTime.Now.Ticks,_genomeAssembly,_numRefSeq)))
		    {
				var lookupTableList = new List<Prediction.Entry>();
			    foreach (var predictionCach in mergedSift)
			    {
				    lookupTableList.AddRange(predictionCach.LookupTable);
			    }

			    writer.Write(lookupTableList.ToArray(), mergedSift.Select(cache => cache.Predictions).ToArray());
		    }
		    Console.WriteLine("Done.");

		    Console.WriteLine("writing combined polyphen");
			var mergedPolyphen = GetMergedPredictions(CacheConstants.PolyPhenPath(_prefix1), CacheConstants.PolyPhenPath(_prefix2));

			using (var writer = new PredictionCacheWriter(CacheConstants.PolyPhenPath(_outPrefix), PredictionCacheHeader.GetHeader(DateTime.Now.Ticks, _genomeAssembly, _numRefSeq)))
			{
				var lookupTableList = new List<Prediction.Entry>();
				foreach (var predictionCach in mergedPolyphen)
				{
					lookupTableList.AddRange(predictionCach.LookupTable);
				}

				writer.Write(lookupTableList.ToArray(), mergedPolyphen.Select(cache => cache.Predictions).ToArray());
			}
		    Console.WriteLine("Done");
		}


		private Dictionary<ushort, int> GetPredictionMatrixCount(string path)
		{
			var countPerRefSeq= new Dictionary<ushort, int>();
			using (var reader1 = new PredictionCacheReader(FileUtilities.GetReadStream(path)))
			{
				_numRefSeq = reader1.FileHeader.Index.Size;
				for (ushort i = 0; i < _numRefSeq; i++)
				{
					var cache1 = reader1.Read(i);

					countPerRefSeq[i] = cache1.PredictionCount;
				}
			}
			return countPerRefSeq;
		}
		private List<PredictionCache> GetMergedPredictions(string path1, string path2)
	    {
		    var mergedPredictions = new List<PredictionCache>();

		    using (var reader1 = new PredictionCacheReader(FileUtilities.GetReadStream(path1)))
		    using (var reader2 = new PredictionCacheReader(FileUtilities.GetReadStream(path2)))
		    {
			    _genomeAssembly = reader1.FileHeader.GenomeAssembly;
			    _numRefSeq      = reader1.FileHeader.Index.Size;

				if (_genomeAssembly != reader2.FileHeader.GenomeAssembly)
					throw  new UserErrorException($"Observed different genome assemblies: {reader1.FileHeader.GenomeAssembly}, {reader2.FileHeader.GenomeAssembly}");

				for (ushort i = 0; i < _numRefSeq; i++)
			    {
				    var cache1 = reader1.Read(i);
				    var cache2 = reader2.Read(i);

					if (cache1 == PredictionCache.Empty ^ cache2==PredictionCache.Empty)
						throw new DataMisalignedException("one of the cache ran out before the other");
					mergedPredictions.Add(cache1.GetMergedCache(cache2));

				}
				//todo: take care of ref sequences unique to one cache
		    }
		    return mergedPredictions;
	    }

	    private List<Transcript> GetCombinedCacheElements(GlobalCache cache1, GlobalCache cache2, out List<RegulatoryElement> combinedRegElements,
		    out List<Gene> combinedGenes, out List<SimpleInterval> combinedIntrons, out List<SimpleInterval> combinedMirnas, out List<string> combinedPeptides)
	    {
			var combinedTranscripts = CombinedTranscripts(cache1, cache2);

		    combinedRegElements = new List<RegulatoryElement>();
		    combinedRegElements.AddRange(cache1.RegulatoryElements);
		    combinedRegElements.AddRange(cache2.RegulatoryElements);
			combinedRegElements.Sort();
		    Console.WriteLine($"combined regulatory elemements count:{combinedRegElements.Count}");

		    combinedGenes = new List<Gene>();
		    combinedGenes.AddRange(cache1.Genes);
		    combinedGenes.AddRange(cache2.Genes);
			combinedGenes.Sort();
		    Console.WriteLine($"combined genes count:{combinedGenes.Count}");

		    combinedIntrons = new List<SimpleInterval>();
		    combinedIntrons.AddRange(cache1.Introns);
		    combinedIntrons.AddRange(cache2.Introns);
			//combinedIntrons.Sort();//should not be sorted as transcripts may access them via index : not sure
			Console.WriteLine($"combined introns count:{combinedIntrons.Count}");

		    combinedMirnas = new List<SimpleInterval>();
		    combinedMirnas.AddRange(cache1.MicroRnas);
		    combinedMirnas.AddRange(cache2.MicroRnas);
			//combinedMirnas.Sort();//should not be sorted as transcripts may access them via index: not sure
			Console.WriteLine($"combined mirna count:{combinedMirnas.Count}");

		    combinedPeptides = new List<string>();
		    combinedPeptides.AddRange(cache1.PeptideSeqs);
		    combinedPeptides.AddRange(cache2.PeptideSeqs);
		    Console.WriteLine($"combined peptide count:{combinedPeptides.Count}");
		    return combinedTranscripts;
	    }

	    private List<Transcript> CombinedTranscripts(GlobalCache cache1, GlobalCache cache2)
	    {
		    var sift1Count = GetPredictionMatrixCount(CacheConstants.SiftPath(_prefix1));
		    var polyphen1Count = GetPredictionMatrixCount(CacheConstants.PolyPhenPath(_prefix1));

		    var combinedTranscripts = new List<Transcript>();
		    combinedTranscripts.AddRange(cache1.Transcripts);
		    foreach (var transcript in cache2.Transcripts)
		    {
			    combinedTranscripts.Add(new Transcript(
				    transcript.ReferenceIndex, transcript.Start, transcript.End,
				    transcript.Id, transcript.Version, transcript.Translation, transcript.BioType,
				    transcript.Gene, transcript.TotalExonLength, transcript.StartExonPhase,
				    transcript.IsCanonical, transcript.Introns, transcript.MicroRnas, transcript.CdnaMaps,
				    transcript.SiftIndex == -1 ? -1 : transcript.SiftIndex + sift1Count[transcript.ReferenceIndex],
				    transcript.PolyPhenIndex == -1 ? -1 : transcript.PolyPhenIndex + polyphen1Count[transcript.ReferenceIndex],
				    transcript.TranscriptSource
				    ));
		    }
			combinedTranscripts.Sort();
		    Console.WriteLine($"combined trascripts count:{combinedTranscripts.Count}");
		    return combinedTranscripts;
	    }

	    public void CheckCacheIntegrity()
        {
            var header  = GlobalCacheReader.GetHeader(_cachePath1);
            var header2 = GlobalCacheReader.GetHeader(_cachePath2);

            var customHeader  = GlobalCacheReader.GetCustomHeader(header);
            var customHeader2 = GlobalCacheReader.GetCustomHeader(header2);

            CheckTranscriptSource(header.TranscriptSource, header2.TranscriptSource);
            Check("genome assembly", header.GenomeAssembly.ToString(), header2.GenomeAssembly.ToString());
            Check("data version", header.DataVersion, header2.DataVersion);
            Check("VEP version", customHeader.VepVersion, customHeader2.VepVersion);
        }

        private static void Check<T>(string description, T first, T second) where T : IEquatable<T>
        {
            if (!first.Equals(second))
            {
                throw new UserErrorException($"The {description} values didn't match between the two cache files: {first} vs {second}");
            }
        }

        private static void CheckTranscriptSource(TranscriptDataSource ts, TranscriptDataSource ts2)
        {
            bool hasRefSeq  = false;
            bool hasEnsembl = false;

            EvaluateTranscriptSource(ts, ref hasEnsembl, ref hasRefSeq);
            EvaluateTranscriptSource(ts2, ref hasEnsembl, ref hasRefSeq);

            if (!hasEnsembl || !hasRefSeq) throw new UserErrorException("Expected one RefSeq and one Ensembl cache file. Please revise the --in and --in2 command-line arguments.");
        }

        private static void EvaluateTranscriptSource(TranscriptDataSource transcriptSource, ref bool hasEnsembl,
            ref bool hasRefSeq)
        {            
            if (transcriptSource == TranscriptDataSource.Ensembl) hasEnsembl = true;
            if (transcriptSource == TranscriptDataSource.RefSeq)  hasRefSeq  = true;
        }
    }
}
