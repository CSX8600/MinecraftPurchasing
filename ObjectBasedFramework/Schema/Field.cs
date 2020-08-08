using ClussPro.Base.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Schema
{
    public class Field
    {
        internal Field() { }

        public SchemaObject ParentSchemaObject { get; internal set; }
        public FieldSpecification.FieldTypes FieldType { get; set; }
        public int DataSize { get; set; }
        public int DataScale { get; set; }
        public string FieldName { get; internal set; }

        internal Func<object, object> GetPrivateDataCallback { get; set; }

        internal Action<DataObject, object> SetPrivateDataCallback { get; set; }

        public object GetValue(DataObject dataObject)
        {
            return GetPrivateDataCallback(dataObject);
        }
    }
}
