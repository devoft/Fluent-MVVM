using System.Collections.Generic;
using devoft.Core.Patterns;
using devoft.ClientModel.ObjectModel;
using devoft.Core.Patterns.Scoping;
using System;

namespace devoft.ClientModel
{
    public interface IViewModelScopeTask<TViewModel>
        : IScopeTask
    {
        TViewModel ViewModel { get; set; }
    }

    public class ViewModelScopeTaskBase<TInheritor, TViewModel> 
        : ScopeTaskBase<TInheritor>,
          IViewModelScopeTask<TViewModel>
        where TInheritor : ViewModelScopeTaskBase<TInheritor, TViewModel>, new()
        where TViewModel : ViewModelBase<TViewModel>
    {
        public ViewModelScopeTaskBase()
        {
        }

        public ViewModelScopeTaskBase(TViewModel viewModel, IEnumerable<IScopeAspect> aspects)
            : base(aspects)
        {
            viewModel.PushActiveScope(this);
            ViewModel = viewModel;
            _aspects.Insert(0, 
                new PropertyNotificationScopeAspect(
                    (obj, props) => PropertyNotificationManager<TViewModel>.PropagateNotification(viewModel, props)));
        }

        public TViewModel ViewModel { get; set; }
    }

    public static class ViewModelScopeTaskBaseExtensions
    {
        public static TScopeTask AttachTo<TScopeTask, TViewModel>(this TScopeTask scopeTask, TViewModel viewModel)
            where TScopeTask : ViewModelScopeTaskBase<TScopeTask, TViewModel>, new()
            where TViewModel : ViewModelBase<TViewModel>
        {
            viewModel.PushActiveScope(scopeTask);
            scopeTask.ViewModel = viewModel;
            return scopeTask;
        }

        public static TScopeTask Configure<TScopeTask, TViewModel>(this TScopeTask scopeTask, TViewModel viewModel)
            where TScopeTask : ViewModelScopeTaskBase<TScopeTask, TViewModel>, new()
            where TViewModel : ViewModelBase<TViewModel>
            => scopeTask
                   .AttachTo(viewModel)
                   .Recording()
                   .DeferNotifyProperties<TScopeTask, TViewModel>();
    }
}
