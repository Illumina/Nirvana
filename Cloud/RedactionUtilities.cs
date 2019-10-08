using System.Text.RegularExpressions;

namespace Cloud
{
    public static class RedactionUtilities
    {
        private static readonly Regex AwsAccessKeyIdRegex = new Regex("AWSAccessKeyId=([^&]+)");
        private static readonly Regex AmzCredentialRegex  = new Regex("X-Amz-Credential=([^/]+)");
        private static readonly Regex AccessKeyRegex      = new Regex("\"accessKey\":\"([^\"]+)");
        private static readonly Regex SecretKeyRegex      = new Regex("\"secretKey\":\"([^\"]+)");
        private static readonly Regex SessionTokenRegex   = new Regex("\"sessionToken\":\"([^\"]+)");

        public static string Redact(this string s)
        {
            var awsAccessKeyIdMatches = AwsAccessKeyIdRegex.Matches(s);
            var amzCredentialMatches  = AmzCredentialRegex.Matches(s);
            var accessKeyMatches      = AccessKeyRegex.Matches(s);
            var secretKeyMatches      = SecretKeyRegex.Matches(s);
            var sessionTokenMatches   = SessionTokenRegex.Matches(s);

            char[] charArray = s.ToCharArray();

            charArray.Mask(awsAccessKeyIdMatches).Mask(amzCredentialMatches).Mask(accessKeyMatches)
                .Mask(secretKeyMatches).Mask(sessionTokenMatches);

            return new string(charArray);
        }

        private static char[] Mask(this char[] charArray, MatchCollection matches)
        {
            foreach (Match match in matches)
            {
                var group = match.Groups[1];
                for (var i = 0; i < group.Length; i++)
                {
                    charArray[group.Index + i] = 'X';
                }
            }

            return charArray;
        }
    }
}
