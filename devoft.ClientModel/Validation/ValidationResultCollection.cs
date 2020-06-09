using System;
using System.Collections.Generic;
using System.Linq;

namespace devoft.ClientModel.Validation
{
    public class ValidationResultCollection : List<ValidationResult>
    {
        public bool IsSucceded => this.All(x => x.Kind == ValidationKind.Success);

        public bool HasErrors => this.Any(x => x.Kind == ValidationKind.Error);

        public void Error<TValidator>(object value, string message = null)
            where TValidator : IValidator, new()
            => Validate<TValidator>(value, message, ValidationKind.Error);

        public void Warning<TValidator>(object value, string message = null)
            where TValidator : IValidator, new()
            => Validate<TValidator>(value, message, ValidationKind.Warning);

        public void Information<TValidator>(object value, string message = null)
            where TValidator : IValidator, new()
            => Validate<TValidator>(value, message, ValidationKind.Information);


        public void Error(bool condition, string message = null)
            => Validate(condition, message, ValidationKind.Error);

        public void SetError(bool condition, string message = null)
            => AddResult(message, ValidationKind.Error);

        public void Warning(bool condition, string message = null)
            => Validate(condition, message, ValidationKind.Warning);

        public void SetWarning(bool condition, string message = null)
            => AddResult(message, ValidationKind.Warning);

        public void Information(bool condition, string message = null)
            => Validate(condition, message, ValidationKind.Information);

        public void SetInformation(bool condition, string message = null)
            => AddResult(message, ValidationKind.Information);


        public ValidationResult Validate<TValidator>(object value, string message, ValidationKind kind)
            where TValidator : IValidator, new() 
            => Validate(new TValidator(), value, message, kind);

        public ValidationResult Validate(IValidator validator, object value, string message, ValidationKind kind)
        {
            ValidationResult result = null;
            if (validator.Validate(value, message) is string msg)
            {
                Add(result = new ValidationResult
                {
                    Message = msg,
                    Kind = kind,
                    Exception = new ValidationException(message)
                });
            }
            return result;
        }

        public ValidationResult Validate(bool condition, string message, ValidationKind kind)
            => condition ? AddResult(message,kind) : null;

        internal ValidationResult AddResult(string message, ValidationKind kind)
        {
            ValidationResult result = null;
            Add(result = new ValidationResult
            {
                Message = message,
                Kind = kind,
                Exception = new ValidationException(message)
            });
            return result;
        }

        internal void PerformValidation(Action validate, Action onErrorChanged)
        {
            var wasValid = IsSucceded;
            Clear();
            validate();
            if (wasValid != IsSucceded)
                onErrorChanged();
        }
    }
}
