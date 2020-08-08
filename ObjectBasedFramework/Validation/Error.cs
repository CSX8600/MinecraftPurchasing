using System;

namespace ClussPro.ObjectBasedFramework.Validation
{
    public class Error
    {
        public Guid ValidationRuleID { get; set; }
        public string FieldName { get; set; }
        public string Message { get; set; }
    }
}
