using ClussPro.ObjectBasedFramework.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Validation.Conditions
{
    public abstract class Condition
    {
        protected List<Expression> Expressions;

        public bool Evaluate(DataObject dataObject)
        {
            List<string> additionalFields = Expressions.SelectMany(exp => exp.GetAdditionalFieldsForDataObject()).ToList();

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            SchemaObject schemaObject = Schema.Schema.GetSchemaObject(dataObject.GetType());
            DataObject dataObjectForValidation = DataObject.GetEditableByPrimaryKey(dataObject.GetType(), schemaObject.PrimaryKeyField.GetValue(dataObject) as long?, null, additionalFields);
            dataObject.Copy(dataObjectForValidation);

            parameters[schemaObject.ObjectName] = dataObjectForValidation;

            return Evaluate(parameters);
        }

        protected abstract bool Evaluate(Dictionary<string, object> parameters);
    }
}
