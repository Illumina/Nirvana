using System;
using System.Collections;
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
		public string DbSnp;
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

	    public override int GetHashCode()
	    {
	        return Chromosome.GetHashCode()
	               ^ ReferenceAllele.GetHashCode()
	               ^ AltAllele.GetHashCode()
	               ^ Start
	               ^ Stop;
	    }

	    public override bool Equals(object obj)
	    {
	        var other = obj as ClinvarVariant;
	        if (other == null) return false;

	        return Chromosome.Equals(other.Chromosome)
	               && Start == other.Start
	               && Stop == other.Stop
	               && ReferenceAllele.Equals(other.ReferenceAllele)
	               && AltAllele.Equals(other.AltAllele);
	    }

	    
	}

	/// <summary>
	/// Parser for ClinVar file
	/// </summary>
	public sealed class ClinVarXmlReader : IEnumerable<ClinVarItem>
    {
        #region members

        private readonly FileInfo _clinVarXmlFileInfo;
		private readonly VariantAligner _aligner;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        #endregion

        #region xmlTags

        const string ClinVarSetTag = "ClinVarSet";
        const string RecordStatusTag = "RecordStatus";

        #endregion

        #region clinVarItem fields

        private readonly List<ClinvarVariant> _variantList= new List<ClinvarVariant>();
		private List<string> _alleleOrigins;
		private string _reviewStatus;
		private string _id;
		private List<string> _prefPhenotypes;
		private List<string> _altPhenotypes;
		private string _significance;

		private List<string> _medGenIDs;
		private List<string> _omimIDs;
        private HashSet<string> _allilicOmimIDs;
		private List<string> _orphanetIDs;

		List<long> _pubMedIds= new List<long>();
		private long _lastUpdatedDate;

		#endregion

		private string _dbSnp;
		private string _recordStatus;
		
		private void ClearClinvarFields()
		{
			_variantList.Clear();
			_reviewStatus      = null;
			_alleleOrigins     = new List<string>();
			_significance      = null;
			_prefPhenotypes    = new List<string>();
			_altPhenotypes     = new List<string>();
			_id                = null;
			_medGenIDs         = new List<string>();
			_omimIDs           = new List<string>();
            _allilicOmimIDs    = new HashSet<string>();
            _orphanetIDs       = new List<string>();
			_pubMedIds         = new List<long>();//we need a new pubmed hash since otherwise, pubmedid hashes of different items interfere. 
			_lastUpdatedDate   = long.MinValue;

		}

		#region IEnumerable implementation

		public IEnumerator<ClinVarItem> GetEnumerator()
        {
            return GetItems().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        // constructor
        public ClinVarXmlReader(FileInfo clinVarXmlFileInfo, ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            _aligner = new VariantAligner(_sequenceProvider.Sequence);
            _clinVarXmlFileInfo = clinVarXmlFileInfo;
            _refChromDict = sequenceProvider.GetChromosomeDictionary();
        }

		public sealed class LitexElement
		{
			public readonly string Name;
			public readonly Dictionary<string, string> Attributes = new Dictionary<string, string>();
			public readonly List<LitexElement> Children = new List<LitexElement>();
			public readonly List<string> StringValues= new List<string>();

			public LitexElement(string name)
			{
				Name = name;
			}

			public bool IsEmpty()
			{
				return Attributes.Count == 0 
					&& Children.Count == 0 
					&& StringValues.Count == 0;
			}
		}

		/// <summary>
		/// Parses a ClinVar file and return an enumeration object containing all the ClinVar objects
		/// that have been extracted
		/// </summary>
		private IEnumerable<ClinVarItem> GetItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_clinVarXmlFileInfo.FullName))
			using (var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true }))
			{
				//skipping the top level element to go down to its children
			    xmlReader.ReadToDescendant(ClinVarSetTag);

				do
				{
					var subTreeReader = xmlReader.ReadSubtree();
				    var xElement = XElement.Load(subTreeReader);

                    var clinVarItems = ExtractClinVarItems(xElement);

					if (clinVarItems == null) continue;

					foreach (var clinVarItem in clinVarItems)
					{
						yield return clinVarItem;
					}
				} while (xmlReader.ReadToNextSibling(ClinVarSetTag));
			}
			
		}

        private const string RefAssertionTag = "ReferenceClinVarAssertion";
        private const string AssertionTag = "ClinVarAssertion";
        private List<ClinVarItem> ExtractClinVarItems(XElement xElement)
		{
            ClearClinvarFields();

			if (xElement == null || xElement.IsEmpty) return null;

			foreach (var element in xElement.Elements(RefAssertionTag))
			    ParseRefClinVarAssertion(element);

		    foreach (var element in xElement.Elements())
                ParseClinvarAssertion(element);
		    

			var clinvarList = new List<ClinVarItem>();
            var clinvarSet = new HashSet<ClinvarVariant>();
			foreach (var variant in _variantList)
			{
                // in order to match the VCF, we leave out the ones that do not have dbsnp id
				if (variant.DbSnp == null) continue;

                if(variant.Chromosome == null) continue;

                if ((variant.VariantType == "Microsatellite" || variant.VariantType=="Variation")
                    && string.IsNullOrEmpty(variant.AltAllele)) continue;

                _sequenceProvider.LoadChromosome(variant.Chromosome);


                if (!ValidateRefAllele(variant)) continue;
                

				ClinvarVariant shiftedVariant= variant;
				//some entries do not have ref allele in the xml file. For those, we extract them from our ref sequence
				if (string.IsNullOrEmpty(variant.ReferenceAllele) && variant.VariantType=="Deletion" )
					shiftedVariant = GenerateRefAllele(variant, _sequenceProvider.Sequence);
				if (string.IsNullOrEmpty(variant.AltAllele) && variant.VariantType == "Duplication")
					shiftedVariant = GenerateAltAllele(variant, _sequenceProvider.Sequence);

				

				//left align the variant
				shiftedVariant = LeftShift(shiftedVariant);
                
                if (string.IsNullOrEmpty(variant.ReferenceAllele) && variant.VariantType == "Indel" && !string.IsNullOrEmpty(variant.AltAllele))
					shiftedVariant = GenerateRefAllele(variant, _sequenceProvider.Sequence);

				_pubMedIds.Sort();
				
				if(string.IsNullOrEmpty(shiftedVariant.ReferenceAllele) && string.IsNullOrEmpty(shiftedVariant.AltAllele)) continue;

                //getting the unique ones
			    clinvarSet.Add(shiftedVariant);
                
			}

		    foreach (var clinvarVariant in clinvarSet)
		    {
		        var extendedOmimIds = new List<string>();
		        extendedOmimIds.AddRange(_omimIDs);
		        if (clinvarVariant.AllelicOmimIds.Count != 0)
		            extendedOmimIds.AddRange(clinvarVariant.AllelicOmimIds);


		        clinvarList.Add(
		            new ClinVarItem(clinvarVariant.Chromosome,
		                clinvarVariant.Start,
		                _alleleOrigins.Distinct().ToList(),
		                clinvarVariant.AltAllele ,
		                _id,
		                _reviewStatus,
		                _medGenIDs.Distinct().ToList(),
		                extendedOmimIds.Distinct().ToList(),
		                _orphanetIDs.Distinct().ToList(),
		                _prefPhenotypes.Count > 0 ? _prefPhenotypes.Distinct().ToList() : _altPhenotypes.Distinct().ToList(),
		                clinvarVariant.ReferenceAllele ,
		                _significance,
		                _pubMedIds.Distinct().ToList(),
		                _lastUpdatedDate));
            }

			return clinvarList.Count > 0 ? clinvarList: null;
		}

	    private bool ValidateRefAllele(ClinvarVariant clinvarVariant)
	    {
	        if (string.IsNullOrEmpty(clinvarVariant.ReferenceAllele) || clinvarVariant.ReferenceAllele == "-") return true;

		    var refAllele = clinvarVariant.ReferenceAllele;
		    if (string.IsNullOrEmpty(refAllele)) return true;

		    var refLength = clinvarVariant.Stop - clinvarVariant.Start + 1;
		    if (refLength != refAllele.Length) return false;

		    return _sequenceProvider.Sequence.Validate(clinvarVariant.Start, clinvarVariant.Stop, refAllele);

	    }

	    private static ClinvarVariant GenerateAltAllele(ClinvarVariant variant, ISequence compressedSequence)
		{
			if (variant == null) return null;
			var extractedAlt = compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);

            return new ClinvarVariant(variant.Chromosome, variant.Start, variant.Stop, variant.ReferenceAllele , extractedAlt, variant.AllelicOmimIds);
		}

		private static ClinvarVariant GenerateRefAllele(ClinvarVariant variant, ISequence compressedSequence)
		{
			if (variant == null) return null;
			var extractedRef = compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);

            return new ClinvarVariant(variant.Chromosome, variant.Start, variant.Stop, extractedRef, variant.AltAllele, variant.AllelicOmimIds);

		}

		private ClinvarVariant LeftShift(ClinvarVariant variant)
		{
			if (variant.ReferenceAllele == null || variant.AltAllele == null) return variant;

			var alignedVariant = _aligner.LeftAlign(variant.Start, variant.ReferenceAllele, variant.AltAllele);
			if (alignedVariant == null) return variant;

		    return new ClinvarVariant(variant.Chromosome, alignedVariant.Item1, variant.Stop, alignedVariant.Item2,alignedVariant.Item3, variant.AllelicOmimIds);
		}

		private static LitexElement ParsexElement(XmlReader xmlReader)
		{
			var xElement = new LitexElement(xmlReader.Name);

			var isEmptyElement = xmlReader.IsEmptyElement;
			if (xmlReader.HasAttributes)
			{
				while (xmlReader.MoveToNextAttribute())
					xElement.Attributes[xmlReader.Name] = xmlReader.Value;
			}

			if (isEmptyElement)
				return xElement.IsEmpty()? null: xElement;

			while (xmlReader.Read())
			{
				//we will read till an end tag is observed
				switch (xmlReader.NodeType)
				{
					case XmlNodeType.Element: // The node is an element.
						var child = ParsexElement(xmlReader);
						if (child != null ) xElement.Children.Add(child);
						break;
					case XmlNodeType.Text:
						if (! string.IsNullOrEmpty(xmlReader.Value))
							xElement.StringValues.Add(xmlReader.Value);
						break;
					case XmlNodeType.EndElement: //Display the end of the element.
						if (xmlReader.Name == xElement.Name)
						{
							return xElement.IsEmpty()? null: xElement;
						}
						Console.WriteLine("WARNING!! encountered unexpected endElement tag:"+xmlReader.Name);
						break;
				}
			}
			return null;
		}

        internal static long ParseDate(string s)
		{
			if (string.IsNullOrEmpty(s) || s == "-") return long.MinValue;
			//Jun 29, 2010
			return DateTime.Parse(s).Ticks;
		}

        private const string UpdateDateTag= "DateLastUpdated";

        private void ParseRefClinVarAssertion(XElement xElement)
		{
			if (xElement==null || xElement.IsEmpty) return;
			//<ReferenceClinVarAssertion DateCreated="2013-10-28" DateLastUpdated="2016-04-20" ID="182406">
		    _lastUpdatedDate = ParseDate(xElement.Attribute(UpdateDateTag)?.Value);
			
            foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "RecordStatus":
						_recordStatus = child.StringValues[0];
						break;
					case "ClinVarAccession":
						_id = child.Attributes["Acc"] + "." + child.Attributes["Version"];
						break;
					case "ClinicalSignificance":
						GetClinicalSignificance(child);
						break;
					case "MeasureSet":
						//get variant info like position ref and alt, etc
						ParseMeasureSet(child);
						break;
					case "TraitSet":
						// contains cross ref, phenotype
						ParseTraitSet(child);
						break;

				}
			}
		}
		
		
		private void ParseClinvarAssertion(LitexElement xElement)
		{
			//the  information we want from SCVs is pubmed ids and allele origins
			if (xElement.Children == null) return;

			foreach (var child in xElement.Children)
			{
				if (child.Name== "Citation")
					ParseCitation(child);
				if(child.Name== "Origin")
					_alleleOrigins.Add(child.StringValues[0]);

				ParseClinvarAssertion(child);//keep going deeper
			}
		}


		private void ParseTraitSet(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "Trait":
						// this element contains xref and phenotype name
						ParseTrait(child);
						break;
				}
			}
		}

		private void ParseTrait(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "XRef":
						// this contains MedGen, Orphanet, Omim ids
						ParseXref(child);
						break;
					case "Name":
						ParsePnenotype(child);
						break;
				}
			}
		}
		/// <summary>
		/// Contains phenotype information for the trait
		/// </summary>
		/// <param name="xElement"></param>
		private void ParsePnenotype(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "ElementValue":
						// contains phenotype
						// <ElementValue Type="Preferred">Breast-ovarian cancer, familial 1</ElementValue>
						ParsePhenotypeElementValue(child);
						if (!IsPreferredPhenotype(child))
							return;//we do not want to parse XRef for alternates
						break;
					case "XRef":
						ParseXref(child);
						break;
				}
			}
		}

		private static bool IsPreferredPhenotype(LitexElement xElement)
		{
			if (!xElement.Attributes.ContainsKey("Type")) return false;

			return xElement.Attributes["Type"] == "Preferred";
				
		}

		private void ParsePhenotypeElementValue(LitexElement xElement)
		{
			if (!xElement.Attributes.ContainsKey("Type")) return;
			if (xElement.Attributes["Type"] == "Preferred") 
				_prefPhenotypes.Add(xElement.StringValues[0]);
			if (xElement.Attributes["Type"] == "Alternate")
				_altPhenotypes.Add(xElement.StringValues[0]);
			
		}

        

        private void ParseXref(LitexElement xElement)
		{
			if (! xElement.Attributes.ContainsKey("DB")) return;

			var idAttributes = xElement.Attributes["ID"].Trim(' ');
			switch (xElement.Attributes["DB"])
			{
				case "MedGen":
					_medGenIDs.Add(idAttributes);
					break;
				case "Orphanet":
					_orphanetIDs.Add(idAttributes);
					break;
				case "OMIM":
					if (xElement.Attributes.ContainsKey("Type"))
					    if (xElement.Attributes["Type"] == "Allelic variant" )
                            _allilicOmimIDs.Add(TrimOmimId(idAttributes));
                        else
                            _omimIDs.Add(TrimOmimId(idAttributes));
					break;
				case "dbSNP":
					_dbSnp = string.IsNullOrEmpty(_dbSnp) ? idAttributes : _dbSnp + "," + idAttributes;
					break;
			}
		}

        
        private String TrimOmimId(string id)
	    {
		    return id.TrimStart('P','S');
	    }

		private void ParseCitation(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			
			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "ID":
						if (child.Attributes.ContainsKey("Source") )
						{
							switch (child.Attributes["Source"])
							{
								case "PubMed":
									var value = child.StringValues[0].TrimEnd('.');
								    {
                                        if (long.TryParse(value, out long l) && l <= 99_999_999)//pubmed ids with more than 8 digits are bad
                                            _pubMedIds.Add(l);
                                        else Console.WriteLine($"WARNING:unexpected pubmedID {value}.");
								    }
								    break;
							}
						}
						break;
				}
			}
		}

		private void ParseMeasureSet(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "Measure":
						// this element contains the sequence location info
						ParseMeasure(child);
						break;
				}

			}

		}
