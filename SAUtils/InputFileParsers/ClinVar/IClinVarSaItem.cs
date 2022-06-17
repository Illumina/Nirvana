using System;
using System.Collections.Generic;
using VariantAnnotation.Interface.SA;

namespace SAUtils.InputFileParsers.ClinVar;

public interface IClinVarSaItem: ISupplementaryDataItem, IComparable<IClinVarSaItem>
{
    string Id { get; }
    IEnumerable<string>        Significances { get; }
    ClinVarCommon.ReviewStatus ReviewStatus  { get; }
}