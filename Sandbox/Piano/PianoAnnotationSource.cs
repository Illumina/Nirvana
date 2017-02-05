using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Algorithms.Consequences;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace Piano
{
	public sealed class PianoAnnotationSource
	{
		#region members

		private const int FlankingLength = 5000;

	    private readonly IIntervalForest<Transcript> _transcriptIntervalForest;
        private List<Transcript> OverlappingTranscripts { get; }
		private readonly ICompressedSequence _compressedSequence;

		private readonly PerformanceMetrics _performanceMetrics;
		private PianoVariant _pianoVariant;

		private bool _enableMitochondrialAnnotation;
        private readonly AminoAcids _aminoAcids;

        private readonly DataFileManager _dataFileManager;
	    private readonly ChromosomeRenamer _renamer;
	    private readonly VID _vid;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public PianoAnnotationSource(Stream transcriptCacheStream, CompressedSequenceReader compressedSequenceReader)
        {
            OverlappingTranscripts = new List<Transcript>();
            _performanceMetrics    = PerformanceMetrics.Instance;

            _compressedSequence = new CompressedSequence();
            _dataFileManager    = new DataFileManager(compressedSequenceReader, _compressedSequence);
            _dataFileManager.Changed += LoadData;

            _renamer = _compressedSequence.Renamer;
            _aminoAcids = new AminoAcids();
            _vid = new VID();

            LoadTranscriptCache(transcriptCacheStream, _renamer.NumRefSeqs, out _transcriptIntervalForest);
        }

        public PianoVariant Annotate(IVariant variant)
        {
            if (variant == null) return null;

            var variantFeature = new VariantFeature(variant as VcfVariant, _renamer, _vid);

            // load the reference sequence
            _dataFileManager.LoadReference(variantFeature.ReferenceIndex, () => {});

            // handle ref no-calls and assign the alternate alleles
            variantFeature.AssignAlternateAlleles();

            // annotate the variant
            _pianoVariant = new PianoVariant(variantFeature);
            Annotate(variantFeature);
            _performanceMetrics.Increment();

            return _pianoVariant;
        }

        private void Annotate(VariantFeature variant)
        {
            if (variant.IsReference) return;
            if (variant.UcscReferenceName == "chrM" && !_enableMitochondrialAnnotation) return;
            if (variant.IsStructuralVariant) return;

            _pianoVariant.AddVariantData(variant);
            GetOverlappingTranscripts(variant);

            if (!HasOverlap(variant)) return;

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

        private void AddTranscriptToVariant(VariantFeature variant, Transcript transcript)
		{
			foreach (var altAllele in variant.AlternateAlleles)
			{
				AnnotateAltAllele(variant, altAllele, transcript);
			}
		}

		private void AnnotateAltAllele(VariantFeature variant, VariantAlternateAllele altAllele, Transcript transcript)
		{
			// handle upstream or downstream transcripts
			if (!Overlap.Partial(transcript.Start, transcript.End, altAllele.Start, altAllele.End)) return;

            var ta = new TranscriptAnnotation
            {
                AlternateAllele = altAllele,
                HasValidCdnaCodingStart = false,
                HasValidCdsStart = false
            };

            MapCdnaCoordinates(transcript, ta, altAllele);
			_pianoVariant.CreateAnnotationObject(transcript, altAllele);

			GetCodingAnnotations(transcript, ta, _compressedSequence);
			var consequence = new Consequences(new VariantEffect(ta, transcript, variant.InternalCopyNumberType));
			consequence.DetermineVariantEffects(variant.InternalCopyNumberType);

			_pianoVariant.FinalizeAndAddAnnotationObject(transcript, ta,consequence.GetConsequenceStrings());
		}

		private void GetCodingAnnotations(Transcript transcript, TranscriptAnnotation ta, ICompressedSequence compressedSequence)
		{
			// coding annotations
			if (!ta.HasValidCdnaStart && !ta.HasValidCdnaEnd) return;
			CalculateCdsPositions(transcript, ta);

			// determine the protein position
			if (!ta.HasValidCdsStart && !ta.HasValidCdsEnd) return;
			GetProteinPosition(ta, transcript, compressedSequence);
		}

        private void GetProteinPosition(TranscriptAnnotation ta, Transcript transcript, ICompressedSequence compressedSequence)
        {
            const int shift = 0;
            if (ta.HasValidCdsStart) ta.ProteinBegin = (int)((ta.CodingDnaSequenceBegin + shift + 2.0) / 3.0);
            if (ta.HasValidCdsEnd) ta.ProteinEnd = (int)((ta.CodingDnaSequenceEnd + shift + 2.0) / 3.0);

            // assign our codons and amino acids
            Codons.AssignExtended(ta, transcript, compressedSequence);
            _aminoAcids.Assign(ta);
        }

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
            int beginOffset = transcript.StartExonPhase - transcript.Translation.CodingRegion.CdnaStart + 1;
            ta.CodingDnaSequenceBegin = ta.BackupCdnaBegin + beginOffset;
			ta.CodingDnaSequenceEnd = ta.BackupCdnaEnd + beginOffset;

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

            CdnaMapper.MapCoordinates(altAllele.Start, altAllele.End, ta, transcript);
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

		private void GetOverlappingTranscripts(VariantFeature variant)
		{
            _transcriptIntervalForest.GetAllOverlappingValues(variant.ReferenceIndex,
                variant.OverlapReferenceBegin - FlankingLength, variant.OverlapReferenceEnd + FlankingLength,
                OverlappingTranscripts);
        }

		public void EnableMitochondrialAnnotation()
		{
			_enableMitochondrialAnnotation = true;
		}

		private void LoadData(object sender, NewReferenceEventArgs e)
		{
            _performanceMetrics.StartCache();
            if (_compressedSequence.GenomeAssembly == GenomeAssembly.GRCh38) EnableMitochondrialAnnotation();
            _performanceMetrics.StopCache();
		}

	    /// <summary>
	    /// loads the transcript cache
	    /// </summary>
	    private static void LoadTranscriptCache(Stream stream, int numRefSeqs,
	        out IIntervalForest<Transcript> transcriptIntervalForest)
	    {	        
            GlobalCache cache;
            using (var reader = new GlobalCacheReader(stream)) cache = reader.Read();
            transcriptIntervalForest = IntervalArrayFactory.CreateIntervalForest(cache.Transcripts, numRefSeqs);
        }
    }
}