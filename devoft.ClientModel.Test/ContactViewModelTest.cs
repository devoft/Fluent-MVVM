using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using devoft.ClientModel.ObjectModel;
using devoft.ClientModel.Validation;
using devoft.Core.Patterns.Scoping;

namespace devoft.ClientModel.Test
{
    [TestClass]
    public class ContactViewModelTest : ViewModelBase<ContactViewModelTest>
    {
        private ConsoleInterceptor _console;

        public ContactViewModelTest()
        {
        }

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(_console = new ConsoleInterceptor());
        }

        [TestMethod]
        public void TestRegisterProperty()
        {
            var namePropDesc = RegisterProperty(x => x.Name);
            Assert.IsNotNull(namePropDesc);
            Assert.IsInstanceOfType(namePropDesc, typeof(ViewModelPropertyDescriptor<ContactViewModelTest, string>));
            var replyNamePropDesc = RegisterProperty(x => x.Name);
            Assert.AreEqual(namePropDesc, replyNamePropDesc);
        }

        [TestMethod]
        public void TestNullValidationOnProperty()
        {
            RegisterProperty(x => x.Name)
                .Validate((t, v, vr) => vr.Error<NotNull>(v), notifyChangeOnValidationError: true, when: ValidationErrorBehavior.BeforeCoerce);
            Name = null;
            Assert.IsTrue(HasErrors);
        }

