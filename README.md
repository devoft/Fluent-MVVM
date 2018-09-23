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

The class `ViewModelBase<T>` implements `INotifyPropertyChanged` and provides a set of methods to support property validation, coerce and so on.

## Property declaration ##

Use `GetValue<TPropertyType>()` and `SetValue<TPropertyType>(TPropertyType value)` to define get and set accessors on properties
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
	public string FirstName 
	{ 
		get => GetValue<string>(); 
		set => SetValue(value); 
	}
}
```
By doing this, the property change is notified whenever the new value is different from old value and validations succedded.

Computed properties can be defined as usual like **FullName** in the following code:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
	public string FirstName { get => GetValue<string>(); set => SetValue(value); }
	public string LastName { get => GetValue<string>(); set => SetValue(value); }
	
	public string FullName => $"{FirstName} {LastName}";
}
```
### Property change notification propagation ###

By marking computed properties with the `[DependUpon(dependencyProperty)]` attribute, every property-change notification happening on the *dependencyProperty* is propagated to this property, which means that another change notification will be raised for this property. In the following code, **FullName** is attributed with `[DependUpon]`:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
	public string FirstName { get => GetValue<string>(); set => SetValue(value); }
	public string LastName { get => GetValue<string>(); set => SetValue(value); }

	[DependUpon(nameof(FirstName))]	
	[DependUpon(nameof(LastName))]	
	public string FullName => $"{FirstName} {LastName}";
}
```
Such that the following code:
```csharp
var editor = new ContactEditor();
editor.FirstName = "John";
```
will notify `PropertyChanged` twice: one for the property **FirstName** and the other for the property **FullName**

Propagation happens from one property to another. Such that in the following code
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
	public string FirstName { get => GetValue<string>(); set => SetValue(value); }
	public string LastName { get => GetValue<string>(); set => SetValue(value); }
	public string Prefix { get => GetValue<string>(); set => SetValue(value); }

	[DependUpon(nameof(FirstName))]	
	[DependUpon(nameof(LastName))]	
	public string FullName => $"{FirstName} {LastName}";

	[DependUpon(nameof(Prefix))]	
	[DependUpon(nameof(FullName))]	
	public string FormalName => $"{Prefix} {FullName}";
}
```
Any change made on **FirstName** or **LastName** will raise `PropertyChanged` 3 times: one for the original porperty, another for **FullName** and because **FormalName** depends upon **FullName** another notification is raised for **FormalName** too. 

Note that any change on the **Prefix** property will raised `PropertyChanged` only for 2 properties **Prefix** and **FormalName**

#### DependUpon: Fluent declaration ####
The property dependencies can be defined also with a fluent api starting from `RegisterProperty` method and continuing with the `DependUpon` fluent declaration as follows:
``` csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
	public ContactEditor()
	{
		RegisterProperty(vm => vm.FullName)
			.DependUpon(nameof(FirstName), nameof(LastName));

		RegisterProperty(vm => vm.FormalName)
			.DependUpon(nameof(Prefix))
			.DependUpon(nameof(FullName));
	}

	public string FirstName { get => GetValue<string>(); set => SetValue(value); }
	public string LastName { get => GetValue<string>(); set => SetValue(value); }
	public string Prefix { get => GetValue<string>(); set => SetValue(value); }

	public string FullName => $"{FirstName} {LastName}";
	public string FormalName => $"{Prefix} {FullName}";
}
```
> Note that you can even use a single call to **DependUpon** with many property names or even chain many **DependUpon** calls
 
This way the declarations of the attribute `[DependUpon(...)]` are not longer required on every computed property.

> Consider to use fluent api from static constructor to prevent performance overhead because of continuous redeclarations

# Getting Started
Just add devoft.Client as a dependency and start coding

# Build and Test
devoft.Client.Test is the main testing project 

# Contribute
Just fork the repo and make us your pull requests 

If you want to learn more about this and other projects visit us at [devoft](http://www.devoft.com)
