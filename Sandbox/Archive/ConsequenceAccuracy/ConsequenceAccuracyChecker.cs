using System;
using System.IO;
using Illumina.VariantAnnotation.AnnotationSources;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;
using Illumina.VariantAnnotation.OutputDestinations;
using Illumina.VariantAnnotation.Utilities;

namespace CdnaEndPointInvestigation
{
    class ConsequenceAccuracyChecker
    {
        #region members

        private readonly NirvanaAnnotationSource _annotationSource;
        private readonly VcfDestination _annotationDestination;
        // private readonly bool _stopOnDifference;

        // private int _numVariantsProcessed;
        // private int _numVariantsDifferent;

        private int _numTranscriptsProcessed;
        private int _numTranscriptsDifferent;

        #endregion

        // constructor
        public ConsequenceAccuracyChecker(bool stopOnDifference, string nirvanaDatabaseDir)
        {
            // _stopOnDifference = stopOnDifference;
            _annotationDestination = new VcfDestination();
            _annotationSource = new NirvanaAnnotationSource(nirvanaDatabaseDir, _annotationDestination, false);
        }

        /// <summary>
        /// parses the specified vcf file and compares the new annotations to the old
        /// annotations
        /// </summary>
        public void Compare(string inputVcfPath, string outputVcfPath, bool silentOutput)
        {
            Console.WriteLine("Running Cdna Accuracy Checker on {0}:", Path.GetFileName(inputVcfPath));

            bool saveRejects = !string.IsNullOrEmpty(outputVcfPath);

            // sanity check: make sure we have annotations
            if (_annotationSource == null)
            {
                throw new ApplicationException("Unable to annotate the VCF file because no annotation sources were provided.");
            }

            string currentRefSeq = null;

            StreamWriter writer = null;

            if (saveRejects)
            {
                writer = new StreamWriter(outputVcfPath);
                writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO\t");
            }

            _numTranscriptsProcessed = 0;
            _numTranscriptsDifferent = 0;

            using (var reader = new LiteVcfReader(inputVcfPath))
            {
                var variant = new VepVariantFeature();
                var csqComparer = new CsqComparer(variant);

                CsqCommon.SetCsqFieldOrder(reader.CsqInfoLine);
                _annotationDestination.Initialize(variant.Csq);

                while (reader.GetNextVariant(variant))
                {
                    // grab the VEP annotations
                    csqComparer.ExtractTruthData(variant.VcfColumns[VcfCommon.InfoIndex]);

                    // check if we need to load annotations
                    if (variant.ReferenceName != currentRefSeq)
                    {
                        ConsoleUtilities.TimedAction("loading annotations", _annotationSource.LoadData, variant.ReferenceName);
                        currentRefSeq = variant.ReferenceName;
                    }

                    var overlappingTranscripts = _annotationSource.GetOverlappingTranscriptList(variant);
                    // I want to get the overlapping transcripts
                    _annotationSource.Annotate(variant);

                    string vepConsequence = "";
                    string entryString = "";

                    foreach (var transcript in overlappingTranscripts)
                    {
                        var observedCsq = CsqCommon.GetEntry(variant.Csq, transcript.StableId, variant.AlternateAlleles[0]);
                        if (!String.IsNullOrEmpty(observedCsq.Consequence))
                        {

                            foreach (var entry in csqComparer.GetCsqTruth())
                            {
                                if (entry.Feature.Contains(transcript.StableId))
                                {
                                    //
                                    vepConsequence= entry.Consequence;
                                    entryString = entry.ToString();
                                }

                            }

                            // Console.WriteLine(entryString);
                            // Console.WriteLine("NIR: " + observedCsq.ComplementaryDnaPosition);

                            // _annotationSource.DrawTranscriptAndVariant(variant, transcript);
                            if (String.Compare(vepConsequence, observedCsq.Consequence, StringComparison.Ordinal) != 0)
                            // if (vepCdnaPositions.Contains("?"))
                            {
                                // Console.WriteLine("VEP: " + vepCdsPositions);
                                Console.WriteLine(entryString);
                                Console.WriteLine("NIR: " + observedCsq.ComplementaryDnaPosition);
                                Console.WriteLine("NIR: " + observedCsq.CdsPosition);
                                Console.WriteLine("NIR: " + observedCsq.Consequence);

                                _annotationSource.DrawTranscriptAndVariant(variant, transcript);
                                _numTranscriptsDifferent++;
                            }

                            // else Console.WriteLine("NIR: " + observedCsq.ComplementaryDnaPosition);

                            // Console.WriteLine(observedCsq.Exon);

                        }
                        _numTranscriptsProcessed++;

                    }
                    /*
                    if (csqComparer.AreCsqTagsDifferent(_stopOnDifference, silentOutput, ref _numTranscriptsProcessed, ref _numTranscriptsDifferent))
                    {
                        if (saveRejects) writer.WriteLine(variant.VcfLine);
                        _numVariantsDifferent++;
                    }
                    */
                    // _numVariantsProcessed++;
                }
            }
            Console.WriteLine("Number of transcripts {0}, differences {1}, {2}", _numTranscriptsProcessed, _numTranscriptsDifferent,
                100.0 * _numTranscriptsDifferent / _numTranscriptsProcessed);
            if (saveRejects) writer.Close();

        }
    }
}
