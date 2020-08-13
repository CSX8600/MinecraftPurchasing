using ClussPro.Base.Extensions;
using ClussPro.ObjectBasedFramework.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClussPro.ObjectBasedFramework.Validation
{
    public static class Validator
    {
        private static bool definitionsLoaded = false;
        private static Dictionary<Type, List<IValidationDefinition>> validationDefinitionsByDataObjectType = new Dictionary<Type, List<IValidationDefinition>>();
        public static bool ValidateObject<T>(T objectToValidate) where T : DataObject
        {
            if (!definitionsLoaded && !LoadDefinitions())
            {
                // We can't guarantee definitions are loaded, so we have to fail validation
                return false;
            }

            List<IValidationDefinition> validationDefinitions = validationDefinitionsByDataObjectType[typeof(T)];

            bool result = true;
            foreach (ValidationRule validationDefinition in validationDefinitions.SelectMany(vd => vd.ValidationRules))
            {
                result = result && validationDefinition.Condition.Evaluate(objectToValidate);
            }

            return result;
        }

        private static bool LoadDefinitions()
        {
            Type validationRuleType = typeof(IValidationDefinition);
            foreach (Type type in AppDomain
                                    .CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(a =>
                                        a
                                        .GetTypes()
                                        .Where(t =>
                                            t != validationRuleType &&
                                            validationRuleType.IsAssignableFrom(t))))
            {
                IValidationDefinition validationDefinition = (IValidationDefinition)Activator.CreateInstance(type);
                SchemaObject schemaObject = Schema.Schema.GetSchemaObject(validationDefinition.Schema, validationDefinition.Object);

                validationDefinitionsByDataObjectType.GetOrSet(schemaObject.DataObjectType, () => new List<IValidationDefinition>()).Add(validationDefinition);
            }

            definitionsLoaded = true;
            return true;
        }
    }
}
