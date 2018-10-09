using System;
using System.IO;
using System.Text;
using System.Threading;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;

namespace OrchestrationLambda
{
    public static class AnnotationJob
    {
        public static MemoryStream Invoke(string functionArn, MemoryStream payLoad, CancellationToken cancellationToken)
        {

            var config = new AmazonLambdaConfig
            {
                Timeout = new TimeSpan(0, 5, 0),
                RegionEndpoint = RegionEndpoint.USEast1
            };
            var lambdaClient = new AmazonLambdaClient(config);

            return GetLambdaResponse(functionArn, payLoad, lambdaClient, cancellationToken).Payload;
        }

        private static InvokeResponse GetLambdaResponse(string functionArn, MemoryStream functionInput, IAmazonLambda lambdaClient, CancellationToken cancellationToken)
        {

            var invokeRequest = new InvokeRequest
            {
                FunctionName = functionArn,
                PayloadStream = functionInput,
                InvocationType = "RequestResponse",
            };

            try
            {
                return lambdaClient.InvokeAsync(invokeRequest, cancellationToken).Result;
            }

            catch (Exception e)
            {
                Console.WriteLine($"Failed job when invoking the annotation job: {Encoding.ASCII.GetString(functionInput.ToArray())}. \nException thrown: {e.Message}, {e.Data}, {e.StackTrace}");
                throw;
            }
        }
    }
}