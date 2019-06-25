using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Compression.Utilities;
using Genome;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class ClinVarXmlReader 
    {
        #region members
        private const string RefAssertionTag = "ReferenceClinVarAssertion";
        private const string ClinVarAssertionTag = "ClinVarAssertion";
        private const string ReviewStatusTag = "ReviewStatus";
        private const string DescriptionTag = "Description";
        private const string ExplanationTag = "Explanation";
        private const int MaxVariantLength = 1000;

        private readonly Dictionary<char, char[]> _iupacBases = new Dictionary<char, char[]>
        {
            ['R'] = new[] { 'A', 'G' },
            ['Y'] = new[] { 'C', 'T' },
            ['S'] = new[] { 'G', 'C' },
            ['W'] = new[] { 'A', 'T' },
            ['K'] = new[] { 'G', 'T' },
            ['M'] = new[] { 'A', 'C' },
            ['B'] = new[] { 'C', 'G', 'T' },
            ['D'] = new[] { 'A', 'G', 'T' },
            ['H'] = new[] { 'A', 'C', 'T' },
            ['V'] = new[] { 'A', 'C', 'G' }
        };

        private const string ChrTag = "Chr";
        private const string StopTag = "display_stop";
        private const string StartTag = "display_start";
        private const string AssemblyTag = "Assembly";
        private const string RefAlleleTag = "referenceAllele";
        private const string AltAlleleTag = "alternateAllele";

        private readonly string _fileName;
		private readonly ISequenceProvider _sequenceProvider;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        private string _lastClinvarAccession;
        #endregion

        
        #region clinVarItem fields

        private readonly List<ClinvarVariant> _variantList= new List<ClinvarVariant>();
		private HashSet<string> _alleleOrigins;
		private string _reviewStatus;
		private string _id;
		private HashSet<string> _prefPhenotypes;
		private HashSet<string> _altPhenotypes;
		private string[] _significances;

		private HashSet<string> _medGenIDs;
		private HashSet<string> _omimIDs;
        private HashSet<string> _allilicOmimIDs;
		private HashSet<string> _orphanetIDs;

        private HashSet<long> _pubMedIds= new HashSet<long>();
		private long _lastUpdatedDate;

        public SaJsonSchema JsonSchema { get; } = ClinVarSchema.Get();

        #endregion

        private void ClearClinvarFields()
		{
			_variantList.Clear();
			_reviewStatus      = null;
			_alleleOrigins     = new HashSet<string>();
			_significances      = null;
			_prefPhenotypes    = new HashSet<string>();
			_altPhenotypes     = new HashSet<string>();
			_id                = null;
			_medGenIDs         = new HashSet<string>();
			_omimIDs           = new HashSet<string>();
            _allilicOmimIDs    = new HashSet<string>();
            _orphanetIDs       = new HashSet<string>();
			_pubMedIds         = new HashSet<long>();//we need a new pubmed hash since otherwise, pubmedid hashes of different items interfere. 
			_lastUpdatedDate   = long.MinValue;
		}

		// constructor
        public ClinVarXmlReader(string filename, ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            _fileName = filename;
            _refChromDict = sequenceProvider.RefNameToChromosome;
        }

        private const string ClinVarSetTag = "ClinVarSet";

        /// <summary>
        /// Parses a ClinVar file and return an enumeration object containing all the ClinVar objects
        /// that have been extracted
        /// </summary>
        public IEnumerable<ClinVarItem> GetItems()
		{
		    var clinVarItems = new List<ClinVarItem>();

            using (var reader = GZipUtilities.GetAppropriateStreamReader(_fileName))
			using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true}))
			{
				//skipping the top level element to go down to its elementren
			    xmlReader.ReadToDescendant(ClinVarSetTag);

                do
				{
					var subTreeReader = xmlReader.ReadSubtree();
				    var xElement = XElement.Load(subTreeReader);
				    List<ClinVarItem> extractedItems;
				    try
				    {
				        extractedItems = ExtractClinVarItems(xElement);
                    }
                    catch (Exception )
				    {
				        Console.WriteLine($"Last clinVar accession observed {_lastClinvarAccession}");
				        throw;
				    }
				    if (extractedItems == null) continue;
				    clinVarItems.AddRange(extractedItems);

                } while (xmlReader.ReadToNextSibling(ClinVarSetTag));
			}
		    clinVarItems.Sort();

		    var validItems = GetValidItems(clinVarItems);

		    var count = 0;
		    foreach (var clinVarItem in validItems.Distinct())
		    {
		        count++;
		        yield return clinVarItem;
		    }
		    Console.WriteLine("Total number of clinvar items written:"+count);

        }

        private List<ClinVarItem> GetValidItems(List<ClinVarItem> clinVarItems)
        {
            var shiftedItems = new List<ClinVarItem>();
            foreach (var item in clinVarItems)
            {
                _sequenceProvider.LoadChromosome(item.Chromosome);

                if (!ValidateRefAllele(item)) continue;

                string refAllele= item.RefAllele, altAllele= item.AltAllele;
                if (string.IsNullOrEmpty(item.RefAllele) && item.VariantType == "Deletion")
                    refAllele = GetReferenceAllele(item, _sequenceProvider.Sequence);

                if (string.IsNullOrEmpty(item.RefAllele) && item.VariantType == "Indel" && !string.IsNullOrEmpty(item.AltAllele))
                    refAllele = GetReferenceAllele(item, _sequenceProvider.Sequence);

                if (string.IsNullOrEmpty(item.AltAllele) && item.VariantType == "Duplication")
                    altAllele = GetAltAllele(item, _sequenceProvider.Sequence);

                if (string.IsNullOrEmpty(refAllele) && string.IsNullOrEmpty(altAllele)) continue;

                int start;
                (start, refAllele, altAllele) = VariantUtils.TrimAndLeftAlign(item.Position, refAllele, altAllele, _sequenceProvider.Sequence);
                
                shiftedItems.Add(new ClinVarItem(item.Chromosome,
                    start,
                    item.Stop,
                    refAllele,
                    altAllele,
                    item.JsonSchema,
                    item.AlleleOrigins, item.VariantType, item.Id,item.VariationId, item.ReviewStatus, item.MedGenIds, item.OmimIds, item.OrphanetIds, item.Phenotypes, item.Significances, item.PubmedIds, item.LastUpdatedDate));
            }

            shiftedItems.Sort();
            return shiftedItems;
        }


        

        private List<ClinVarItem> ExtractClinVarItems(XElement xElement)
		{
            ClearClinvarFields();

			if (xElement == null || xElement.IsEmpty) return null;

			ParseAssertions(xElement);

		    var clinvarList = new List<ClinVarItem>();

            foreach (var variant in _variantList)
            {
                if (IsInvalidVariant(variant)) continue;

                var extendedOmimIds = GetOmimIds(variant);

                var reviewStatEnum = ReviewStatus.no_assertion;
                if (ClinVarItem.ReviewStatusNameMapping.ContainsKey(_reviewStatus))
                    reviewStatEnum = ClinVarItem.ReviewStatusNameMapping[_reviewStatus];

                clinvarList.Add(
                    new ClinVarItem(variant.Chromosome,
                        variant.Start,
                        variant.Stop,
                        variant.ReferenceAllele,
                        variant.AltAllele,
                        JsonSchema,
                        _alleleOrigins.Count > 0 ? _alleleOrigins : null,
                        variant.VariantType,
                        _id,
                        variant.VariantId,
                        reviewStatEnum,
                        _medGenIDs.Count > 0 ? _medGenIDs : null,
                        extendedOmimIds.Count > 0 ? extendedOmimIds : null,
                        _orphanetIDs.Count > 0 ? _orphanetIDs : null,
                        _prefPhenotypes.Count > 0 ? _prefPhenotypes : _altPhenotypes,
                        _significances,
                        _pubMedIds.Count > 0 ? _pubMedIds.OrderBy(x => x) : null, 
                        _lastUpdatedDate));
            }

			return clinvarList.Count > 0 ? clinvarList: null;
		}

        
        
        private HashSet<string> GetOmimIds(ClinvarVariant variant)
        {
            var extendedOmimIds = new HashSet<string>(_omimIDs);

            foreach (var omimId in variant.AllelicOmimIds)
            {
                extendedOmimIds.Add(omimId);
            }

            return extendedOmimIds;
        }

        private void ParseAssertions(XElement xElement)
        {
            foreach (var element in xElement.Elements(RefAssertionTag))
                ParseRefClinVarAssertion(element);

            foreach (var element in xElement.Elements(ClinVarAssertionTag))
                ParseClinvarAssertion(element);
        }

        private static bool IsInvalidVariant(ClinvarVariant variant)
        {
            return variant.Chromosome == null
                   || (variant.VariantType == "Microsatellite" || variant.VariantType == "Variation")
                   && string.IsNullOrEmpty(variant.AltAllele);
            
        }

        private bool ValidateRefAllele(ClinVarItem clinvarVariant)
	    {
	        if (string.IsNullOrEmpty(clinvarVariant.RefAllele) || clinvarVariant.RefAllele == "-") return true;

		    string refAllele = clinvarVariant.RefAllele;
		    if (string.IsNullOrEmpty(refAllele)) return true;

	        int refLength = clinvarVariant.Stop - clinvarVariant.Position + 1;
	        return refLength == refAllele.Length && _sequenceProvider.Sequence.Validate(clinvarVariant.Position, clinvarVariant.Stop, refAllele);
        }

        private static string GetReferenceAllele(ClinVarItem variant, ISequence compressedSequence)
        {
            return variant == null ? null : compressedSequence.Substring(variant.Position - 1, variant.Stop - variant.Position + 1);
        }

        private static string GetAltAllele(ClinVarItem variant, ISequence compressedSequence)
        {
            return variant == null ? null : compressedSequence.Substring(variant.Position - 1, variant.Stop - variant.Position + 1);
        }

        internal static long ParseDate(string s)
		{
			if (string.IsNullOrEmpty(s) || s == "-") return long.MinValue;
			//Jun 29, 2010
			return DateTime.Parse(s).Ticks;
		}

        private const string UpdateDateTag= "DateLastUpdated";
        private const string AccessionTag = "Acc";
        private const string VersionTag = "Version";
        private const string ClinVarAccessionTag = "ClinVarAccession";
        private const string ClinicalSignificanceTag = "ClinicalSignificance";
        private const string MeasureSetTag = "MeasureSet";
        private const string TraitSetTag = "TraitSet";
        private const string ObservedInTag = "ObservedIn";
        private const string SampleTag = "Sample";

        private void ParseRefClinVarAssertion(XElement xElement)
		{
			if (xElement==null || xElement.IsEmpty) return;
			//<ReferenceClinVarAssertion DateCreated="2013-10-28" DateLastUpdated="2016-04-20" ID="182406">
            _lastUpdatedDate      = ParseDate(xElement.Attribute(UpdateDateTag)?.Value);
		    _lastClinvarAccession = xElement.Element(ClinVarAccessionTag)?.Attribute(AccessionTag)?.Value;
            _id                   =  _lastClinvarAccession + "." + xElement.Element(ClinVarAccessionTag)?.Attribute(VersionTag)?.Value;
            
            GetClinicalSignificance(xElement.Element(ClinicalSignificanceTag));
            ParseGenotypeSet(xElement.Element(GenotypeSetTag));
		    ParseMeasureSet(xElement.Element(MeasureSetTag));
		    ParseTraitSet(xElement.Element(TraitSetTag));
		}

        private const string CitationTag = "Citation";
        private const string OriginTag = "Origin";

        private void ParseClinvarAssertion(XElement xElement)
		{
		    if (xElement == null || xElement.IsEmpty) return;

            foreach (var element in xElement.Descendants(CitationTag))
				ParseCitation(element);

		    foreach (var element in xElement.Elements(ObservedInTag))
                ParseObservedIn(element);

        }

        private void ParseObservedIn(XElement xElement)
        {
            var samples = xElement?.Elements(SampleTag);
            if (samples == null) return;

            foreach (var sample in samples)
            {
                foreach (var origin in sample.Elements(OriginTag))
                    _alleleOrigins.Add(origin.Value);
            }
        }

        private const string TraitTag = "Trait";

        private void ParseTraitSet(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

			foreach (var element in xElement.Elements(TraitTag))
			    ParseTrait(element);
		}

        private const string XrefTag = "XRef";
        private const string NameTag = "Name";
		private void ParseTrait(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

		    foreach (var element in xElement.Elements(XrefTag))
		        ParseXref(element);

		    foreach (var element in xElement.Elements(NameTag))
                ParsePnenotype(element);
		}

        private const string ElementValueTag = "ElementValue";
        private void ParsePnenotype(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

	        ParsePhenotypeElementValue(xElement.Element(ElementValueTag));
		}

        private const string TypeTag = "Type";

        private void ParsePhenotypeElementValue(XElement xElement)
		{
		    var phenotype = xElement.Attribute(TypeTag);
		    if (phenotype == null) return;

		    if (phenotype.Value == "Preferred")
		    {
		        _prefPhenotypes.Add(xElement.Value);
		    }
		    else if (phenotype.Value == "Alternate")
		    {
		        _altPhenotypes.Add(xElement.Value);
		    }
		}


        private const string DbTag = "DB";
        private const string IdTag = "ID";
        private void ParseXref(XElement xElement)
        {
            var db = xElement.Attribute(DbTag);

            if (db == null) return;

			string id = xElement.Attribute(IdTag)?.Value.Trim(' '); // Trimming is necessary here, don't turn it off.

			switch (db.Value)
			{
				case "MedGen":
					_medGenIDs.Add(id);
					break;
				case "Orphanet":
					_orphanetIDs.Add(id);
					break;
				case "OMIM":
				    var type = xElement.Attribute(TypeTag);
					if (type !=null)
					    if (type.Value == "Allelic variant" )
                            _allilicOmimIDs.Add(TrimOmimId(id));
                        else
                            _omimIDs.Add(TrimOmimId(id));
					break;
			}
		}

        
        private static string TrimOmimId(string id)
	    {
		    return id.TrimStart('P','S');
	    }

        private const string SourceTag = "Source";
        private const string PubmedIdTag = "PubMed";

        private void ParseCitation(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

			
			foreach (var element in xElement.Elements(IdTag))
			{
			    var source = element.Attribute(SourceTag);
			    if (source == null) continue;

			    if (source.Value != PubmedIdTag) continue;

			    string pubmedId = element.Value.TrimEnd('.');
                //pubmed ids with more than 8 digits are bad
                if (long.TryParse(pubmedId, out long l) && l <= 99_999_999)//pubmed ids with more than 8 digits are bad
                    _pubMedIds.Add(l);
                else Console.WriteLine($"WARNING:unexpected pubmedID {pubmedId}.");
                
    		}
		}

        private const string MeasureTag = "Measure";
        private const string GenotypeSetTag = "GenotypeSet";

        private void ParseGenotypeSet(XElement xElement)
        {
            if (xElement == null || xElement.IsEmpty) return;
            
            foreach (var measureSet in xElement.Elements(MeasureSetTag))
            {
                ParseMeasureSet(measureSet);
            }
        }

        private void ParseMeasureSet(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;
            var variantId = xElement.Attribute(IdTag) == null ? (int?)null : int.Parse(xElement.Attribute(IdTag)?.Value);
            foreach (var element in xElement.Elements(MeasureTag))
		    {
		        ParseMeasure(element, variantId);
            }
            
		}


        private const string SeqLocationTag = "SequenceLocation";
        private void ParseMeasure(XElement xElement, int? variantId)
		{
			if (xElement == null || xElement.IsEmpty) return;

            _allilicOmimIDs.Clear();

			//the variant type is available in the attributes
            var varType = xElement.Attribute(TypeTag)?.Value;
            
            var variantList = new List<ClinvarVariant>();

		    foreach (var element in xElement.Elements(XrefTag))
                ParseXref(element);

		    foreach (var element in xElement.Elements(SeqLocationTag))
            {
		        var variant = GetClinvarVariant(element, _sequenceProvider.Assembly, _refChromDict, variantId);

		        if (variant == null) continue;

		        variant.VariantType = varType;
		        if (variant.AltAllele != null && variant.AltAllele.Length == 1 && _iupacBases.ContainsKey(variant.AltAllele[0]))
		            AddIupacVariants(variant, variantList);
		        else
		            variantList.Add(variant);
		    }

            if (_allilicOmimIDs.Count != 0) 
            {
                foreach (var variant in variantList)
                {
                    variant.AllelicOmimIds.AddRange(_allilicOmimIDs);
                }
            }

            _variantList.AddRange(variantList);
		    
		}

        
        private void AddIupacVariants(ClinvarVariant variant, List<ClinvarVariant> variantList)
		{
			foreach (char altAllele in _iupacBases[variant.AltAllele[0]])
			{
			    variantList.Add(new ClinvarVariant(variant.Chromosome,variant.Start, variant.Stop,variant.VariantId, variant.ReferenceAllele, altAllele.ToString()));
			}
		}



        private static ClinvarVariant GetClinvarVariant(XElement xElement, GenomeAssembly genomeAssembly, IDictionary<string, IChromosome> refChromDict, int? variantId)
        {
		    if (xElement == null ) return null;
			//<SequenceLocation Assembly="GRCh38" Chr="17" Accession="NC_000017.11" start="43082402" stop="43082402" variantLength="1" referenceAllele="A" alternateAllele="C" />

			if (genomeAssembly.ToString()!= xElement.Attribute(AssemblyTag)?.Value
                && genomeAssembly != GenomeAssembly.Unknown) return null;

            var chromosome      = refChromDict.ContainsKey(xElement.Attribute(ChrTag)?.Value) ? refChromDict[xElement.Attribute(ChrTag)?.Value] : null;
            int start           = Convert.ToInt32(xElement.Attribute(StartTag)?.Value);
		    int stop            = Convert.ToInt32(xElement.Attribute(StopTag)?.Value);
		    string referenceAllele = xElement.Attribute(RefAlleleTag)?.Value;
		    string altAllele       = xElement.Attribute(AltAlleleTag)?.Value;

            if (stop - start + 1 > MaxVariantLength) return null;
            AdjustVariant(ref start, ref referenceAllele, ref altAllele);
		    
            return new ClinvarVariant(chromosome, start, stop, variantId, referenceAllele, altAllele);
		}

		private static void AdjustVariant(ref int start, ref string referenceAllele, ref string altAllele)
		{
		    if (referenceAllele == "-")
		    {
		        referenceAllele = "";
		        start++;
		    }

            if (altAllele == "-")
				altAllele = "";
		}

        private void GetClinicalSignificance(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

		    _reviewStatus = xElement.Element(ReviewStatusTag)?.Value;
            var description = xElement.Element(DescriptionTag)?.Value;
            var explanation = xElement.Element(ExplanationTag)?.Value;

            _significances = GetSignificances(description, explanation);

            ValidateSignificance(_significances);
        }

        private void ValidateSignificance(string[] significances)
        {
            foreach (var significance in significances)
            {
                if (!_validPathogenicity.Contains(significance)) 
                    throw new InvalidDataException($"Invalid pathogenicity found in {_id}. Observed: {significance}");
            }
        }

        private static string[] GetSignificances(string description, string explanation)
        {
            if(string.IsNullOrEmpty(explanation)) return description?.ToLower().Split('/', ',').Select(x=>x.Trim()).ToArray();
            //<Explanation DataSource="ClinVar" Type="public">Pathogenic(1);Uncertain significance(1)</Explanation>
            var sigList=new List<string>();
            foreach (var significance in explanation.ToLower().Split('/',';'))
            {
                var openParenthesisIndex = significance.IndexOf('(');
                sigList.Add(openParenthesisIndex < 0 ? significance : significance.Substring(0, openParenthesisIndex));
            }

            return sigList.ToArray();
        }

        private readonly HashSet<string> _validPathogenicity = new HashSet<string>
        {
            "uncertain significance",
            "not provided",
            "benign",
            "likely benign",
            "likely pathogenic",
            "pathogenic",
            "drug response",
            "histocompatibility",
            "association",
            "risk factor",
            "protective",
            "affects",
            "conflicting data from submitters",
            "other"
        };
    }
}
