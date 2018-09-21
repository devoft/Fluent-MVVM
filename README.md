# Introduction 
A toolkit help to build MVVM client applications with Blazor, WPF, UWP, Xamarin or Windows Forms. Making `ViewModelBase<TInheritor>` the base class of your view model type (`TInheritor`) your View Model class is empowered with Property Notification, Validations, Property Coertion, Editing Scopes and Undo/Redo; all configured by Fluent API as you can see in the following examples:

## View Model Definition ##
Start by inheriting from `ViewModelBase<YourClass>` like this:
```csharp

public class ContactEditor : ViewModelBase<ContactEditor>
{
  ...
}

```
Note that the generic parameter `T` on `ViewModelBase<T>` is substituted by your view model (`ContactEditor` in the example) 

# Getting Started
Just add devoft.Client as a dependency and start coding

# Build and Test
devoft.Client.Test is the main testing project 

# Contribute
Just fork the repo and make us your pull requests 

If you want to learn more about this and other projects visit us at [devoft](http://www.devoft.com)
