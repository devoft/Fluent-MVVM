namespace devoft.ClientModel.Validation
{
    public interface IValidator
    {
        string Validate(object value, string message = null);
    }
}
