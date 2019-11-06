using System.Data;
using System.IO;
using System.Text;
using Cloud;
using Cloud.Utilities;
using Newtonsoft.Json;

namespace GeneAnnotationLambda
{
    public static class LambdaResponse
    {
        private const string OutputBeforeNirvanaJson = ",\"annotation\":";
        private const string OutputEnd = "}";

        public static Stream Create(string id, string status, string nirvanaJson)
        {
            string statusJson = JsonConvert.SerializeObject(status);
            string outputStart = $"{{\"id\":\"{id}\",\"status\":{statusJson}";
            string output;

            if (status == LambdaUrlHelper.SuccessMessage)
            {
                if (nirvanaJson == null)
                    throw new NoNullAllowedException("Nirvana annotation cannot be null when the job is successful.");
                output = outputStart + OutputBeforeNirvanaJson + nirvanaJson + OutputEnd;
            }
            else
            {
                output = outputStart + OutputEnd;
            }

            LogUtilities.LogObject("Result", output);

            var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(output));
            return outputStream;
        }
    }
}