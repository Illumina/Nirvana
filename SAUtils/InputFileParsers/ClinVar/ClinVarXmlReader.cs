using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
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
			ReferenceAllele = refAllele;
			AltAllele       = altAllele;
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

		public sealed class LiteXmlElement
		{
			public readonly string Name;
			public readonly Dictionary<string, string> Attributes = new Dictionary<string, string>();
			public readonly List<LiteXmlElement> Children = new List<LiteXmlElement>();
			public readonly List<string> StringValues= new List<string>();

			public LiteXmlElement(string name)
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
				string elementName = null;
				
				//skipping the top level element to go down to its children
				xmlReader.ReadToDescendant("ClinVarSet");

				do
				{
					LiteXmlElement xmlElement = null;

					switch (xmlReader.NodeType)
					{
						case XmlNodeType.Element: // The node is an element.
							elementName = xmlReader.Name;
							xmlElement = ParseXmlElement(xmlReader);
							break;
						case XmlNodeType.EndElement: //Display the end of the element.
							// Release set is the top level element we skipped. So, we will encounter this mismatch.
							if (xmlReader.Name != "ReleaseSet" && xmlReader.Name != elementName)
								throw new InvalidDataException("WARNING!! encountered unexpected endElement tag:" + xmlReader.Name);
							break;
						default:
							continue;
					}

					var clinVarItems = ExtractClinVarItems(xmlElement);

					if (clinVarItems == null) continue;

					foreach (var clinVarItem in clinVarItems)
					{
						yield return clinVarItem;
					}
				} while (xmlReader.Read());
			}
			
		}

		private List<ClinVarItem> ExtractClinVarItems(LiteXmlElement xmlElement)
		{
			ClearClinvarFields();

			if (xmlElement == null) return null;
			if (xmlElement.IsEmpty()) return null;

			foreach (var child in xmlElement.Children)
			{
				switch (child.Name)
				{
					case "ReferenceClinVarAssertion":
						ParseRefClinVarAssertion(child);
						break;
					case "ClinVarAssertion":
						ParseScv(child);
						break;
				}

			}

			if (_recordStatus != "current")
			{
				Console.WriteLine($"record status not current: {_recordStatus} for {_id}");
				return null;
			}

			var clinvarList = new List<ClinVarItem>();
            var clinvarSet = new HashSet<ClinvarVariant>();
			foreach (var variant in _variantList)
			{
                // in order to match the VCF, we leave out the ones that do not have dbsnp id
				if (variant.DbSnp == null) continue;

                if(variant.Chromosome == null) continue;

                if (variant.VariantType == "Microsatellite" 
                    && string.IsNullOrEmpty(variant.AltAllele)) continue;

                _sequenceProvider.LoadChromosome(variant.Chromosome);


                if (!ValidateRefAllele(variant)) continue;
                

				ClinvarVariant shiftedVariant= variant;
				//some entries do not have ref allele in the xml file. For those, we extract them from our ref sequence
				if (variant.ReferenceAllele == null && variant.VariantType=="Deletion")
					shiftedVariant = GenerateRefAllele(variant, _sequenceProvider.Sequence);
				if (variant.AltAllele == null && variant.VariantType == "Duplication")
					shiftedVariant = GenerateAltAllele(variant, _sequenceProvider.Sequence);

				

				//left align the variant
				shiftedVariant = LeftShift(shiftedVariant);
                
                if (variant.ReferenceAllele == null && variant.VariantType == "Indel" && variant.AltAllele != null)
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
		                clinvarVariant.AltAllele ?? "",
		                _id,
		                _reviewStatus,
		                _medGenIDs.Distinct().ToList(),
		                extendedOmimIds.Distinct().ToList(),
		                _orphanetIDs.Distinct().ToList(),
		                _prefPhenotypes.Count > 0 ? _prefPhenotypes.Distinct().ToList() : _altPhenotypes.Distinct().ToList(),
		                clinvarVariant.ReferenceAllele ?? "",
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

			return new ClinvarVariant(variant.Chromosome, variant.Start, variant.Stop, variant.ReferenceAllele ?? "", extractedAlt, variant.AllelicOmimIds);
		}

		private static ClinvarVariant GenerateRefAllele(ClinvarVariant variant, ISequence compressedSequence)
		{
			if (variant == null) return null;
			var extractedRef = compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);

			return new ClinvarVariant(variant.Chromosome, variant.Start, variant.Stop, extractedRef, variant.AltAllele?? "", variant.AllelicOmimIds);
		}

		private ClinvarVariant LeftShift(ClinvarVariant variant)
		{
			if (variant.ReferenceAllele == null || variant.AltAllele == null) return variant;

			var alignedVariant = _aligner.LeftAlign(variant.Start, variant.ReferenceAllele, variant.AltAllele);
			if (alignedVariant == null) return variant;

			return new ClinvarVariant(variant.Chromosome, alignedVariant.Item1, variant.Stop, alignedVariant.Item2,alignedVariant.Item3, variant.AllelicOmimIds);
		}

		private static LiteXmlElement ParseXmlElement(XmlReader xmlReader)
		{
			var xmlElement = new LiteXmlElement(xmlReader.Name);

			var isEmptyElement = xmlReader.IsEmptyElement;
			if (xmlReader.HasAttributes)
			{
				while (xmlReader.MoveToNextAttribute())
					xmlElement.Attributes[xmlReader.Name] = xmlReader.Value;
			}

			if (isEmptyElement)
				return xmlElement.IsEmpty()? null: xmlElement;

			while (xmlReader.Read())
			{
				//we will read till an end tag is observed
				switch (xmlReader.NodeType)
				{
					case XmlNodeType.Element: // The node is an element.
						var child = ParseXmlElement(xmlReader);
						if (child != null ) xmlElement.Children.Add(child);
						break;
					case XmlNodeType.Text:
						if (! string.IsNullOrEmpty(xmlReader.Value))
							xmlElement.StringValues.Add(xmlReader.Value);
						break;
					case XmlNodeType.EndElement: //Display the end of the element.
						if (xmlReader.Name == xmlElement.Name)
						{
							return xmlElement.IsEmpty()? null: xmlElement;
						}
						Console.WriteLine("WARNING!! encountered unexpected endElement tag:"+xmlReader.Name);
						break;
				}
			}
			return null;
		}

