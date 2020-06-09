using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using devoft.Core.Patterns;
using devoft.System;
using devoft.System.Collections.Generic;

namespace devoft.ClientModel.ObjectModel
{
    public class CollectionChangeRecord : IUndoable
    {
        public CollectionChangeRecord(object target, string propertyName, NotifyCollectionChangedEventArgs args)
        {
            Target = target;
            PropertyName = propertyName;
            Changes = args;
        }

        public object Target { get; }
        public string PropertyName { get; }
        public NotifyCollectionChangedEventArgs Changes { get; }


        public async Task Redo()
        {
            await Task.Yield();
            var collection = Target.Get<IList>(PropertyName);
            switch (Changes.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Changes.NewItems.Cast<object>().ForEach(x => collection.Add(x));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Changes.OldItems.Cast<object>().ForEach(x => collection.Remove(x));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    collection.Clear();
                    break;
                case NotifyCollectionChangedAction.Move:
                    for (var i = Changes.OldStartingIndex; i < Changes.OldItems.Count; i++)
                        collection.RemoveAt(Changes.OldStartingIndex);
                    for (var i = Changes.NewStartingIndex; i < Changes.OldItems.Count; i++)
                        collection.Insert(i, Changes.OldItems[i]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (var i = Changes.OldStartingIndex; i < Changes.OldItems.Count; i++)
                        collection.RemoveAt(Changes.OldStartingIndex);
                    for (var i = Changes.NewStartingIndex; i < Changes.NewItems.Count; i++)
                        collection.Insert(i, Changes.NewItems[i]);
                    break;
            }
        }

        public async Task Undo()
        {
            await Task.Yield();
            var collection = Target.Get<IList>(PropertyName);
            switch (Changes.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Changes.NewItems.Cast<object>().ForEach(x => collection.Remove(x));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Changes.OldItems.Cast<object>().ForEach(x => collection.Add(x));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in Changes.OldItems)
                        collection.Add(item);
                    break;
                case NotifyCollectionChangedAction.Move:
                    for (var i = Changes.NewStartingIndex; i < Changes.OldItems.Count; i++)
                        collection.RemoveAt(Changes.NewStartingIndex);
                    for (var i = Changes.OldStartingIndex; i < Changes.OldItems.Count; i++)
                        collection.Insert(i, Changes.OldItems[i]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (var i = Changes.NewStartingIndex; i < Changes.NewItems.Count; i++)
                        collection.RemoveAt(Changes.NewStartingIndex);
                    for (var i = Changes.OldStartingIndex; i < Changes.OldItems.Count; i++)
                        collection.Insert(i, Changes.OldItems[i]);
                    break;
            }
        }
    }
}
