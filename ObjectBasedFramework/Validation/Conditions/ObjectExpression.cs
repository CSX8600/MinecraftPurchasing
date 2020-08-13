using ClussPro.ObjectBasedFramework.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Validation.Conditions
{
    public class ObjectExpression<T> : Expression where T:DataObject
    {
        private string field;
        public ObjectExpression(string field)
        {
            if (!field.Contains("."))
            {
                throw new ArgumentException("Object Expression field needs to contain a path.");
            }

            this.field = field;
        }

        public override List<string> GetAdditionalFieldsForDataObject()
        {
            return new List<string>() { field.Substring(field.IndexOf(".") + 1) };
        }

        public override object Evaluate(Dictionary<string, object> parameters)
        {
            string[] parts = field.Split('.');
            string objectName = parts[0];
            if (!parameters.ContainsKey(objectName))
            {
                throw new KeyNotFoundException("Object with name " + objectName + " not found");
            }

            object latestObject = parameters[objectName];
            for(int i = 1; i < parts.Length; i++)
            {
                SchemaObject schemaObject = Schema.Schema.GetSchemaObject(latestObject.GetType());
                Field field = schemaObject.GetField(parts[i]);

                if (field != null)
                {
                    return field.GetValue((DataObject)latestObject);
                }

                Relationship relationship = schemaObject.GetRelationship(parts[i]);
                if (relationship == null)
                {
                    throw new ArgumentException("Could not determine path during Object Expression evaluation");
                }

                latestObject = relationship.GetPrivateDataCallback(latestObject);
            }

            return null;
        }
    }
}
