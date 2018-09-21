using System;

namespace devoft.ClientModel.Validation
{
    public class ValidationException : AggregateException
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(Exception innerException) : base(innerException)
        {
        }
    }
}
