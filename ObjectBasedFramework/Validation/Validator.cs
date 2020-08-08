using System;
using System.Linq;

namespace ClussPro.ObjectBasedFramework.Validation
{
    public static class Validator
    {
        public static bool ValidateObject(DataObject objectToValidate)
        {
            Type validationRuleType = typeof(ValidationRule<>);
            bool result = true;
            foreach(Type type in AppDomain
                                    .CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(a => 
                                        a
                                        .GetTypes()
                                        .Where(t => 
                                            t != validationRuleType && 
                                            validationRuleType.IsAssignableFrom(t) && 
                                            t.GetGenericArguments()[0] == objectToValidate.GetType())))
            {
                ValidationRule validationRule = (ValidationRule)Activator.CreateInstance(type);
                result = validationRule.Evaluate(objectToValidate) && result;
            }

            return result;
        }
    }
}
