using System;
using devoft.ClientModel.Validation;

namespace devoft.ClientModel
{
    public interface IViewModelProperty
    {
        ValidationResultCollection ValidationResults { get; }
    }
}
