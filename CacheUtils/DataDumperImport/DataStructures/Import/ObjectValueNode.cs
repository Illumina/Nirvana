using System.Collections.Generic;

namespace CacheUtils.DataDumperImport.DataStructures.Import
{
    public sealed class ObjectValueNode : IListMember
    {
        public string Type { get; }
        public string Key { get; }
        public List<IImportNode> Values { get; }

        internal ObjectValueNode(string type, List<IImportNode> values)
        {
            Key    = null;
            Type   = type;
            Values = values;
        }
    }
}
