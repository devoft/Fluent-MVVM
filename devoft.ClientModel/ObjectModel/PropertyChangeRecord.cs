using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using devoft.Core.Patterns;

namespace devoft.ClientModel.ObjectModel
{
    public class PropertyChangeRecord : IUndoable
    {
        public PropertyChangeRecord(object target, string propertyName, object oldValue, object newValue)
        {
            Target = target;
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public object Target { get; }
        public string PropertyName { get; }
        public object OldValue { get; }
        public object NewValue { get; }


        public async Task Redo()
        {
            await Task.Yield();
            Target.Set(PropertyName, NewValue);
        }

        public async Task Undo()
        {
            await Task.Yield();
            Target.Set(PropertyName, OldValue);
        }

        public class EqualityByTargetAndProperty : IEqualityComparer<PropertyChangeRecord>
        {
            public bool Equals(PropertyChangeRecord x, PropertyChangeRecord y)
                => Equals(x?.Target, y?.Target) && Equals(x?.PropertyName, y?.PropertyName);

            public int GetHashCode(PropertyChangeRecord obj)
                => (obj.Target + "." + obj.PropertyName).GetHashCode();
        }
    }
}
