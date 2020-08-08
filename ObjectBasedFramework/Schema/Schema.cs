using ClussPro.Base.Data;
using ClussPro.Base.Data.Query;
using ClussPro.ObjectBasedFramework.Schema.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.Schema
{
    public class Schema
    {
        private static Schema instance;
        internal static Schema Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Schema();
                }

                return instance;
            }
        }

        private List<SchemaObject> schemaObjects = new List<SchemaObject>();
        private Dictionary<Type, SchemaObject> schemaObjectsByType = new Dictionary<Type, SchemaObject>();

        private Schema()
        {
            foreach(Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(t => t.GetCustomAttribute<TableAttribute>() != null)))
            {
                SchemaObject newSchemaObject = new SchemaObject(type);

                schemaObjects.Add(newSchemaObject);
                schemaObjectsByType.Add(type, newSchemaObject);
            }

            foreach(SchemaObject schemaObject in schemaObjects)
            {
                foreach(Relationship relationship in schemaObject.GetRelationships())
                {
                    relationship.RelatedSchemaObject = schemaObjectsByType[relationship.RelatedObjectType];
                    relationship.ParentKeyField = relationship.RelatedSchemaObject.GetField(relationship.RelationshipAttribute.ParentKeyField) ?? relationship.RelatedSchemaObject.PrimaryKeyField;
                }
            }
        }

        public static SchemaObject GetSchemaObject<T>()
        {
            return GetSchemaObject(typeof(T));
        }

        public static SchemaObject GetSchemaObject(Type type)
        {
            if (!Instance.schemaObjectsByType.ContainsKey(type))
            {
                return null;
            }

            return Instance.schemaObjectsByType[type];
        }

        public static void Deploy()
        {
            ITransaction deploymentTransaction = SQLProviderFactory.GenerateTransaction();

            try
            {
                HashSet<string> schemas = Instance.schemaObjects.Select(schemaObject => schemaObject.SchemaName).ToHashSet();

                foreach (string schema in schemas)
                {
                    ICreateSchema createSchema = SQLProviderFactory.GetCreateSchemaQuery();
                    createSchema.SchemaName = schema;
                    createSchema.Execute(deploymentTransaction);
                }

                foreach(SchemaObject schemaObject in Instance.schemaObjects)
                {
                    ICreateTable createTable = SQLProviderFactory.GetCreateTableQuery();
                    createTable.SchemaName = schemaObject.SchemaName;
                    createTable.TableName = schemaObject.ObjectName;
                    
                    foreach(Field field in schemaObject.GetFields())
                    {
                        FieldSpecification fieldSpec = new FieldSpecification(field.FieldType, field.DataSize, field.DataScale);
                        if (field == schemaObject.PrimaryKeyField)
                        {
                            fieldSpec.IsPrimary = true;
                        }

                        createTable.Columns.Add(field.FieldName, fieldSpec);
                    }

                    createTable.Execute(deploymentTransaction);
                }

                foreach(Relationship relationship in Instance.schemaObjects.SelectMany(so => so.GetRelationships()))
                {
                    string fkName = $"FK{relationship.ParentSchemaObject.ObjectName}_{relationship.RelatedSchemaObject.ObjectName}_{relationship.ForeignKeyField.FieldName}";
                    IAlterTable alterTableQuery = SQLProviderFactory.GetAlterTableQuery();
                    alterTableQuery.Schema = relationship.ParentSchemaObject.SchemaName;
                    alterTableQuery.Table = relationship.ParentSchemaObject.ObjectName;
                    alterTableQuery.AddForeignKey(fkName, relationship.ForeignKeyField.FieldName, relationship.RelatedSchemaObject.SchemaName, relationship.RelatedSchemaObject.ObjectName, relationship.ParentKeyField.FieldName, deploymentTransaction);
                }

                deploymentTransaction.Commit();
            }
            finally
            {
                if (deploymentTransaction.IsActive)
                {
                    deploymentTransaction.Rollback();
                }
            }
        }

        public static void UnDeploy()
        {
            ITransaction undeploymentTransaction = null;

            try
            {
                undeploymentTransaction = SQLProviderFactory.GenerateTransaction();

                foreach(Relationship relationship in Instance.schemaObjects.SelectMany(so => so.GetRelationships()))
                {
                    string fkName = $"FK{relationship.ParentSchemaObject.ObjectName}_{relationship.RelatedSchemaObject.ObjectName}_{relationship.ForeignKeyField.FieldName}";
                    IAlterTable alterTableQuery = SQLProviderFactory.GetAlterTableQuery();
                    alterTableQuery.Schema = relationship.ParentSchemaObject.SchemaName;
                    alterTableQuery.Table = relationship.ParentSchemaObject.ObjectName;
                    alterTableQuery.DropConstraint(fkName, undeploymentTransaction);
                }

                foreach(SchemaObject schemaObject in Instance.schemaObjects)
                {
                    IDropTable dropTable = SQLProviderFactory.GetDropTableQuery();
                    dropTable.Schema = schemaObject.SchemaName;
                    dropTable.Table = schemaObject.ObjectName;
                    dropTable.Execute(undeploymentTransaction);
                }

                HashSet<string> schemas = Instance.schemaObjects.Select(so => so.SchemaName).ToHashSet();
                foreach(string schema in schemas)
                {
                    IDropSchema dropSchema = SQLProviderFactory.GetDropSchemaQuery();
                    dropSchema.Schema = schema;
                    dropSchema.Execute(undeploymentTransaction);
                }

                undeploymentTransaction.Commit();
            }
            finally
            {
                if (undeploymentTransaction != null && undeploymentTransaction.IsActive)
                {
                    undeploymentTransaction.Rollback();
                }
            }
        }
    }
}
