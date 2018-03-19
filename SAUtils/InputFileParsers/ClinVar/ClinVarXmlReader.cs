using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class ClinvarVariant
	{
		public readonly IChromosome Chromosome;
		public int Start { get; }
		public readonly int Stop;
		public readonly string ReferenceAllele;
		public readonly string AltAllele;
		public string VariantType;
	    public readonly List<string> AllelicOmimIds;

		public ClinvarVariant(IChromosome chr, int start, int stop, string refAllele, string altAllele, List<string> allilicOmimIds =null)
		{
			Chromosome      = chr;
			Start           = start;
			Stop            = stop;
			ReferenceAllele = refAllele ?? "";
			AltAllele       = altAllele ?? "";
            AllelicOmimIds  = allilicOmimIds ?? new List<string>();
		}

	}

	public sealed class ClinVarXmlReader 
    {
        #region members

        private readonly FileInfo _clinVarXmlFileInfo;
		private readonly VariantAligner _aligner;
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
		private string _significance;

		private HashSet<string> _medGenIDs;
		private HashSet<string> _omimIDs;
        private HashSet<string> _allilicOmimIDs;
		private HashSet<string> _orphanetIDs;

        private HashSet<long> _pubMedIds= new HashSet<long>();
		private long _lastUpdatedDate;

		#endregion

		private bool _hasDbSnpId;

        private void ClearClinvarFields()
		{
			_variantList.Clear();
			_reviewStatus      = null;
			_alleleOrigins     = new HashSet<string>();
			_significance      = null;
			_prefPhenotypes    = new HashSet<string>();
			_altPhenotypes     = new HashSet<string>();
			_id                = null;
			_medGenIDs         = new HashSet<string>();
			_omimIDs           = new HashSet<string>();
            _allilicOmimIDs    = new HashSet<string>();
            _orphanetIDs       = new HashSet<string>();
			_pubMedIds         = new HashSet<long>();//we need a new pubmed hash since otherwise, pubmedid hashes of different items interfere. 
			_lastUpdatedDate   = long.MinValue;
		    _hasDbSnpId = false;

		}

		// constructor
        public ClinVarXmlReader(FileInfo clinVarXmlFileInfo, ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            _aligner = new VariantAligner(_sequenceProvider.Sequence);
            _clinVarXmlFileInfo = clinVarXmlFileInfo;
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

            using (var reader = GZipUtilities.GetAppropriateStreamReader(_clinVarXmlFileInfo.FullName))
			using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true}))
			{
				//skipping the top level element to go down to its elementren
			    xmlReader.ReadToDescendant(ClinVarSetTag);

			    //var benchmark = new Benchmark();
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

		    var validItems = GetValidVariants(clinVarItems);

		    var count = 0;
		    foreach (var clinVarItem in validItems.Distinct())
		    {
		        count++;
		        yield return clinVarItem;
		    }
		    Console.WriteLine("Total number of clinvar items written:"+count);

        }

        private List<ClinVarItem> GetValidVariants(List<ClinVarItem> clinVarItems)
        {
            var shiftedItems = new List<ClinVarItem>();
            foreach (var item in clinVarItems)
            {
                _sequenceProvider.LoadChromosome(item.Chromosome);

                if (!ValidateRefAllele(item)) continue;

                string refAllele= item.ReferenceAllele, altAllele= item.AlternateAllele;
                if (string.IsNullOrEmpty(item.ReferenceAllele) && item.VariantType == "Deletion")
                    refAllele = GetReferenceAllele(item, _sequenceProvider.Sequence);

                if (string.IsNullOrEmpty(item.ReferenceAllele) && item.VariantType == "Indel" && !string.IsNullOrEmpty(item.AlternateAllele))
                    refAllele = GetReferenceAllele(item, _sequenceProvider.Sequence);

                if (string.IsNullOrEmpty(item.AlternateAllele) && item.VariantType == "Duplication")
                    altAllele = GetAltAllele(item, _sequenceProvider.Sequence);

                if (string.IsNullOrEmpty(refAllele) && string.IsNullOrEmpty(altAllele)) continue;

                int start;
                (start, refAllele, altAllele) = LeftShift(item.Start, refAllele, altAllele);
                shiftedItems.Add(new ClinVarItem(item.Chromosome,
                    start,
                    item.Stop,
                    item.AlleleOrigins,
                    altAllele,
                    item.VariantType,
                    item.Id,
                    item.ReviewStatus,
                    item.MedGenIDs,
                    item.OmimIDs,
                    item.OrphanetIDs,
                    item.Phenotypes,
                    refAllele,
                    item.Significance,
                    item.PubmedIds,
                    item.LastUpdatedDate));
            }

            shiftedItems.Sort();
            return shiftedItems;
        }

        private const string RefAssertionTag = "ReferenceClinVarAssertion";
        private const string ClinVarAssertionTag = "ClinVarAssertion";
        private List<ClinVarItem> ExtractClinVarItems(XElement xElement)
		{
            ClearClinvarFields();

			if (xElement == null || xElement.IsEmpty) return null;

			foreach (var element in xElement.Elements(RefAssertionTag))
			    ParseRefClinVarAssertion(element);

		    foreach (var element in xElement.Elements(ClinVarAssertionTag))
                ParseClinvarAssertion(element);
		    

			
		    var clinvarList = new List<ClinVarItem>();

            foreach (var variant in _variantList)
		    {
		        if (variant.Chromosome == null) continue;

		        if ((variant.VariantType == "Microsatellite" || variant.VariantType == "Variation")
		            && string.IsNullOrEmpty(variant.AltAllele)) continue;

                var extendedOmimIds = new HashSet<string>(_omimIDs);

		        foreach (var omimId in variant.AllelicOmimIds)
		        {
		            extendedOmimIds.Add(omimId);
                }

		        var reviewStatEnum = ReviewStatusEnum.no_assertion;
		        if (ClinVarItem.ReviewStatusNameMapping.ContainsKey(_reviewStatus))
		            reviewStatEnum = ClinVarItem.ReviewStatusNameMapping[_reviewStatus];

                clinvarList.Add(
		            new ClinVarItem(variant.Chromosome,
		                variant.Start,
                        variant.Stop,
		                _alleleOrigins.Any()? _alleleOrigins: null,
		                variant.AltAllele ,
                        variant.VariantType,
		                _id,
		                reviewStatEnum,
		                _medGenIDs.Any()?_medGenIDs: null,
		                extendedOmimIds.Any()?extendedOmimIds:null,
		                _orphanetIDs.Any()?_orphanetIDs:null,
		                _prefPhenotypes.Any() ? _prefPhenotypes: _altPhenotypes,
		                variant.ReferenceAllele ,
		                _significance,
		                _pubMedIds.Any()? _pubMedIds.OrderBy(x=>x): null,
		                _lastUpdatedDate));
            }

			return clinvarList.Count > 0 ? clinvarList: null;
		}

	    private bool ValidateRefAllele(ClinVarItem clinvarVariant)
	    {
	        if (string.IsNullOrEmpty(clinvarVariant.ReferenceAllele) || clinvarVariant.ReferenceAllele == "-") return true;

		    var refAllele = clinvarVariant.ReferenceAllele;
		    if (string.IsNullOrEmpty(refAllele)) return true;

	        var refLength = clinvarVariant.Stop - clinvarVariant.Start + 1;
	        return refLength == refAllele.Length && _sequenceProvider.Sequence.Validate(clinvarVariant.Start, clinvarVariant.Stop, refAllele);

	        //var stop = clinvarVariant.Start + refAllele.Length - 1;
            //return _sequenceProvider.Sequence.Validate(clinvarVariant.Start, stop, refAllele);

        }

        private static string GetReferenceAllele(ClinVarItem variant, ISequence compressedSequence)
        {
            return variant == null ? null : compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);
        }

        private static string GetAltAllele(ClinVarItem variant, ISequence compressedSequence)
        {
            return variant == null ? null : compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);
        }

        private (int Start, string RefAllele, string AltAllele) LeftShift(int start, string refAllele, string altAllele)
		{
			if (refAllele == null || altAllele == null) return (start, refAllele, altAllele);

			return _aligner.LeftAlign(start, refAllele, altAllele);
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
        private const string XrefTag = "XRef";
        private void ParsePnenotype(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

		    var isPreferred = ParsePhenotypeElementValue(xElement.Element(ElementValueTag));
		    if (!isPreferred)
		        return;//we do not want to parse XRef for alternates

		    foreach (var element in xElement.Elements(XrefTag))
                ParseXref(element);
		}

        private const string TypeTag = "Type";

        private bool ParsePhenotypeElementValue(XElement xElement)
		{
		    var phenotype = xElement.Attribute(TypeTag);
		    if (phenotype == null) return false;

			if (phenotype.Value == "Preferred") 
				_prefPhenotypes.Add(xElement.Value);
			if (phenotype.Value == "Alternate")
				_altPhenotypes.Add(xElement.Value);

		    return phenotype.Value == "Preferred";
		}


        private const string DbTag = "DB";
        private const string IdTag = "ID";
        private void ParseXref(XElement xElement)
        {
            var db = xElement.Attribute(DbTag);

            if (db == null) return;

			var id = xElement.Attribute(IdTag)?.Value.Trim(' '); // Trimming is necessary here, don't turn it off.

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
				case "dbSNP":
				    _hasDbSnpId = true;
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

			    var pubmedId = element.Value.TrimEnd('.');
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

		    foreach (var element in xElement.Elements(MeasureTag))
		    {
		        ParseMeasure(element);
            }
            
		}


        private const string SeqLocationTag = "SequenceLocation";
        private void ParseMeasure(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

			_hasDbSnpId = false;
            _allilicOmimIDs.Clear();

			//the variant type is available in the attributes
            string varType = xElement.Attribute(TypeTag)?.Value;

			var variantList = new List<ClinvarVariant>();

		    foreach (var element in xElement.Elements(XrefTag))
                ParseXref(element);

		    foreach (var element in xElement.Elements(SeqLocationTag))
            {
		        var variant = GetClinvarVariant(element, _sequenceProvider.GenomeAssembly, _refChromDict);

		        if (variant == null) continue;

		        variant.VariantType = varType;
		        if (variant.AltAllele != null && variant.AltAllele.Length == 1 && _iupacBases.ContainsKey(variant.AltAllele[0]))
		            AddIupacVariants(variant, variantList);
		        else
		            variantList.Add(variant);
		    }

            if (! _hasDbSnpId)
            {
                _variantList.Clear();
                return;
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
			foreach (var altAllele in _iupacBases[variant.AltAllele[0]])
			{
			    variantList.Add(new ClinvarVariant(variant.Chromosome,variant.Start, variant.Stop, variant.ReferenceAllele, altAllele.ToString()));
			}
		}

        private readonly Dictionary<char, char[]> _iupacBases = new Dictionary<char, char[]>
        {
			['R'] = new[] {'A','G'},
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

        private const string ChrTag       = "Chr";
        private const string StopTag      = "display_stop";
        private const string StartTag     = "display_start";
        private const string AssemblyTag  = "Assembly";
        private const string RefAlleleTag = "referenceAllele";
        private const string AltAlleleTag = "alternateAllele";

        private static ClinvarVariant GetClinvarVariant(XElement xElement, GenomeAssembly genomeAssembly,IDictionary<string,IChromosome> refChromDict)
		{
		    if (xElement == null ) return null;//|| xElement.IsEmpty) return null;
			//<SequenceLocation Assembly="GRCh38" Chr="17" Accession="NC_000017.11" start="43082402" stop="43082402" variantLength="1" referenceAllele="A" alternateAllele="C" />

			if (genomeAssembly.ToString()!= xElement.Attribute(AssemblyTag)?.Value
                && genomeAssembly != GenomeAssembly.Unknown) return null;

            var chromosome      = refChromDict.ContainsKey(xElement.Attribute(ChrTag)?.Value) ? refChromDict[xElement.Attribute(ChrTag)?.Value] : null;
            var start           = Convert.ToInt32(xElement.Attribute(StartTag)?.Value);
		    var stop            = Convert.ToInt32(xElement.Attribute(StopTag)?.Value);
		    var referenceAllele = xElement.Attribute(RefAlleleTag)?.Value;
		    var altAllele       = xElement.Attribute(AltAlleleTag)?.Value;

            AdjustVariant(ref start,ref stop, ref referenceAllele, ref altAllele);
		    
            return new ClinvarVariant(chromosome, start, stop, referenceAllele, altAllele);
		}

		private static void AdjustVariant(ref int start, ref int stop, ref string referenceAllele, ref string altAllele)
		{
            if (referenceAllele == "-" && !string.IsNullOrEmpty(altAllele) && stop == start + 1)
            {
				referenceAllele = "";
				start++;
			}

			if (altAllele == "-")
				altAllele = "";
		}

        private const string ReviewStatusTag = "ReviewStatus";
        private const string DescriptionTag = "Description";

        private void GetClinicalSignificance(XElement xElement)
		{
			if (xElement == null || xElement.IsEmpty) return;

		    _reviewStatus = xElement.Element(ReviewStatusTag)?.Value;
		    _significance = xElement.Element(DescriptionTag)?.Value.ToLower();
		}
    }
}
