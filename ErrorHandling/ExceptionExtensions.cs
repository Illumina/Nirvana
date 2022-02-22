using System;

namespace ErrorHandling;

public static class ExceptionExtensions
{
    public const string UserError = "UserError";
        
    public static Exception MakeUserError(this Exception e)
    {
        // ReSharper disable once HeapView.BoxingAllocation
        e.Data[UserError] = true;
        return e;
    }
}