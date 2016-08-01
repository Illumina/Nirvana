using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Algorithms.Consequences;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotationSources
{
    public class NirvanaAnnotationSource : IAnnotationSource
    {
        #region members

        private readonly NirvanaDataStore _dataStore;
        private IntervalTree<Transcript> TranscriptIntervalTree { get; }
        private readonly IntervalTree<RegulatoryFeature> _regulatoryIntervalTree;
        private readonly IntervalTree<Gene> _geneIntervalTree;

        private List<Transcript> OverlappingTranscripts { get; }
        private List<CustomInterval> OverlappingCustomIntervals { get; }
        private readonly List<SupplementaryInterval> _overlappingSupplementaryIntervals;
        private List<RegulatoryFeature> OverlappingRegulatoryFeatures { get; }
        private List<Gene> OverlappingGenes { get; }

        private readonly ICompressedSequence _compressedSequence;

        private const int FlankingLength = 5000;

        private readonly Dictionary<string, string> _knownReferenceSequences;

        private readonly bool _useOnlyCanonicalTranscripts;

        private bool _hasConservationScores;
        private bool _hasSupplementaryAnnotations;
        private bool _hasCustomAnnotations;
        private bool _hasCustomIntervals;

        private PhylopReader _conservationScoreReader;
        private SupplementaryAnnotationReader _supplementaryAnnotationReader;
        private List<SupplementaryAnnotationReader> _customAnnotationReaders;

        private readonly IntervalTree<CustomInterval> _customIntervalTree;
        private readonly IntervalTree<SupplementaryInterval> _suppIntervalTree;

        private readonly string _supplementaryAnnotationDir;
        private readonly IEnumerable<string> _customAnnotationDirs;
        private readonly IEnumerable<string> _customIntervalDirs;

        private readonly PerformanceMetrics _performanceMetrics;

        private const int MaxDownstreamLength = 5000;

        private bool _enableReferenceNoCalls;
        private bool _limitReferenceNoCallsToTranscripts;

        private readonly List<DataSourceVersion> _dataSourceVersions;
        private readonly string _nirvanaDataVersion;

        private bool _enableMitochondrialAnnotation;

        private UnifiedJson _json;
        private bool _useAnnotationLoader = true;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        private NirvanaAnnotationSource(string supplementaryAnnotationDir, Dictionary<string, string> refSeqsToPaths,
            bool useOnlyCanonicalTranscripts, IEnumerable<string> customAnnotationDirs = null,
            IEnumerable<string> customIntervalDirs = null)
        {
            _dataStore                         = new NirvanaDataStore();
            TranscriptIntervalTree             = new IntervalTree<Transcript>();
            _regulatoryIntervalTree            = new IntervalTree<RegulatoryFeature>();
            _geneIntervalTree                  = new IntervalTree<Gene>();
            _customIntervalTree                = new IntervalTree<CustomInterval>();
            _suppIntervalTree                  = new IntervalTree<SupplementaryInterval>();
            OverlappingTranscripts             = new List<Transcript>();
            OverlappingCustomIntervals         = new List<CustomInterval>();
            OverlappingRegulatoryFeatures      = new List<RegulatoryFeature>();
            OverlappingGenes                   = new List<Gene>();
            _overlappingSupplementaryIntervals = new List<SupplementaryInterval>();
            _knownReferenceSequences           = refSeqsToPaths ?? new Dictionary<string, string>();
            _useOnlyCanonicalTranscripts       = useOnlyCanonicalTranscripts;
            _supplementaryAnnotationDir        = supplementaryAnnotationDir;
            _customAnnotationDirs              = customAnnotationDirs;
            _customIntervalDirs                = customIntervalDirs;
            _customAnnotationReaders           = new List<SupplementaryAnnotationReader>();

            _compressedSequence = AnnotationLoader.Instance.CompressedSequence;
            _performanceMetrics = PerformanceMetrics.Instance;

            AnnotationLoader.Instance.Changed += LoadData;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public NirvanaAnnotationSource(IAnnotatorPaths annotatorPaths) : this(annotatorPaths.SupplementaryAnnotation, null, false, annotatorPaths.CustomAnnotation, annotatorPaths.CustomIntervals)
        {
            _performanceMetrics = PerformanceMetrics.Instance;

            _dataSourceVersions = new List<DataSourceVersion>();
            CacheDirectory cacheDirectory;
            SupplementaryAnnotationDirectory saDirectory;

	        var observedGenomeAssemblies = new HashSet<GenomeAssembly>();

			NirvanaDatabaseCommon.CheckDirectoryIntegrity(annotatorPaths.Cache, _dataSourceVersions, out cacheDirectory);
	        observedGenomeAssemblies.Add(cacheDirectory.GenomeAssembly);

	        PhylopDirectory phylopDirectory;
            PhylopCommon.CheckDirectoryIntegrity(annotatorPaths.SupplementaryAnnotation, _dataSourceVersions, out phylopDirectory);

	        if (phylopDirectory != null)
		        observedGenomeAssemblies.Add(phylopDirectory.GenomeAssembly);

            SupplementaryAnnotationCommon.CheckDirectoryIntegrity(annotatorPaths.SupplementaryAnnotation, _dataSourceVersions, out saDirectory);

			if (saDirectory?.GenomeAssembly != null) observedGenomeAssemblies.Add(saDirectory.GenomeAssembly);

			foreach (var caPath in annotatorPaths.CustomAnnotation)
            {
                SupplementaryAnnotationCommon.CheckDirectoryIntegrity(caPath, _dataSourceVersions, out saDirectory);
            }

	        if (saDirectory?.GenomeAssembly != null) observedGenomeAssemblies.Add(saDirectory.GenomeAssembly);
			

            ushort saDataVersion = saDirectory?.DataVersion ?? 0;
            _nirvanaDataVersion = cacheDirectory.GetVepVersion(saDataVersion);

            _knownReferenceSequences = cacheDirectory.RefSeqsToPaths;
			AnnotationLoader.Instance.Clear();
            AnnotationLoader.Instance.LoadCompressedSequence(annotatorPaths.CompressedReference);

	        observedGenomeAssemblies.Add(AnnotationLoader.Instance.GenomeAssembly);

	        if (observedGenomeAssemblies.Count > 1)
	        {
		        throw new UserErrorException("Found more than one genome assemblies from the input ref cache,sa or phylop");
	        }

        }

        public IEnumerable<IDataSourceVersion> GetDataSourceVersions()
        {
            return _dataSourceVersions;
        }

        internal void SetSupplementaryAnnotationReader(SupplementaryAnnotationReader saReader)
        {
            _hasSupplementaryAnnotations = true;
            _supplementaryAnnotationReader = saReader;

            var suppIntervals = saReader.GetSupplementaryIntervals();
            if (suppIntervals == null) return;

            foreach (var interval in suppIntervals)
            {
                _suppIntervalTree.Add(new IntervalTree<SupplementaryInterval>.Interval(interval.ReferenceName, interval.Start, interval.End, interval));
            }
        }

        internal void SetCustomAnnotationReader(List<SupplementaryAnnotationReader> caReaders)
        {
            _hasCustomAnnotations = true;
            _customAnnotationReaders = caReaders;
        }

        /// <summary>
        /// adds upstream and downstream transcripts to our results
        /// </summary>
        private void AddFlankingTranscript(TranscriptAnnotation ta, VariantFeature variant, Transcript transcript)
        {
            var altAllele = ta.AlternateAllele;

            // flanking distance doesn't seem to exhaustively use distance like distanceToVariant
            int flankingDistance = Math.Min(Math.Abs(transcript.Start - altAllele.ReferenceEnd),
                Math.Abs(transcript.End - altAllele.ReferenceBegin));

            if (flankingDistance > FlankingLength) return;

            // figure out if we should label this as a downstream or upstream transcript
            bool isDownstream = altAllele.ReferenceEnd < transcript.Start == transcript.OnReverseStrand;

            // determine the cDNA position                        
            CdnaMapper.MapCoordinates(altAllele.ReferenceBegin, altAllele.ReferenceEnd, ta, transcript);

            // set the functional consequence
            var consequence = new Consequences(new VariantEffect(ta, transcript,variant.InternalCopyNumberType));
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
                if (variant.AlternateAlleles.Any(altAllele => overlapInterval.Overlaps(altAllele.ReferenceBegin, altAllele.ReferenceEnd))) return true;
            }

            return false;
        }
        /// <summary>
        /// returns the annotator version
        /// </summary>
        public static string GetVersion()
        {
            return "Nirvana " + CommandLineUtilities.Version;
        }

        private static void AddConservationScore(ref string varConservationScore, string altConservationScore)
        {
            if (string.IsNullOrEmpty(varConservationScore))
            {
                varConservationScore = altConservationScore ?? ".";
                return;
            }

            if (altConservationScore == null)
                varConservationScore += ",.";
            else
                varConservationScore += "," + altConservationScore;
        }

        /// <summary>
        /// returns an annotated variant given a raw variant
        /// </summary>
        public IAnnotatedVariant Annotate(IVariant variant)
        {
            if (variant == null) return null;
            var variantFeature = new VariantFeature(variant as VcfVariant, _enableReferenceNoCalls, _limitReferenceNoCallsToTranscripts, TranscriptIntervalTree);

            _json = new UnifiedJson(variantFeature);

            if (_useAnnotationLoader) AnnotationLoader.Instance.Load(variantFeature.UcscReferenceName);

            Annotate(variantFeature);

            _performanceMetrics.Increment();

            return _json;
        }

        /// <summary>
        /// adds annotations to the variant feature object
        /// </summary>
        private void Annotate(VariantFeature variant)
        {
            if (variant.IsReference && !variant.IsSingletonRefSite && !variant.IsRefNoCall) return;

            if ((variant.IsReference && !variant.IsSingletonRefSite && variant.IsRefNoCall) ||
                (variant.UcscReferenceName == "chrM" && !_enableMitochondrialAnnotation))
            {
                _json.AddVariantData(variant);
                return;
            }

            // retrieve the supplementary annotations
            if (_hasSupplementaryAnnotations)
            {
                // get overlapping supplementary intervals.
                _overlappingSupplementaryIntervals.Clear();

                if (variant.IsStructuralVariant)
                {
                    var variantBegin = variant.AlternateAlleles[0].NirvanaVariantType == VariantType.insertion
                        ? variant.AlternateAlleles[0].ReferenceEnd
                        : variant.AlternateAlleles[0].ReferenceBegin;
                    var variantEnd = variant.AlternateAlleles[0].ReferenceEnd;
                    var suppInterval = new IntervalTree<SupplementaryInterval>.Interval(variant.EnsemblReferenceName,
                        variantBegin, variantEnd);
                    _suppIntervalTree.GetAllOverlappingValues(suppInterval, _overlappingSupplementaryIntervals);

                    variant.AddSupplementaryIntervals(_overlappingSupplementaryIntervals);
                }
                else variant.SetSupplementaryAnnotation(_supplementaryAnnotationReader);
            }
		
			// return if this is not a ref but not ref minor
			if (variant.IsReference && !variant.IsRefMinor)
			{
				_json.AddVariantData(variant);
				return;
			}



			if (_hasCustomAnnotations) variant.AddCustomAnnotation(_customAnnotationReaders);

			AssignConservationScores(variant);
			AssignCytogeneticBand(variant);
			AssignConservationScores(variant);
			// grab overlapping custom intervals
			GetCustomIntervals(variant);

			_json.AddVariantData(variant);

          
			AddRegulatoryFeatures(variant);

			// add overlapping genes for CNV or SVs
			if (variant.IsStructuralVariant)
            {
                ExtractTanscriptsForCnv(variant);
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
            if (_compressedSequence != null) variant.CheckForGenomicDuplicates();

            // setting the protein coding scheme 
            AminoAcids.CodonConversionScheme = variant.UcscReferenceName == "chrM" ? AminoAcids.CodonConversion.HumanMitochondria : AminoAcids.CodonConversion.HumanChromosome;

            foreach (var transcript in OverlappingTranscripts)
            {
                AddTranscriptToVariant(variant, transcript);
            }
        }

        private void ExtractTanscriptsForCnv(VariantFeature variant)
        {
            GetOverlappingGenes(variant);

            if (OverlappingGenes.Count <= 0) return;

            foreach (var altAllele in variant.AlternateAlleles)
            {
                _json.AddOverlappingGenes(OverlappingGenes, altAllele);
            }

            // get transcripts for partial overlap genes
            OverlappingTranscripts.Clear();

            var partialOverlappingGenes = new HashSet<Tuple<string, int, int>>();
            foreach (var gene in OverlappingGenes.Where(gene => !InternalGene(gene, variant)))
            {
                partialOverlappingGenes.Add(Tuple.Create(gene.Symbol, gene.Start, gene.End));
            }
            var transcriptInterval = new IntervalTree<Transcript>.Interval(variant.UcscReferenceName,
                variant.AlternateAlleles[0].ReferenceBegin, variant.AlternateAlleles[0].ReferenceEnd);
            TranscriptIntervalTree.GetAllOverlappingValues(transcriptInterval, OverlappingTranscripts);

            foreach (Transcript transcript in OverlappingTranscripts.Where(transcript => partialOverlappingGenes.Contains(Tuple.Create(transcript.GeneSymbol, transcript.GeneStart, transcript.GeneEnd))))
            {
                AddTranscriptToVariant(variant, transcript);
            }
        }

        private void AddTranscriptToVariant(VariantFeature variant, Transcript transcript)
        {
            // skip non-canonical transcripts in some cases
            if (_useOnlyCanonicalTranscripts && !transcript.IsCanonical) return;

            // evaluate each alternate allele
            // Parallel.ForEach(variant.AlternateAlleles, altAllele =>
            //{
            //    AnnotateAltAllele(variant, altAllele, transcript);
            //});

            foreach (var altAllele in variant.AlternateAlleles)
            {
                AnnotateAltAllele(variant, altAllele, transcript);
            }
        }

        private static bool InternalGene(Gene gene, VariantFeature variant)
        {
            if (gene.Start >= variant.AlternateAlleles[0].ReferenceBegin && gene.End <= variant.AlternateAlleles[0].ReferenceEnd) return true;
            return false;
        }

        private void GetOverlappingGenes(VariantFeature variant)
        {
            // cosidering the start base for CNV/SV is one base after VcfReferenceStart/OverlapReferenceStart
            OverlappingGenes.Clear();
            var geneInterval = new IntervalTree<Gene>.Interval(variant.UcscReferenceName, variant.AlternateAlleles[0].ReferenceBegin, variant.AlternateAlleles[0].ReferenceEnd);
            _geneIntervalTree.GetAllOverlappingValues(geneInterval, OverlappingGenes);
        }
        private void GetOverlappingTranscripts(VariantFeature variant)
        {
            OverlappingTranscripts.Clear();

            // grab the overlapping transcripts (including non-overlapping upstream and downstream)
            var transcriptInterval = new IntervalTree<Transcript>.Interval(variant.UcscReferenceName,
                variant.OverlapReferenceBegin - FlankingLength, variant.OverlapReferenceEnd + FlankingLength);
            TranscriptIntervalTree.GetAllOverlappingValues(transcriptInterval, OverlappingTranscripts);
        }

        private void AnnotateAltAllele(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript)
        {
            var ta = new TranscriptAnnotation
            {
                AlternateAllele = altAllele,
                HasValidCdnaCodingStart = transcript.CompDnaCodingStart > 0,
                HasValidCdsStart = false
            };

            GetAnnotationBeforeHgvs(transcript, ta, altAllele);
            _json.CreateAnnotationObject(transcript, altAllele);

            // handle upstream or downstream transcripts
            if (!transcript.Overlaps(altAllele.ReferenceBegin, altAllele.ReferenceEnd))
            {
                AddFlankingTranscript(ta, variant, transcript);
                return;
            }

            string exonNumber;
            string intronNumber;
            transcript.ExonIntronNumber(ta, out exonNumber, out intronNumber);

            // generate new TranscriptAnnotation for Hgvs
            // transcript 3 prime shift
            bool shiftToEnd = false;
            var altAlleleAfterRotating = CodingSequenceRotate3Prime(altAllele, transcript, ref shiftToEnd);

            bool isGenomicDuplicateAfterRotating =
                altAlleleAfterRotating.CheckForDuplicationForAltAlleleWithinTranscript(transcript);

            var taForHgvs = new TranscriptAnnotation
            {
                AlternateAllele = altAlleleAfterRotating,
                HasValidCdnaCodingStart = transcript.CompDnaCodingStart > 0,
                HasValidCdsStart = false
            };

            GetAnnotationBeforeHgvs(transcript, taForHgvs, altAlleleAfterRotating);

            GetCodingAnnotations(transcript, ta, taForHgvs);

            AssignHgvsNotations(variant, altAllele, transcript, shiftToEnd, taForHgvs, isGenomicDuplicateAfterRotating, ta);

            GetSiftPolyphen(variant, altAllele, transcript, ta);

            // exon overlap-specific CSQ tags
            _json.AddExonData(ta, exonNumber);

            // intronic annotations
            IntronicAnnotation(variant, altAllele, transcript, ta, intronNumber, taForHgvs, isGenomicDuplicateAfterRotating);

            // set the functional consequence
            var consequence = new Consequences(new VariantEffect(ta, transcript,variant.InternalCopyNumberType));
            consequence.DetermineVariantEffects(variant.InternalCopyNumberType);

            _json.FinalizeAndAddAnnotationObject(transcript, ta, consequence.GetConsequenceStrings());
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
                    var hgvsCoding = new HgvsCodingNomenclature(taForHgvs, transcript, variant, isGenomicDuplicateAfterRotating);
                    hgvsCoding.SetAnnotation();
                }
            }
            ta.HgvsCodingSequenceName = taForHgvs.HgvsCodingSequenceName;
        }

        private void GetSiftPolyphen(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript,
            TranscriptAnnotation ta)
        {
            if (!ta.HasValidCdnaStart && !ta.HasValidCdnaEnd) return;
            // the protein begin and end ought to be set by now if they are to be. If it involves only 1 AA change, we will extract sift and polyphen annotation
            if ((altAllele.VepVariantType == VariantType.SNV || altAllele.VepVariantType == VariantType.MNV) &&
                ta.AlternateAminoAcids != null
                && ta.ProteinBegin == ta.ProteinEnd)
            {
                variant.SiftPrediction = null;
                variant.PolyPhenPrediction = null;

                transcript.Sift?.GetPrediction(ta.AlternateAminoAcids[0], ta.ProteinBegin, out variant.SiftPrediction,
                    out variant.SiftScore);
                transcript.PolyPhen?.GetPrediction(ta.AlternateAminoAcids[0], ta.ProteinBegin, out variant.PolyPhenPrediction,
                    out variant.PolyPhenScore);

                if (variant.SiftPrediction != null || variant.PolyPhenPrediction != null)
                {
                    _json.AddProteinChangeEffect(variant);
                }
            }
        }

        private static void AssignHgvsNotations(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript,
            bool shiftToEnd, TranscriptAnnotation taForHgvs, bool isGenomicDuplicateAfterRotating, TranscriptAnnotation ta)
        {
            if (!altAllele.IsStructuralVariant && !altAllele.AlternateAllele.Contains("N") && !shiftToEnd)
            {
                if (taForHgvs.HasValidCdnaStart && taForHgvs.HasValidCdnaEnd)
                {
                    var hgvsCoding = new HgvsCodingNomenclature(taForHgvs, transcript, variant, isGenomicDuplicateAfterRotating);
                    hgvsCoding.SetAnnotation();
                }

                if (taForHgvs.HasValidCdsStart && taForHgvs.HasValidCdsEnd)
                {
                    var variantEffect = new VariantEffect(taForHgvs, transcript,variant.InternalCopyNumberType);
                    var hgvsProtein = new HgvsProteinNomenclature(variantEffect, taForHgvs, transcript, variant);
                    hgvsProtein.SetAnnotation();
                }
            }

            ta.HgvsCodingSequenceName = taForHgvs.HgvsCodingSequenceName;
            ta.HgvsProteinSequenceName = taForHgvs.HgvsProteinSequenceName;
        }

        private static void GetCodingAnnotations(Transcript transcript, TranscriptAnnotation ta, TranscriptAnnotation taForHgvs)
        {
            // coding annotations
            if (!ta.HasValidCdnaStart && !ta.HasValidCdnaEnd && !taForHgvs.HasValidCdnaEnd && !taForHgvs.HasValidCdnaStart)
                return;
            CalculateCdsPositions(transcript, taForHgvs);
            CalculateCdsPositions(transcript, ta);

            // determine the protein position
            if (!taForHgvs.HasValidCdsStart && !taForHgvs.HasValidCdsEnd && !ta.HasValidCdsStart && !ta.HasValidCdsEnd)
                return;
            GetProteinPosition(taForHgvs, transcript);
            GetProteinPosition(ta, transcript);
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

        /// <summary>
        /// assigns the cytogenetic band
        /// </summary>
        private static void AssignCytogeneticBand(VariantFeature variant)
        {
            var cytogeneticBands = AnnotationLoader.Instance.CytogeneticBands;
            if (cytogeneticBands == null) return;

            variant.CytogeneticBand = cytogeneticBands.GetCytogeneticBand(variant.EnsemblReferenceName,
                variant.VcfReferenceBegin, variant.VcfReferenceEnd);
        }

        private void AssignConservationScores(VariantFeature variant)
        {
            // grab the phyloP conservation score (position-specific)
            bool hasConservationScore = false;

            // for refMinors with no global major, we shall not create any alt alleles. But we still want the conservation scores
            if (variant.IsRefMinor && variant.SupplementaryAnnotation.GlobalMajorAllele == null)
            {
                variant.ConservationScore = GetConservationScore(variant.VcfReferenceBegin);
            }

            foreach (var altAllele in variant.AlternateAlleles)
            {
                // to make sure that the scores set for unit tests are not overwritten.
                if (altAllele.ConservationScore == null)
                    altAllele.ConservationScore = altAllele.VepVariantType == VariantType.SNV
                        ? GetConservationScore(altAllele.ReferenceBegin)
                        : null;
                if (altAllele.ConservationScore != null) hasConservationScore = true;
            }

            if (hasConservationScore)
            {
                foreach (var alternateAllele in variant.AlternateAlleles)
                {
                    AddConservationScore(ref variant.ConservationScore, alternateAllele.ConservationScore);
                }
            }
        }

        private void GetCustomIntervals(VariantFeature variant)
        {
            if (!_hasCustomIntervals) return;

            OverlappingCustomIntervals.Clear();
            var customInterval = new IntervalTree<CustomInterval>.Interval(variant.UcscReferenceName,
                variant.OverlapReferenceBegin, variant.OverlapReferenceEnd);
            _customIntervalTree.GetAllOverlappingValues(customInterval, OverlappingCustomIntervals);

            if (OverlappingCustomIntervals.Count > 0)
                foreach (var altAllele in variant.AlternateAlleles)
                    altAllele.AddCustomIntervals(OverlappingCustomIntervals);
        }

        private static void GetProteinPosition(TranscriptAnnotation ta, Transcript transcript)
        {
            const int shift = 0;
            if (ta.HasValidCdsStart) ta.ProteinBegin = (int)((ta.CodingDnaSequenceBegin + shift + 2.0) / 3.0);
            if (ta.HasValidCdsEnd) ta.ProteinEnd = (int)((ta.CodingDnaSequenceEnd + shift + 2.0) / 3.0);

            // assign our codons and amino acids
            Codons.Assign(ta, transcript);
            AminoAcids.Assign(ta);
        }

        /// <summary>
        /// Calculates the cDNA coordinates before we evaluate using HGVS criteria
        /// </summary>
		private static void GetAnnotationBeforeHgvs(Transcript transcript, TranscriptAnnotation ta, VariantAlternateAllele altAllele)
        {
            if (transcript.OnReverseStrand)
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
            CdnaMapper.MapCoordinates(altAllele.ReferenceBegin, altAllele.ReferenceEnd, ta, transcript);
        }

        /// <summary>
        /// Adds the regulatory regions to the output destination
        /// </summary>
        private void AddRegulatoryFeatures(VariantFeature variant)
        {
            int intervalBegin = 0;
            int intervalEnd = 0;

            foreach (var altAllele in variant.AlternateAlleles)
            {
                // In case of insertions, the base(s) are assumed to be inserted at the end position

                // if this is an insertion just before the beginning of the regulatory element, this takes care of it
                var altBegin = altAllele.NirvanaVariantType == VariantType.insertion ? altAllele.ReferenceEnd : altAllele.ReferenceBegin;
                var altEnd = altAllele.ReferenceEnd;

                if (intervalBegin != altBegin || intervalEnd != altEnd)
                {
                    intervalBegin = altBegin;
                    intervalEnd = altEnd;
                    // extract overlapping regulatory regions only if needed
                    OverlappingRegulatoryFeatures.Clear();

                    var regulatoryInterval = new IntervalTree<RegulatoryFeature>.Interval(variant.UcscReferenceName, intervalBegin, intervalEnd);
                    _regulatoryIntervalTree.GetAllOverlappingValues(regulatoryInterval, OverlappingRegulatoryFeatures);
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
                        altAllele.ReferenceBegin, altAllele.ReferenceEnd, altAllele.IsStructuralVariant,variant.InternalCopyNumberType);

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

            if ((ta.BackupCdnaEnd < transcript.CompDnaCodingStart) ||
                (ta.BackupCdnaBegin > transcript.CompDnaCodingEnd))
            {
                // if the variant is completely non overlapping with the transcript's coding start
                ta.HasValidCdsStart = false;
                ta.HasValidCdsEnd = false;
                return;
            }

            // calculate the CDS position
            int posStartExonPhase;
            if (transcript.StartExon == null)
            {
                posStartExonPhase = 0;
            }
            else posStartExonPhase = transcript.StartExon.Phase > 0
                ? transcript.StartExon.Phase
                : 0;

            int beginOffset = posStartExonPhase - transcript.CompDnaCodingStart + 1;
            ta.CodingDnaSequenceBegin = ta.BackupCdnaBegin + beginOffset;
            ta.CodingDnaSequenceEnd = ta.BackupCdnaEnd + beginOffset;

            if (ta.CodingDnaSequenceBegin < 1 || ta.HasValidCdnaStart == false)
            {
                ta.HasValidCdsStart = false;
            }
            if ((ta.CodingDnaSequenceEnd > transcript.CompDnaCodingEnd + beginOffset)
                || ta.HasValidCdnaEnd == false)
            {
                ta.HasValidCdsEnd = false;
            }
        }

        /// <summary>
        /// returns the conservation score if we have the necessary files. Returns null otherwise
        /// </summary>
        private string GetConservationScore(int referencePosition)
        {
            //return _hasConservationScores ? _conservationScoreReader.GetPhylopScore(referencePosition) : null;
            return _hasConservationScores ? _conservationScoreReader.GetScore(referencePosition) : null;
        }

        /// <summary>
        /// This is a recommissioning of the GetIntronIndex function above.
        /// It also characterize intronic effects [BaseTranscriptVariation.pm:531 _intron_effects]
        /// </summary>
        private static void SetIntronEffects(Intron[] introns, TranscriptAnnotation ta)
        {
            // sanity check: make sure we have some introns defined
            if (introns == null) return;

            var altAllele = ta.AlternateAllele;

            int min = Math.Min(altAllele.ReferenceBegin, altAllele.ReferenceEnd);
            int max = Math.Max(altAllele.ReferenceBegin, altAllele.ReferenceEnd);

            var variantInterval = new AnnotationInterval(altAllele.ReferenceBegin, altAllele.ReferenceEnd);
            var minMaxInterval = new AnnotationInterval(min, max);
            bool isInsertion = ta.AlternateAllele.NirvanaVariantType == VariantType.insertion;

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
                        (isInsertion && ((altAllele.ReferenceBegin == intron.Start + 2)
                                         || (altAllele.ReferenceEnd == intron.End - 2))))
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
                                              ((altAllele.ReferenceBegin == intron.Start) ||
                                               (altAllele.ReferenceEnd == intron.End) ||
                                               (altAllele.ReferenceBegin == intron.Start + 2) ||
                                               (altAllele.ReferenceEnd == intron.End - 2));
            }
        }

        /// <summary>
        /// loads the annotation data for this particular reference sequence
        /// </summary>
        private void 
			LoadData(object sender, EventArgs e)
        {
            var ucscReferenceName = AnnotationLoader.Instance.CurrentReferenceName;
	        var ensemblReferenceName = AnnotationLoader.Instance.ChromosomeRenamer.GetEnsemblReferenceName(ucscReferenceName);

            if (AnnotationLoader.Instance.GenomeAssembly == GenomeAssembly.GRCh38)
                EnableMitochondrialAnnotation();

            // add conservation score support to the data store
            _hasConservationScores = false;

            if (!string.IsNullOrEmpty(_supplementaryAnnotationDir))
            {
                string conservationScorePath = Path.Combine(_supplementaryAnnotationDir, ucscReferenceName + ".npd");

                if (File.Exists(conservationScorePath))
                {
                    _conservationScoreReader = new PhylopReader(new BinaryReader(File.Open(conservationScorePath, FileMode.Open, FileAccess.Read, FileShare.Read)));
                    _hasConservationScores = true;
                }
            }

            if (!_hasConservationScores) _conservationScoreReader = null;

            // add supplementary database support to the data store
            _hasSupplementaryAnnotations = false;

            if (!string.IsNullOrEmpty(_supplementaryAnnotationDir))
            {
                string supplementaryAnnotationPath = Path.Combine(_supplementaryAnnotationDir, ucscReferenceName + ".nsa");

                if (File.Exists(supplementaryAnnotationPath))
                {
                    _hasSupplementaryAnnotations = true;
                    _supplementaryAnnotationReader = new SupplementaryAnnotationReader(supplementaryAnnotationPath);
                    _suppIntervalTree.Clear();
                    var intervalList = _supplementaryAnnotationReader.GetSupplementaryIntervals();

                    if (intervalList != null)
                        foreach (var interval in intervalList)
                        {
                            _suppIntervalTree.Add(new IntervalTree<SupplementaryInterval>.Interval(ensemblReferenceName, interval.Start, interval.End, interval));
                        }

                }
            }

            if (!_hasSupplementaryAnnotations) _supplementaryAnnotationReader = null;

            // support for multiple custom annotation dbs
            if (_customAnnotationDirs != null)
            {
                foreach (var customAnnotationPath in _customAnnotationDirs)
                {
                    if (string.IsNullOrEmpty(customAnnotationPath)) continue;
                    var fullCustomAnnotationPath = Path.Combine(customAnnotationPath, ucscReferenceName + ".nsa");

                    if (!File.Exists(fullCustomAnnotationPath)) continue;
                    _customAnnotationReaders.Add(new SupplementaryAnnotationReader(fullCustomAnnotationPath));
                    _hasCustomAnnotations = true;
                }
            }

            if (!_hasCustomAnnotations) _customAnnotationReaders.Clear();

            _customIntervalTree.Clear();

            // support for multiple custom interval dbs
            if (_customIntervalDirs != null)
            {
                foreach (var customIntervalPath in _customIntervalDirs)
                {
                    if (!string.IsNullOrEmpty(customIntervalPath))
                    {
                        string fullcustomIntervalPath = Path.Combine(customIntervalPath, ucscReferenceName + ".nci");

                        if (File.Exists(fullcustomIntervalPath))
                        {
                            var customIntervalReader = new CustomIntervalReader(fullcustomIntervalPath);

                            var customInterval = customIntervalReader.GetNextCustomInterval();

                            while (customInterval != null)
                            {
                                _customIntervalTree.Add(new IntervalTree<CustomInterval>.Interval(ucscReferenceName, customInterval.Start, customInterval.End, customInterval));
                                customInterval = customIntervalReader.GetNextCustomInterval();
                            }

                            _hasCustomIntervals = true;
                        }
                    }
                }
            }

            // grab the path for the reference sequence database
            string refSeqPath;
            if (!_knownReferenceSequences.TryGetValue(ucscReferenceName, out refSeqPath))
            {
                Console.WriteLine($"Unable to find the file associated with the supplied reference sequence ({ucscReferenceName})");
                return;
            }
            _performanceMetrics.StartCache();

            // populate the data store with our VEP annotations
            using (var reader = new NirvanaDatabaseReader(refSeqPath))
            {
                reader.PopulateData(_dataStore, TranscriptIntervalTree, _regulatoryIntervalTree, _geneIntervalTree);
            }

            _performanceMetrics.StopCache();
        }

        /// <summary>
        /// loads the annotation data for this particular reference sequence
        /// </summary>
        public void LoadData(Stream stream)
        {
            using (var reader = new NirvanaDatabaseReader(stream))
            {
                reader.PopulateData(_dataStore, TranscriptIntervalTree, _regulatoryIntervalTree, _geneIntervalTree);
            }

            // TODO: add the ability to refer to our conservation score database by stream
        }

        private VariantAlternateAllele CodingSequenceRotate3Prime(VariantAlternateAllele altAllele, Transcript transcript, ref bool shiftToEnd)
        {

            bool onReverseStrand = transcript.OnReverseStrand;
            var altAlleleAfterRotating = new VariantAlternateAllele(altAllele);
            if (_compressedSequence == null) return altAlleleAfterRotating;

            if (altAllele.VepVariantType != VariantType.deletion && altAllele.VepVariantType != VariantType.insertion)
                return altAlleleAfterRotating;

            // insertionDeletion is not correctly handled here
            if ((altAllele.VepVariantType == VariantType.deletion) && (altAllele.AlternateAllele != "")) return altAlleleAfterRotating;
            if ((altAllele.VepVariantType == VariantType.insertion) && (altAllele.ReferenceAllele != "")) return altAlleleAfterRotating;

            // if variant is before the transcript start, do not perform 3 prime shift
            if (onReverseStrand && altAllele.ReferenceEnd > transcript.End) return altAlleleAfterRotating;

            if (!onReverseStrand && altAllele.ReferenceBegin < transcript.Start) return altAlleleAfterRotating;

            // consider insertion since insertion Begin is larger than end
            if (!onReverseStrand && altAllele.ReferenceBegin >= transcript.End) return altAlleleAfterRotating;

            if (onReverseStrand && altAllele.ReferenceEnd <= transcript.Start) return altAlleleAfterRotating;

            var rotatingBases = altAllele.VepVariantType == VariantType.insertion
                ? altAllele.AlternateAllele
                : altAllele.ReferenceAllele;

            int numBases = rotatingBases.Length;

            rotatingBases = onReverseStrand ? SequenceUtilities.GetReverseComplement(rotatingBases) : rotatingBases;
            int downStreamLength = onReverseStrand ? altAllele.ReferenceBegin - transcript.Start : transcript.End - altAllele.ReferenceEnd;

            downStreamLength = Math.Min(downStreamLength, MaxDownstreamLength);

            var downStreamSeq = onReverseStrand
                ? SequenceUtilities.GetReverseComplement(
                    _compressedSequence.Substring(altAllele.ReferenceBegin - 1 - downStreamLength, downStreamLength))
                : _compressedSequence.Substring(altAllele.ReferenceEnd, downStreamLength);

            var combinedSequence = rotatingBases + downStreamSeq;

            int shiftStart, shiftEnd;
            var hasShifted = false;

            for (shiftStart = 0, shiftEnd = numBases; shiftEnd <= combinedSequence.Length - numBases; shiftStart++, shiftEnd++)
            {
                if (combinedSequence[shiftStart] != combinedSequence[shiftEnd]) break;
                hasShifted = true;
            }

            if (shiftStart >= combinedSequence.Length - numBases) shiftToEnd = true;

            if (!hasShifted) return altAlleleAfterRotating;

            var referenceBeginAfterRotating = onReverseStrand
                 ? altAllele.ReferenceBegin - shiftStart
                 : altAllele.ReferenceBegin + shiftStart;

            int referenceEndAfterRotating = onReverseStrand
                ? altAllele.ReferenceEnd - shiftStart
                : altAllele.ReferenceEnd + shiftStart;

            // create a new alternative allele 
            string seqAfterRotating = combinedSequence.Substring(shiftStart, numBases);
            string seqToUpdate = onReverseStrand ? SequenceUtilities.GetReverseComplement(seqAfterRotating) : seqAfterRotating;

            string referenceAlleleAfterRotating = altAllele.ReferenceAllele;
            string alternateAlleleAfterRotating = altAllele.AlternateAllele;

            if (altAllele.VepVariantType == VariantType.insertion)
                alternateAlleleAfterRotating = seqToUpdate;
            else referenceAlleleAfterRotating = seqToUpdate;

            altAlleleAfterRotating.ReferenceBegin = referenceBeginAfterRotating;
            altAlleleAfterRotating.ReferenceEnd = referenceEndAfterRotating;
            altAlleleAfterRotating.ReferenceAllele = referenceAlleleAfterRotating;
            altAlleleAfterRotating.AlternateAllele = alternateAlleleAfterRotating;
            altAlleleAfterRotating.SupplementaryAnnotation = altAllele.SupplementaryAnnotation;

            return altAlleleAfterRotating;
        }

        string IAnnotationSource.GetDataVersion()
        {
            return _nirvanaDataVersion;
        }

        /// <summary>
        /// disables the annotation loader (useful when unit testing)
        /// </summary>
        public void DisableAnnotationLoader()
        {
            _useAnnotationLoader = false;
        }

        public void EnableReferenceNoCalls(bool limitReferenceNoCallsToTranscripts)
        {
            _enableReferenceNoCalls = true;
            _limitReferenceNoCallsToTranscripts = limitReferenceNoCallsToTranscripts;
        }

        /// <summary>
        /// enables the annotation of the mitochondrial genome
        /// </summary>
	    public void EnableMitochondrialAnnotation()
        {
            _enableMitochondrialAnnotation = true;
        }

        /// <summary>
        /// adds custom intervals to the annotation source
        /// </summary>
        public void AddCustomIntervals(IEnumerable<ICustomInterval> customIntervals)
        {
            foreach (var interval in customIntervals)
            {
                _customIntervalTree.Add(new IntervalTree<CustomInterval>.Interval(interval.ReferenceName,
                    interval.Start, interval.End, interval as CustomInterval));
            }

            _hasCustomIntervals = true;
        }

        /// <summary>
        /// adds supplementary intervals to the annotation source
        /// </summary>
        public void AddSupplementaryIntervals(IEnumerable<ISupplementaryInterval> supplementaryIntervals)
        {
            foreach (var interval in supplementaryIntervals)
            {
                _suppIntervalTree.Add(new IntervalTree<SupplementaryInterval>.Interval(interval.ReferenceName,
                    interval.Start, interval.End, interval as SupplementaryInterval));
            }

            _hasSupplementaryAnnotations = true;
        }

        /// <summary>
        /// finalizes the annotator metrics before disposal
        /// </summary>
        public void FinalizeMetrics()
        {
            // force the output of the annotation time
            _performanceMetrics.StartReference("");
        }
    }
}
