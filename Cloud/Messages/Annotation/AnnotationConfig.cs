﻿using System.Collections.Generic;

namespace Cloud.Messages.Annotation
{
    public sealed class AnnotationConfig
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable NotAccessedField.Global
        public string          id;
        public string          genomeAssembly;
        public string          vcfUrl;
        public string          tabixUrl;
        public S3Path          outputDir;
        public string          outputPrefix;
        public List<SaUrls>    customAnnotations;
        public List<string>    desiredVcfInfo;
        public List<string>    desiredVcfSampleInfo;
        public string          customStrUrl;
        public AnnotationRange annotationRange;
        // ReSharper restore NotAccessedField.Global
        // ReSharper restore InconsistentNaming
    }
}