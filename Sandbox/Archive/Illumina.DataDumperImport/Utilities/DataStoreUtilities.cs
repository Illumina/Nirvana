using System.Collections.Generic;
using System.Linq;
using IVD = Illumina.VariantAnnotation.DataStructures;
using IDD = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Utilities
{
    public static class DataStoreUtilities
    {
        /// <summary>
        /// converts the data in the import data store to the Nirvana data store
        /// </summary>
        public static IVD.NirvanaDataStore ConvertData(IDD.ImportDataStore importDataStore, IVD.TranscriptDataSource transcriptDataSource)
        {
            var nirvanaDataStore = new IVD.NirvanaDataStore();

            // convert the transcripts
            ConvertTranscripts(nirvanaDataStore, importDataStore.Transcripts, transcriptDataSource);

            // copy the objects from the transcripts to the datastore
            PopulateTranscriptObjects(nirvanaDataStore);

            // convert the regulatory features
            ConvertRegulatoryFeatures(nirvanaDataStore, importDataStore.RegulatoryFeatures);

            return nirvanaDataStore;
        }

        /// <summary>
        /// copy the objects from the transcripts to the datastore
        /// </summary>
        public static void PopulateTranscriptObjects(IVD.NirvanaDataStore nirvanaDataStore)
        {
            var cdnaCoordinateMapSet = new HashSet<IVD.CdnaCoordinateMap>();
            var exonSet              = new HashSet<IVD.Exon>();
            var intronSet            = new HashSet<IVD.Intron>();
            var microRnaSet          = new HashSet<IVD.MicroRna>();
            var polyPhenSet          = new HashSet<IVD.PolyPhen>();
            var siftSet              = new HashSet<IVD.Sift>();

            // extract each of the transcript objects
            foreach (var transcript in nirvanaDataStore.Transcripts)
            {
                // CdnaCoordinateMaps
                if (transcript.CdnaCoordinateMaps != null)
                {
                    foreach (var cdnaCoordinateMap in transcript.CdnaCoordinateMaps)
                    {
                        cdnaCoordinateMapSet.Add(cdnaCoordinateMap);
                    }
                }

                // Exons
                if (transcript.Exons != null)
                {
                    foreach (var exon in transcript.Exons) exonSet.Add(exon);
                }

                // Introns
                if (transcript.Introns != null)
                {
                    foreach (var intron in transcript.Introns) intronSet.Add(intron);
                }

                // MicroRnas
                if (transcript.MicroRnas != null)
                {
                    foreach (var miRna in transcript.MicroRnas) microRnaSet.Add(miRna);
                }

                // PolyPhens
                if (transcript.PolyPhen != null) polyPhenSet.Add(transcript.PolyPhen);

                // Sifts
                if (transcript.Sift != null) siftSet.Add(transcript.Sift);
            }

            // populate the transcript object lists
            nirvanaDataStore.CdnaCoordinateMaps = cdnaCoordinateMapSet.ToList();
            nirvanaDataStore.Exons              = exonSet.ToList();
            nirvanaDataStore.Introns            = intronSet.ToList();
            nirvanaDataStore.MicroRnas          = microRnaSet.ToList();
            nirvanaDataStore.PolyPhens          = polyPhenSet.ToList();
            nirvanaDataStore.Sifts              = siftSet.ToList();
        }

        /// <summary>
        /// converts the import regulatory feature objects to the final Nirvana regulatory feature objects
        /// </summary>
        private static void ConvertRegulatoryFeatures(IVD.NirvanaDataStore nirvanaDataStore, List<IDD.VEP.RegulatoryFeature> importRegulatoryFeatures) 
        {
            var regulatoryFeatures = new List<IVD.RegulatoryFeature>();

            foreach (var importRegulatoryFeature in importRegulatoryFeatures)
            {
                regulatoryFeatures.Add(new IVD.RegulatoryFeature(importRegulatoryFeature.Start, importRegulatoryFeature.End, importRegulatoryFeature.StableId));
            }

            nirvanaDataStore.RegulatoryFeatures = regulatoryFeatures.OrderBy(x => x.Start).ToList();
        }

        /// <summary>
        /// converts the import transcript objects to the final Nirvana transcript objects
        /// </summary>
        private static void ConvertTranscripts(IVD.NirvanaDataStore nirvanaDataStore, List<IDD.VEP.Transcript> importTranscripts, IVD.TranscriptDataSource transcriptDataSource) 
        {
            var transcripts = new List<IVD.Transcript>();
            var genes       = new HashSet<IVD.Gene>();

            foreach (var importTranscript in importTranscripts)
            {
                // convert the transcript objects found in the import transcript
                var sortedExons     = ConvertExons(importTranscript.TransExons);
                var sortedMicroRnas = SortMiRnas(importTranscript.MicroRnas);

                IVD.Exon startExon = null;
                if (importTranscript.Translation != null)
                {
                    startExon = ConvertStartExon(importTranscript.Translation.StartExon);
                }

                IVD.CdnaCoordinateMap[] sortedCdnaMaps = null;
                IVD.Intron[] sortedIntrons             = null;
                IVD.Sift sift                          = null;
                IVD.PolyPhen polyPhen                  = null;

                if (importTranscript.VariantEffectCache != null)
                {
                    sortedIntrons = ConvertIntrons(importTranscript.VariantEffectCache.Introns);

                    if (importTranscript.VariantEffectCache.Mapper?.ExonCoordinateMapper?.PairGenomic != null)
                    {
                        sortedCdnaMaps = ConvertCdnaCoordinateMaps(importTranscript.VariantEffectCache.Mapper.ExonCoordinateMapper.PairGenomic.Genomic);
                    }

                    if (importTranscript.VariantEffectCache.ProteinFunctionPredictions != null)
                    {
                        sift     = ConvertSift(importTranscript.VariantEffectCache.ProteinFunctionPredictions.Sift);
                        polyPhen = ConvertPolyPhen(importTranscript.VariantEffectCache.ProteinFunctionPredictions.PolyPhen);
                    }
                }

                byte proteinVersion = importTranscript.Translation?.Version ?? (byte)1;

				var transcript = new IVD.Transcript(
                    sortedExons,
                    startExon,
                    importTranscript.GetTotalExonLength(),
                    sortedIntrons,
                    sortedCdnaMaps,
                    importTranscript.VariantEffectCache.Peptide,
                    importTranscript.VariantEffectCache.TranslateableSeq,
                    importTranscript.OnReverseStrand,
                    importTranscript.IsCanonical,
                    importTranscript.GetCodingRegionStart(),
                    importTranscript.GetCodingRegionEnd(),
                    importTranscript.CompDnaCodingStart,
                    importTranscript.CompDnaCodingEnd,
                    importTranscript.Start,
                    importTranscript.End,
                    importTranscript.CcdsId,
                    importTranscript.ProteinId,
                    importTranscript.GeneStableId,
                    importTranscript.StableId,
                    importTranscript.GeneSymbol,
                    importTranscript.GeneSymbolSource,
                    importTranscript.HgncId,
                    importTranscript.Version,
                    proteinVersion,
                    importTranscript.BioType,
                    transcriptDataSource,
                    sortedMicroRnas,
                    sift,
                    polyPhen,
					importTranscript.Gene.Start,
					importTranscript.Gene.End
					);

                transcripts.Add(transcript);

                // add genes
                if (importTranscript.Gene != null)
                {
                    var geneSymbol = importTranscript.GeneSymbol ?? "unknown";
                    var gene       = new IVD.Gene(geneSymbol, importTranscript.Gene.Start, importTranscript.Gene.End);
                    genes.Add(gene);
                }
            }

            nirvanaDataStore.Genes       = genes.OrderBy(x => x.Start).ToList();
            nirvanaDataStore.Transcripts = transcripts.OrderBy(x => x.Start).ToList();
        }

        /// <summary>
        /// returns an array of Nirvana exons that have been converted from import exons
        /// </summary>
        private static IVD.Exon[] ConvertExons(IDD.VEP.Exon[] exons)
        {
            // sanity check: handle null array
            if (exons == null) return null;

            // convert the exons
            var nirvanaExons = new IVD.Exon[exons.Length];
            for (int i = 0; i < exons.Length; i++) nirvanaExons[i] = exons[i].Convert();

            // return the exons in sorted order
            return nirvanaExons.OrderBy(x => x.Start).ToArray();
        }

        /// <summary>
        /// returns a Nirvana exon that has been converted from an import exon
        /// </summary>
        private static IVD.Exon ConvertStartExon(IDD.VEP.Exon exon)
        {
            return exon?.Convert();
        }

        /// <summary>
        /// returns an array of sorted Nirvana miRNAs
        /// </summary>
        private static IVD.MicroRna[] SortMiRnas(IVD.MicroRna[] miRnas)
        {
            // sanity check: handle null array

            // return the miRNAs in sorted order
            return miRnas?.OrderBy(x => x.Start).ToArray();
        }

        /// <summary>
        /// returns an array of Nirvana cDNA maps that have been converted from import cDNA maps
        /// </summary>
        private static IVD.CdnaCoordinateMap[] ConvertCdnaCoordinateMaps(List<IDD.VEP.MapperPair> mapperPairs)
        {
            // sanity check: handle null array
            if (mapperPairs == null) return null;

            // convert the cDNA maps
            var nirvanaCdnaMaps = new IVD.CdnaCoordinateMap[mapperPairs.Count];
            for (int i = 0; i < mapperPairs.Count; i++)
            {
                nirvanaCdnaMaps[i] = IDD.VEP.PairGenomic.ConvertMapperPair(mapperPairs[i]);
            }

            // return the cDNA maps in sorted order
            return nirvanaCdnaMaps.OrderBy(x => x.Genomic.Start).ToArray();
        }

        /// <summary>
        /// returns an array of Nirvana introns that have been converted from import introns
        /// </summary>
        private static IVD.Intron[] ConvertIntrons(IDD.VEP.Intron[] introns)
        {
            // sanity check: handle null array
            if (introns == null) return null;

            // convert the introns
            var nirvanaIntrons = new IVD.Intron[introns.Length];
            for (int i = 0; i < introns.Length; i++) nirvanaIntrons[i] = introns[i].Convert();

            // return the introns in sorted order
            return nirvanaIntrons.OrderBy(x => x.Start).ToArray();
        }

        /// <summary>
        /// returns a Nirvana Sift object that has been converted from an import Sift object
        /// </summary>
        private static IVD.Sift ConvertSift(IDD.VEP.Sift sift)
        {
            return sift?.Convert();
        }

        /// <summary>
        /// returns a Nirvana PolyPhen object that has been converted from an import PolyPhen object
        /// </summary>
        private static IVD.PolyPhen ConvertPolyPhen(IDD.VEP.PolyPhen polyPhen)
        {
            return polyPhen?.Convert();
        }

        /// <summary>
        /// returns a list of genes that corresponds to the specified transcripts
        /// </summary>
        public static List<IVD.Gene> GetGenesSubset(List<IVD.Gene> oldGenes, List<IVD.Transcript> transcripts)
        {
            // create a new dictionary
            var oldGenesDictionary = new Dictionary<string, IVD.Gene>();
            foreach (var gene in oldGenes) oldGenesDictionary[gene.Symbol] = gene;

            // search for the desired genes
            var foundGenes = new HashSet<IVD.Gene>();

            foreach (var transcript in transcripts)
            {
                var geneSymbol = transcript.GeneSymbol ?? "unknown";

                IVD.Gene gene;
                if (!oldGenesDictionary.TryGetValue(geneSymbol, out gene))
                {
                    throw new KeyNotFoundException($"Unable to find the following gene symbol: ${transcript.GeneSymbol}");
                }

                foundGenes.Add(gene);
            }

            // return the sorted gene list
            return foundGenes.OrderBy(x => x.Start).ToList();
        }
    }
}
