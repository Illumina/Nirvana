using System;
using System.IO;
using Cloud.Notifications;
using Cloud.Utilities;
using ErrorHandling;
using IO;

namespace SingleAnnotationLambda
{
    public static class ExceptionHandler
    {
        public static Stream GetStream(string id, string snsTopicArn, Exception e)
        {
            Logger.LogLine(e.Message);
            Logger.LogLine(e.StackTrace);
            GC.Collect();

            string snsMessage = SNS.CreateMessage(e.Message, "exception", e.StackTrace);
            SNS.SendMessage(snsTopicArn, snsMessage);

            ErrorCategory errorCategory = ExceptionUtilities.ExceptionToErrorCategory(e);
            string message = GetMessage(errorCategory, e.Message);

            LogUtilities.LogObject("Result", message);

            return SingleResult.Create(id, message, null);
        }

        private static string GetMessage(ErrorCategory errorCategory, string exceptionMessage)
        {
            if (errorCategory == ErrorCategory.UserError) return "User error: " + FirstCharToLower(exceptionMessage);
            return "Nirvana error: an unexpected annotation error occurred while annotating this variant.";
        }

        private static string FirstCharToLower(string input) => string.IsNullOrEmpty(input) || char.IsLower(input[0])
            ? input
            : char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}
