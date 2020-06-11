using devoft.System;
using System;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Validates whether an string is in a right Url format
    /// eg. RegisterProperty(...).Validate<ValidUrl>()
    /// </summary>
    public class ValidUrl : IValidator
    {
        public string Validate(object value, string message = null)
            => value is string str && !FormatValidation.IsValidUrl(str) ? message ?? "Invalid Url" : null;
    }

}
