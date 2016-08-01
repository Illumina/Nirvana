using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Utilities
{
    public static class DumperUtilities
    {
        /// <summary>
        /// returns an int given an AbstractData type that is
        /// inherited by a StringKeyValue
        /// </summary>
        public static int GetInt32(AbstractData ad)
        {
            var s = GetString(ad);

            // sanity check: handle null values
            if (s == null) return -1;

            int ret;

            if (!int.TryParse(s, out ret))
            {
                throw new ApplicationException($"Unable to convert the string ({s}) to an integer.");
            }

            return ret;
        }

        /// <summary>
        /// returns an bool given an AbstractData type that is
        /// inherited by a StringKeyValue
        /// </summary>
        public static bool GetBool(AbstractData ad)
        {
            int num = GetInt32(ad);
            return num == 1;
        }

        /// <summary>
        /// returns a string given an AbstractData type that is
        /// inherited by a StringKeyValue
        /// </summary>
        public static string GetString(AbstractData ad)
        {
            var stringKeyValue = ad as StringKeyValue;

            if (stringKeyValue == null)
            {
                throw new ApplicationException(
                    $"Unable to convert the AbstractData type to a StringKeyValue type: [{ad.Key}]");
            }

            return stringKeyValue.Value;
        }

        /// <summary>
        /// returns true if the data type is a pointer to another type
        /// </summary>
        public static bool IsReference(AbstractData ad)
        {
            var referenceStringValue = ad as ReferenceStringValue;
            if ((referenceStringValue != null) && referenceStringValue.Value.StartsWith("$VAR1->")) return true;

            var referenceKeyValue = ad as ReferenceKeyValue;
            return (referenceKeyValue != null) && referenceKeyValue.Value.StartsWith("$VAR1->");
        }

        /// <summary>
        /// returns true if the data type is undefined
        /// </summary>
        public static bool IsUndefined(AbstractData ad)
        {
            var stringKeyValue = ad as StringKeyValue;
            if (stringKeyValue == null) return false;
            return stringKeyValue.Value == null;
        }

        /// <summary>
        /// retrieves a pre-populated list
        /// </summary>
        public static List<T> GetPopulatedList<T>(int desiredSize)
        {
            var list = new List<T>(desiredSize);
            for (int i = 0; i < desiredSize; i++) list.Add(default(T));
            return list;
        }
    }
}
