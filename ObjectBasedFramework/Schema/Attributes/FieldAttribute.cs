using ClussPro.Base.Data;
using System;

namespace ClussPro.ObjectBasedFramework.Schema.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public Guid FieldID { get; set; }
        public FieldSpecification.FieldTypes? FieldType { get; set; }
        public int DataSize { get; set; } = -1;
        public int DataScale { get; set; }
        public FieldAttribute(string FieldID)
        {
            this.FieldID = new Guid(FieldID);
        }
    }
}
