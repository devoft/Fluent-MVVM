using System;

namespace devoft.ClientModel.Validation
{
    public class ValidationResult
    {
        public ValidationKind Kind { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
        public object AdditionalInfo { get; set; }
        public bool IsSucceded => Kind != ValidationKind.Error;
    }
}
