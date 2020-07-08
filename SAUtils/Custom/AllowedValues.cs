using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;

namespace SAUtils.Custom
{
    public static class AllowedValues
    {
        private const int MaxFilterLength = 20;
        private const int MaxIdentifierLength = 50;
        private const int MaxDescriptionLength = 100;
        private static readonly string[] EmptyValues = {".", ""};
        private static readonly HashSet<string> PredictionValues = new HashSet<string>
        {
            "pathogenic",
            "p",
            "likely pathogenic",
            "lp",
            "vus",
            "likely benign",
            "lb",
            "benign",
            "b"
        };

        public static void ValidatePredictionValue(string value, string line)
        {
            if (!IsEmptyValue(value) && !PredictionValues.Contains(value.ToLower()))
                throw new UserErrorException($"{value} is not a valid prediction value.\nInput line: {line}");
        }

        public static void ValidateFilterValue(string value, string line) => CheckValueLength(value, line, MaxFilterLength);

        public static void ValidateIdentifierValue(string value, string line) => CheckValueLength(value, line, MaxIdentifierLength);

        public static void ValidateDescriptionValue(string value, string line) => CheckValueLength(value, line, MaxDescriptionLength);

        public static void ValidateScoreValue(string value, string line)
        {
            if (double.TryParse(value, out _)) return;
            
            var e = new UserErrorException(
                $"{value} is not a valid score value. Scores are expected to be numbers.");
            e.Data["Line"] = line;
            throw e;

        }

        public static bool IsEmptyValue(string value) => EmptyValues.Contains(value);

        private static void CheckValueLength(string value, string line, int maxLength)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
                throw new UserErrorException($"\"{value}\" exceeds the allowed length for descriptions ({maxLength} characters).\nInput line:{line}");
        }
    }
}