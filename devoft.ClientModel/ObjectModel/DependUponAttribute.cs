using System;

namespace devoft.ClientModel.ObjectModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependUponAttribute : Attribute
    {
        public DependUponAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }
    }
}