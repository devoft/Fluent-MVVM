namespace devoft.ClientModel.Validation
{
    public class NotEmpty : IValidator
    {
        public string Validate(object value, string message = null)
            => string.IsNullOrWhiteSpace(value + "") ? message ?? "Cannot be empty" : null;
    }
}
