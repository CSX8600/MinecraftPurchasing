using System.Collections.Generic;

namespace ClussPro.ObjectBasedFramework.Validation
{
    public interface IValidationDefinition
    {
        string Schema { get; }
        string Object { get; }
        IEnumerable<ValidationRule> ValidationRules { get; }
    }
}
