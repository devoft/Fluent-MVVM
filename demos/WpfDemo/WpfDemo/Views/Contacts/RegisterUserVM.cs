using devoft.ClientModel;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;
using devoft.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ValidationResult = devoft.ClientModel.Validation.ValidationResult;

namespace WpfDemo.Views.Contacts
{
    public class RegisterUserVM : ViewModelBase<RegisterUserVM>
    {
        static RegisterUserVM()
        {
            RegisterProperty(vm => vm.Name)
                .Coerce(CoerceNames)
                .Validate<NotEmpty>();

            RegisterProperty(vm => vm.LastName)
                .Coerce(CoerceNames)
                .Validate<NotEmpty>();

            RegisterProperty(vm => vm.Email)
                .Validate<ValidEmail>()
                .Validate(condition: (vm, x) => x?.EndsWith(".com") == true, 
                          kind:      ValidationKind.Warning, 
                          message:   "Try not use commercial emails here");

            RegisterProperty(vm => vm.Age)
                .Validate(new ValidRange(0..120));

            static string CoerceNames(string name)
                => string.Join(" ", name.Split(" ").Select(w => (string.IsNullOrWhiteSpace(w) ? w : w.ToAlphaOnly(true)).Trim()));
        }

        public RegisterUserVM()
        {
            RegisterCommand("Ok", x => Success = true, (vm, x) => !HasErrors);
            RegisterCommand("Cancel", x => Success = false);
        }

        public bool? Success
        {
            get => GetValue<bool?>();
            set => SetValue(value);
        }

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LastName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string Email
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [DependOn(nameof(Name)), DependOn(nameof(LastName))]
        public string FullName => $"{Name} {LastName}";

        public int Age
        {
            get => GetValue<int>();
            set => SetValue(value);
        }
    }
}
