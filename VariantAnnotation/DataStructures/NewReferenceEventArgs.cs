using System;

namespace VariantAnnotation.DataStructures
{
    public sealed class NewReferenceEventArgs : EventArgs
    {
        public readonly ReferenceNameData Data;

        /// <summary>
        /// constructor
        /// </summary>
        public NewReferenceEventArgs(ReferenceNameData data)
        {
            Data = data;
        }
    }

    public sealed class ReferenceNameData
    {
        public ushort ReferenceIndex;
        public string UcscReferenceName;
        public string EnsemblReferenceName;
    }
}