/*
        private void ParseAttributeSet(LitexElement xElement)
        {
            if (xElement.Children == null) return;

            foreach (var child in xElement.Children)
            {
                switch (child.Name)
                {
                    case "XRef":
                        ParseXref(child);
                        break;
                }
            }
        }
*/
        private void ParseMeasure(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			_dbSnp = null;
            _allilicOmimIDs.Clear();

			//the variant type is available in the attributes
			string varType = null;
			foreach (var attribute in xElement.Attributes)
			{
				if (attribute.Key == "Type")
					varType = attribute.Value;
			}
            var variantList = new List<ClinvarVariant>();
			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "SequenceLocation":
						var variant = GetClinvarVariant(child,  _sequenceProvider.GenomeAssembly,_refChromDict);
				        
				        if (variant != null)
						{
							variant.VariantType = varType;
							if (variant.AltAllele !=null && variant.AltAllele.Length == 1 && _iupacBases.ContainsKey(variant.AltAllele[0]))
								AddIupacVariants(variant, variantList);
							else
                                variantList.Add(variant);
                        }
						break;
					case "XRef":
						ParseXref(child);
						break;
            //        case "AttributeSet":
				        //ParseAttributeSet(child);
				        //break;


				}
			}
            if (_dbSnp == null)
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
            //if we don't have a dbSNP for this variant, we will skip it
            foreach (var variant in _variantList)
			{
				variant.DbSnp = _dbSnp;
			}
            
		    
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

		private static ClinvarVariant GetClinvarVariant(LitexElement xElement, GenomeAssembly genomeAssembly,IDictionary<string,IChromosome> refChromDict)
		{
			if (xElement.Children == null) return null;
			//<SequenceLocation Assembly="GRCh38" Chr="17" Accession="NC_000017.11" start="43082402" stop="43082402" variantLength="1" referenceAllele="A" alternateAllele="C" />

			string  referenceAllele = null, altAllele = null;
		    IChromosome chromosome = null;
			int start=0, stop=0;
			foreach (var attribute in xElement.Attributes)
			{
				switch (attribute.Key)
				{
					case "Assembly":
                        if (attribute.Value != genomeAssembly.ToString()
                            && genomeAssembly != GenomeAssembly.Unknown) return null;
                        break;
					case "Chr":                       
						chromosome = refChromDict.ContainsKey(attribute.Value)?refChromDict[attribute.Value]:null ;
						break;
					case "display_start":
						start = Convert.ToInt32(attribute.Value);
						break;
					case "display_stop":
						stop = Convert.ToInt32(attribute.Value);
						break;
					case "referenceAllele":
						referenceAllele = attribute.Value;
						break;
					case "alternateAllele":
						altAllele= attribute.Value;
						break;
				}
			}

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


		private void GetClinicalSignificance(LitexElement xElement)
		{
			if (xElement.Children == null) return;

			foreach (var child in xElement.Children)
			{
				switch (child.Name)
				{
					case "ReviewStatus":
						_reviewStatus = child.StringValues[0];
						break;
					case "Description":
						_significance = child.StringValues[0].ToLower();
						break;
				}
			}
		}
    }
}
