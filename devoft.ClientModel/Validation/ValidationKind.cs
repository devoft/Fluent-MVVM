namespace devoft.ClientModel.Validation
{
    /// <summary>
    /// Indicates the kind of validation results;
    /// </summary>
    public enum ValidationKind
    {
        /// <summary>
        /// Validation success
        /// </summary>
        Success = 0,
        /// <summary>
        /// Validation success with observations
        /// </summary>
        Information = 2,
        /// <summary>
        /// Validation success, but attention from user is required because other validations could not
        /// </summary>
        Warning = 1,
        /// <summary>
        /// Validation fails. Value must be changed by a right one
        /// </summary>
        Error = 3
    }
}
