using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;
using devoft.Core.Patterns.Scoping;

namespace devoft.ClientModel
{
    public class ViewModelPropertyDescriptor<TOwner, TResult>
        where TOwner : ViewModelBase<TOwner>
    {
        internal List<ValidationRecord> ValidationRecords { get; } = new List<ValidationRecord>();
        internal List<Func<TResult, TResult>> Coerces { get; } = new List<Func<TResult, TResult>>();
        internal List<Action<TResult>> Setters { get; } = new List<Action<TResult>>();
        internal bool ValidateAlways { get; } = true;
        public ViewModelPropertyDescriptor(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
        public bool IsRecordingEnabled { get; private set; }

        public ViewModelPropertyDescriptor<TOwner, TResult> Validate(
            Action<TOwner,TResult, ValidationResultCollection> validate,
            bool continueOnValidationError = true,
            bool notifyChangeOnValidationError = true,
            ValidationErrorBehavior when = ValidationErrorBehavior.AfterCoerce)
        {
            ValidationRecords.Add(new ValidationRecord()
            {
                Validate = validate,
                ContinueOnValidationError = continueOnValidationError,
                NotifyChangeOnValidationError = notifyChangeOnValidationError,
                BehaviorOnCoerce = when
            });
            return this;
        }

        public ViewModelPropertyDescriptor<TOwner, TResult> Coerce(params Func<TResult, TResult>[] coerces)
        {
            foreach (var coerce in coerces)
                Coerces.Add(coerce);
            return this;
        }

        public ViewModelPropertyDescriptor<TOwner, TResult> Set(params Action<TResult>[] setters)
        {
            foreach (var setter in setters)
                Setters.Add(setter);
            return this;
        }

        public ViewModelPropertyDescriptor<TOwner, TResult> EnableRecording()
        {
            IsRecordingEnabled = true;
            return this;
        }

        public ViewModelPropertyDescriptor<TOwner, TResult> DependUpon<TDependencyResult>(Expression<Func<TOwner, TDependencyResult>> exp)
        {
            PropertyNotificationManager<TOwner>.DependUpon(PropertyName, ((MemberExpression)exp.Body).Member.Name);
            return this;
        }

        public ViewModelPropertyDescriptor<TOwner, TResult> DependUpon(params string[] propertyNames)
        {
            PropertyNotificationManager<TOwner>.DependUpon(PropertyName, propertyNames);
            return this;
        }

        public class ValidationRecord
        {
            public Action<TOwner, TResult, ValidationResultCollection> Validate { get; set; }
            public bool ContinueOnValidationError { get; set; }
            public bool NotifyChangeOnValidationError { get; set; }
            public ValidationErrorBehavior BehaviorOnCoerce { get; internal set; }
        }
    }
}
