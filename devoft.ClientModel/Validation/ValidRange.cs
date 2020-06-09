using System;

namespace devoft.ClientModel.Validation
{
    public class ValidRange : IValidator
    {
        private Range _range;

        public ValidRange(Range range)
        {
            _range = range;
        }

        public string Validate(object value, string message = null)
        {
            var val = Convert.ToInt32(value);
            return val < _range.Start.Value || val > _range.End.Value 
                        ? $"Value out of range: {_range.Start.Value}..{_range.End.Value}" 
                        : null;
        }
    }
}
