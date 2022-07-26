﻿using System;

namespace RESTyard.WebApi.Extensions.ErrorHandling
{
    public class ExceptionProblemJson : ProblemJson
    {
        public ExceptionProblemJson(Exception exception)
        {
#if DEBUG
            this.ExceptionDetail = exception.ToString();
#endif
        }

#if DEBUG
        public string ExceptionDetail { get; set; }
#endif
    }
}