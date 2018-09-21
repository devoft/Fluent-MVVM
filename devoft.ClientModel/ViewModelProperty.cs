using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;
using devoft.Core.Patterns.Scoping;

namespace devoft.ClientModel
{

    public class ViewModelProperty<TOwner, TResult> : IViewModelProperty
        where TOwner : ViewModelBase<TOwner>
    {
        public ViewModelProperty()
        {
            ValidationResults = new ValidationResultCollection();
            Value = default(TResult);
            OldValue = default(TResult);
        }

        public TResult Value { get; set; }
        public TResult OldValue { get; set; }
        public ValidationResultCollection ValidationResults { get; } 

        internal bool SetValue(TOwner owner, string propertyName, TResult value)
        {
            var successful = true;
            var notifyChangeOnValidationError = true;
            var descriptor = ViewModelBase<TOwner>.RegisterProperty<TResult>(propertyName);
            var scopeContext = owner.ActiveScope?.CurrentScopeContext;

            foreach (var record in descriptor.ValidationRecords
                                             .Where(x => x.BehaviorOnCoerce == ValidationErrorBehavior.BeforeCoerce ||
                                                         x.BehaviorOnCoerce == ValidationErrorBehavior.BeforeAndAfterCoerce))
            {
                var validateFunc = record.Validate;
                var success = DoValidate(owner, value, validateFunc);
                if (!success)
                {
                    owner.OnErrorChanged(propertyName);
                    if (!record.ContinueOnValidationError)
                        return false;
                }
                successful &= success;
                notifyChangeOnValidationError &= record.NotifyChangeOnValidationError;
            }

            foreach (var coerce in descriptor.Coerces)
                if (successful && coerce != null)
                    value = coerce(value);

            if (Equals(Value, value))
                return false;

            OldValue = Value;

            foreach (var record in descriptor.ValidationRecords
                                             .Where(x => x.BehaviorOnCoerce == ValidationErrorBehavior.AfterCoerce ||
                                                         x.BehaviorOnCoerce == ValidationErrorBehavior.BeforeAndAfterCoerce))
            {
                var validateFunc = record.Validate;
                var success = DoValidate(owner, value, validateFunc);
                if (!success)
                {
                    owner.OnErrorChanged(propertyName);
                    if (!record.ContinueOnValidationError)
                        return false;
                }
                successful &= success;
                notifyChangeOnValidationError &= record.NotifyChangeOnValidationError;
            }

            Value = value;

            foreach (var setter in descriptor.Setters)
                setter?.Invoke(value);

            if (successful || notifyChangeOnValidationError)
                PropertyNotificationScopeAspect.TryRecordOrElseNotify(scopeContext, owner, propertyName, OldValue, value, descriptor.IsRecordingEnabled);

            return true;
        }

        public static implicit operator TResult (ViewModelProperty<TOwner, TResult> me)
            => me.Value;

        private bool DoValidate(TOwner owner, TResult value, Action<TOwner, TResult, ValidationResultCollection> validate)
        {
            try
            {
                ValidationResults.Clear();
                validate?.Invoke(owner, value, ValidationResults);
            }
            catch (Exception ex)
            {
                ValidationResults.Add(new ValidationResult
                {
                    Exception = ex as ValidationException ?? new ValidationException(ex),
                    Kind = ValidationKind.Error,
                    Message = $"Exception while validating: {ex.Message}"
                });
                return false;
            }
            return ValidationResults.IsSucceded;
        }

        internal void AddValidationResult(TOwner owner, string propertyName, string message, ValidationKind kind)
        {
            ValidationResults.AddResult(message, kind);
            owner.OnErrorChanged(propertyName);
        }
    }
}
