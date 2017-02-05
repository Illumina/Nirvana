using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using XmlTextReader = System.Xml.XmlReader;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.ClinVar
{
	public sealed class ClinvarVariant
	{
		public readonly string Chromosome;
		public int Start { get; }
		public readonly int Stop;
		public readonly string ReferenceAllele;
		public readonly string AltAllele;
		public string DbSnp;
		public string VariantType;

		public ClinvarVariant(string chr, int start, int stop, string refAllele, string altAllele)
		{
			Chromosome      = chr;
			Start           = start;
			Stop            = stop;
			ReferenceAllele = refAllele;
			AltAllele       = altAllele;
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
        private readonly DataFileManager _dataFileManager;
        private readonly ICompressedSequence _compressedSequence;

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
        public ClinVarXmlReader(FileInfo clinVarXmlFileInfo, CompressedSequenceReader reader,
            ICompressedSequence compressedSequence)
        {
            _dataFileManager = new DataFileManager(reader, compressedSequence);
            _compressedSequence = compressedSequence;
            _aligner = new VariantAligner(compressedSequence);
            _clinVarXmlFileInfo = clinVarXmlFileInfo;
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
			using (var xmlReader = XmlTextReader.Create(reader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, IgnoreWhitespace = true }))
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
			foreach (var variant in _variantList)
			{
				// in order to match the VCF, we leave out the ones that do not have dbsnp id
				if (variant.DbSnp == null) continue;
				if (!InputFileParserUtilities.IsDesiredChromosome(variant.Chromosome, _compressedSequence.Renamer)) continue;
				if (variant.VariantType == "Microsatellite") continue;

                var refIndex = _compressedSequence.Renamer.GetReferenceIndex(variant.Chromosome);
                if (refIndex == ChromosomeRenamer.UnknownReferenceIndex) throw new GeneralException($"Could not find the reference index for: {variant.Chromosome}");
                _dataFileManager.LoadReference(refIndex, () => {});

                ClinvarVariant shiftedVariant= variant;
				//some entries do not have ref allele in the xml file. For those, we extract them from our ref sequence
				if (variant.ReferenceAllele == null && variant.VariantType=="Deletion")
					shiftedVariant = GenerateRefAllele(variant, _compressedSequence);
				if (variant.AltAllele == null && variant.VariantType == "Duplication")
					shiftedVariant = GenerateAltAllele(variant, _compressedSequence);


				//left align the variant
				shiftedVariant = LeftShift(shiftedVariant);

				if (variant.ReferenceAllele == null && variant.VariantType == "Indel" && variant.AltAllele != null)
					shiftedVariant = GenerateRefAllele(variant, _compressedSequence);

				_pubMedIds.Sort();
				
				if(string.IsNullOrEmpty(shiftedVariant.ReferenceAllele) && string.IsNullOrEmpty(shiftedVariant.AltAllele)) continue;

				clinvarList.Add(
					new ClinVarItem(shiftedVariant.Chromosome, 
					shiftedVariant.Start, 
					_alleleOrigins.Distinct().ToList(), 
					shiftedVariant.AltAllele??"", 
					_id, 
					_reviewStatus,
					_medGenIDs.Distinct().ToList(),
					_omimIDs.Distinct().ToList(), 
					_orphanetIDs.Distinct().ToList(), 
					_prefPhenotypes.Count > 0? _prefPhenotypes.Distinct().ToList(): _altPhenotypes.Distinct().ToList(), 
					shiftedVariant.ReferenceAllele??"", 
					_significance, 
					_pubMedIds.Distinct().ToList(), 
					_lastUpdatedDate));
			}

			return clinvarList.Count > 0 ? clinvarList: null;
		}

		
		private static ClinvarVariant GenerateAltAllele(ClinvarVariant variant, ICompressedSequence compressedSequence)
		{
			if (variant == null) return null;
			var extractedAlt = compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);

			return new ClinvarVariant(variant.Chromosome, variant.Start, variant.Stop, variant.ReferenceAllele ?? "", extractedAlt);
		}

		private static ClinvarVariant GenerateRefAllele(ClinvarVariant variant, ICompressedSequence compressedSequence)
		{
			if (variant == null) return null;
			var extractedRef = compressedSequence.Substring(variant.Start - 1, variant.Stop - variant.Start + 1);

			return new ClinvarVariant(variant.Chromosome, variant.Start, variant.Stop, extractedRef, variant.AltAllele?? "");
		}

		private ClinvarVariant LeftShift(ClinvarVariant variant)
		{
			if (variant.ReferenceAllele == null || variant.AltAllele == null) return variant;

			var alignedVariant = _aligner.LeftAlign(variant.Start, variant.ReferenceAllele, variant.AltAllele);
			if (alignedVariant == null) return variant;

			return new ClinvarVariant(variant.Chromosome, alignedVariant.Item1, variant.Stop, alignedVariant.Item2,alignedVariant.Item3);
		}

		private static LiteXmlElement ParseXmlElement(XmlTextReader xmlReader)
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
			
			switch (xmlElement.Attributes["DB"])
			{
				case "MedGen":
					_medGenIDs.Add(xmlElement.Attributes["ID"]);
					break;
				case "Orphanet":
					_orphanetIDs.Add(xmlElement.Attributes["ID"]);
					break;
				case "OMIM":
					if (xmlElement.Attributes.ContainsKey("Type"))
						if (xmlElement.Attributes["Type"]=="MIM")
							_omimIDs.Add(xmlElement.Attributes["ID"]);
					break;
				case "dbSNP":
					_dbSnp = string.IsNullOrEmpty(_dbSnp) ? xmlElement.Attributes["ID"] : _dbSnp + "," + xmlElement.Attributes["ID"];
					break;
			}
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

		private void ParseMeasure(LiteXmlElement xmlElement)
		{
			if (xmlElement.Children == null) return;

			_dbSnp = null;

			//the variant type is available in the attributes
			string varType = null;
			foreach (var attribute in xmlElement.Attributes)
			{
				if (attribute.Key == "Type")
					varType = attribute.Value;
			}

			foreach (var child in xmlElement.Children)
			{
				switch (child.Name)
				{
					case "SequenceLocation":
						var variant = GetClinvarVariant(child, _compressedSequence.GenomeAssembly);
						if (variant != null)
						{
							variant.VariantType = varType;
							if (variant.AltAllele !=null && variant.AltAllele.Length == 1 && _iupacBases.ContainsKey(variant.AltAllele[0]))
								AddIupacVariants(variant);
							else _variantList.Add(variant);
						}
						break;
					case "XRef":
						ParseXref(child);
						break;
					
				}
			}
			//if we don't have a dbSNP for this variant, we will skip it
			if (_dbSnp == null)
			{
				_variantList.Clear();
				return;
			}
			foreach (var variant in _variantList)
			{
				variant.DbSnp = _dbSnp;
			}
		}

		private void AddIupacVariants(ClinvarVariant variant)
		{
			foreach (var altAllele in _iupacBases[variant.AltAllele[0]])
			{
				_variantList.Add(new ClinvarVariant(variant.Chromosome,variant.Start, variant.Stop, variant.ReferenceAllele, altAllele.ToString()));
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

		private static ClinvarVariant GetClinvarVariant(LiteXmlElement xmlElement, GenomeAssembly genomeAssembly)
		{
			if (xmlElement.Children == null) return null;
			//<SequenceLocation Assembly="GRCh38" Chr="17" Accession="NC_000017.11" start="43082402" stop="43082402" variantLength="1" referenceAllele="A" alternateAllele="C" />

			string chromosome = null, referenceAllele = null, altAllele = null;
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
						chromosome = attribute.Value;
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
