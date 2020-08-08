using System;

namespace ClussPro.ObjectBasedFramework.Validation
{
    public abstract class ValidationRule
    {
        public Guid ValidationRuleID { get; set; }
        public string Field { get; set; }
        public string Message { get; set; }
        public Conditions.Condition Condition { get; set; }

        public abstract bool Evaluate(object objectToValidate);
    }

    public class ValidationRule<T> : ValidationRule where T : DataObject
    {
        public override bool Evaluate(object objectToValidate)
        {
            if (!(objectToValidate is T))
            {
                throw new InvalidCastException("Expected type of " + typeof(IErrorContainer).Name + " during validation, got " + objectToValidate.GetType().Name);
            }

            T theObject = (T)objectToValidate;
            bool result = Condition.Evaluate(theObject);

            if (!result)
            {
                theObject.Errors.Add(Field, Message);
            }

            return result;
        }
    }
}
