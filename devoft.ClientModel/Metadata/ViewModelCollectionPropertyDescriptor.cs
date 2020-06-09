﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;
using devoft.Core.Patterns.Scoping;

namespace devoft.ClientModel.Metadata
{
    /// <summary>
    /// Entry point to fluent API configuration of the ViewModel properties behavior to property changes propagation, coercing, validation, etc.
    /// </summary>
    /// <typeparam name="TOwner">Property declaring type</typeparam>
    /// <typeparam name="TResult">Type of the property</typeparam>
    public class ViewModelCollectionPropertyDescriptor<TOwner, TResult>
        where TOwner : ViewModelBase<TOwner>
        where TResult : IEnumerable
    {
        internal List<ValidationRecord> ValidationRecords { get; } = new List<ValidationRecord>();
        internal List<Action<TResult>> ChangedActions { get; } = new List<Action<TResult>>();
        internal bool ValidateAlways { get; } = true;
        internal ViewModelCollectionPropertyDescriptor(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// The name of the property that is being configured
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Indicates whether recording is enabled on this property. 
        /// If recording is enabled any change made on the property will be recorded while in an active edition scope. <see cref="ScopeTaskBase{TInheritor}"/>
        /// Recorded property changes can be Undo or Redo
        /// </summary>
        public bool IsRecordingEnabled { get; private set; }

        /// <summary>
        /// Fluent API: Used to define the validation rules an results. Validations will be evaluated in the same order they are defined
        /// </summary>
        /// <param name="validate">Validation action passing the object, the validating object and a <see cref="ValidationResultCollection"/> to put the validations result into</param>
        /// <param name="continueOnValidationError">Indicates whether to stop or continue the property change when some validation error happens</param>
        /// <param name="notifyChangeOnValidationError">Indicates if the property must be changed even when some validation errors occur. Setting this to true includes the error in the object validation results. If false, the property change will not be notified.</param>
        /// <param name="when">Indicates whether to execute this rule before, after coerce, or both</param>
        /// <returns>this</returns>
        /// <remarks>
        /// The following example registers an error when a number is found on a person name, before coerce, the wrong value will be set and the change will be notified
        /// RegisterProperty<Person>(p => p.Name).Validate((t, v, vr) => vr.Error((v?.Any(ch => char.IsDigit(ch)) == true), "Cannot contain numbers"), notifyChangeOnValidationError: true, when: ValidationErrorBehavior.BeforeCoerce);
        /// </remarks>
        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> Validate(
            Action<TOwner, TResult, ValidationResultCollection> validate,
            bool continueOnValidationError = true,
            bool notifyChangeOnValidationError = true)
        {
            ValidationRecords.Add(new ValidationRecord()
            {
                Validate = validate,
                ContinueOnValidationError = continueOnValidationError,
                NotifyChangeOnValidationError = notifyChangeOnValidationError
            });
            return this;
        }

        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> Validate(
            Func<TOwner, TResult, bool> condition,
            ValidationKind kind = ValidationKind.Error,
            string message = null,
            bool continueOnValidationError = true,
            bool notifyChangeOnValidationError = true)
        {
            ValidationRecords.Add(new ValidationRecord()
            {
                Validate = (vm, x, res) => res.Validate(condition(vm, x), message, kind),
                ContinueOnValidationError = continueOnValidationError,
                NotifyChangeOnValidationError = notifyChangeOnValidationError
            });
            return this;
        }

        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> Validate<TValidator>(
            ValidationKind kind = ValidationKind.Error,
            string message = null,
            bool continueOnValidationError = true,
            bool notifyChangeOnValidationError = true)
            where TValidator : IValidator, new()
        {
            ValidationRecords.Add(new ValidationRecord()
            {
                Validate = (vm, x, res) => res.Validate<TValidator>(x, message, kind),
                ContinueOnValidationError = continueOnValidationError,
                NotifyChangeOnValidationError = notifyChangeOnValidationError
            });
            return this;
        }

        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> Validate(
            IValidator validator,
            ValidationKind kind = ValidationKind.Error,
            string message = null,
            bool continueOnValidationError = true,
            bool notifyChangeOnValidationError = true)
        {
            ValidationRecords.Add(new ValidationRecord()
            {
                Validate = (vm, x, res) => res.Validate(validator, x, message, kind),
                ContinueOnValidationError = continueOnValidationError,
                NotifyChangeOnValidationError = notifyChangeOnValidationError
            });
            return this;
        }


        /// <summary>
        /// Used to provider a custom setting actions. This setters will be invoked just after the new
        /// value is set on the property. They setters will be executed in the same order they are defined
        /// </summary>
        /// <param name="actions">A list of setters</param>
        /// <returns>this</returns>
        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> OnChanged(params Action<TResult>[] actions)
        {
            foreach (var action in actions)
                ChangedActions.Add(action);
            return this;
        }

        /// <summary>
        /// Enables recording of property assignments when they are doing in an edition scope. <see cref="ScopeTaskBase{TInheritor}"/>
        /// </summary>
        /// <returns>this</returns>
        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> EnableRecording()
        {
            IsRecordingEnabled = true;
            return this;
        }

        /// <summary>
        /// Tells whether the target property depends on the property referenced by the param <paramref name="exp"/>
        /// </summary>
        /// <typeparam name="TDependencyResult">The type of the dependency property</typeparam>
        /// <param name="exp">Expression refering the dependency property</param>
        /// <returns>this</returns>
        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> DependOn<TDependencyResult>(Expression<Func<TOwner, TDependencyResult>> exp)
        {
            PropertyNotificationManager<TOwner>.DependOn(PropertyName, ((MemberExpression)exp.Body).Member.Name);
            return this;
        }

        /// <summary>
        /// Tells whether the target property depends on the properties referenced by the param <paramref name="propertyNames"/>
        /// </summary>
        /// <param name="propertyNames">names of the dependency properties</param>
        /// <returns>this</returns>
        public ViewModelCollectionPropertyDescriptor<TOwner, TResult> DependOn(params string[] propertyNames)
        {
            PropertyNotificationManager<TOwner>.DependOn(PropertyName, propertyNames);
            return this;
        }

        public class ValidationRecord
        {
            public Action<TOwner, TResult, ValidationResultCollection> Validate { get; set; }
            public bool ContinueOnValidationError { get; set; }
            public bool NotifyChangeOnValidationError { get; set; }
        }
    }
}
