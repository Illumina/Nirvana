using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface ITranscript
    {
        string AminoAcids { get; }     
        string CdsPosition { get; }    
        string Codons { get; }         
        string ComplementaryDnaPosition { get; }
        IEnumerable<string> Consequence { get; }
        string Exons { get; } 
        string Introns { get; }
        string Gene { get; }  
        string Hgnc { get; }  
        string HgvsCodingSequenceName { get; } 
        string HgvsProteinSequenceName { get; }
        string IsCanonical { get; }  
        string PolyPhenPrediction { get; }  
        string PolyPhenScore { get; }  
        string ProteinID { get; }   
        string ProteinPosition { get; } 
        string SiftPrediction { get; }  
        string SiftScore { get; }   
        string TranscriptID { get; }
    }
}