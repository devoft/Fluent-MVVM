using System.Collections;
using System.Linq;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Validates whether a string or collection is empty (or whitespace, in case of string)
    /// eg. RegisterCollectionProperty(...).Validate<NotEmpty>()
    /// </summary>
    public class NotEmpty : IValidator
    {
        public string Validate(object value, string message = null)
            => value switch
               {
                   string str                  => string.IsNullOrWhiteSpace(str) ? message ?? "Cannot be empty" : null,
                   IList { Count: 0 }          => message ?? "Cannot be empty",
                   ICollection { Count: 0 }    => message ?? "Cannot be empty",
                   IEnumerable e               => e.Cast<object>().Count() == 0 ? message ?? "Cannot be empty" : null,
                   null                        => message ?? "Cannot be empty",
                   _                           => null,
               };
    }
}
