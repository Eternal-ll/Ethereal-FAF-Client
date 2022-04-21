using System;

namespace beta.Models
{
    public class ExceptionWrapper
    {
        public Exception Exception { get; }
        public ExceptionWrapper(Exception exception)
        {
            Exception = exception;
        }
    }
}
