using devoft.System;
using System;

namespace devoft.ClientModel.Validation
{
    public class ValidEmail : IValidator
    {
        public string Validate(object value, string message = null)
            => value is string str && !FormatValidation.IsValidEmail(str) ? message ?? "Invalid Email" : null;
    }

}
