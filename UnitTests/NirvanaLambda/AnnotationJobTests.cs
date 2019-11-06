using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Model;
using ErrorHandling;
using NirvanaLambda;
using Xunit;

namespace UnitTests.NirvanaLambda
{
    public sealed class AnnotationJobTests
    {
        [Fact]
        public void GetResultSummaryFromSuccessInvocation_AsExpected()
        {
            const string annotationResult = "{\"id\":\"Test\",\"status\":\"Success\",\"filePath\":\"result/input_00001.json.gz\"}";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(annotationResult));

            var processed = AnnotationJob.GetResultSummaryFromSuccessInvocation(memoryStream);

            Assert.Equal("input_00001.json.gz", processed.FileName);
            Assert.Null(processed.ErrorMessage);
            Assert.Null(processed.ErrorCategory);
        }

        [Fact]
        public void GetResultSummaryFromSuccessInvocation_PassFailedStatus_FromAnnotationJob()
        {
            const string annotationResult = "{\"id\":\"Test\",\"status\":\"Something Wrong!\",\"filePath\":\"\",\"ErrorCategory\":\"NirvanaError\"}";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(annotationResult));

            var processed = AnnotationJob.GetResultSummaryFromSuccessInvocation(memoryStream);

            Assert.Equal("", processed.FileName);
            Assert.Equal("Something Wrong!", processed.ErrorMessage);
            Assert.Equal(ErrorCategory.NirvanaError, processed.ErrorCategory);
        }

        [Fact]
        public void CheckResponse_AsExpected()
        {
            Assert.Throws<Exception>(() => new AnnotationJob(null, 1).CheckResponse(new InvokeResponse(){FunctionError = "Unhandled"}));
            Assert.Throws<Exception>(() => new AnnotationJob(null, 1).CheckResponse(null));
        }

        [Fact]
        public void GetResultSummaryFromFailedInvocation_AsExpected()
        {
            var job = new AnnotationJob(null, 1);
            var generalExpection = new Exception("first level exception", new Exception("second level exception", new Exception("third level exception")));
            var taskCanceledExpection = new Exception("first level exception", new TaskCanceledException("second level exception", new Exception("third level exception")));

            var generalResult = job.GetResultSummaryFromFailedInvocation(generalExpection);
            var taskCanceledResult = job.GetResultSummaryFromFailedInvocation(taskCanceledExpection);

            Assert.Equal(ErrorCategory.NirvanaError, generalResult.ErrorCategory);
            Assert.Equal("Failed job when invoking the annotation job: third level exception.", generalResult.ErrorMessage);

            Assert.Equal(ErrorCategory.TimeOutError, taskCanceledResult.ErrorCategory);
            Assert.Equal("Failed job when invoking the annotation job: third level exception. Annotation job was not finished in 0 milliseconds.", taskCanceledResult.ErrorMessage);
        }
    }
}