namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Defines validators to be used on Validate methods:
    /// e.g. RegisterProperty(...).Validate<[IValidator]>();
    /// </summary>
    public interface IValidator
    {
        string Validate(object value, string message = null);
    }
}
