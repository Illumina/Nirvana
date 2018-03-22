using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Import;

namespace CacheUtils.DataDumperImport.Utilities
{
    public static class ImportUtilities
    {
        public static string GetPredictionData(this IImportNode node)
        {
            string predictionData = null;

            if (node is ObjectKeyValueNode predictionNode)
            {
                predictionData = ImportPrediction.Parse(predictionNode.Value);
            }
            else if (!node.IsUndefined())
            {
                throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
            }

            return predictionData;
        }

        public static T[] ParseObjectKeyValueNode<T>(this IImportNode node, Func<ObjectValueNode, T[]> parseFunc)
        {
            T[] results;

            if (node is ObjectKeyValueNode keyValueNode)
            {
                results = parseFunc(keyValueNode.Value);
            }
            else
            {
                throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
            }

            return results;
        }

        public static T[] ParseListObjectKeyValueNode<T>(this IImportNode node, Func<List<IListMember>, T[]> parseFunc)
        {
            T[] results = null;

            if (node is ListObjectKeyValueNode listObjectKeyValueNode)
            {
                results = parseFunc(listObjectKeyValueNode.Values);
            }
            else if (!node.IsUndefined())
            {
                throw new InvalidDataException($"Could not transform the AbstractData object into a ListObjectKeyValue: [{node.GetType()}]");
            }

            return results;
        }
    }
}
