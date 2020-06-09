using System.Collections.Generic;

namespace devoft.ClientModel.ObjectModel
{
    public interface IPropertyChangeRecord
    {
        object Target { get; }
        string PropertyName { get; }
        
        public class EqualityByTargetAndProperty : IEqualityComparer<IPropertyChangeRecord>
        {
            public bool Equals(IPropertyChangeRecord x, IPropertyChangeRecord y)
                => Equals(x?.Target, y?.Target) && Equals(x?.PropertyName, y?.PropertyName);

            public int GetHashCode(IPropertyChangeRecord obj)
                => (obj.Target + "." + obj.PropertyName).GetHashCode();
        }

        public class EqualityByTargetAndProperty1 : IEqualityComparer<PropertyChangeRecord>
        {
            public bool Equals(PropertyChangeRecord x, PropertyChangeRecord y)
                => Equals(x?.Target, y?.Target) && Equals(x?.PropertyName, y?.PropertyName);

            public int GetHashCode(PropertyChangeRecord obj)
                => (obj.Target + "." + obj.PropertyName).GetHashCode();
        }
    }
}
