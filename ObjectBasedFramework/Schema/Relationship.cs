using ClussPro.ObjectBasedFramework.Schema.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Schema
{
    public class Relationship
    {
        internal Relationship() { }

        internal RelationshipAttribute RelationshipAttribute { get; set; }
        public SchemaObject ParentSchemaObject { get; internal set; }
        public SchemaObject RelatedSchemaObject { get; internal set; }
        public string RelationshipName { get; internal set; }
        public Type RelatedObjectType { get; internal set; }
        public Field ForeignKeyField { get; internal set; }
        public Field ParentKeyField { get; internal set; }

        internal Func<object, object> GetPrivateDataCallback { get; set; }
        internal Action<DataObject, object> SetPrivateDataCallback { get; set; }

        public DataObject GetValue(DataObject dataObject)
        {
            return (DataObject)GetPrivateDataCallback(dataObject);
        }
    }
}
