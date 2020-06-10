using devoft.ClientModel;
using devoft.ClientModel.ObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfDemo.Views.Main
{
    public class UserViewModel : ViewModelBase<UserViewModel>
    {
        public string FullName      { get => GetValue<string>();    set => SetValue(value); }
        public int Age              { get => GetValue<int>();       set => SetValue(value); }
        public string Email         { get => GetValue<string>();    set => SetValue(value); }

        public override bool Equals(object obj) 
            => Equals(FullName, (obj as UserViewModel)?.FullName);

        public override int GetHashCode() 
            => FullName?.GetHashCode() ?? base.GetHashCode();
    }
}
