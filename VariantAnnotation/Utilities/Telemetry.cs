using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace VariantAnnotation.Utilities
{
    public static class Telemetry
    {
        private const string FormUrl = "https://docs.google.com/forms/d/1jqEEUQ_0NculatJ9i4vjqoovJuPXI1m2fWtctvsDW2Q/viewform";
        private const string PostUrl = "https://docs.google.com/forms/d/1jqEEUQ_0NculatJ9i4vjqoovJuPXI1m2fWtctvsDW2Q/formResponse";

        public static List<KeyValuePair<string, string>> PackTelemetry(string inputVcfPath, int numVariants, int numRefPositions,
            int numCustomAnnotationDirs, int numCustomIntervalDirs, bool enableRefNoCall, bool enableGvcf,
            bool enableVcf, string nirvanaVersion, ushort cacheVersion, ushort saVersion, ushort refVersion,
            string wallTime, double peakMemoryUsageGB, int exitCode)
        {
            // package the responses
            var refNoCall        = enableRefNoCall ? "T" : "F";
            var enableVcfOutput  = enableVcf ? "T" : "F";
            var enableGvcfOutput = enableGvcf ? "T" : "F";
            var unixMinuteLoad   = "0.00";

            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("entry.236195033", "default"),
                new KeyValuePair<string, string>("entry.860126325", Environment.MachineName),
                new KeyValuePair<string, string>("entry.1601006912", inputVcfPath),
                new KeyValuePair<string, string>("entry.1549117526", numVariants.ToString()),
                new KeyValuePair<string, string>("entry.1646669399", numRefPositions.ToString()),
                new KeyValuePair<string, string>("entry.2077372634", numCustomAnnotationDirs.ToString()),
                new KeyValuePair<string, string>("entry.1171739189", numCustomIntervalDirs.ToString()),
                new KeyValuePair<string, string>("entry.1033207411", refNoCall),
                new KeyValuePair<string, string>("entry.808984550", enableVcfOutput),
                new KeyValuePair<string, string>("entry.1675142442", enableGvcfOutput),
                new KeyValuePair<string, string>("entry.445784701", nirvanaVersion),
                new KeyValuePair<string, string>("entry.714981864", cacheVersion.ToString()),
                new KeyValuePair<string, string>("entry.1225579168", saVersion.ToString()),
                new KeyValuePair<string, string>("entry.627887172", refVersion.ToString()),
                new KeyValuePair<string, string>("entry.2072581361", wallTime),
                new KeyValuePair<string, string>("entry.483245201", peakMemoryUsageGB.ToString("0.###")),
                new KeyValuePair<string, string>("entry.1827959306", exitCode.ToString()),
                new KeyValuePair<string, string>("entry.906486769", unixMinuteLoad),
                new KeyValuePair<string, string>("pageHistory", "0"),
                new KeyValuePair<string, string>("fromEmail", "false")
            };
        }

        /// <summary>
        /// updates our Google Form with telemetry info
        /// </summary>
        public static void Update(List<KeyValuePair<string, string>> telemetry)
        {
            try
            {
                // get the response ID
                var responseId = GetResponseId();

                // add responseId-dependent items to the telemetry
                telemetry.Add(new KeyValuePair<string, string>("draftResponse", $"[,,\"{responseId}\"]"));
                telemetry.Add(new KeyValuePair<string, string>("fbzx", responseId));

                // send the results
                UploadResults(responseId, telemetry);
            }
            catch (Exception)
            {
                // we're going to eat the exception instead of retrying
            }

        }

        /// <summary>
        /// uploads our results to the Google Form
        /// </summary>
        private static void UploadResults(string responseId, IEnumerable<KeyValuePair<string, string>> telemetry)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Referer", $"{FormUrl}?fbzx={responseId}");
                var content = new FormUrlEncodedContent(telemetry);
                client.PostAsync(PostUrl, content);
            }
        }

        /// <summary>
        /// extracts the response ID from the Google Form
        /// </summary>
        private static string GetResponseId()
        {
            string html;

            using (var client = new HttpClient())
            {
                html = client.GetStringAsync(FormUrl).Result;
            }

            var regex = new Regex("<input type=\"hidden\" name=\"fbzx\" value=\"([^\"]+)\">", RegexOptions.Compiled);
            var match = regex.Match(html);

            return !match.Success ? null : match.Groups[1].Value;
        }

        /// <summary>
        /// returns the wall time string given a timespan
        /// </summary>
        public static string GetWallTime(TimeSpan span)
        {
            int totalHours = (int)span.TotalHours;
            return $"{totalHours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }
    }
}
