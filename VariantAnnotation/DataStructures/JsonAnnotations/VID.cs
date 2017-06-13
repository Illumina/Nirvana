using System.Security.Cryptography;
using System.Text;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    public class VID
    {
        private readonly StringBuilder _md5Builder = new StringBuilder();
        private readonly MD5 _md5Hash = MD5.Create();

        /// <summary>
        /// constructs a VID based on the supplied feature
        /// </summary>
        public string Create(IChromosomeRenamer renamer, string referenceName, VariantAlternateAllele altAllele)
        {
            referenceName = renamer.GetEnsemblReferenceName(referenceName);
            string vid;
            switch (altAllele.NirvanaVariantType)
            {
                case VariantType.SNV:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.AlternateAllele}";
                    break;

                case VariantType.insertion:
                    vid = altAllele.IsStructuralVariant
                        ? $"{referenceName}:{altAllele.Start}:{altAllele.End}:INS"
                        : $"{referenceName}:{altAllele.Start}:{altAllele.End}:{GetInsertedAltAllele(altAllele.AlternateAllele)}";
                    break;

                case VariantType.deletion:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}";
                    break;

                case VariantType.MNV:
                case VariantType.indel:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:{GetInsertedAltAllele(altAllele.AlternateAllele)}";
                    break;

                case VariantType.duplication:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:DUP";
                    break;

                case VariantType.tandem_duplication:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:TDUP";
                    break;

                case VariantType.translocation_breakend:
                    vid = altAllele.BreakEnds?[0].ToString();
                    break;

                case VariantType.inversion:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:Inverse";
                    break;

                case VariantType.mobile_element_insertion:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:MEI";
                    break;

                case VariantType.copy_number_gain:
                case VariantType.copy_number_loss:
                case VariantType.copy_number_variation:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:{altAllele.CopyNumber}";
                    break;

                case VariantType.reference_no_call:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:NC";
                    break;
				case VariantType.short_tandem_repeat_variant:
					vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}:{altAllele.RepeatUnit}:{altAllele.RepeatCount}";
					break;
                default:
                    vid = $"{referenceName}:{altAllele.Start}:{altAllele.End}";
                    break;
            }

            return vid;
        }

        private string GetInsertedAltAllele(string altAllele)
        {
            if (altAllele.Length <= 32) return altAllele;

            var data = _md5Hash.ComputeHash(Encoding.UTF8.GetBytes(altAllele));

            _md5Builder.Clear();
            foreach (var b in data) _md5Builder.Append(b.ToString("x2"));
            return _md5Builder.ToString();
        }
    }
}
