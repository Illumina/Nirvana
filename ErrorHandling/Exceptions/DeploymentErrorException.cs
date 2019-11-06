using System;

namespace ErrorHandling.Exceptions
{
    public class DeploymentErrorException : Exception
    {
        public DeploymentErrorException(string message) : base(message) { }
    }
}