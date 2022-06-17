using System.Collections.Generic;
using System.Text;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.IO;

namespace Vcf.Info
{
    public sealed class CustomFields:ICustomFields
    {
        private readonly Dictionary<string, string> _keyValues=new ();

        public void Add(string key, string value)
        {
            _keyValues.Add(key, value);
        }

        public void Clear()
        {
            _keyValues.Clear();
        }

        public bool IsEmpty() =>_keyValues.Count == 0;
        
        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);

            foreach (var (key, value) in _keyValues)
            {
                jsonObject.AddStringValue(key, value);
            }
            sb.Append(JsonObject.CloseBrace);
        }

        public override string ToString()
        {
            var sb = StringBuilderPool.Get();
            SerializeJson(sb);
            var s = sb.ToString();
            StringBuilderPool.Return(sb);
            return s;
        }
    }
}