using devoft.System;
using System;

namespace devoft.ClientModel.Validation
{
    public class ValidUrl : IValidator
    {
        public string Validate(object value, string message = null)
            => value is string str && !FormatValidation.IsValidUrl(str) ? message ?? "Invalid Url" : null;
    }

}
