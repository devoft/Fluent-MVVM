using System;
using System.Collections.Generic;
using System.Text;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Validates whether the value is not null
    /// eg. RegisterProperty(...).Validate<NotNull>()
    /// </summary>
    public class NotNull : IValidator
    {
        public string Validate(object value, string message = null)
            => value == null ? message ?? "Value cannot be null" : null;
    }
}
