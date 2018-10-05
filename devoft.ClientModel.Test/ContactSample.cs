using System;
using System.Collections.Generic;
using System.Text;
using devoft.ClientModel.Validation;

namespace devoft.ClientModel.Test
{
    public class ContactEditorViewModel : ViewModelBase<ContactEditorViewModel>
    {
        static ContactEditorViewModel()
        {
            Initialize();
        }

        public ContactEditorViewModel(IDispatcher dispatcher)
            : base(dispatcher)
        {

        }

        public static void Initialize()
        {
            RegisterProperty(x => x.Name)
                .Validate((t, v, vr) => vr.Error<NotNull>(v), notifyChangeOnValidationError: true)
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "Name cannot be empty or whitespace"), notifyChangeOnValidationError: true)
                .Coerce(name => name.Trim(),
                        name => char.ToUpper(name[0]) + name.Substring(1).ToLower());

            RegisterProperty(x => x.LastName)
                .Validate((t, v, vr) => vr.Error(v != null && string.IsNullOrWhiteSpace(v), "LastName cannot be empty or whitespace"), notifyChangeOnValidationError: true)
                .Validate((t, v, vr) => vr.Warning(!FormatValidation.IsPersonName(v), "Last names are usually in the same format as first names"))
                .Coerce(name => name.Trim());

            RegisterProperty(x => x.FullName)
                .DependOn(nameof(Name), nameof(LastName))
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "FullName cannot be empty or whitespace"));

            RegisterProperty(x => x.Age)
                .DependOn(x => x.Birthdate)
                .Validate((t, v, vr) => vr.Error(v < 0, "Age cannot be negative"));


            RegisterProperty(x => x.Email)
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "Email cannot be empty or whitespace"), notifyChangeOnValidationError: true)
                .Validate((t, v, vr) => vr.Error(!FormatValidation.IsValidEmail(v), "Email is in invalid format"), notifyChangeOnValidationError: true)
                .Coerce(name => name.Trim());
        }


        #region [ = Birthdate = ]
        public DateTime Birthdate
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }
        #endregion

        #region [ = Age ]
        public int Age
        {
            get => DateTime.Today.Year - Birthdate.Year;
        }
        #endregion 

        #region [ = Name = ]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        #endregion

        #region [ = LastName = ]
        public string LastName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        #endregion

        #region [ = FullName ]
        public string FullName
            => $"{Name} {LastName}";
        #endregion

        #region [ = Email = ]
        public string Email
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        #endregion

    }
}
