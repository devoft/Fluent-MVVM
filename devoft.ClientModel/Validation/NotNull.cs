using System;
using System.Collections.Generic;
using System.Text;

namespace devoft.ClientModel.Validation
{
    public class NotNull : IValidator
    {
        public string Validate(object value, string message = null)
            => value == null ? message ?? "Value cannot be null" : null;
    }

}
