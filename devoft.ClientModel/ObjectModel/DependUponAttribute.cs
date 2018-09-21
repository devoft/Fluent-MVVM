using System;

namespace devoft.ClientModel.ObjectModel
{
    public class DependUponAttribute : Attribute
    {
        public DependUponAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }
    }
}