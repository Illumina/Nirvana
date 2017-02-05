using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public sealed class SupplementaryPositionCreator
	{
		#region member

		public SupplementaryAnnotationPosition SaPosition { get; }
		public int ReferencePosition => SaPosition.ReferencePosition;
		public string RefSeqName { get; set; }
		public string RefAllele { private get; set; }

		public double RefAlleleFreq = Double.MinValue;
		private const double RmaFreqThreshold = 0.95;

		#endregion

		public SupplementaryPositionCreator(SupplementaryAnnotationPosition saPosition = null)
		{			
			if(saPosition == null) saPosition = new SupplementaryAnnotationPosition();
			SaPosition = saPosition;
		}

		public void MergeSaCreator(SupplementaryPositionCreator other)
		{
			if (ReferencePosition != other.ReferencePosition || RefSeqName != other.RefSeqName) return;

			// first the allele specific annotations are merged
			MergeAlleleSpecificAnnotations(other);


			// merge positional annotations
			MergePositionalAnnotations(other);

			// merging custom annotations: they cannot be categorized into positional or otherwise since that is only known from each customItem's IsAlleleSpecificFlag
			SaPosition.CustomItems.AddRange(other.SaPosition.CustomItems);
		}

		private void MergeAlleleSpecificAnnotations(SupplementaryPositionCreator other)
		{
			foreach (var otherAlleleAnnotation in other.SaPosition.AlleleSpecificAnnotations)
			{
				var otherAsa = otherAlleleAnnotation.Value;

				AlleleSpecificAnnotation asa;
				if (SaPosition.AlleleSpecificAnnotations.TryGetValue(otherAlleleAnnotation.Key, out asa))
				{
					asa.MergeAlleleSpecificAnnotation(otherAsa);
				}
				else
				{
					// this is for a new alternate allele
					SaPosition.AlleleSpecificAnnotations[otherAlleleAnnotation.Key] = otherAsa;
				}
			}


		}
		private void MergePositionalAnnotations(SupplementaryPositionCreator other)
		{
			//TODO: figure out if this part is necessary, maybe we can calculate at the last step
			if (!other.RefAlleleFreq.Equals(Double.MinValue))
			{
				RefAllele = other.RefAllele;
				RefAlleleFreq = other.RefAlleleFreq;
			}


			// a cosmic id may have multiple entries each for a study. So this needs special handling
			foreach (var cosmicItem in other.SaPosition.CosmicItems)
				cosmicItem.AddCosmicToSa(this);

			SaPosition.ClinVarItems.AddRange(other.SaPosition.ClinVarItems);
		}


		public void FinalizePositionalAnnotations()
		{

			CheckReferenceMinor();//should be silenced for GRCh38 till we find a reliable data source.

			var count = 0;
			if (!RefAlleleFreq.Equals(Double.MinValue)) count++;

			//set allele frequencies for alternative allele;
			foreach (var asa in SaPosition.AlleleSpecificAnnotations.Values)
			{
				if(!asa.HasDataSource(DataSourceCommon.DataSource.DbSnp)) continue;
				var dbSnpAnnotation = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;
				if(dbSnpAnnotation == null) continue;

				if (!dbSnpAnnotation.AltAlleleFreq.Equals(Double.MinValue))
					asa.AltAlleleFreq = dbSnpAnnotation.AltAlleleFreq;
			}

			count += SaPosition.AlleleSpecificAnnotations.Count(asa => !asa.Value.AltAlleleFreq.Equals(Double.MinValue));

			if (count == 0) return;

			//certain values like the GMAF and GMA can be computed only after all alt alleles have been seen
			var alleleFreqDict = new Dictionary<string, double>(count);

			if (!RefAlleleFreq.Equals(Double.MinValue)) alleleFreqDict[RefAllele] = RefAlleleFreq;

			foreach (var asa in SaPosition.AlleleSpecificAnnotations)
			{
				alleleFreqDict[asa.Key] = asa.Value.AltAlleleFreq;
			}

			SaPosition.GlobalMajorAllele = GetMostFrequentAllele(alleleFreqDict, RefAllele);
			if (SaPosition.GlobalMajorAllele != null)
				SaPosition.GlobalMajorAlleleFrequency = alleleFreqDict[SaPosition.GlobalMajorAllele].ToString(CultureInfo.InvariantCulture);
			else return;//no global alleles available

			alleleFreqDict.Remove(SaPosition.GlobalMajorAllele);

			SaPosition.GlobalMinorAllele = GetMostFrequentAllele(alleleFreqDict, RefAllele, false);
			if (SaPosition.GlobalMinorAllele != null)
				SaPosition.GlobalMinorAlleleFrequency = alleleFreqDict[SaPosition.GlobalMinorAllele].ToString(CultureInfo.InvariantCulture);

		}

		private static string GetMostFrequentAllele(Dictionary<string, double> alleleFreqDict, string refAllele, bool isRefPreferred = true)
		{
			if (alleleFreqDict.Count == 0) return null;

			// find all alleles that have max frequency.
			var maxFreq = alleleFreqDict.Values.Max();
			if (Math.Abs(maxFreq - Double.MinValue) < Double.Epsilon) return null;

			var maxFreqAlleles = (from pair in alleleFreqDict where Math.Abs(pair.Value - maxFreq) < Double.Epsilon select pair.Key).ToList();


			// if there is only one with max frequency, return it
			if (maxFreqAlleles.Count == 1)
				return maxFreqAlleles[0];

			// if ref is preferred (as in global major) it is returned
			if (isRefPreferred && maxFreqAlleles.Contains(refAllele))
				return refAllele;

			// else refAllele is removed and the first of the remaining allele is returned (arbitrary selection)
			maxFreqAlleles.Remove(refAllele);
			return maxFreqAlleles[0];

		}


		private void CheckReferenceMinor()
		{
			double totalMinorAlleleFreq = 0;
			SaPosition.IsRefMinorAllele = false;

			// we have to check if the total minor allele freq has crossed the threshold to be tagged as a ref minor
			// Note that for now only SNVs are considered.
			foreach (var asaPair in SaPosition.AlleleSpecificAnnotations)
			{
				if (!IsSnv(asaPair.Key)) continue;
				var asa = asaPair.Value;

				if (!asa.HasDataSource(DataSourceCommon.DataSource.OneKg) ||
					asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] == null) continue;

				var oneKAsa = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as OneKGenAnnotation;
				if (oneKAsa?.OneKgAllAn != null && oneKAsa.OneKgAllAn.Value > 0 && oneKAsa.OneKgAllAc != null)
				{
					totalMinorAlleleFreq += (double)oneKAsa.OneKgAllAc / oneKAsa.OneKgAllAn.Value;
				}
			}

			SaPosition.IsRefMinorAllele = totalMinorAlleleFreq >= RmaFreqThreshold;

			if (!(totalMinorAlleleFreq > RmaFreqThreshold)) return;

			SaPosition.IsRefMinorAllele = true;
		}

	    public static bool IsSnv(string allele)
		{
			if (allele.Length != 1) return false;

			allele = allele.ToUpper();

			if (allele == "A" || allele == "C" || allele == "G" || allele == "T") return true;

			return false;
		}

		public bool IsEmpty()
		{
			return SaPosition.GlobalMajorAllele == null && SaPosition.GlobalMinorAllele == null &&
			       SaPosition.GlobalMajorAlleleFrequency == null && SaPosition.GlobalMinorAlleleFrequency == null &&
			       SaPosition.ClinVarItems.Count == 0 && SaPosition.AlleleSpecificAnnotations.Count == 0 &&
				   SaPosition.CosmicItems.Count == 0 && SaPosition.CustomItems.Count == 0;
		}

		public bool IsRefMinor()
		{
			return SaPosition.IsRefMinorAllele;
		}

		public void WriteAnnotation(ExtendedBinaryWriter writer)
		{
			SaPosition.Write(writer);
		}



		public void AddExternalDataToAsa(DataSourceCommon.DataSource dataSource, string altAllele, ISupplementaryAnnotation annotation)
		{
			if(annotation == null) return;
			var dataSourceIndex = DataSourceCommon.GetIndex(dataSource);

			if (!SaPosition.AlleleSpecificAnnotations.ContainsKey(altAllele))
			{
				SaPosition.AlleleSpecificAnnotations[altAllele] = new AlleleSpecificAnnotation();
				DataSourceCommon.AddDataSource(dataSource,ref SaPosition.AlleleSpecificAnnotations[altAllele].SaDataSourceFlag);
				SaPosition.AlleleSpecificAnnotations[altAllele].Annotations[dataSourceIndex] =
					annotation;
				return;
			}

			if (!SaPosition.AlleleSpecificAnnotations[altAllele].HasDataSource(dataSource))
			{
				DataSourceCommon.AddDataSource(dataSource, ref SaPosition.AlleleSpecificAnnotations[altAllele].SaDataSourceFlag);

				SaPosition.AlleleSpecificAnnotations[altAllele].Annotations[dataSourceIndex] =
					annotation;
				return;
			}

			SaPosition.AlleleSpecificAnnotations[altAllele].Annotations[dataSourceIndex].MergeAnnotations(annotation);
		}
	}

}