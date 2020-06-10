using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;

namespace devoft.ClientModel.Metadata
{
    public class ViewModelCollectionProperty<TOwner, TResult> : IViewModelProperty
       where TOwner  : ViewModelBase<TOwner>
       where TResult : ICollection, INotifyCollectionChanged
    {
        public ViewModelCollectionProperty()
        {
            ValidationResults = new ValidationResultCollection();
        }

        public TResult Collection { get; set; }
        public ValidationResultCollection ValidationResults { get; }

        internal void SetValue(TOwner owner, string propertyName, TResult collection)
        {
            if (Collection != null)
                Collection.CollectionChanged -= CollectionChanged;
            Collection = collection;
            if (Collection != null)
                Collection.CollectionChanged += CollectionChanged;
            
            void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
                => OnCollectionChanged(owner, propertyName, e);
        }

        void OnCollectionChanged(TOwner owner, string propertyName, NotifyCollectionChangedEventArgs e)
        {
            var successful = true;
            var notifyChangeOnValidationError = true;
            var descriptor = ViewModelBase<TOwner>.RegisterCollectionProperty<TResult>(propertyName);
            if (descriptor is null)
                throw new InvalidOperationException("Raw property found where collection property expected. Try using RegisterCollectionProperty instead of RegisterProperty");

            var scopeContext = owner.ActiveScope?.CurrentScopeContext;

            ValidationResults.Clear();

            foreach (var record in descriptor.ValidationRecords)
            {
                var validateFunc = record.Validate;
                var success = DoValidate(owner, Collection, validateFunc);
                if (!success)
                {
                    owner.OnErrorChanged(propertyName);
                    if (!record.ContinueOnValidationError)
                        return;
                }
                successful &= success;
                notifyChangeOnValidationError &= record.NotifyChangeOnValidationError;
            }

            foreach (var action in descriptor.ChangedActions)
                action?.Invoke(Collection);

            if (successful || notifyChangeOnValidationError)
                PropertyNotificationScopeAspect.TryRecordOrElseNotify(scopeContext, owner, propertyName, e, descriptor.IsRecordingEnabled);

            return;
        }


        //public static implicit operator TResult(ViewModelProperty<TOwner, TResult> me)
        //    => me.Value;

        private bool DoValidate(TOwner owner, TResult value, Action<TOwner, TResult, ValidationResultCollection> validate)
        {
            try
            {
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
