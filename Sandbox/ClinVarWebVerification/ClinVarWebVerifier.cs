using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace ClinVarWebVerification
{
    public static class ClinVarWebVerifier
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ClinVarWebVarifier [clinvar xml] [compressed ref]");
                return;
            }
            int passCount = 0, failCount = 0;
            var compressedSequence = new CompressedSequence();
            var compressedSequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(args[1]), compressedSequence);
            

            const string clinVarWebBase = "http://www.ncbi.nlm.nih.gov/clinvar/";

            var clinVarXmlReader = new ClinVarXmlReader(new FileInfo(args[0]), compressedSequenceReader,
                compressedSequence);
            Console.WriteLine("Acc\tField\tWeb\tXML");

            //the resume point. We will skip entries before this.
            var restartAfter = "RCV000186558";
            int restartCount = 0;
            bool resumeVerificaiton = false;

            foreach (var clinVarItem in clinVarXmlReader)
            {
                var rcv = clinVarItem.ID.Split('.')[0];

                if (!resumeVerificaiton)
                {
                    if (restartAfter == rcv)
                    {
                        resumeVerificaiton = true;
                        Console.WriteLine($"Restarting at {rcv}. Skipped {restartCount} entries.");
                    }
                    restartCount++;
                    continue;
                }

                // .NET Core doesn't support WebClient, we should change to HttpClient
                //using (var client = new WebClient())
                //{
                //    string xmlString;
                //    try
                //    {
                //        xmlString = client.DownloadString(clinVarWebBase + rcv);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e.ToString());
                //        Console.WriteLine($"no of entries passed {passCount}. Failed {failCount}");
                //        Task.Delay(11000);//wait 11 seconds
                //        continue;//we resume from the next item
                //    }


                //    if (!HasSameVersions(clinVarItem.ID, xmlString)) continue;

                //    if (!CheckPubmedIds(xmlString, clinVarItem))
                //        failCount++;


                //    if (!CheckDiseaseDbIds(xmlString, @"<a href=""https://www.ncbi.nlm.nih.gov/medgen/([A-Za-z0-9]+)", clinVarItem.MedGenIDs.First(), rcv, "MedGen"))
                //        failCount++;

                //    if (!CheckDiseaseDbIds(xmlString, @"Orphanet"">(\d+)", clinVarItem.OrphanetIDs.First(), rcv, "Orphanet"))
                //        failCount++;

                //    //if (!CheckDiseaseDbIds(xmlString, @"<a href=""http://www.omim.org/entry/(\d+)", clinVarItem.OmimID))
                //    //{
                //    //	Console.WriteLine("Missing omim ids for :" + rcv);
                //    //	break;
                //    //}

                //    passCount++;
                //}

            }
            Console.WriteLine($"no of entries passed {passCount}. Failed {failCount}");
        }

        private static bool HasSameVersions(string clinVarId, string xmlString)
        {
            //<dt>Accession:</dt><dd>RCV000003254.4
            var match = Regex.Match(xmlString, @"<dt>Accession:</dt><dd>(RCV\d+\.\d+)");

            if (!match.Success) throw new InvalidDataException($"NO accession found for {clinVarId}");

            var webRcv = match.Groups[1].Value;
            return clinVarId == webRcv;
        }

        private static bool CheckDiseaseDbIds(string xmlString, string pattern, string diseaseDbIds, string rcv, string fieldName)
        {
            var match = Regex.Match(xmlString, pattern);

            var idHash = new HashSet<string>();
            while (match.Success)
            {
                idHash.Add(match.Groups[1].Value);
                match = match.NextMatch();
            }

            var dbIds = new HashSet<string>();
            if (diseaseDbIds != null)
                foreach (var id in diseaseDbIds.Split(','))
                    dbIds.Add(id);

            // we are using xor here. if they are not the same, we return false.
            if (dbIds.Count == 0 ^ idHash.Count == 0)
            {
                PrintDiff(rcv, fieldName, idHash, dbIds);
                return false;
            }

            foreach (var id in idHash)
            {
                if (!dbIds.Contains(id))
                {
                    PrintDiff(rcv, fieldName, idHash, dbIds);
                    return false;
                }
            }
            return true;
        }

        private static void PrintDiff(string rcv, string fieldName, HashSet<string> idHash, HashSet<string> dbIds)
        {
            Console.Write(rcv + '\t' + fieldName + '\t');

            if (idHash != null)
                foreach (var x in idHash.OrderBy(x => x))
                {
                    Console.Write(x + ',');
                }
            Console.Write('\t');

            if (dbIds != null)
                foreach (var x in dbIds.OrderBy(x => x))
                {
                    Console.Write(x + ',');
                }
            Console.WriteLine();
        }

        private static bool CheckPubmedIds(string xmlString, ClinVarItem clinVarItem)
        {
            var match = Regex.Match(xmlString, @"<a href=""/pubmed/(\d+)"">");

            var pubmedHash = new HashSet<long>();
            while (match.Success)
            {
                pubmedHash.Add(Convert.ToInt64(match.Groups[1].Value));
                match = match.NextMatch();
            }

            // we are using xor here. if they are not the same, we return false.
            if (clinVarItem.PubmedIds == null ^ pubmedHash.Count == 0)
            {
                PrintDiff(clinVarItem.ID.Split('.')[0], "pubmed", clinVarItem, pubmedHash);
                return false;
            }

            foreach (var pId in pubmedHash)
            {
                if (clinVarItem.PubmedIds == null) return false;
                if (!clinVarItem.PubmedIds.Contains(pId))
                {
                    PrintDiff(clinVarItem.ID.Split('.')[0], "pubmed", clinVarItem, pubmedHash);
                    return false;
                }
            }

            return true;
        }

        private static void PrintDiff(string rcv, string fieldName, ClinVarItem clinVarItem, HashSet<long> pubmedHash)
        {
            Console.Write(rcv + '\t' + fieldName + '\t');

            if (pubmedHash != null)
                foreach (var id in pubmedHash.OrderBy(x => x))
                {
                    Console.Write(id.ToString() + ',');
                }

            Console.Write('\t');
            if (clinVarItem.PubmedIds != null)
                foreach (var id in clinVarItem.PubmedIds.OrderBy(x => x))
                {
                    Console.Write(id.ToString() + ',');
                }
            Console.WriteLine();
        }
    }
}
