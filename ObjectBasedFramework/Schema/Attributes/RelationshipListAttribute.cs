using System;

namespace ClussPro.ObjectBasedFramework.Schema.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RelationshipListAttribute : Attribute
    {
        public Guid RelationshipListID { get; set; }
        public string ForeignKeyName { get; set; }
        public bool AutoDeleteReferences { get; set; }
        public bool AutoRemoveReferences { get; set; }

        public RelationshipListAttribute(string RelationshipListID, string ForeignKeyName)
        {
            this.RelationshipListID = new Guid(RelationshipListID);
            this.ForeignKeyName = ForeignKeyName;
        }
    }
}
