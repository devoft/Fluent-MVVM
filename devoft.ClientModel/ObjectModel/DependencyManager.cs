using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using devoft.Core.Patterns;
using devoft.Core.System;
using devoft.Core.Patterns;

namespace devoft.ClientModel.ObjectModel
{
    public class PropertyNotificationManager<T>
        where T : IPropertyChangedNotifier
    {
        private Dictionary<string, HashSet<string>> _dependencies = new Dictionary<string, HashSet<string>>();
        private bool _metadataInitialized;
        private PropertyNotificationManager() { }
        public static PropertyNotificationManager<T> Instance { get; } = new PropertyNotificationManager<T>();

        public void NotifyPropertyChanged(T target, string propertyName, Action<T, string> performNotification)
        {
            var visitedProperties = new HashSet<string>();
            NotifyPropertyChanged(propertyName);

            void NotifyPropertyChanged(string propName)
            {
                if (visitedProperties.Contains(propName))
                    return;

                if (performNotification != null)
                    performNotification(target, propertyName);
                else
                    target.OnPropertyChanged(propName);

                visitedProperties.Add(propName);
                foreach (var dependency in DependenciesFor(propName))
                    NotifyPropertyChanged(dependency);
            }
        }

        public static void DependUpon(string propertyName, params string[] dependencies)
        {
            var dep = Instance._dependencies;
            lock (dep)
               foreach (var dependUpon in dependencies)
                   dep.Ensure(dependUpon)
                      .Add(propertyName);
        }

        public static void PropagateNotification(T target, string propertyName, Action<T,string> performNotification = null)
            => Instance.NotifyPropertyChanged(target, propertyName, performNotification);

        public IEnumerable<string> DependenciesFor(string propertyName)
        {
            EnsureMetadata();
            return _dependencies.TryGetValue(propertyName, out var dependencies)
                        ? dependencies
                        : Enumerable.Empty<string>();
        }

        private void EnsureMetadata()
        {
            if (_metadataInitialized)
                return;

            lock (_dependencies)
            {
                foreach (var property in typeof(T).GetProperties())
                    foreach (var dependUpon in property.GetCustomAttributes<DependUponAttribute>(true))
                        _dependencies.Ensure(dependUpon.PropertyName)
                                     .Add(property.Name);

                _metadataInitialized = true;
            }
        }
        
    }
}