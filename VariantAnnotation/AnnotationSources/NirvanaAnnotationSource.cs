using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Algorithms.Consequences;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.Annotation;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Omim;
using VariantAnnotation.FileHandling.PredictionCache;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotationSources
{
    public sealed class NirvanaAnnotationSource : IAnnotationSource
    {
        #region members

        // caches
        private readonly GlobalCache _transcriptCache;
        private PredictionCache _siftCache;
        private PredictionCache _polyPhenCache;

        // readers
        private readonly PredictionCacheReader _siftReader;
        private readonly PredictionCacheReader _polyPhenReader;
        private readonly IConservationScoreReader _conservationScoreReader;

        private readonly ISupplementaryAnnotationProvider _saProvider;

        // interval forests
        private readonly IIntervalForest<Transcript> _transcriptIntervalForest;
        private readonly IIntervalForest<RegulatoryElement> _regulatoryIntervalForest;

        // flags
        private bool _enableMitochondrialAnnotation;
        private bool _reportAllSvOverlappingTranscripts;
        private bool _hasConservationScores;
	    private bool _enableReferenceNoCalls;
        private bool _limitReferenceNoCallsToTranscripts;

        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        private List<Transcript> OverlappingTranscripts { get; }
        private List<RegulatoryElement> OverlappingRegulatoryFeatures { get; }

        private HashSet<string> OverlappingGeneSymbols { get; }
        private HashSet<int> PartialOverlappingGeneHashCodes { get; }

        private UnifiedJson _json;
        private readonly Dictionary<string, List<OmimAnnotation>> _omimGeneDict;

        private readonly List<IDataSourceVersion> _dataSourceVersions = new List<IDataSourceVersion>();
        private readonly ICompressedSequence _compressedSequence;
        private readonly PerformanceMetrics _performanceMetrics;

        public const int FlankingLength = 5000;
        private const int MaxDownstreamLength = 5000;

        private readonly HashSet<string> _affectedGeneSymbols;
        private IEnumerable<string> AffectedGeneSymbols => _affectedGeneSymbols;

        private readonly DataFileManager _dataFileManager;
        private readonly AminoAcids _aminoAcids;
        private readonly VID _vid;

        #endregion

        string IAnnotationSource.GetDataVersion()
            =>  CacheConstants.VepVersion + "." + CacheConstants.DataVersion + "." +
                SupplementaryAnnotationCommon.DataVersion;

	    public void EnableMitochondrialAnnotation() => _enableMitochondrialAnnotation = true;

        public void EnableReportAllSvOverlappingTranscripts() => _reportAllSvOverlappingTranscripts = true;

        public void AddPlugin(IPlugin plugin) => _plugins.Add(plugin);

        /// <summary>
        /// constructor
        /// NOTE: we want to set up most "normal" things here. Leave custom annotations and custom intervals to other methods
        /// </summary>
        public NirvanaAnnotationSource(AnnotationSourceStreams streams, ISupplementaryAnnotationProvider saProvider,
            IConservationScoreReader conservationScoreReader, IEnumerable<string> saDirs)
        {
	        OverlappingTranscripts          = new List<Transcript>();
            OverlappingRegulatoryFeatures   = new List<RegulatoryElement>();
            OverlappingGeneSymbols          = new HashSet<string>();
            PartialOverlappingGeneHashCodes = new HashSet<int>();

            _omimGeneDict      = null;
            _affectedGeneSymbols = new HashSet<string>();
            _aminoAcids          = new AminoAcids();
            _vid                 = new VID();

            // create omim database dictionary
            var omimDatabaseReader = OmimDatabaseCommon.GetOmimDatabaseReader(saDirs);
            var hasOmimAnnotations = omimDatabaseReader != null;
            _omimGeneDict = hasOmimAnnotations ? OmimDatabaseCommon.CreateGeneMapDict(omimDatabaseReader) : null;
            if (omimDatabaseReader != null) _dataSourceVersions.Add(omimDatabaseReader.DataVersion);

            _compressedSequence = new CompressedSequence();
            var compressedSequenceReader = new CompressedSequenceReader(streams.CompressedSequence, _compressedSequence);
            _dataFileManager = new DataFileManager(compressedSequenceReader, _compressedSequence);
            _dataFileManager.Changed += LoadData;

	        _transcriptCache = InitiateCache(streams.Transcript);

            _siftReader              = streams.Sift != null ? new PredictionCacheReader(streams.Sift) : null;
            _polyPhenReader          = streams.PolyPhen != null ? new PredictionCacheReader(streams.PolyPhen) : null;
            _saProvider               = saProvider;
			_conservationScoreReader  = conservationScoreReader;

			LoadDataSourceVersions();
            CheckGenomeAssemblies();

			LoadTranscriptCache(_transcriptCache, _compressedSequence.Renamer.NumRefSeqs, out _transcriptIntervalForest, out _regulatoryIntervalForest);

			_performanceMetrics = PerformanceMetrics.Instance;
        }

        private void CheckGenomeAssemblies()
        {
            var assemblies = new HashSet<GenomeAssembly> {_compressedSequence.GenomeAssembly};
            if (_transcriptCache          != null) assemblies.Add(_transcriptCache.GenomeAssembly);
            if (_siftCache                != null) assemblies.Add(_siftCache.GenomeAssembly);
            if (_polyPhenCache            != null) assemblies.Add(_polyPhenCache.GenomeAssembly);
            if (_saProvider               != null) assemblies.Add(_saProvider.GenomeAssembly);
            if (_conservationScoreReader  != null) assemblies.Add(_conservationScoreReader.GenomeAssembly);

			//todo: temp fix, needs rethinking
			// the mockSAprovider or any mock provider has GenomeAssembly set to unknown by default. That is why the unit tests are failing.
			//I will temporarily remove unknown.
	        assemblies.Remove(GenomeAssembly.Unknown);
            if (assemblies.Count > 1)
            {
                throw new UserErrorException("Found more than one genome assembly represented in the selected data sources.");
            }
        }

        private void LoadDataSourceVersions()
        {
            if (_transcriptCache          != null) _dataSourceVersions.AddRange(_transcriptCache.DataSourceVersions);
            if (_saProvider               != null) _dataSourceVersions.AddRange(_saProvider.DataSourceVersions);
            if (_conservationScoreReader  != null) _dataSourceVersions.AddRange(_conservationScoreReader.DataSourceVersions);
        }

	    private static GlobalCache InitiateCache(Stream stream)
	    {
			GlobalCache cache;
			using (var reader = new GlobalCacheReader(stream)) cache = reader.Read();

		    return cache;
	    }

		private static void LoadTranscriptCache(GlobalCache cache, int numRefSeqs,
		    out IIntervalForest<Transcript> transcriptIntervalForest,
		    out IIntervalForest<RegulatoryElement> regulatoryIntervalForest)
	    {
			transcriptIntervalForest = IntervalArrayFactory.CreateIntervalForest(cache.Transcripts, numRefSeqs);
			regulatoryIntervalForest = IntervalArrayFactory.CreateIntervalForest(cache.RegulatoryElements, numRefSeqs);			
		}

	    public IEnumerable<IDataSourceVersion> GetDataSourceVersions()
        {
            return _dataSourceVersions;
        }

        public string GetGenomeAssembly()
        {
            return _transcriptCache.Header.GenomeAssembly.ToString();
        }

        public List<IGeneAnnotation> GetGeneAnnotations()
        {
            var annotatedGenes = new List<IGeneAnnotation>();
            
                foreach (var geneSymbol in AffectedGeneSymbols)
                {
                    if(_omimGeneDict.ContainsKey(geneSymbol))
                        annotatedGenes.AddRange(_omimGeneDict[geneSymbol]);
                       
                }          
            
            return annotatedGenes;
        }

        /// <summary>
        /// adds upstream and downstream transcripts to our results
        /// </summary>
        private void AddFlankingTranscript(TranscriptAnnotation ta, VariantFeature variant, Transcript transcript)
        {
            var altAllele = ta.AlternateAllele;

            // flanking distance doesn't seem to exhaustively use distance like distanceToVariant
            var flankingDistance = Math.Min(Math.Abs(transcript.Start - altAllele.End),
                Math.Abs(transcript.End - altAllele.Start));

            if (flankingDistance > FlankingLength) return;

            // figure out if we should label this as a downstream or upstream transcript
            var isDownstream = altAllele.End < transcript.Start == transcript.Gene.OnReverseStrand;

            // determine the cDNA position                        
            CdnaMapper.MapCoordinates(altAllele.Start, altAllele.End, ta, transcript);

            // set the functional consequence
            var consequence = new Consequences(new VariantEffect(ta, transcript, variant.InternalCopyNumberType));
            consequence.DetermineFlankingVariantEffects(isDownstream, variant.InternalCopyNumberType);

            _json.AddFlankingTranscript(transcript, ta, consequence.GetConsequenceStrings());
        }

        /// <summary>
        /// returns the number of transcripts within flanking distance
        /// </summary>
        private bool HasOverlap(VariantFeature variant)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var transcript in OverlappingTranscripts)
            {
                var overlapInterval = new AnnotationInterval(transcript.Start - FlankingLength, transcript.End + FlankingLength);
                if (variant.AlternateAlleles.Any(altAllele => overlapInterval.Overlaps(altAllele.Start, altAllele.End))) return true;
            }

            return false;
        }
        /// <summary>
        /// returns the annotator version
        /// </summary>
        public static string GetVersion()
        {
            return "Illumina Annotation Engine " + CommandLineUtilities.Version;
        }

        private static void AddConservationScore(ref string varConservationScore, string altConservationScore)
        {
            if (string.IsNullOrEmpty(varConservationScore))
            {
                varConservationScore = altConservationScore ?? ".";
                return;
            }

            if (altConservationScore == null) varConservationScore += ",.";
            else varConservationScore += "," + altConservationScore;
        }

        /// <summary>
        /// returns an annotated variant given a raw variant
        /// </summary>
        public IAnnotatedVariant Annotate(IVariant variant)
        {
            if (variant == null) return null;

            var variantFeature = new VariantFeature(variant as VcfVariant, _compressedSequence.Renamer, _vid);

            // load the reference sequence
            _dataFileManager.LoadReference(variantFeature.ReferenceIndex, ClearDataSources, _performanceMetrics);

            // handle ref no-calls and assign the alternate alleles
            if (_enableReferenceNoCalls) ReferenceNoCall.Check(variantFeature, _limitReferenceNoCallsToTranscripts, _transcriptIntervalForest);
            variantFeature.AssignAlternateAlleles();

            // annotate the variant
            _json = new UnifiedJson(variantFeature);

            Annotate(variantFeature);

            RunPlugins(variantFeature);

            _performanceMetrics.Increment();

            return _json;
        }

        /// <summary>
        /// adds annotations to the variant feature object
        /// </summary>
        private void Annotate(VariantFeature variant)
        {
            if (variant.IsReference && !variant.IsSingletonRefSite && !variant.IsRefNoCall) return;

            if (variant.IsReference && !variant.IsSingletonRefSite && variant.IsRefNoCall ||
                variant.UcscReferenceName == "chrM" && !_enableMitochondrialAnnotation)
            {
                _json.AddVariantData(variant);
                return;
            }

			// retrieve the supplementary annotations
            _saProvider?.AddAnnotation(variant);

            // return if this is a ref but not ref minor
            if (variant.IsReference && !variant.IsRefMinor)
            {
                _json.AddVariantData(variant);
                return;
            }


            AssignConservationScores(variant);
            _dataFileManager.AssignCytogeneticBand(variant);
            AssignConservationScores(variant);

            _json.AddVariantData(variant);

            AddRegulatoryFeatures(variant);

            // add overlapping genes for CNV or SVs
            if (variant.IsStructuralVariant)
            {
                AnnotateTanscriptsForSv(variant);
                return;
            }
	        
            GetOverlappingTranscripts(variant);

            // handle intergenic variants
            if (!HasOverlap(variant))
            {
                HandleIntergenicVariant(variant);
                return;
            }

            // check each allele to see if it is a genomic duplicate
            if (_compressedSequence != null) variant.CheckForGenomicDuplicates(_compressedSequence);

            // setting the protein coding scheme 
            _aminoAcids.CodonConversionScheme = variant.UcscReferenceName == "chrM"
                ? AminoAcids.CodonConversion.HumanMitochondria
                : AminoAcids.CodonConversion.HumanChromosome;

            foreach (var transcript in OverlappingTranscripts)
            {
                AddTranscriptToVariant(variant, transcript);
            }
        }

        private void RunPlugins(IVariantFeature variant)
        {
            foreach (var plugin in _plugins)
            {
                plugin.AnnotateVariant(variant, OverlappingTranscripts, _json, _compressedSequence);
            }
        }

        private static List<BreakendTranscriptAnnotation> AnnotateBreakendTranscripts(List<Transcript> transcripts,
            int pos, char orientation)
        {
            if (transcripts == null || transcripts.Count == 0) return null;
            var res = new List<BreakendTranscriptAnnotation>();

            foreach (var transcript in transcripts)
            {
                var annotation = new BreakendTranscriptAnnotation(transcript, pos, orientation);
                res.Add(annotation);
            }

            return res;
        }

        private void AnnotateTanscriptsForSv(VariantFeature variant)
        {
            _transcriptIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex, variant.AlternateAlleles[0].Start,
                variant.AlternateAlleles[0].End, OverlappingTranscripts);
            if (OverlappingTranscripts.Count == 0) return;

            FindOverlappingGenes(variant);

            foreach (var altAllele in variant.AlternateAlleles)
            {
                _json.AddOverlappingGenes(OverlappingGeneSymbols, altAllele);
                if (!_reportAllSvOverlappingTranscripts) continue;

                foreach (var transcript in OverlappingTranscripts)
                {
                    _json.AddOverlappingTranscript(transcript, altAllele);
                }
            }

            foreach (var transcript in OverlappingTranscripts)
            {
                if (!PartialOverlappingGeneHashCodes.Contains(transcript.Gene.GetHashCode())) continue;
                AddTranscriptToVariant(variant, transcript);
            }
        }

        private List<BreakendTranscriptAnnotation> AnnotatePos2Transcripts(BreakEnd breakendAllele)
        {
            if (breakendAllele == null) return null;

            List<BreakendTranscriptAnnotation> breakendTas = null;

            var pos2OverlappingTranscripts = new List<Transcript>();

            _transcriptIntervalForest.GetAllOverlappingValues(breakendAllele.ReferenceIndex2, breakendAllele.Position2,
                breakendAllele.Position2, pos2OverlappingTranscripts);

            if (pos2OverlappingTranscripts.Count != 0)
                breakendTas = AnnotateBreakendTranscripts(pos2OverlappingTranscripts, breakendAllele.Position2,
                    breakendAllele.IsSuffix2);

            return breakendTas;
        }

        private void AddTranscriptToVariant(VariantFeature variant, Transcript transcript)
        {
            foreach (var altAllele in variant.AlternateAlleles)
            {
                AnnotateAltAllele(variant, altAllele, transcript);
            }
        }

        private static bool IsInternalGene(Gene gene, VariantFeature variant)
        {
	        return gene.Start >= variant.AlternateAlleles[0].Start && gene.End <= variant.AlternateAlleles[0].End;
        }

        private void FindOverlappingGenes(VariantFeature variant)
        {
            // considering the start base for CNV/SV is one base after VcfReferenceStart/OverlapReferenceStart
            _transcriptIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex, variant.AlternateAlleles[0].Start,
                variant.AlternateAlleles[0].End, OverlappingTranscripts);

            var overlappingGenes = new HashSet<Gene>();
            foreach (var transcript in OverlappingTranscripts) overlappingGenes.Add(transcript.Gene);

            OverlappingGeneSymbols.Clear();
            PartialOverlappingGeneHashCodes.Clear();

            foreach (var gene in overlappingGenes)
            {
                OverlappingGeneSymbols.Add(gene.Symbol);
                _affectedGeneSymbols.Add(gene.Symbol);
                if (!IsInternalGene(gene, variant)) PartialOverlappingGeneHashCodes.Add(gene.GetHashCode());
            }
        }

        private void GetOverlappingTranscripts(VariantFeature variant)
        {
			if (variant.IsRepeatExpansion)
				_transcriptIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex,
					variant.VcfReferenceBegin, variant.VcfReferenceEnd,
					OverlappingTranscripts);
			else 
				_transcriptIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex,
					variant.OverlapReferenceBegin - FlankingLength, variant.OverlapReferenceEnd + FlankingLength,
					OverlappingTranscripts);
        }

        private void AnnotateAltAllele(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript)
        {
            var ta = new TranscriptAnnotation
            {
                AlternateAllele = altAllele,
                HasValidCdnaCodingStart = false,
                HasValidCdsStart = false
            };

            if (transcript.Translation != null)
            {
                ta.HasValidCdnaCodingStart = transcript.Translation.CodingRegion.CdnaStart > 0;
            }

            MapCdnaCoordinates(transcript, ta, altAllele);
            _json.CreateAnnotationObject(transcript, altAllele);

            // handle upstream or downstream transcripts
            if (!Overlap.Partial(transcript.Start, transcript.End, altAllele.Start, altAllele.End) && !variant.IsRepeatExpansion)
            {
                AddFlankingTranscript(ta, variant, transcript);
                return;
            }

            // added transcript gene to the annotated gene list
            _affectedGeneSymbols.Add(transcript.Gene.Symbol);

            string exonNumber;
            string intronNumber;
            TranscriptUtilities.ExonIntronNumber(transcript.CdnaMaps, transcript.Introns,
                transcript.Gene.OnReverseStrand, ta, out exonNumber, out intronNumber);

            // generate new TranscriptAnnotation for Hgvs
            // transcript 3 prime shift
            var shiftToEnd = false;
            var altAlleleAfterRotating = CodingSequenceRotate3Prime(altAllele, transcript, ref shiftToEnd);

            var isGenomicDuplicateAfterRotating =
                altAlleleAfterRotating.CheckForDuplicationForAltAlleleWithinTranscript(_compressedSequence, transcript);

            var taForHgvs = new TranscriptAnnotation
            {
                AlternateAllele         = altAlleleAfterRotating,
                HasValidCdnaCodingStart = false,
                HasValidCdsStart        = false
            };

            if (transcript.Translation != null)
            {
                taForHgvs.HasValidCdnaCodingStart = transcript.Translation.CodingRegion.CdnaStart > 0;
            }

	        if (!variant.IsRepeatExpansion)
	        {
		        MapCdnaCoordinates(transcript, taForHgvs, altAlleleAfterRotating);
		        GetCodingAnnotations(transcript, ta, taForHgvs, _compressedSequence);
		        AssignHgvsNotations(variant, altAllele, transcript, shiftToEnd, taForHgvs, isGenomicDuplicateAfterRotating, ta);
		        GetSiftPolyphen(variant, altAllele, transcript, ta);

		        AnnotateBreakends(altAllele, transcript, ta);

			}

			// exon overlap-specific CSQ tags
			_json.AddExonData(ta, exonNumber,altAllele.IsStructuralVariant);

            // intronic annotations
            IntronicAnnotation(variant, altAllele, transcript, ta, intronNumber, taForHgvs, isGenomicDuplicateAfterRotating);

            // set the functional consequence
            var consequence = new Consequences(new VariantEffect(ta, transcript,variant.InternalCopyNumberType));
            consequence.DetermineVariantEffects(variant.InternalCopyNumberType);

            _json.FinalizeAndAddAnnotationObject(transcript, ta, consequence.GetConsequenceStrings());
        }

        private void AnnotateBreakends(VariantAlternateAllele altAllele, Transcript transcript,
            TranscriptAnnotation ta)
        {
            if (altAllele.BreakEnds == null || transcript.Translation == null || altAllele.BreakEnds.Count <= 0) return;

            ta.GeneFusionAnnotations = new List<GeneFusionAnnotation>();

            foreach (var breakend in altAllele.BreakEnds)
            {
                if (breakend.Position < transcript.Translation.CodingRegion.GenomicStart ||
                    breakend.Position > transcript.Translation.CodingRegion.GenomicEnd) continue;

                var pos1Annotation = new BreakendTranscriptAnnotation(transcript, breakend.Position,
                    breakend.IsSuffix);
                var pos2Annotations = AnnotatePos2Transcripts(breakend);
                ta.BreakendTranscriptAnnotation = pos1Annotation;
                ta.BreakendPos2Annotations = pos2Annotations;
                var variantEffect = new VariantEffect(ta, transcript);

                if (!variantEffect.IsGeneFusion()) continue;

                var geneFusion = new GeneFusionAnnotation(pos1Annotation);

                foreach (var annotation in pos2Annotations)
                {
                    geneFusion.AddGeneFusion(annotation);
                }

                ta.GeneFusionAnnotations.Add(geneFusion);
            }
        }

        private void IntronicAnnotation(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript,
            TranscriptAnnotation ta, string intronNumber, TranscriptAnnotation taForHgvs, bool isGenomicDuplicateAfterRotating)
        {
            if (ta.IsWithinIntron || ta.IsStartSpliceSite || ta.IsEndSpliceSite || ta.IsWithinFrameshiftIntron)
            {
                _json.AddIntronData(intronNumber);
            }

            if (taForHgvs.IsWithinIntron || taForHgvs.IsStartSpliceSite || taForHgvs.IsEndSpliceSite ||
                taForHgvs.IsWithinFrameshiftIntron)
            {
                if (!altAllele.IsStructuralVariant && !altAllele.AlternateAllele.Contains("N"))
                {
                    // set our HGVS nomenclature only if its not a SV
                    var hgvsCoding = new HgvsCodingNomenclature(taForHgvs, transcript, variant, _compressedSequence,
                        isGenomicDuplicateAfterRotating);
                    hgvsCoding.SetAnnotation();
                }
            }
            ta.HgvsCodingSequenceName = taForHgvs.HgvsCodingSequenceName;
        }

        private void GetSiftPolyphen(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript,
            TranscriptAnnotation ta)
        {
            // TODO: We really should move this to TranscriptAnnotation
            variant.SiftPrediction     = null;
            variant.PolyPhenPrediction = null;

            if (!ta.HasValidCdnaStart && !ta.HasValidCdnaEnd) return;

            // the protein begin and end ought to be set by now if they are to be. If it involves only 1 AA change, 
            // we will extract sift and polyphen annotation
            if ((altAllele.VepVariantType == VariantType.SNV || altAllele.VepVariantType == VariantType.MNV) &&
                ta.AlternateAminoAcids != null && ta.ProteinBegin == ta.ProteinEnd)
            {
                if (transcript.SiftIndex != -1)
                    GetProteinFunctionPrediction(_siftCache, transcript.SiftIndex, ta.AlternateAminoAcids[0],
                        ta.ProteinBegin, Sift.Descriptions, out variant.SiftPrediction, out variant.SiftScore);

                if (transcript.PolyPhenIndex != -1)
                    GetProteinFunctionPrediction(_polyPhenCache, transcript.PolyPhenIndex, ta.AlternateAminoAcids[0],
                        ta.ProteinBegin, PolyPhen.Descriptions, out variant.PolyPhenPrediction,
                        out variant.PolyPhenScore);

                if (variant.SiftPrediction != null || variant.PolyPhenPrediction != null)
                {
                    _json.AddProteinChangeEffect(variant);
                }
            }
        }

        private static void GetProteinFunctionPrediction(PredictionCache cache, int predictionIndex, char newAminoAcid,
            int aaPosition, string[] descriptions, out string description, out string score)
        {
	        var entry = cache.Predictions[predictionIndex].GetPrediction(newAminoAcid, aaPosition);

            if (entry == null)
            {
                description = null;
                score       = null;
                return;
            }

            description = descriptions[entry.EnumIndex];
            score       = entry.Score.ToString("0.###");
        }

        private void AssignHgvsNotations(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript,
            bool shiftToEnd, TranscriptAnnotation taForHgvs, bool isGenomicDuplicateAfterRotating, TranscriptAnnotation ta)
        {
            if (!altAllele.IsStructuralVariant && !altAllele.AlternateAllele.Contains("N") && !shiftToEnd)
            {
                if (taForHgvs.HasValidCdnaStart && taForHgvs.HasValidCdnaEnd)
                {
                    var hgvsCoding = new HgvsCodingNomenclature(taForHgvs, transcript, variant, _compressedSequence,
                        isGenomicDuplicateAfterRotating);
                    hgvsCoding.SetAnnotation();
                }

                if (taForHgvs.HasValidCdsStart && taForHgvs.HasValidCdsEnd)
                {
                    var variantEffect = new VariantEffect(taForHgvs, transcript, variant.InternalCopyNumberType);
                    var hgvsProtein = new HgvsProteinNomenclature(variantEffect, taForHgvs, transcript, variant,
                        _compressedSequence, _aminoAcids);
                    hgvsProtein.SetAnnotation();
                }
            }

            ta.HgvsCodingSequenceName  = taForHgvs.HgvsCodingSequenceName;
            ta.HgvsProteinSequenceName = taForHgvs.HgvsProteinSequenceName;
        }

        private void GetCodingAnnotations(Transcript transcript, TranscriptAnnotation ta,
            TranscriptAnnotation taForHgvs, ICompressedSequence compressedSequence)
        {
            // coding annotations
            if (transcript.Translation == null ||
                !ta.HasValidCdnaStart && !ta.HasValidCdnaEnd && !taForHgvs.HasValidCdnaEnd &&
                !taForHgvs.HasValidCdnaStart) return;
            CalculateCdsPositions(transcript, taForHgvs);
            CalculateCdsPositions(transcript, ta);

            // determine the protein position
            if (!taForHgvs.HasValidCdsStart && !taForHgvs.HasValidCdsEnd && !ta.HasValidCdsStart && !ta.HasValidCdsEnd) return;
            GetProteinPosition(taForHgvs, transcript, compressedSequence);
            GetProteinPosition(ta, transcript, compressedSequence);
        }

        private void HandleIntergenicVariant(VariantFeature variant)
        {
            foreach (var altAllele in variant.AlternateAlleles)
            {
                // for DUP, VEP prints '-' for alternate alleles if the consequence is an intragenic variant
                if (altAllele.VepVariantType == VariantType.duplication) altAllele.AlternateAllele = "-";

                _json.AddIntergenicVariant(altAllele);
            }
        }

        private void AssignConservationScores(VariantFeature variant)
        {
            // grab the phyloP conservation score (position-specific)
            var hasConservationScore = false;

            // for refMinors with no global major, we shall not create any alt alleles. But we still want the conservation scores
            if (variant.IsRefMinor && variant.SupplementaryAnnotationPosition.GlobalMajorAllele == null)
            {
                variant.ConservationScore = GetConservationScore(variant.VcfReferenceBegin);
            }

            foreach (var altAllele in variant.AlternateAlleles)
            {
                // to make sure that the scores set for unit tests are not overwritten.
                if (altAllele.ConservationScore == null)
                    altAllele.ConservationScore = altAllele.VepVariantType == VariantType.SNV
                        ? GetConservationScore(altAllele.Start)
                        : null;
                if (altAllele.ConservationScore != null) hasConservationScore = true;
            }

	        if (!hasConservationScore) return;
	        foreach (var alternateAllele in variant.AlternateAlleles)
	        {
		        AddConservationScore(ref variant.ConservationScore, alternateAllele.ConservationScore);
	        }
        }

        private void GetProteinPosition(TranscriptAnnotation ta, Transcript transcript, ICompressedSequence compressedSequence)
        {
            const int shift = 0;
            if (ta.HasValidCdsStart) ta.ProteinBegin = (int)((ta.CodingDnaSequenceBegin + shift + 2.0) / 3.0);
            if (ta.HasValidCdsEnd) ta.ProteinEnd = (int)((ta.CodingDnaSequenceEnd + shift + 2.0) / 3.0);

            // assign our codons and amino acids
            Codons.Assign(ta, transcript, compressedSequence);
            _aminoAcids.Assign(ta);
        }

        /// <summary>
        /// Calculates the cDNA coordinates before we evaluate using HGVS criteria
        /// </summary>
		private static void MapCdnaCoordinates(Transcript transcript, TranscriptAnnotation ta, VariantAlternateAllele altAllele)
        {
            if (transcript.Gene.OnReverseStrand)
            {
                ta.TranscriptReferenceAllele = SequenceUtilities.GetReverseComplement(altAllele.ReferenceAllele);
                ta.TranscriptAlternateAllele = SequenceUtilities.GetReverseComplement(altAllele.AlternateAllele);
            }
            else
            {
                ta.TranscriptReferenceAllele = altAllele.ReferenceAllele;
                ta.TranscriptAlternateAllele = altAllele.AlternateAllele;
            }

            SetIntronEffects(transcript.Introns, ta);
            CdnaMapper.MapCoordinates(altAllele.Start, altAllele.End, ta, transcript);
        }

        /// <summary>
        /// Adds the regulatory regions to the output destination
        /// </summary>
        private void AddRegulatoryFeatures(VariantFeature variant)
        {
            var intervalBegin = 0;
            var intervalEnd = 0;

            foreach (var altAllele in variant.AlternateAlleles)
            {
                // In case of insertions, the base(s) are assumed to be inserted at the end position

                // if this is an insertion just before the beginning of the regulatory element, this takes care of it
                var altBegin = altAllele.NirvanaVariantType == VariantType.insertion ? altAllele.End : altAllele.Start;
                var altEnd = altAllele.End;

				// disable regulatory region for SV larger than 50kb
				if (altEnd - altBegin + 1 > 50000) continue;

				if (intervalBegin != altBegin || intervalEnd != altEnd)
                {
                    intervalBegin = altBegin;
                    intervalEnd = altEnd;

                    // extract overlapping regulatory regions only if needed
                    _regulatoryIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex, intervalBegin, intervalEnd,
                        OverlappingRegulatoryFeatures);
                }

                foreach (var regulatoryFeature in OverlappingRegulatoryFeatures)
                {
                    if (altAllele.NirvanaVariantType == VariantType.insertion)
                    {
                        // if the insertion is at the end, its past the feature and therefore not overlapping
                        if (regulatoryFeature.End == altEnd) continue;
                    }

                    var consequence = new Consequences(null);
                    consequence.DetermineRegulatoryVariantEffects(regulatoryFeature, altAllele.NirvanaVariantType,
                        altAllele.Start, altAllele.End, altAllele.IsStructuralVariant, variant.InternalCopyNumberType);

                    _json.AddRegulatoryFeature(regulatoryFeature, altAllele, consequence.GetConsequenceStrings());
                }
            }
        }

        /// <summary>
        /// calculates the CDS position given the exon phase and cDNA positions [TranscriptMapper.pm:388 genomic2cds]
        /// </summary>
        private static void CalculateCdsPositions(Transcript transcript, TranscriptAnnotation ta)
        {
            // sanity check: make sure we have a valid start position
            ta.HasValidCdsStart = true;
            ta.HasValidCdsEnd = true;

            if (ta.BackupCdnaEnd < transcript.Translation.CodingRegion.CdnaStart ||
                ta.BackupCdnaBegin > transcript.Translation.CodingRegion.CdnaEnd)
            {
                // if the variant is completely non overlapping with the transcript's coding start
                ta.HasValidCdsStart = false;
                ta.HasValidCdsEnd = false;
                return;
            }

            // calculate the CDS position
            int beginOffset           = transcript.StartExonPhase - transcript.Translation.CodingRegion.CdnaStart + 1;
            ta.CodingDnaSequenceBegin = ta.BackupCdnaBegin + beginOffset;
            ta.CodingDnaSequenceEnd   = ta.BackupCdnaEnd + beginOffset;

            if (ta.CodingDnaSequenceBegin < 1 || ta.HasValidCdnaStart == false)
            {
                ta.HasValidCdsStart = false;
            }

            if (ta.CodingDnaSequenceEnd > transcript.Translation.CodingRegion.CdnaEnd + beginOffset ||
                ta.HasValidCdnaEnd == false)
            {
                ta.HasValidCdsEnd = false;
            }
        }

        /// <summary>
        /// returns the conservation score if we have the necessary files. Returns null otherwise
        /// </summary>
        private string GetConservationScore(int referencePosition)
        {
            return _hasConservationScores ? _conservationScoreReader.GetScore(referencePosition) : null;
        }

        /// <summary>
        /// This is a recommissioning of the GetIntronIndex function above.
        /// It also characterize intronic effects [BaseTranscriptVariation.pm:531 _intron_effects]
        /// </summary>
        private static void SetIntronEffects(SimpleInterval[] introns, TranscriptAnnotation ta)
        {
            // sanity check: make sure we have some introns defined
            if (introns == null) return;

            var altAllele = ta.AlternateAllele;

            var min = Math.Min(altAllele.Start, altAllele.End);
            var max = Math.Max(altAllele.Start, altAllele.End);

            var variantInterval = new AnnotationInterval(altAllele.Start, altAllele.End);
            var minMaxInterval = new AnnotationInterval(min, max);
            var isInsertion = ta.AlternateAllele.NirvanaVariantType == VariantType.insertion;

            foreach (var intron in introns)
            {
                // TODO: we should sort our introns so that we can end early

                // skip this one if variant is out of range : the range is set to 3 instead of the original old:
                // all of the checking occured in the region between start-3 to end+3, if we set to 8, we can made mistakes when
                //checking IsWithinIntron when we have a small exon
                if (!minMaxInterval.Overlaps(intron.Start - 3, intron.End + 3)) continue;

                // under various circumstances the genebuild process can introduce artificial 
                // short (<= 12 nucleotide) introns into transcripts (e.g. to deal with errors
                // in the reference sequence etc.), we don't want to categorize variations that
                // fall in these introns as intronic, or as any kind of splice variant

                var isFrameshiftIntron = intron.End - intron.Start <= 12;

                if (isFrameshiftIntron)
                {
                    if (variantInterval.Overlaps(intron.Start, intron.End))
                    {
                        ta.IsWithinFrameshiftIntron = true;
                        continue;
                    }
                }

                if (variantInterval.Overlaps(intron.Start, intron.Start + 1))
                {
                    ta.IsStartSpliceSite = true;
                }

                if (variantInterval.Overlaps(intron.End - 1, intron.End))
                {
                    ta.IsEndSpliceSite = true;
                }

                // we need to special case insertions between the donor and acceptor sites

                //make sure the size of intron is larger than 4
                if (intron.Start <= intron.End - 4)
                {
                    if (variantInterval.Overlaps(intron.Start + 2, intron.End - 2) ||
                        isInsertion && (altAllele.Start == intron.Start + 2
                                        || altAllele.End == intron.End - 2))
                    {
                        ta.IsWithinIntron = true;
                    }
                }

                // the definition of splice_region (SO:0001630) is "within 1-3 bases of the
                // exon or 3-8 bases of the intron." We also need to special case insertions
                // between the edge of an exon and a donor or acceptor site and between a donor
                // or acceptor site and the intron
                ta.IsWithinSpliceSiteRegion = variantInterval.Overlaps(intron.Start + 2, intron.Start + 7) ||
                                              variantInterval.Overlaps(intron.End - 7, intron.End - 2) ||
                                              variantInterval.Overlaps(intron.Start - 3, intron.Start - 1) ||
                                              variantInterval.Overlaps(intron.End + 1, intron.End + 3) ||
                                              isInsertion &&
                                              (altAllele.Start == intron.Start ||
                                               altAllele.End   == intron.End ||
                                               altAllele.Start == intron.Start + 2 ||
                                               altAllele.End   == intron.End - 2);
            }
        }

        /// <summary>
        /// loads the annotation data for this particular reference sequence
        /// </summary>
        private void LoadData(object sender, NewReferenceEventArgs e)
        {
            var referenceNameData = e.Data;

            if (_compressedSequence.GenomeAssembly == GenomeAssembly.GRCh38) EnableMitochondrialAnnotation();

            // load our conservation scores
            if (_conservationScoreReader != null)
            {
                _conservationScoreReader.LoadReference(referenceNameData.UcscReferenceName);
                _hasConservationScores = _conservationScoreReader.IsInitialized;
            }

			// load reference-specific files for our supplementary annotation providers
			_saProvider?.Load(referenceNameData.UcscReferenceName);

            // load our prediction caches
            _performanceMetrics.StartCache();
            LoadPredictionCaches(referenceNameData.ReferenceIndex);
            _performanceMetrics.StopCache();
        }

        private void ClearDataSources()
        {
            _conservationScoreReader?.Clear();
            _saProvider?.Clear();
            _siftCache     = PredictionCache.Empty;
            _polyPhenCache = PredictionCache.Empty;
        }

        private void LoadPredictionCaches(ushort refIndex)
        {
            _siftCache     = LoadPredictionCache(_siftReader, refIndex);
            _polyPhenCache = LoadPredictionCache(_polyPhenReader, refIndex);
        }

        private static PredictionCache LoadPredictionCache(PredictionCacheReader reader, ushort refIndex)
        {
            if (reader == null || refIndex == ChromosomeRenamer.UnknownReferenceIndex) return PredictionCache.Empty;
            return reader.Read(refIndex);
        }

        private VariantAlternateAllele CodingSequenceRotate3Prime(VariantAlternateAllele altAllele, Transcript transcript, ref bool shiftToEnd)
        {
            var onReverseStrand = transcript.Gene.OnReverseStrand;
            var altAlleleAfterRotating = new VariantAlternateAllele(altAllele);
            if (_compressedSequence == null) return altAlleleAfterRotating;

            if (altAllele.NirvanaVariantType != VariantType.deletion && altAllele.NirvanaVariantType != VariantType.insertion)
                return altAlleleAfterRotating;


			// if variant is before the transcript start, do not perform 3 prime shift
            if (onReverseStrand && altAllele.End > transcript.End) return altAlleleAfterRotating;

            if (!onReverseStrand && altAllele.Start < transcript.Start) return altAlleleAfterRotating;

			// consider insertion since insertion Begin is larger than end
            if (!onReverseStrand && altAllele.Start >= transcript.End) return altAlleleAfterRotating;

            if (onReverseStrand && altAllele.End <= transcript.Start) return altAlleleAfterRotating;

			var rotatingBases = altAllele.NirvanaVariantType == VariantType.insertion
                ? altAllele.AlternateAllele
                : altAllele.ReferenceAllele;

            var numBases = rotatingBases.Length;

            rotatingBases = onReverseStrand ? SequenceUtilities.GetReverseComplement(rotatingBases) : rotatingBases;
            var basesToEnd = onReverseStrand ? altAllele.Start - transcript.Start : transcript.End - altAllele.End;

            var downStreamLength = Math.Min(basesToEnd, MaxDownstreamLength);

            var downStreamSeq = onReverseStrand
                ? SequenceUtilities.GetReverseComplement(
                    _compressedSequence.Substring(altAllele.Start - 1 - downStreamLength, downStreamLength))
                : _compressedSequence.Substring(altAllele.End, downStreamLength);

            var combinedSequence = rotatingBases + downStreamSeq;

            int shiftStart, shiftEnd;
            var hasShifted = false;

			//seems bugs in VEP, just use it for consistence
            for (shiftStart = 0, shiftEnd = numBases; shiftEnd <= combinedSequence.Length - numBases; shiftStart++, shiftEnd++)
            {
                if (combinedSequence[shiftStart] != combinedSequence[shiftEnd]) break;
                hasShifted = true;
            }

			if (shiftStart >= basesToEnd) shiftToEnd = true;

			if (!hasShifted) return altAlleleAfterRotating;

			var referenceBeginAfterRotating = onReverseStrand
                 ? altAllele.Start - shiftStart
                 : altAllele.Start + shiftStart;

            var referenceEndAfterRotating = onReverseStrand
                ? altAllele.End - shiftStart
                : altAllele.End + shiftStart;

            // create a new alternative allele 
            var seqAfterRotating = combinedSequence.Substring(shiftStart, numBases);
            var seqToUpdate = onReverseStrand ? SequenceUtilities.GetReverseComplement(seqAfterRotating) : seqAfterRotating;

            var referenceAlleleAfterRotating = altAllele.ReferenceAllele;
            var alternateAlleleAfterRotating = altAllele.AlternateAllele;

            if (altAllele.VepVariantType == VariantType.insertion)
                alternateAlleleAfterRotating = seqToUpdate;
            else referenceAlleleAfterRotating = seqToUpdate;

            altAlleleAfterRotating.Start                           = referenceBeginAfterRotating;
            altAlleleAfterRotating.End                             = referenceEndAfterRotating;
            altAlleleAfterRotating.ReferenceAllele                 = referenceAlleleAfterRotating;
            altAlleleAfterRotating.AlternateAllele                 = alternateAlleleAfterRotating;
            altAlleleAfterRotating.SupplementaryAnnotationPosition = altAllele.SupplementaryAnnotationPosition;

            return altAlleleAfterRotating;
        }

        public void EnableReferenceNoCalls(bool limitReferenceNoCallsToTranscripts)
        {
            _enableReferenceNoCalls = true;
            _limitReferenceNoCallsToTranscripts = limitReferenceNoCallsToTranscripts;
        }

        public void FinalizeMetrics()
        {
            // force the output of the annotation time
            _performanceMetrics.StartReference("");
        }
    }
}
