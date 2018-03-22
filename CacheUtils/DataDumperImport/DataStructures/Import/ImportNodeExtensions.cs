using System.IO;

namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public static class ImportNodeExtensions
    {
        public static int GetInt32(this IImportNode node)
        {
            string s = GetString(node);
            if (s == null) return -1;

            if (!int.TryParse(s, out int ret))
            {
                throw new InvalidDataException($"Unable to convert the string ({s}) to an integer.");
            }

            return ret;
        }

        public static bool GetBool(this IImportNode node)
        {
            int num = GetInt32(node);
            return num == 1;
        }

        public static string GetString(this IImportNode node)
        {
            if (!(node is StringKeyValueNode stringKeyValue))
            {
                throw new InvalidDataException($"Unable to convert the AbstractData type to a StringKeyValue type: [{node.Key}]");
            }

            string s = stringKeyValue.Value;
            if (s == "" || s == "-") s = null;
            return s;
        }

        public static bool IsUndefined(this IImportNode node)
        {
            if (!(node is StringKeyValueNode stringKeyValue)) return false;
            return stringKeyValue.Value == null;
        }
    }
}
