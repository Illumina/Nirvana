using System;
using IO;

namespace Cloud
{
    public static class SnsUtilities
    {
        public static void SendSnsMessage(string snsTopicArn, string snsMessage)
        {
            try
            {
                var snsClient = new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient();
                snsClient.PublishAsync(snsTopicArn, snsMessage).Wait();
            }
            catch (Exception e)
            {
                Logger.LogLine("Unable to log to SNS!!");
                Logger.LogLine(e.Message);
            }
        }
    }
}