using System;

namespace WebApiHypermediaExtensionsCore.ErrorHandling
{
    public class ExceptionProblemJson : ProblemJson
    {
        public ExceptionProblemJson(Exception exception)
        {
#if DEBUG
            this.StackTrace = exception.StackTrace;
#endif
        }

#if DEBUG
        public string StackTrace { get; set; }
#endif
    }
}