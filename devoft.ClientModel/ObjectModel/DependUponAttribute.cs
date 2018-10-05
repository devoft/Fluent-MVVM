using System;

namespace devoft.ClientModel.ObjectModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependOnAttribute : Attribute
    {
        public DependOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }
    }
}