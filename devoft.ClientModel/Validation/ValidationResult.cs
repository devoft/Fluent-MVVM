using System;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Contains validation result information
    /// e.g. myVM.GetValidationResults(myPropertyName);
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Kind of validation result (Error, Warning, Information, Success)
        /// </summary>
        public ValidationKind Kind { get; set; }
        
        /// <summary>
        /// Exception thrown during validation or change, if such
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Message associated with validation.
        /// e.g. RegiserProperty(...).Validate(condition, "[this Message]");
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Additional information related with the validation
        /// </summary>
        public object AdditionalInfo { get; set; }

        /// <summary>
        /// Indicates whether the kind of validation is not Error.
        /// </summary>
        public bool IsSucceded => Kind != ValidationKind.Error;
    }
}
