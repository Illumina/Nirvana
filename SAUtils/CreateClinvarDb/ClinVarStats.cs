using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace SAUtils.CreateClinvarDb;


public class ClinVarStats
{
    public          int       RcvCount               = 0;
    public          int       VcvCount               = 0;
    public          int       InvalidRefAlleleCount  = 0;
    public readonly KeyCounts RcvPathogenicityCounts = new KeyCounts(ClinVarCommon.ValidPathogenicity);
    public readonly KeyCounts RcvReviewStatusCounts  = new KeyCounts(ClinVarCommon.ReviewStatusStrings.Values);
    public readonly KeyCounts VcvPathogenicityCounts = new KeyCounts(ClinVarCommon.ValidPathogenicity);
    public readonly KeyCounts VcvReviewStatusCounts  = new KeyCounts(ClinVarCommon.ReviewStatusStrings.Values);
    
    public void GetClinvarSaItemsStats(List<IClinVarSaItem> items)
    {
        foreach (IClinVarSaItem item in items)
        {
            if (item.Id.StartsWith("RCV"))
            {
                RcvCount++;
                foreach (string significance in item.Significances)
                {
                    RcvPathogenicityCounts.Increment(significance);
                }

                RcvReviewStatusCounts.Increment(ClinVarCommon.ReviewStatusStrings[item.ReviewStatus]);

            }
            else
            {
                VcvCount++;
                foreach (string significance in item.Significances)
                {
                    VcvPathogenicityCounts.Increment(significance);
                }

                VcvReviewStatusCounts.Increment(ClinVarCommon.ReviewStatusStrings[item.ReviewStatus]);
            }
        }

    }

    public override string ToString()
    {
        var sb = StringBuilderPool.Get();
        var jo = new JsonObject(sb);
        sb.Append(JsonObject.OpenBrace);

        jo.AddIntValue("rcvCount", RcvCount);
        jo.AddObjectValue("rcvPathogenicity", RcvPathogenicityCounts);
        jo.AddObjectValue("rcvReviewStatus",  RcvReviewStatusCounts);
        
        jo.AddIntValue("vcvCount", VcvCount);
        jo.AddObjectValue("vcvPathogenicity", VcvPathogenicityCounts);
        jo.AddObjectValue("vcvReviewStatus",  VcvReviewStatusCounts);
        sb.Append(JsonObject.CloseBrace);

        return StringBuilderPool.GetStringAndReturn(sb);

    }
}