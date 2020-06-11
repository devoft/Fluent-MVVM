using System;

namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Validates whether a number is whiting the <see cref="Range"/> indicated in the constructor.
    /// eg. RegisterProperty(...).Validate(new ValidRange(0..120))
    /// </summary>
    public class ValidRange : IValidator
    {
        public ValidRange(Range range)
        {
            Range = range;
        }

        public Range Range { get; }

        public string Validate(object value, string message = null)
        {
            var val = Convert.ToInt32(value);
            return val < Range.Start.Value || val > Range.End.Value 
                        ? $"Value out of range: {Range.Start.Value}..{Range.End.Value}" 
                        : null;
        }
    }
}
