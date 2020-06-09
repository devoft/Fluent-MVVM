using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using devoft.Core.Patterns;
using devoft.Core.Patterns.Scoping;

namespace devoft.ClientModel.ObjectModel
{
    public class PropertyNotificationScopeAspect
        : ScopeAspectBase<PropertyNotificationScopeAspect>
    {
        private Action<object, string[]> _notificationAction;
        private IPropertyChangeRecord.EqualityByTargetAndProperty _comparer = new IPropertyChangeRecord.EqualityByTargetAndProperty();

        public PropertyNotificationScopeAspect(Action<object, string[]> notificationAction)
        {
            _notificationAction = notificationAction;
        }

        public override void End(ScopeContext context, bool result)
        {
            //base.End(context, result);
            //if (result && _notificationAction != null)
            //{
            //    var group = context.Get<ScopeRecorder>()
            //           ?.Records
            //            .OfType<PropertyChangeRecord>()
            //            .Where(r => r.Target is IPropertyChangedNotifier)
            //            .Distinct(_comparer)
            //            .Select(r => new { Record = r, Property = r.PropertyName })
            //            .GroupBy(r => r.Record.Target);
            //    foreach (var g in group)
            //        _notificationAction(g.Key, g.Select(r => r.Property).ToArray());
            //}
            base.End(context, result);
            if (result && _notificationAction != null)
            {
                var group = context.Get<ScopeRecorder>()
                                   ?.Records
                                    .OfType<IPropertyChangeRecord>()
                                    .Where(r => r.Target is IPropertyChangedNotifier)
                                    .Distinct(_comparer)
                                    .Select(r => new { Record = r, Property = r.PropertyName })
                                    .GroupBy(r => r.Record.Target);
                foreach (var g in group)
                    _notificationAction(g.Key, g.Select(r => r.Property).ToArray());
            }
        }

        public static void TryRecordOrElseNotify<TOwner, TResult>(ScopeContext scopeContext, TOwner owner, string propertyName, TResult oldValue, TResult value, bool isRecordingEnabled)
            where TOwner : IPropertyChangedNotifier
        {
            var notify = false;
            if (scopeContext != null && isRecordingEnabled)
            {
                scopeContext.Get<ScopeRecorder>()
                            ?.Record(scopeContext,
                                     new PropertyChangeRecord(owner, propertyName, oldValue, value));

                if (!scopeContext.ContainsKey<PropertyNotificationScopeAspect>())
                    notify = true;
            }
            else notify = true;
            if (notify)
                PropertyNotificationManager<TOwner>.PropagateNotification(owner, propertyName);
        }

        public static void TryRecordOrElseNotify<TOwner>(ScopeContext scopeContext, TOwner owner, string propertyName, NotifyCollectionChangedEventArgs args, bool isRecordingEnabled)
            where TOwner : IPropertyChangedNotifier
        {
            var notify = false;
            if (scopeContext != null && isRecordingEnabled)
            {
                scopeContext.Get<ScopeRecorder>()
                            ?.Record(scopeContext,
                                     new CollectionChangeRecord(owner, propertyName, args));

                if (!scopeContext.ContainsKey<PropertyNotificationScopeAspect>())
                    notify = true;
            }
            else notify = true;
            if (notify)
                PropertyNotificationManager<TOwner>.PropagateNotification(owner, propertyName);
        }
    }

    public static class PropertyChangeScopeAspectExtensions
    {
        public static TInheritor DeferNotifyPropertiesOf<TInheritor, TViewModel>(this TInheritor scopeTask, TViewModel viewModel)
            where TInheritor : ScopeTaskBase<TInheritor>, new()
            where TViewModel : IPropertyChangedNotifier
        {
            scopeTask.AddAspects(new PropertyNotificationScopeAspect(
                (obj, props) => PropertyNotificationManager<TViewModel>.PropagateNotification(viewModel, props)));

            return scopeTask;
        }

        public static TInheritor DeferNotifyProperties<TInheritor, TViewModel>(this TInheritor viewModelScopeTask)
            where TInheritor : ViewModelScopeTaskBase<TInheritor, TViewModel>, new()
            where TViewModel : ViewModelBase<TViewModel>
        {
            var viewModel = viewModelScopeTask.ViewModel;
            viewModelScopeTask.AddAspects(new PropertyNotificationScopeAspect(
                (obj, props) => PropertyNotificationManager<TViewModel>.PropagateNotification(viewModel, props)));

            return viewModelScopeTask;
        }
    }
}
