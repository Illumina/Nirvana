using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Illumina.VariantAnnotation.DataStructures;

namespace CdnaEndPointInvestigation
{
    public class CsqComparer
    {
        #region members

        private readonly List<ColorText> _colorTextList;
        private readonly StringBuilder _sb;
        private readonly ConsoleColor _defaultConsoleColor;

        private readonly VepVariantFeature _variant;
        private readonly List<CsqEntry> _csqTruth;
        private readonly List<CsqEntry> _csq;
        
        #endregion

        // constructor
        public CsqComparer(VepVariantFeature variant)
        {
            _colorTextList = new List<ColorText>();
            _sb            = new StringBuilder();
            _csqTruth      = new List<CsqEntry>();
            _csq           = variant.Csq;
            _variant       = variant;

            Console.ResetColor();
            _defaultConsoleColor = Console.ForegroundColor;
        }

        public List<CsqEntry> GetCsqTruth()
        {
            return _csqTruth;
        }

        /// <summary>
        /// compares the new CSQ tags to the ones that were there before
        /// </summary>
        public bool AreCsqTagsDifferent(bool stopOnDifference, bool silentOutput, ref int numTranscriptsProcessed, ref int numTranscriptsDifferent)
        {
            _sb.Clear();

            if (!silentOutput)
            {
                _sb.AppendFormat("CSQ Truth count: {0}, CSQ count: {1}\n", _csqTruth.Count, _csq.Count);
            }

            const int maxFieldLength = 100;
            _colorTextList.Clear();
            _colorTextList.Add(new ColorText(_defaultConsoleColor, _variant.ToString()));

            // get the longest field name
            FieldInfo[] csqFields = typeof(CsqEntry).GetFields();
            int longestFieldName  = csqFields.Select(f => f.Name.Length).Concat(new[] { 0 }).Max() + 1;

            // associate the fields with each other
            var uniqueFeatures  = new HashSet<int>();
            var csqEntries      = new Dictionary<int, CsqEntry>();
            var csqTruthEntries = new Dictionary<int, CsqEntry>();

            foreach (var csq in _csq)
            {
                int hashCode = csq.GetHashCode();
                uniqueFeatures.Add(hashCode);
                csqEntries[hashCode] = csq;
            }

            foreach (var csq in _csqTruth)
            {
                int hashCode = csq.GetHashCode();
                uniqueFeatures.Add(hashCode);
                csqTruthEntries[hashCode] = csq;
            }

            var sortedFeatures = uniqueFeatures.OrderBy(x => x).ToArray();

            // ===========================
            // get the longest field value
            // ===========================

            int longestFieldValue = 0;

            foreach (var currentFeature in sortedFeatures)
            {
                CsqEntry currentCsq;
                CsqEntry currentCsqTruth;

                bool hasCsq      = csqEntries.TryGetValue(currentFeature, out currentCsq);
                bool hasCsqTruth = csqTruthEntries.TryGetValue(currentFeature, out currentCsqTruth);

                foreach (var f in csqFields)
                {
                    string csqValue;
                    string csqTruthValue;

                    if (hasCsq) csqValue = (string)f.GetValue(currentCsq);
                    else csqValue = null;

                    if (hasCsqTruth) csqTruthValue = (string)f.GetValue(currentCsqTruth);
                    else csqTruthValue = null;

                    if (csqValue == null) csqValue = string.Empty;
                    if (csqTruthValue == null) csqTruthValue = string.Empty;

                    if (csqValue.Length > longestFieldValue) longestFieldValue = csqValue.Length;
                    if (csqTruthValue.Length > longestFieldValue) longestFieldValue = csqTruthValue.Length;
                }
            }

            if (longestFieldValue > maxFieldLength) longestFieldValue = maxFieldLength;

            string stringFormat = string.Format(@"   {{0,-{0}}} {{1,-{1}}} ",
                longestFieldName, longestFieldValue);

            // ============================
            // dump the data to the display
            // ============================

            // Console.WriteLine("string format: [{0}]", stringFormat);

            // dump the header
            if (!silentOutput)
            {
                string header = string.Format(stringFormat + "{2}", "Attribute", "CSQ Truth Value", "CSQ Value");
                _sb.AppendLine();
                _sb.AppendLine(header);
                _sb.AppendLine(new string('=', header.Length));
            }

            // dump the key/value pairs
            bool hasMoreEntriesThanTruth = _csq.Count > _csqTruth.Count;
            bool foundError = hasMoreEntriesThanTruth;

            for (int csqIndex = 0; csqIndex < sortedFeatures.Length; csqIndex++)
            {
                if (!silentOutput)
                {
                    _colorTextList.Add(new ColorText(_defaultConsoleColor, _sb.ToString()));
                    _sb.Clear();

                    _colorTextList.Add(new ColorText(ConsoleColor.Yellow, "Entry " + (csqIndex + 1) + ":"));
                }

                var currentFeature = sortedFeatures[csqIndex];

                CsqEntry currentCsq;
                CsqEntry currentCsqTruth;

                bool hasCsq = csqEntries.TryGetValue(currentFeature, out currentCsq);
                bool hasCsqTruth = csqTruthEntries.TryGetValue(currentFeature, out currentCsqTruth);

                if (currentCsqTruth != null)
                {
                    // DEBUG: skip regulatory features for now
                    if (currentCsqTruth.FeatureType == "RegulatoryFeature")
                    {
                        if (!silentOutput) _sb.AppendLine("*** skipping (regulatory) ***");
                        continue;
                    }

                    // DEBUG: skip domains for now
                    if (currentCsqTruth.FeatureType == "Domains")
                    {
                        if (!silentOutput) _sb.AppendLine("*** skipping (domains) ***");
                        continue;
                    }

                    // DEBUG: skip motif features for now
                    if (currentCsqTruth.FeatureType == "MotifFeature")
                    {
                        if (!silentOutput) _sb.AppendLine("*** skipping (motif feature) ***");
                        continue;
                    }

                    // DEBUG: skip buggy deletions
                    // ENSP00000311997.6:p.Ala744_Lys749delinsdel
                    // ENSP00000311997.6:p.Ala744_Lys749del
                    if (currentCsqTruth.HgvsProteinSequenceName.Contains("delinsdel"))
                    {
                        if (!silentOutput) _sb.AppendLine("*** skipping (buggy deletions) ***");
                        continue;
                    }

                    // DEBUG: skip buggy deletions
                    // ENSP00000401470.2:p.Glu395_Glu396insGluGlu
                    // ENSP00000401470.2:p.Glu394_Glu395dup
                    if (hasCsq && currentCsqTruth.HgvsProteinSequenceName.Contains("insGluGlu") &&
                        currentCsq.HgvsProteinSequenceName.Contains("dup"))
                    {
                        if (!silentOutput) _sb.AppendLine("*** skipping (buggy duplications) ***");
                        continue;
                    }

                    if (hasCsq && currentCsqTruth.HgvsCodingSequenceName.Contains("del") &&
                        currentCsqTruth.HgvsCodingSequenceName.Contains("ins"))
                    {
                        // DEBUG: skip buggy multiples
                        // ENSESTT00000051688.1:c.7-8707_7-8704delNNNN
                        // ENSESTT00000051688.1:c.7-8707_7-8704[3]...
                        if (Regex.IsMatch(currentCsq.HgvsCodingSequenceName, "\\[\\d+\\]"))
                        {
                            if (!silentOutput) _sb.AppendLine("*** skipping (buggy multiples) ***");
                            continue;
                        }

                        // DEBUG: skip buggy duplications
                        // ENSESTT00000023500.1:c.93+11204delNinsTT
                        // ENSESTT00000023500.1:c.93+11204dupN
                        if (currentCsq.HgvsCodingSequenceName.Contains("dup"))
                        {
                            if (!silentOutput) _sb.AppendLine("*** skipping (buggy duplicates) ***");
                            continue;
                        }
                    }
                }

                foreach (var f in csqFields)
                {
                    string csqValue;
                    string csqTruthValue;

                    if (hasCsq) csqValue = (string)f.GetValue(currentCsq);
                    else csqValue = null;

                    if (hasCsqTruth) csqTruthValue = (string)f.GetValue(currentCsqTruth);
                    else csqTruthValue = null;

                    if (csqValue == null) csqValue = string.Empty;
                    if (csqTruthValue == null) csqTruthValue = string.Empty;

                    if (csqValue.Length > maxFieldLength)
                    {
                        csqValue = string.Format("{0}...", csqValue.Substring(0, maxFieldLength - 3));
                    }

                    if (csqTruthValue.Length > maxFieldLength)
                    {
                        csqTruthValue = string.Format("{0}...", csqTruthValue.Substring(0, maxFieldLength - 3));
                    }

                    bool printKey = !string.IsNullOrEmpty(csqValue) || !string.IsNullOrEmpty(csqTruthValue);

                    bool foundDifference = csqValue != csqTruthValue;
                    
                    bool isWarning = false;

                    if (foundDifference)
                    {
                        // DEBUG: ignore Sift, PolyPhen, and Domains for now
                        if ((f.Name != "Sift") && (f.Name != "PolyPhen") && (f.Name != "Domains"))
                        {
                            numTranscriptsDifferent++;
                            foundError = true;
                        }
                        else
                        {
                            isWarning = true;
                        }
                    }

                    numTranscriptsProcessed++;

                    if (!silentOutput && printKey)
                    {
                        if (_sb.Length > 0) _colorTextList.Add(new ColorText(_defaultConsoleColor, _sb.ToString()));
                        _sb.Clear();

                        var newColor = _defaultConsoleColor;
                        if (foundDifference)
                        {
                            newColor = isWarning ? ConsoleColor.Yellow : ConsoleColor.Red;
                        }

                        _sb.AppendFormat(stringFormat, f.Name, csqTruthValue);
                        _sb.Append(csqValue);

                        _colorTextList.Add(new ColorText(newColor, _sb.ToString()));
                        _sb.Clear();
                    }
                }
            }
            ColorText.DisplayList(_colorTextList);
            
            if (foundError)
            {
                ColorText.DisplayList(_colorTextList);
            }
            
            // stop if we found an error
            if (stopOnDifference && foundError)
            {
                // ColorText.DisplayList(_colorTextList);
                Console.WriteLine("Found error.");
                Environment.Exit(1);
            }

            return foundError;
        }

        /// <summary>
        /// extracts the truth data from the info field
        /// </summary>
        public void ExtractTruthData(string infoField)
        {
            CsqCommon.GetCsqEntries(infoField, _csqTruth);
        }

        private class ColorText
        {
            private readonly ConsoleColor _color;
            private readonly string _text;

            // constructor
            public ColorText(ConsoleColor color, string text)
            {
                _color = color;
                _text = text;
            }

            public static void DisplayList(IEnumerable<ColorText> colorTextList)
            {
                Console.ResetColor();
                var currentColor = Console.ForegroundColor;

                foreach (var colorText in colorTextList)
                {
                    if (currentColor != colorText._color)
                    {
                        currentColor = colorText._color;
                        Console.ForegroundColor = currentColor;
                    }

                    Console.WriteLine(colorText._text);
                }

                Console.ResetColor();
            }
        }
    }
}
