using devoft.System;
using System;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Validates whether the value is in a right email format.
    /// eg. RegisterProperty(...).Validate<ValidEmail>()
    /// </summary>
    public class ValidEmail : IValidator
    {
        public string Validate(object value, string message = null)
            => value is string str && !FormatValidation.IsValidEmail(str) ? message ?? "Invalid Email format" : null;
    }

}
