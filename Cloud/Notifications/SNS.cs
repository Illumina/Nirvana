using System;
using IO;

namespace Cloud.Notifications
{
    public static class SNS
    {
        public static void SendMessage(string snsTopicArn, string snsMessage)
        {
            try
            {
                using (var snsClient = new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient())
                {
                    snsClient.PublishAsync(snsTopicArn, snsMessage).Wait();
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Unable to log to SNS!!");
                Logger.WriteLine(e.Message);
            }
        }

        public static string CreateMessage(string message, string status, string stackTrace) => $"{message}\n{status}\nStackTrace: {stackTrace}";
    }
}