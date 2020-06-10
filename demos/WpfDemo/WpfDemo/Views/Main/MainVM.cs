using devoft.ClientModel;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;

namespace WpfDemo.Views.Main
{
    public class MainVM : ViewModelBase<MainVM>
    {
        static MainVM()
        {
            RegisterCollectionProperty(x => x.Users)
                .EnableRecording()
                .Validate((vm, col) => col.Count != 0 && col.Distinct().Count() != col.Count,
                          ValidationKind.Error,
                          "Collection may not have repeated values")
                .Validate((vm, col, res) =>
                          {
                              var user = col?.FirstOrDefault(x => x.Email.EndsWith(".com"));
                              res.Warning(user != null, $"{user?.FullName} has commercial email");
                          })
                .Validate((vm, col) => col.Count != 0 && col.Select(x => x.Email).Distinct().Count() != col.Count, 
                          ValidationKind.Information,
                          $"emails should be distinct");
        }

        public MainVM()
        {
            RegisterCommand("Add",       x => Record(s => Users.Add(x as UserViewModel), false));
            RegisterCommand("Duplicate", x => Record(s => Users.Add(x as UserViewModel), false),       (vm, x) => SelectedUser != null);
            RegisterCommand("Remove",    x => Record(s => Users.Remove(x as UserViewModel), false),    (vm, x) => SelectedUser != null);
        }

        public UserViewModel SelectedUser
        {
            get => GetValue<UserViewModel>();
            set => SetValue(value);
        }

        public ObservableCollection<UserViewModel> Users
        {
            get => GetCollection<ObservableCollection<UserViewModel>>();
        }

        [DependOn(nameof(Users))]
        public IEnumerable UsersErrors => GetErrors(nameof(Users));

    }
}