        [TestMethod]
        public void TestEmptyValidationOnProperty()
        {
            RegisterProperty(x => x.Name)
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "Name cannot be empty or whitespace"), notifyChangeOnValidationError: true);
            Name = "";
            Assert.IsTrue(HasErrors);
        }

        [TestMethod]
        public void TestClearValidations()
        {
            RegisterProperty(x => x.Name)
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "Name cannot be empty or whitespace"), notifyChangeOnValidationError: true);
            Name = "";
            Assert.IsTrue(HasErrors);
            ClearValidationResults();
            Assert.IsFalse(HasErrors);
        }

        [TestMethod]
        public void TestValidationResults()
        {
            RegisterProperty(x => x.Name)
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "Name cannot be empty or whitespace"), notifyChangeOnValidationError: true);
            Name = "";
            var results = GetValidationResults(nameof(Name));
            Assert.AreEqual(results.Count, 1);
            Assert.AreEqual(results[0].Message, "Name cannot be empty or whitespace");
            Assert.AreEqual(results[0].Exception.Message, "Name cannot be empty or whitespace");
        }


        [TestMethod]
        public void TestCoerceOnProperty()
        {
            RegisterProperty(x => x.Name)
                .Coerce(name => name.Trim())
                .Coerce(name => char.ToUpper(name[0]) + name.Substring(1).ToLower());
            Name = "   abc   ";
            Assert.AreEqual("Abc", Name);
        }

        [TestMethod]
        public void TestCoerceAndValidationOnProperty()
        {
            RegisterProperty(x => x.Name)
                .Coerce(name => name.Trim())
                .Coerce(name => char.ToUpper(name[0]) + name.Substring(1).ToLower())
                .Validate((t, v, vr) => vr.Error<NotNull>(v), notifyChangeOnValidationError: true, when: ValidationErrorBehavior.BeforeCoerce)
                .Validate((t, v, vr) => vr.Error(string.IsNullOrWhiteSpace(v), "Name cannot be empty or whitespace"), notifyChangeOnValidationError: true);
            Assert.IsFalse(HasErrors);
            Name = "   abc   ";
            Assert.AreEqual("Abc", Name);
        }

        [TestMethod]
        public void TestValidationBeforeAndAfterCoerceOnProperty()
        {
            Name = null;
            RegisterProperty(x => x.Name)
                .Coerce(name => name.Trim())
                .Coerce(name => char.ToUpper(name[0]) + name.Substring(1).ToLower())
                .Validate((t, v, vr) => vr.Error((v?.Any(ch => char.IsDigit(ch)) == true), "Cannot contain numbers"), notifyChangeOnValidationError: true, when: ValidationErrorBehavior.BeforeCoerce);
            Name = "   abc   ";
            Assert.IsFalse(HasErrors);
            Assert.AreEqual("Abc", Name);
        }

        [TestMethod]
        public void TestValidationCoerceAvoidingValidationErrorsOnProperty()
        {
            LastName = null;
            RegisterProperty(x => x.LastName)
                .Coerce(str => "" + str)
                .Validate((t, v, vr) => vr.Error<NotNull>(v), notifyChangeOnValidationError: true, when: ValidationErrorBehavior.AfterCoerce);
            LastName = null;
            Assert.IsFalse(HasErrors);
            Assert.AreEqual("", LastName);
        }


        [TestMethod]
        public void TestPropertyNotificationPropagation()
        {
            RegisterProperty(x => x.FullName)
                .DependOn(nameof(Name), nameof(LastName));
            int[] array = new int[1];
            PropertyChangedEventHandler handler = (e, s) => array[0]++;
            PropertyChanged += handler;
            Name = "John";
            LastName = "Smith";
            Assert.AreEqual(array[0], 4);
            PropertyChanged -= handler;
        }

        
        [TestMethod]
        public async Task NotificationPrunedOnScope()
        {
            int[] array = new int[1];
            PropertyChangedEventHandler handler = (e, s) => array[0]++;
            PropertyChanged += handler;

            RegisterProperty(x => x.Name)
                .EnableRecording();

            RegisterProperty(x => x.LastName)
                .EnableRecording();

            RegisterProperty(x => x.FullName)
                 .DependOn(x => x.Name)
                 .DependOn(x => x.LastName)
                 .EnableRecording();


            var observer = new FunctionObserver<object>(s => Console.WriteLine(s));
            var result = await BeginScope(sc =>
                                    {
                                        Name = "Abc";
                                        Name = "Bcd";
                                        LastName = "Cde";
                                        LastName = "Def";
                                        LastName = "Efg";
                                    })
                            .Observe(ob => ob.Subscribe(observer))
                            .StartAsync();

            PropertyChanged -= handler;

            Assert.AreEqual("Bcd Efg", FullName);
            Assert.AreEqual(0, _console.Lines.Count);
            Assert.IsTrue(array[0] == 3);
        }

        [TestMethod]
        public async Task UndoRedo()
        {
            RegisterProperty(x => x.Name)
                .EnableRecording();

            RegisterProperty(x => x.LastName)
                .EnableRecording();

            var scope1 = BeginScope(sc =>
                            {
                                Name = "Aaaa";
                                LastName = "Bbbb";
                            });

            var scope2 = BeginScope(sc =>
                            {
                                Name = "Cccc";
                                LastName = "Dddd";
                            });

            await scope1.StartAsync();
            await scope2.StartAsync();

            await scope2.UndoAsync();

            Assert.AreEqual("Aaaa", Name);
            Assert.AreEqual("Bbbb", LastName);

            await scope2.RedoAsync();

            Assert.AreEqual("Cccc", Name);
            Assert.AreEqual("Dddd", LastName);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public async Task AttemptToUndoSubScopesThrowsInvalidOperationExceptions()
        {
            _console.Reset();

            RegisterProperty(x => x.Name)
                .EnableRecording();

            RegisterProperty(x => x.LastName)
                .EnableRecording();

            var scope1 = BeginScope(async sc =>
                            {
                                Name = "Aaaa";
                                var scope2 = BeginScope(subScope => Name = "Cccc", sc);
                                await scope2.StartAsync();
                                await scope2.UndoAsync();
                            });
            await scope1.StartAsync();
        }


        #region [ = Birthdate = ]
        public DateTime Birthdate
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }
        #endregion

        #region [ = Age ]
        [DependOn(nameof(Birthdate))]
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

    public class ScopeTaskMoq : ViewModelScopeTaskBase<ScopeTaskMoq, ContactViewModelTest> { }

    public class FunctionObserver<T> : IObserver<T>
    {
        private Action<T> _action;
        private Action<Exception> _error;

        public FunctionObserver(Action<T> action = null, Action<Exception> error = null)
        {
            _action = action;
            _error = error;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            _error?.Invoke(error);
        }

        public void OnNext(T value)
        {
            _action?.Invoke(value);
        }
    }

    public class ConsoleInterceptor : TextWriter
    {
        public string LastText { get; private set; }
        public List<string> Lines { get; } = new List<string>();

        public override Encoding Encoding => Encoding.Default;

        public override void WriteLine(string value)
        {
            LastText = value;
            Lines.Add(value);
        }

        public void Reset()
        {
            Lines.Clear();
            LastText = null;
        }
    }
}
