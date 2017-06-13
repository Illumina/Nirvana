using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.Annotation
{
    public sealed class GeneFusionAnnotation : IJsonSerializer
    {
        #region members

        private readonly int? _exon;
        private readonly int? _intron;
        private readonly List<GeneFusion> _fusions;
        private readonly BreakendTranscriptAnnotation _pos1Annotation;

        #endregion

        private sealed class GeneFusion : IJsonSerializer
        {
            public string HgvsCodingName;
            public int? Exon;
            public int? Intron;

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                // data section
                sb.Append(JsonObject.OpenBrace);

                jsonObject.AddStringValue("hgvsc", HgvsCodingName);

             
                jsonObject.AddIntValue("intron", Intron);
                jsonObject.AddIntValue("exon", Exon);

                sb.Append(JsonObject.CloseBrace);
            }
        }

        public GeneFusionAnnotation(BreakendTranscriptAnnotation breakendTa)
        {
            _exon = breakendTa.Exon;
            _intron = breakendTa.Intron;
            _fusions = new List<GeneFusion>();
            _pos1Annotation = breakendTa;
        }

        public void AddGeneFusion(BreakendTranscriptAnnotation pos2Annotation)
        {
            if (!_pos1Annotation.IsGeneFusion(pos2Annotation)) return;
            var fusion = new GeneFusion
            {
                Exon = pos2Annotation.Exon,
                Intron = pos2Annotation.Intron,
                HgvsCodingName = AssignHgvsc(pos2Annotation)
            };

            _fusions.Add(fusion);

        }

        private string AssignHgvsc(BreakendTranscriptAnnotation pos2Annotation)
        {
            if (_pos1Annotation.IsTranscriptSuffix)
            {
                return pos2Annotation.HgvsDescription + "_" + _pos1Annotation.HgvsDescription;
            }
            return _pos1Annotation.HgvsDescription + "_" + pos2Annotation.HgvsDescription;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            jsonObject.AddIntValue("intron", _intron);
            jsonObject.AddIntValue("exon", _exon);

            if (_fusions.Count > 0) jsonObject.AddObjectValues("fusions", _fusions);

            sb.Append(JsonObject.CloseBrace);
        }
    }
}