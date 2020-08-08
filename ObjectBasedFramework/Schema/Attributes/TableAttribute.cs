using System;

namespace ClussPro.ObjectBasedFramework.Schema.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public Guid TableID { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }

        public TableAttribute(string TableID)
        {
            this.TableID = new Guid(TableID);
        }
    }
}
