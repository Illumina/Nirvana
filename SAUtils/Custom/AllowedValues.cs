using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;

namespace SAUtils.Custom
{
    public static class AllowedValues
    {
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

        public static bool IsEmptyValue(string value) => EmptyValues.Contains(value);

        public static void ValidateFilterValue(string value, string line)
        {
            if(!string.IsNullOrEmpty(value) && value.Length>20)
                throw  new UserErrorException($"\"{value}\" exceeds the allowed length for filters (20 characters).\nInput line:{line}");
        }
    }
}