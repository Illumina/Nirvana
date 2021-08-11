using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptimizedCore;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedPosition : IAnnotatedPosition
    {
        public IPosition                       Position               { get; private set; }
        public string                          CytogeneticBand        { get; set; }
        public IAnnotatedVariant[]             AnnotatedVariants      { get; private set; }
        public IList<ISupplementaryAnnotation> SupplementaryIntervals { get; } = new List<ISupplementaryAnnotation>();

        public void Initialize(IPosition position, IAnnotatedVariant[] annotatedVariants)
        {
            Position          = position;
            AnnotatedVariants = annotatedVariants;
            SupplementaryIntervals.Clear();
        }

   
        public StringBuilder GetJsonStringBuilder()
        {
            if (AnnotatedVariants == null || AnnotatedVariants.Length == 0) return null;

            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);

            string originalChromName = Position.VcfFields[0];

            jsonObject.AddStringValue("chromosome",  originalChromName);
            jsonObject.AddIntValue("position",       Position.Start);

            if (Position.HasShortTandemRepeat)
            {
                jsonObject.AddStringValue("repeatUnit",  Position.InfoData?.RepeatUnit);
                jsonObject.AddIntValue("refRepeatCount", Position.InfoData?.RefRepeatCount);
            }

            if (Position.HasStructuralVariant) jsonObject.AddIntValue("svEnd", Position.InfoData?.End);

            jsonObject.AddStringValue("refAllele", Position.RefAllele);
            jsonObject.AddStringValues("altAlleles", Position.AltAlleles);

            jsonObject.AddDoubleValue("quality", Position.Quality);

            jsonObject.AddStringValues("filters", Position.Filters);

            jsonObject.AddIntValues("ciPos",   Position.InfoData?.CiPos);
            jsonObject.AddIntValues("ciEnd",   Position.InfoData?.CiEnd);
            jsonObject.AddIntValue("svLength", Position.InfoData?.SvLength);
            jsonObject.AddStringValue("breakendEventId", Position.InfoData?.BreakendEventId);

            jsonObject.AddDoubleValue("strandBias",             Position.InfoData?.StrandBias,JsonCommon.FrequencyRoundingFormat);
            jsonObject.AddDoubleValue("fisherStrandBias",             Position.InfoData?.FisherStrandBias,"0.###");
            jsonObject.AddDoubleValue("mappingQuality",             Position.InfoData?.MappingQuality,"0.##");
            jsonObject.AddIntValue("jointSomaticNormalQuality", Position.InfoData?.JointSomaticNormalQuality);
            jsonObject.AddDoubleValue("recalibratedQuality",    Position.InfoData?.RecalibratedQuality);

            jsonObject.AddStringValue("cytogeneticBand", CytogeneticBand);

			if (Position.Samples != null && Position.Samples.Length > 0) jsonObject.AddStringValues("samples", Position.Samples.Select(s => s.GetJsonString()), false);

            if (SupplementaryIntervals != null && SupplementaryIntervals.Any())
            {
                AddSuppIntervalToJsonObject(jsonObject);
            }

            
			var variantStringBuilders = AnnotatedVariants.Select(v => v.GetJsonStringBuilder(originalChromName)).ToArray();
            jsonObject.AddStringValues("variants", variantStringBuilders , false);

            foreach (StringBuilder builder in variantStringBuilders)
            {
                StringBuilderPool.Return(builder);
            }
			sb.Append(JsonObject.CloseBrace);
            return sb;
        }
        private void AddSuppIntervalToJsonObject(JsonObject jsonObject)
        {
            foreach (var si in SupplementaryIntervals) jsonObject.AddObjectValue(si.JsonKey, si);
        }

        
    }
}