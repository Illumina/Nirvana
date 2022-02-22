using System;

namespace Cloud
{
    public static class NirvanaHelper
    {
        private const string UrlBaseEnvironmentVariableName       = "NirvanaDataUrlBase";

        public static readonly string S3Url                       = GetDataUrlBase();

        private static string GetDataUrlBase()
        {
            var urlBase = Environment.GetEnvironmentVariable(UrlBaseEnvironmentVariableName);
            if (string.IsNullOrEmpty(urlBase))
                throw new Exception($"{UrlBaseEnvironmentVariableName} has not been defined as an environment variable.");

            return urlBase;
        }
    }
}