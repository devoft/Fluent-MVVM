using System.Collections;
using System.Linq;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Validates whether a collection has distinct values
    /// eg. RegisterCollectionProperty(...).Validate<AreDistinct>()
    /// </summary>
    public class AreDistinct : IValidator
    {
        public string Validate(object value, string message = null)
        {
            if (!(value is IEnumerable e))
                return null;
            var col = e.Cast<object>();
            return col.Distinct().Count() != col.Count()
                        ? message ?? "Repeated items found"
                        : null;
        }
    }
}
