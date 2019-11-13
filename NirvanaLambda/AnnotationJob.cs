using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Cloud.Messages.Annotation;
using Cloud.Utilities;
using ErrorHandling;
using IO;

namespace NirvanaLambda
{
    public sealed class AnnotationJob
    {
        private const int MinAnnotationTime          = 5_000;
        private const int ReservedPostAnnotationTime = 10_000;
        private const int WaitBeforeRetry            = 2_000;
        private const string UnknownErrorMessage     = "Unknown error -1";

        private int _numRetries;
        private double _annotationTimeOut;
        private readonly ILambdaContext _lambdaContext;
        private readonly int _jobIndex;
        private ErrorCategory? _errorCategory;

        public AnnotationJob(ILambdaContext context, int jobIndex)
        {
            _lambdaContext = context;
            _jobIndex      = jobIndex;
        }

        public AnnotationResultSummary Invoke(string functionArn, string functionInput)
        {
            try
            {
                return InvokeAndRetryWhenThrottled(functionArn, functionInput).Result;
            }
            catch (Exception e)
            {
                Logger.Log(e);
                return GetResultSummaryFromFailedInvocation(e);
            }
        }

        private async Task<AnnotationResultSummary> InvokeAndRetryWhenThrottled(string functionArn, string functionInput)
        {
            AnnotationResultSummary resultSummary;

            while (true)
            {
                try
                {
                    var invokeRequest = new InvokeRequest
                    {
                        FunctionName   = functionArn,
                        Payload        = functionInput,
                        InvocationType = "RequestResponse"
                    };

                    var payload   = GetAnnotationResult(invokeRequest);
                    resultSummary = GetResultSummaryFromSuccessInvocation(payload);
                    break;
                }
                catch (Exception e) when (ExceptionUtilities.HasException<TooManyRequestsException>(e))
                {
                    Logger.LogLine($"Job {_jobIndex}: Invocation is throttled. Retry in {WaitBeforeRetry} ms.");
                    _numRetries++;
                    await Task.Delay(WaitBeforeRetry);
                }
                catch (Exception e) when (e.HasErrorMessage(UnknownErrorMessage))
                {
                    Logger.LogLine($"Job {_jobIndex}: {UnknownErrorMessage}. Retry in {WaitBeforeRetry} ms.");
                    _numRetries++;
                    await Task.Delay(WaitBeforeRetry);
                }
            }

            return resultSummary;
        }

        internal static AnnotationResultSummary GetResultSummaryFromSuccessInvocation(MemoryStream payload)
        {
            var annotationResult = JsonUtilities.Deserialize<AnnotationResult>(payload);
            string errorMessage  = annotationResult.errorCategory == null ? null : annotationResult.status;
            return AnnotationResultSummary.Create(annotationResult, annotationResult.errorCategory, errorMessage);
        }

        private MemoryStream GetAnnotationResult(InvokeRequest invokeRequest)
        {
            CheckRemainingTime();

            var config = new AmazonLambdaConfig
            {
                Timeout = TimeSpan.FromMilliseconds(_annotationTimeOut)
            };

            InvokeResponse response;
            using (var lambdaClient = new AmazonLambdaClient(config))
            {
                response = lambdaClient.InvokeAsync(invokeRequest).Result;
            }

            CheckResponse(response);
            return response.Payload;
        }

        private void CheckRemainingTime()
        {
            double currentRemainingTime = _lambdaContext.RemainingTime.TotalMilliseconds;

            if (currentRemainingTime < MinAnnotationTime + ReservedPostAnnotationTime)
            {
                if (_numRetries > 0)
                {
                    _errorCategory = ErrorCategory.InvocationThrottledError;
                    throw new Exception($"Invocation is still throttled after {_numRetries} retries.");
                }

                _errorCategory = ErrorCategory.TimeOutError;
                throw new Exception($"Only {currentRemainingTime} ms left. No enough time for annotation job.");
            }

            _annotationTimeOut = currentRemainingTime - ReservedPostAnnotationTime;
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        internal void CheckResponse(InvokeResponse response)
        {
            if (response == null)
            {
                _errorCategory = ErrorCategory.NirvanaError;
                throw new Exception("Failed to get the response from annotation job");
            }

            if (response.FunctionError == "Unhandled")
            {
                _errorCategory = ErrorCategory.NirvanaError;
                throw new Exception("There is unhandled error in annotation job. A possible reason for this is the out-of-memory issue.");
            }
        }

        internal AnnotationResultSummary GetResultSummaryFromFailedInvocation(Exception e)
        {
            var additionalDescription = "";
            if (ExceptionUtilities.HasException<TaskCanceledException>(e))
            {
                _errorCategory = ErrorCategory.TimeOutError;
                additionalDescription = $" Annotation job was not finished in {_annotationTimeOut} milliseconds.";
            }

            if (_errorCategory == null) _errorCategory = ExceptionUtilities.ExceptionToErrorCategory(e);

            e = ExceptionUtilities.GetInnermostException(e);
            string errorMessage = $"Failed job when invoking the annotation job: {e.Message}.{additionalDescription}";

            return AnnotationResultSummary.Create(null, _errorCategory, errorMessage); 
        }
    }
}