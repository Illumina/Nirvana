using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.NSA
{
    public sealed class SupplementaryAnnotation:ISupplementaryAnnotation
    {
        public string JsonKey { get; }
        private readonly bool _isArray;
        private readonly bool _isPositional;
        private readonly string _jsonString;
        private readonly IEnumerable<string> _jsonStrings;
        
        public SupplementaryAnnotation(string key, bool isArray, bool isPositional, string jsonString, IEnumerable<string> jsonStrings)
        {
            JsonKey       = key;
            _isArray      = isArray;
            _isPositional = isPositional;
            _jsonString   = jsonString;
            _jsonStrings  = jsonStrings;

            if (_isArray && _jsonStrings == null)
            {
                throw new UserErrorException($"No list of json strings provided for a supplementary annotation of array type!! JsonKey: {JsonKey}");
            }
            if (!_isArray && string.IsNullOrEmpty(jsonString))
                throw new UserErrorException("ERROR: No json string provided for a supplementary annotation of non-array type!!");
        }

        public string GetJsonString()
        {
            if (_isPositional) return _jsonString;
            return !_isArray ? $"{{{_jsonString}}}" : $"[{string.Join(',',_jsonStrings.Select(FormatJsonArrayString))}]";
        }

        private string FormatJsonArrayString(string x)
        {
            return x.StartsWith("\"rs") ? x : "{" + x + "}";
        }

    }
}