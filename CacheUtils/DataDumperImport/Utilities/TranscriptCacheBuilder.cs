using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;

namespace CacheUtils.DataDumperImport.Utilities
{
    public sealed class TranscriptCacheBuilder
    {
        #region members

        private readonly IFileHeader _originalHeader;
        private readonly List<TranscriptPacket> _packets;

        private Gene[] _genes;
        private SimpleInterval[] _introns;
        private SimpleInterval[] _mirnas;
        private string[] _peptideSeqs;
        private Transcript[] _transcripts;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public TranscriptCacheBuilder(IFileHeader header, List<TranscriptPacket> packets)
        {
            _originalHeader = header;
            _packets        = packets;
        }

        public GlobalCache Create()
        {
            ExtractObjects();

            var header = new FileHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, _originalHeader.TranscriptSource, DateTime.Now.Ticks,
                _originalHeader.GenomeAssembly, _originalHeader.Custom);

            return new GlobalCache(header, _transcripts, null, _genes, _introns, _mirnas, _peptideSeqs);
        }

        private void ExtractObjects()
        {
            var geneSet    = new HashSet<Gene>();
            var intronSet  = new HashSet<SimpleInterval>();
            var mirnaSet   = new HashSet<SimpleInterval>();
            var peptideSet = new HashSet<string>();

            _transcripts = new Transcript[_packets.Count];

            for (int i = 0; i < _packets.Count; i++)
            {
                var packet = _packets[i];
                _transcripts[i] = packet.Transcript;

                geneSet.Add(packet.Transcript.Gene);
                if (packet.Transcript.Translation?.PeptideSeq != null) peptideSet.Add(packet.Transcript.Translation.PeptideSeq);
                if (packet.Transcript.Introns != null) foreach (var intron in packet.Transcript.Introns) intronSet.Add(intron);
                if (packet.Transcript.MicroRnas != null) foreach (var mirna in packet.Transcript.MicroRnas) mirnaSet.Add(mirna);
            }

            _genes       = geneSet.Count    > 0 ? geneSet.ToArray()    : null;
            _introns     = intronSet.Count  > 0 ? intronSet.ToArray()  : null;
            _mirnas      = mirnaSet.Count   > 0 ? mirnaSet.ToArray()   : null;
            _peptideSeqs = peptideSet.Count > 0 ? peptideSet.ToArray() : null;
        }
    }
}
