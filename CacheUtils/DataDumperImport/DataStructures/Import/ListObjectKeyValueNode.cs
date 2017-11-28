using System.Collections.Generic;

namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public sealed class ListObjectKeyValueNode : IImportNode
    {
        public string Key { get; }
        public List<IListMember> Values { get; } = new List<IListMember>();

        public ListObjectKeyValueNode(string key) => Key = key;
        public void Add(IListMember node) => Values.Add(node);
    }
}
