using System;

namespace ErrorHandling.Exceptions
{
    public sealed class DeploymentErrorException : Exception
    {
        public DeploymentErrorException(string message) : base(message) { }
    }
}