namespace ErrorHandling
{
    /// <summary>
    /// Common Windows Error Codes: https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx
    /// C:\Program Files (x86)\Windows Kits\8.1\Include\shared\winerror.h
    /// </summary>
    public enum ExitCodes
    {
        // ================
        // Windows-specific
        // ================

        Success            = 0,
        InvalidFunction    = 1,
        FileNotFound       = 2,
        PathNotFound       = 3,
        AccessDenied       = 5,
        BadFormat          = 11,
        InvalidData        = 13,
        OutofMemory        = 14,
        SharingViolation   = 32,
        CallNotImplemented = 120,
        BadArguments       = 160,

        // =================
        // Illumina-specific
        // =================

        // command-line (200 - 209)
        UnknownCommandLineOption = 200,
        MissingCommandLineOption = 201,

        // general (210 - 219)
        UserError = 210,

        // file (220 - 229)
        InvalidFileFormat         = 220,
        FileNotSorted             = 221,
        MissingCompressionLibrary = 223,

        // services (230 - 239)
        AnnotationLambdasFailed   = 230,

        // functionality (240 - 259)
        Compression = 240
    }
}