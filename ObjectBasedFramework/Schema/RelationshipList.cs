using ClussPro.ObjectBasedFramework.Schema.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Schema
{
    public class RelationshipList
    {
        internal RelationshipList() { }
        
        public string ForeignKeyName { get; internal set; }
        public SchemaObject ParentSchemaObject { get; internal set; }
        public string RelationshipListName { get; internal set; }
        public Type RelatedObjectType { get; internal set; }
        public bool AutoDeleteReferences { get; internal set; }
        public bool AutoRemoveReferences { get; internal set; }

        internal Func<object, object> GetPrivateDataCallback { get; set; }
        internal Action<DataObject, object> SetPrivateDataCallback { get; set; }

        public IReadOnlyCollection<DataObject> GetValue(DataObject dataObject)
        {
            return (IReadOnlyCollection<DataObject>)GetPrivateDataCallback(dataObject);
        }
    }
}