/*
		private static void ReadToEndOfElement(XmlTextReader xmlReader)
		{
			var elementName = xmlReader.Name;

			if (xmlReader.IsEmptyElement) return;

			while (xmlReader.Read())
			{
				switch (xmlReader.NodeType)
				{
					case XmlNodeType.EndElement: //Display the end of the element.
						if (xmlReader.Name == elementName)
							return;
						break;
				}
			}
			
		}
*/

        internal static long ParseDate(string s)
		{
			if (s == "-") return long.MinValue;
			//Jun 29, 2010
			return DateTime.Parse(s).Ticks;
		}

		private void ParseRefClinVarAssertion(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;
			//<ReferenceClinVarAssertion DateCreated="2013-10-28" DateLastUpdated="2016-04-20" ID="182406">
			foreach (var attribute in xmlElement.Attributes)
			{
				if (attribute.Key == "DateLastUpdated")
					_lastUpdatedDate = ParseDate(attribute.Value);
			}

			foreach (var child in xmlElement.Children)
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
		
		
		private void ParseScv(LiteXmlElement xmlElement)
		{
			//the  information we want from SCVs is pubmed ids and allele origins
			if (xmlElement.Children == null) return;

			foreach (var child in xmlElement.Children)
			{
				if (child.Name== "Citation")
					ParseCitation(child);
				if(child.Name== "Origin")
					_alleleOrigins.Add(child.StringValues[0]);

				ParseScv(child);//keep going deeper
			}
		}


		private void ParseTraitSet(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			foreach (var child in xmlElement.Children)
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

		private void ParseTrait(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			foreach (var child in xmlElement.Children)
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
		/// <param name="xmlElement"></param>
		private void ParsePnenotype(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			foreach (var child in xmlElement.Children)
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

		private static bool IsPreferredPhenotype(LiteXmlElement xmlElement)
		{
			if (!xmlElement.Attributes.ContainsKey("Type")) return false;

			return xmlElement.Attributes["Type"] == "Preferred";
				
		}

		private void ParsePhenotypeElementValue(LiteXmlElement xmlElement)
		{
			if (!xmlElement.Attributes.ContainsKey("Type")) return;
			if (xmlElement.Attributes["Type"] == "Preferred") 
				_prefPhenotypes.Add(xmlElement.StringValues[0]);
			if (xmlElement.Attributes["Type"] == "Alternate")
				_altPhenotypes.Add(xmlElement.StringValues[0]);
			
		}

        

        private void ParseXref(LiteXmlElement xmlElement)
		{
			if (! xmlElement.Attributes.ContainsKey("DB")) return;

			var idAttributes = xmlElement.Attributes["ID"].Trim(' ');
			switch (xmlElement.Attributes["DB"])
			{
				case "MedGen":
					_medGenIDs.Add(idAttributes);
					break;
				case "Orphanet":
					_orphanetIDs.Add(idAttributes);
					break;
				case "OMIM":
					if (xmlElement.Attributes.ContainsKey("Type"))
					    if (xmlElement.Attributes["Type"] == "Allelic variant" )
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

		private void ParseCitation(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			
			foreach (var child in xmlElement.Children)
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
									value = value.TrimStart('0');
									if (value.All(char.IsDigit) && value.Length <= 8)//pubmed ids with more than 9 digits are bad
										_pubMedIds.Add(Convert.ToInt64(value));

									break;

							}
						}
						break;
				}
			}
		}

		private void ParseMeasureSet(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			foreach (var child in xmlElement.Children)
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
        private void ParseAttributeSet(LiteXmlElement xmlElement)
        {
            if (xmlElement.Children == null) return;

            foreach (var child in xmlElement.Children)
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
        private void ParseMeasure(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			_dbSnp = null;
            _allilicOmimIDs.Clear();

			//the variant type is available in the attributes
			string varType = null;
			foreach (var attribute in xmlElement.Attributes)
			{
				if (attribute.Key == "Type")
					varType = attribute.Value;
			}
            var variantList = new List<ClinvarVariant>();
			foreach (var child in xmlElement.Children)
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

		private static ClinvarVariant GetClinvarVariant(LiteXmlElement xmlElement, GenomeAssembly genomeAssembly,IDictionary<string,IChromosome> refChromDict)
		{
			if (xmlElement.Children == null) return null;
			//<SequenceLocation Assembly="GRCh38" Chr="17" Accession="NC_000017.11" start="43082402" stop="43082402" variantLength="1" referenceAllele="A" alternateAllele="C" />

			string  referenceAllele = null, altAllele = null;
		    IChromosome chromosome = null;
			int start=0, stop=0;
			foreach (var attribute in xmlElement.Attributes)
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


		private void GetClinicalSignificance(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			foreach (var child in xmlElement.Children)
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
