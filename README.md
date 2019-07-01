# Introduction 
A toolkit help to build MVVM client applications with Blazor, WPF, UWP, Xamarin or Windows Forms. Making `ViewModelBase<TInheritor>` the base class of your view model type (`TInheritor`) your View Model class is empowered with Property Notification, Validations, Property Coertion, Editing Scopes and Undo/Redo; all configured by Fluent API as you can see in the following examples:
# Features #
- [View Model base](https://github.com/devoft/Fluent-MVVM#view-model-definition)
- [Property declaration](https://github.com/devoft/Fluent-MVVM#property-declaration)
- [Propagation of Property change notification](https://github.com/devoft/Fluent-MVVM#propagation-of-property-change-notification)
- [Automatic property value coercion](https://github.com/devoft/Fluent-MVVM#coerce-property-value)
- [Validation](https://github.com/devoft/Fluent-MVVM#validation)
- [Commands](https://github.com/devoft/Fluent-MVVM#commands)
- [Scopes](https://github.com/devoft/Fluent-MVVM#scopes)
  - [Scopes are Observables](https://github.com/devoft/Fluent-MVVM#scopesare-observables)
  - [Undo](https://github.com/devoft/Fluent-MVVM#undo)
- [Putting it all together](https://github.com/devoft/Fluent-MVVM#putting-it-all-together)
  - [Property change propagation with Coerce](https://github.com/devoft/Fluent-MVVM#property-change-propagation-with-Coerce)
  - [Property change with validation](https://github.com/devoft/Fluent-MVVM#property-change-with-validation)
  - [Coerce with validation](https://github.com/devoft/Fluent-MVVM#coerce-with-validation)
  - [Scope optimizes property change notifications](https://github.com/devoft/Fluent-MVVM#scope-optimizes-Property-change-notifications)
 
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

Use `GetValue<TPropertyType>()` and `SetValue<TPropertyType>(TPropertyType value)` to define get and set accessors on properties.
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

### Fluent property declarations ###

The `RegisterProperty` method can be use to defined the behaviors of a property about change notifications, validations, etc. Is the starting point of the Fluent API on properties.  For example, in the following code:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public static ContactEditor()
    {
    	RegisterProperty(vm => vm.FirstName)
    		.Coerce(name => name.Trim())
    }
    
    public string FirstName 
    { 
    	get => GetValue<string>(); 
    	set => SetValue(value); 
    }
}
```
The `Coerce` method guaranties that the value is trimmed before set. This configuration is made by using the Fluent API starting with the `RegisterProperty` method

### Computed properties ###

Computed properties is defined as usual like **FullName** in the following code:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public string FirstName { get => GetValue<string>(); set => SetValue(value); }
    public string LastName { get => GetValue<string>(); set => SetValue(value); }
    
    public string FullName => $"{FirstName} {LastName}";
}
```
## Propagation of Property change notification ##

By marking computed properties with the `[DependOn(dependencyProperty)]` attribute, every property-change notification happening on the *dependencyProperty* is propagated to this property, which means that another change notification will be raised for this property. In the following code, **FullName** is attributed with `[DependOn]`:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public string FirstName { get => GetValue<string>(); set => SetValue(value); }
    public string LastName { get => GetValue<string>(); set => SetValue(value); }
    
    [DependOn(nameof(FirstName))]	
    [DependOn(nameof(LastName))]	
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
    
    [DependOn(nameof(FirstName))]	
    [DependOn(nameof(LastName))]	
    public string FullName => $"{FirstName} {LastName}";
    
    [DependOn(nameof(Prefix))]	
    [DependOn(nameof(FullName))]	
    public string FormalName => $"{Prefix} {FullName}";
}
```
Any change made on **FirstName** or **LastName** will raise `PropertyChanged` 3 times: one for the original porperty, another for **FullName** and because **FormalName** depends on **FullName** another notification is raised for **FormalName** too. 

Note that any change on the **Prefix** property will raised `PropertyChanged` only for 2 properties **Prefix** and **FormalName**

#### DependOn: Fluent declaration ####
The property dependencies can be defined also with a fluent api starting from `RegisterProperty` method and continuing with the `DependOn` fluent declaration as follows:
``` csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public static ContactEditor()
    {
        RegisterProperty(vm => vm.FullName)
            .DependOn(nameof(FirstName), nameof(LastName));

        RegisterProperty(vm => vm.FormalName)
            .DependOn(nameof(Prefix))
            .DependOn(nameof(FullName));
    }

    public string FirstName { get => GetValue<string>(); set => SetValue(value); }
    public string LastName { get => GetValue<string>(); set => SetValue(value); }
    public string Prefix { get => GetValue<string>(); set => SetValue(value); }

    public string FullName => $"{FirstName} {LastName}";
    public string FormalName => $"{Prefix} {FullName}";
}
```
> Note that you can even use a single call to **DependOn** with many property names or even chain many **DependOn** calls
 
This way the declarations of the attribute `[DependOn(...)]` are not longer required on every computed property.

> Consider to use fluent api from static constructor to prevent performance overhead because of continuous redeclarations

## Coerce Property Value ##
Use fluent API to coerce any value assigned to the properties:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public static ContactEditor()
    {
        RegisterProperty(c => c.FirstName)
            .Coerce(name => name.Trim(), 
                    name => char.ToUpper(name[0]) + name.Substring(1).ToLower());
    }
}

...

contactEditor.FirstName = " john   "; // contactEditor.FirstName == "John"
```

This can be useful to automaticaly "fix" values introduced through declarative two-way bindings:

```xml
<TextBox Text="{Binding FirstName, Mode=TwoWay}"/>
```
> Coerce transformation will occur in the same order they are defined

## Validation ##

Use `Validate` to add validation rules that will be applied before the value is set to the property:
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public static ContactEditor()
    {
        RegisterProperty(x => x.LastName)
                .Validate((viewModel, value, validationResultCollection) 
                           => validationResultCollection.Error(string.IsNullOrWhiteSpace(val), 
                                                               "LastName cannot be empty or whitespace"));
    }
}
```
`Validate` has the following optional parameters:
- `notifyChangeOnValidationError=true`: see [Putting it all together: Property change with validation](https://github.com/devoft/Fluent-MVVM#property-change-with-validation)
- `BehaviorOnCoerce=ValidationErrorBehavior.AfterCoerce`): see [Putting it all together: Coerce with validation](https://github.com/devoft/Fluent-MVVM#coerce-with-validation)
- `ContinueOnValidationError=true`: If this param is set to false then any error detected during validation will (silently) abort the property change.

Validations are applied in the same order they are defined

### Validation Results ###
Validation Results can be of type: **Error**, **Warning** or **Information**. Validation is considered succeded if there is no error in the collection when validation finishes.
Each validation rule can add 1, many or 0 validation results. Use the following `ViewModelBase<T>` methods:
- `GetValidationResults(propertyName)` to know the validation results at any moment. Then, use `Validate()` of this `ValidationResultCollection` if you want to apply additional validations from imperative code
- `ClearValidationResult()` to reset validation results
 

### INotifyDataErrorInfo support ###
`ViewModelBase<T>` implements `INotifyDataErrorInfo`, so it is integrated with binding and validation system of WPF and UWP, and
its members can be used to know whether is it some validation errors (`HasError`), whether the appear os disappear (`ErrorChanged`) 
and what validation error is happening per property.
> **Warnings** and **Informations** is considered Errors too in the `INotifyDataErrorInfo` logic

## Commands ##
Use `RegisterCommand` to add commands to the View Model:
```csharp
public override Task InitializeAsync(IDispatcher dispatcher = null)
{
    ICommand cmd = RegisterCommand("RegisterNewUser",
                                   execute: x => RegisterUser(x),
                                   canExecuteCondition: x => !ExistsUser(x));

    return base.InitializeAsync(dispatcher);
}

```
This way commands can be bound in WPF and UWP like this:

```xml
<Button Command="{Binding ViewModel.Commands.RegisterNewUser}"/>
```

> Note that this method creates an object which is `System.Windows.Input.ICommand` 
> It is recommended to register commands from the `InitializeAsync` method

Every command registered this way will be invalidated when any property changes. 
See: [Putting it all together: Invalidate commands on property changes](https://github.com/devoft/Fluent-MVVM#invalidate-commands-on-property-changes)

## Scopes ##
Use Scopes to work with the View Model features in a transactional way. You can optimize property changes notifications, record changes to enable Undo/Redo operations, and so on...

Scopes are blocks of code defined using `BeginScope` and executed through `StartAsync`:
```csharp
var scope = vm.BeginScope(sc =>
            {
               vm.FirstName = "John";
               vm.LastName = "Smith";
            });
await scope.StartAsync();
```
### Scopes are Observables ###

Scopes are `IObservable<object>` The way of notify partial results is using: `Yield(object)`:
```csharp
var scope = await vm.BeginScope(sc =>
            {
               for (var item in myList)
                   sc.Yield(item);
            })
            .Observe(x => x.Subscribe(myObserver))
            .StartAsync();
```
Use `Observe(Action<IObservable<object>>)` to react to scope yield-results, or even to apply [Reactive extensions](https://github.com/dotnet/reactive) queries:
```csharp
var scope = await vm.BeginScope(sc =>
            {
               for (var item in myContactList)
                   sc.Yield(item);
            })
            .Observe(c => c.Where(c => c.Age >= 18)
                           .DistinctUntilChanged()
                           .Subscribe(Console.WriteLine))
            .StartAsync();
```

### Undo ###
Using `UndoAsync` in the scope you can undo every changes applied in the scope to those properties registered with recording enabled:
```csharp
RegisterProperty(vm => vm.FirstName)
    .EnableRecording();
RegisterProperty(vm => vm.LastName);
```
Then you can apply `UndoAsync()` as follows:
```csharp
var vm.FirstName = "Unknown";
var vm.LastName = "Unknown";
var scope = vm.BeginScope(sc =>
            {
               ...
               vm.FirstName = "John";
               vm.LastName = "Wood";
            });
await scope.StartAsync();
await scope.UndoAsync();
var firstName = vm.FirstName // firstName == "Unknown"
var lastName = vm.LastName // lastName == "Wood"
```
From this example, see that because `EnableRecording` is not applied explicitly on *LastName* the change on this property during the scope was not undone through `UndoAsync`

## Putting it all together ##

### Property change propagation with Coerce ###
Property changes and its propagation will happen always after the coercions are applied on the source property. E.g:
```csharp
contactEditor.PropertyChanged += (s.e) => 
    {
    	if (e.PropertyName == "FullName")
    		Console.WriteLine(contactEditor.FullName); // "John"
    };
contactEditor.FirstName = " john   ";
```

### Property change with validation ###

Setting optional parameter: `notifyChangeOnValidationError` of the `Validate` method, you can decide whether the property change notification is raise even when validation fails.

### Coerce with validation ###

The `Validate` method has the parameter `BehaviorOnCoerce` used to indicate whether some specific validation rule should be applied before or after the Coerce, or both.
```csharp
public class ContactEditor : ViewModelBase<ContactEditor>
{
    public static ContactEditor()
    {
        RegisterProperty(x => x.Name)
                .Coerce(x => x[0].ToUpper() + x.Substring(1).ToLower())
                .Validate((t, v, vr) => vr.Error<NotNull>(v) && v.Length > 1, 
                          continueOnValidationError: false,
                          when: ValidationErrorBehavior.BeforeCoerce);
    }
}
```

Because the validation will apply before coerce and because `continueOnValidationError=false`
it is safe to apply the Coerce with no errors

### Invalidate commands on property changes ###
Every time one property is changed all registered commands are invalidated so their `CanExecute` method will be called and their event `CanExecuteChanged` will be raised.

### Scope optimizes Property change notifications ###
```csharp
var scope = vm.BeginScope(sc =>
            {
               vm.FirstName = "Johnn";
               vm.FirstName = "John";
               vm.LastName = "Smit";
               vm.LastName = "Smith";
            });
await scope.StartAsync();
```
Doing this, the PropertyChanged event is raised once for `FirstName`, another time for `LastName`, and even when `FullName` is depending on both, it will be notified only once from the scope.

# Getting Started
Just add devoft.ClientModel as a dependency and start coding

# Build and Test
devoft.Client.Test is the main testing project 

# Contribute
Just fork the repo and make us your pull requests 

If you want to learn more about this and other projects visit us at [devoft](http://www.devoft.com)

