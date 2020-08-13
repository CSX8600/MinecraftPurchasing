using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using ClussPro.Base.Data;
using ClussPro.Base.Data.Conditions;
using ClussPro.Base.Data.Operand;
using ClussPro.Base.Data.Query;
using ClussPro.ObjectBasedFramework.DataSearch;
using ClussPro.ObjectBasedFramework.Schema;
using ClussPro.ObjectBasedFramework.Validation;

namespace ClussPro.ObjectBasedFramework
{
    public class DataObject
    {
        private bool isEditable;
        private bool isInsert;

        protected bool IsEditable => isEditable;

        private HashSet<string> retrievedPaths = new HashSet<string>();

        public Errors Errors { get; } = new Errors();

        private List<FKConstraintConflict> fKConstraintConflicts = new List<FKConstraintConflict>();
        public IReadOnlyCollection<FKConstraintConflict> ForeignKeyConstraintConflicts => fKConstraintConflicts;

        public DataObject()
        {
            isEditable = true;
            isInsert = true;
        }

        private DataObject(bool isEditable, bool isInsert)
        {
            this.isEditable = isEditable;
            this.isInsert = isInsert;
        }

        public bool Save(ITransaction transaction = null)
        {
            if (!isEditable)
            {
                throw new System.Data.ReadOnlyException("Attempt to call Save on a Read Only Data Object");
            }

            PreValidate();
            Validate();

            if (Errors.Any())
            {
                return false;
            }

            ITransaction localTransaction = transaction == null ? SQLProviderFactory.GenerateTransaction() : transaction;
            try
            {
                if (!PreSave(localTransaction))
                {
                    return false;
                }

                if (!(isInsert ? SaveInsert(localTransaction) : SaveUpdate(localTransaction)))
                {
                    return false;
                }

                if (!PostSave(localTransaction))
                {
                    return false;
                }

                if (transaction == null)
                {
                    localTransaction.Commit();
                }
            }
            finally
            {
                if (transaction == null && localTransaction.IsActive)
                {
                    localTransaction.Rollback();
                }
            }

            return true;
        }

        public bool Delete(ITransaction transaction = null)
        {
            ITransaction localTransaction = transaction == null ? SQLProviderFactory.GenerateTransaction() : transaction;

            try
            {
                if (!isEditable)
                {
                    throw new System.Data.ReadOnlyException("Attempt to call Delete on a Read Only Data Object");
                }

                PreValidate();
                Validate();

                if (Errors.Any())
                {
                    return false;
                }

                fKConstraintConflicts = GetFKConstraintConflicts(localTransaction);
                if (fKConstraintConflicts.Any())
                {
                    HandleFKConstraintConflicts(fKConstraintConflicts, localTransaction);
                    if (fKConstraintConflicts.Any())
                    {
                        return false;
                    }
                }

                if (!PreDelete(localTransaction))
                {
                    return false;
                }

                SchemaObject schemaObject = Schema.Schema.GetSchemaObject(GetType());

                IDeleteQuery deleteQuery = SQLProviderFactory.GetDeleteQuery();
                deleteQuery.Table = new Table(schemaObject.SchemaName, schemaObject.ObjectName);
                deleteQuery.Condition = new Condition()
                {
                    Left = (Base.Data.Operand.Field)schemaObject.PrimaryKeyField.FieldName,
                    ConditionType = Condition.ConditionTypes.Equal,
                    Right = new Literal(schemaObject.PrimaryKeyField.GetValue(this))
                };

                deleteQuery.Execute(localTransaction);

                if (!PostDelete(localTransaction))
                {
                    return false;
                }

                if (transaction == null)
                {
                    localTransaction.Commit();
                }
            }
            finally
            {
                if (transaction == null && localTransaction.IsActive)
                {
                    localTransaction.Rollback();
                }
            }

            return true;
        }

        protected virtual List<FKConstraintConflict> GetFKConstraintConflicts(ITransaction transaction)
        {
            List<FKConstraintConflict> fKConstraintConflicts = new List<FKConstraintConflict>();
            SchemaObject mySchemaObject = Schema.Schema.GetSchemaObject(GetType());
            Schema.Field primaryKeyField = mySchemaObject.PrimaryKeyField;

            foreach(RelationshipList relationshipList in Schema.Schema.GetSchemaObject(GetType()).GetRelationshipLists())
            {
                SchemaObject relatedSchemaObject = Schema.Schema.GetSchemaObject(relationshipList.RelatedObjectType);
                Schema.Field relatedField = relatedSchemaObject.GetField(relationshipList.ForeignKeyName);

                ISelectQuery relationshipListQuery = SQLProviderFactory.GetSelectQuery();
                relationshipListQuery.SelectList.Add(relatedSchemaObject.PrimaryKeyField.FieldName);
                relationshipListQuery.Table = new Table(relatedSchemaObject.SchemaName, relatedSchemaObject.ObjectName);
                relationshipListQuery.WhereCondition = new Condition()
                {
                    Left = (Base.Data.Operand.Field)relatedField.FieldName,
                    ConditionType = Condition.ConditionTypes.Equal,
                    Right = new Literal(primaryKeyField.GetValue(this))
                };

                DataTable results = relationshipListQuery.Execute(transaction);
                foreach(DataRow row in results.Rows)
                {
                    FKConstraintConflict fKConstraintConflict = new FKConstraintConflict();
                    fKConstraintConflict.ConflictType = relationshipList.RelatedObjectType;
                    fKConstraintConflict.ForeignKey = row[relatedSchemaObject.PrimaryKeyField.FieldName] as long?;
                    fKConstraintConflict.ForeignKeyName = relatedField.FieldName;
                    fKConstraintConflict.ActionType = relationshipList.AutoDeleteReferences ?
                                                        FKConstraintConflict.ActionTypes.AutoDeleteReference :
                                                        relationshipList.AutoRemoveReferences ?
                                                            FKConstraintConflict.ActionTypes.AutoRemoveReference :
                                                            FKConstraintConflict.ActionTypes.Conflict;

                    fKConstraintConflicts.Add(fKConstraintConflict);
                }
            }

            return fKConstraintConflicts;
        }

        private void HandleFKConstraintConflicts(List<FKConstraintConflict> conflicts, ITransaction transaction)
        {
            HashSet<FKConstraintConflict> handled = new HashSet<FKConstraintConflict>();
            IEnumerable<IGrouping<Type, FKConstraintConflict>> constraintsByType = conflicts.GroupBy(conf => conf.ConflictType);

            foreach(IGrouping<Type, FKConstraintConflict> constraintGroup in constraintsByType)
            {
                SchemaObject foreignSchemaObject = Schema.Schema.GetSchemaObject(constraintGroup.Key);
                foreach (IGrouping<string, FKConstraintConflict> constraintGroupByField in constraintGroup.GroupBy(fk => fk.ForeignKeyName))
                {
                    List<long> autoDeleteReferenceKeys = new List<long>();
                    List<long> autoRemoveReferenceKeys = new List<long>();

                    foreach (FKConstraintConflict fKConstraintConflict in constraintGroup)
                    {
                        switch (fKConstraintConflict.ActionType)
                        {
                            case FKConstraintConflict.ActionTypes.AutoDeleteReference:
                                autoDeleteReferenceKeys.Add(fKConstraintConflict.ForeignKey.Value);
                                handled.Add(fKConstraintConflict);
                                break;
                            case FKConstraintConflict.ActionTypes.AutoRemoveReference:
                                autoRemoveReferenceKeys.Add(fKConstraintConflict.ForeignKey.Value);
                                handled.Add(fKConstraintConflict);
                                break;
                        }
                    }

                    if (autoDeleteReferenceKeys.Any())
                    {
                        IDeleteQuery deleteQuery = SQLProviderFactory.GetDeleteQuery();
                        deleteQuery.Table = new Table(foreignSchemaObject.SchemaName, foreignSchemaObject.ObjectName);
                        deleteQuery.Condition = new Condition()
                        {
                            Left = (Base.Data.Operand.Field)foreignSchemaObject.PrimaryKeyField.FieldName,
                            ConditionType = Condition.ConditionTypes.List,
                            Right = (CSV<long>)autoDeleteReferenceKeys
                        };

                        deleteQuery.Execute(transaction);
                    }

                    if (autoRemoveReferenceKeys.Any())
                    {
                        IUpdateQuery updateQuery = SQLProviderFactory.GetUpdateQuery();
                        updateQuery.Table = new Table(foreignSchemaObject.SchemaName, foreignSchemaObject.ObjectName);
                        updateQuery.FieldValueList.Add(new FieldValue()
                        {
                            FieldName = constraintGroupByField.Key,
                            Value = null
                        });
                        updateQuery.Condition = new Condition()
                        {
                            Left = (Base.Data.Operand.Field)foreignSchemaObject.PrimaryKeyField.FieldName,
                            ConditionType = Condition.ConditionTypes.List,
                            Right = (CSV<long>)autoRemoveReferenceKeys
                        };

                        updateQuery.Execute(transaction);
                    }
                }
            }

            conflicts.RemoveAll(fk => handled.Contains(fk));
        }

        public bool Validate()
        {
            return Validator.ValidateObject(this);
        }

        private bool SaveInsert(ITransaction transaction)
        {
            SchemaObject schemaObject = Schema.Schema.GetSchemaObject(GetType());

            IInsertQuery insertQuery = SQLProviderFactory.GetInsertQuery();
            insertQuery.Table = new Table(schemaObject.SchemaName, schemaObject.ObjectName);
            
            foreach(Schema.Field field in schemaObject.GetFields())
            {
                if (field == schemaObject.PrimaryKeyField) { continue; }

                FieldValue fieldValue = new FieldValue();
                fieldValue.FieldName = field.FieldName;
                fieldValue.Value = field.GetValue(this);
                insertQuery.FieldValueList.Add(fieldValue);
            }

            long? primaryKey = insertQuery.Execute(transaction);
            if (primaryKey != null)
            {
                schemaObject.PrimaryKeyField.SetPrivateDataCallback(this, primaryKey);
                isInsert = false;
                return true;
            }

            return false;
        }

        private bool SaveUpdate(ITransaction transaction)
        {
            SchemaObject schemaObject = Schema.Schema.GetSchemaObject(GetType());

            IUpdateQuery updateQuery = SQLProviderFactory.GetUpdateQuery();
            updateQuery.Table = new Table(schemaObject.SchemaName, schemaObject.ObjectName);

            foreach(Schema.Field field in schemaObject.GetFields())
            {
                FieldValue fieldValue = new FieldValue();
                fieldValue.FieldName = field.FieldName;
                fieldValue.Value = field.GetValue(this);
                updateQuery.FieldValueList.Add(fieldValue);
            }

            updateQuery.Condition = new Condition()
            {
                Left = (Base.Data.Operand.Field)schemaObject.PrimaryKeyField.FieldName,
                ConditionType = Condition.ConditionTypes.Equal,
                Right = new Literal(schemaObject.PrimaryKeyField.GetValue(this))
            };

            updateQuery.Execute(transaction);

            return true;
        }

        protected virtual void PreValidate() { }
        protected virtual bool PreSave(ITransaction transaction) { return true; }
        protected virtual bool PostSave(ITransaction transaction) { return true; }
        protected virtual bool PreDelete(ITransaction transaction) { return true; }
        protected virtual bool PostDelete(ITransaction transaction) { return true; }

        public static TDataObject GetReadOnlyByPrimaryKey<TDataObject>(long? primaryKey, ITransaction transaction, IEnumerable<string> fields) where TDataObject: DataObject
        {
            DataObject dataObject = Activator.CreateInstance<TDataObject>();
            dataObject.isEditable = false;

            SchemaObject thisSchemaObject = Schema.Schema.GetSchemaObject<TDataObject>();

            IOrderedEnumerable<string> sortedFields = fields.Where(f => f.Contains(".")).OrderBy(str => str);
            Dictionary<string, string> tableAliasesForFieldPath = new Dictionary<string, string>()
            {
                { "", "table000" }
            };
            int tableAliasCounter = 1;

            List<Join> joinList = new List<Join>();
            foreach (string fieldPath in sortedFields.Where(f => f.Contains(".")).Select(f => f.Substring(0, f.LastIndexOf("."))))
            {
                string[] fieldPathParts = fieldPath.Split('.');

                string checkedFieldPath = "";
                DataObject lastObject = dataObject;
                SchemaObject lastSchemaObject = thisSchemaObject;
                for(int i = 0; i < fieldPathParts.Length - 1; i++)
                {
                    string myAlias = tableAliasesForFieldPath[checkedFieldPath];

                    if (!string.IsNullOrEmpty(checkedFieldPath))
                    {
                        checkedFieldPath += ".";
                    }

                    checkedFieldPath += fieldPathParts[i];

                    if (tableAliasesForFieldPath.ContainsKey(checkedFieldPath))
                    {
                        continue;
                    }

                    Relationship relationship = lastSchemaObject.GetRelationship(checkedFieldPath);
                    SchemaObject relatedSchemaObject = relationship.RelatedSchemaObject;

                    DataObject relatedDataObject = relationship.GetValue(lastObject);
                    if (relatedDataObject == null)
                    {
                        relatedDataObject = (DataObject)Activator.CreateInstance(relationship.RelatedObjectType);
                        relatedDataObject.isEditable = false;
                        relationship.SetPrivateDataCallback(lastObject, relatedDataObject);
                    }

                    tableAliasCounter++;
                    string otherAlias = $"table{tableAliasCounter.ToString("D3")}";
                    tableAliasesForFieldPath.Add(checkedFieldPath, otherAlias);

                    Join join = new Join();
                    join.Table = new Table(relatedSchemaObject.SchemaName, relatedSchemaObject.ObjectName, otherAlias);
                    join.JoinType = Join.JoinTypes.Left;
                    join.Condition = lastObject.GetRelationshipCondition(relationship, myAlias, otherAlias);

                    lastObject = relatedDataObject;
                    lastSchemaObject = relatedSchemaObject;
                }
            }

            ISelectQuery selectQuery = SQLProviderFactory.GetSelectQuery();
            selectQuery.JoinList = joinList;
            
            foreach(string field in sortedFields)
            {
                string path = "";
                string fieldName = "";
                if (field.Contains('.'))
                {
                    path = field.Substring(0, field.LastIndexOf('.'));
                    fieldName = field.Substring(field.LastIndexOf('.') + 1);
                }
                else
                {
                    fieldName = field;
                }

                string alias = tableAliasesForFieldPath[path];

                Select select = new Select() { SelectOperand = (Base.Data.Operand.Field)$"{alias}.{fieldName}", Alias = $"{alias}_{fieldName}" };
                selectQuery.SelectList.Add(select);
            }

            selectQuery.WhereCondition = new Condition()
            {
                Left = (Base.Data.Operand.Field)("table000_" + thisSchemaObject.PrimaryKeyField.FieldName),
                ConditionType = Condition.ConditionTypes.Equal,
                Right = new Literal(primaryKey)
            };

            DataTable dataTable = selectQuery.Execute(transaction);
            if (dataTable.Rows.Count <= 0)
            {
                return null;
            }

            DataRow row = dataTable.Rows[0];
            foreach(string field in sortedFields)
            {
                DataObject lastObject = dataObject;
                SchemaObject lastSchemaObject = thisSchemaObject;
                if (field.Contains('.'))
                {
                    string[] parts = field.Split('.');
                    for(int i = 0; i < parts.Length - 1; i++)
                    {
                        lastObject.retrievedPaths.Add(parts[i]);
                        Relationship relationship = lastSchemaObject.GetRelationship(parts[i]);

                        lastObject = relationship.GetValue(lastObject);
                        lastSchemaObject = relationship.RelatedSchemaObject;
                    }
                }

                string path = "";
                string pathField = "";
                if (field.Contains('.'))
                {
                    path = field.Substring(0, field.LastIndexOf('.'));
                    pathField = field.Substring(field.LastIndexOf('.'));
                }
                else
                {
                    pathField = field;
                }

                string alias = tableAliasesForFieldPath[path];
                object value = row[$"{alias}_{pathField}"];
                lastSchemaObject.GetField(pathField).SetPrivateDataCallback(lastObject, value);
                lastObject.retrievedPaths.Add(pathField);
            }

            return (TDataObject)dataObject;
        }

        public static DataObject GetEditableByPrimaryKey(Type dataObjectType, long? primaryKey, ITransaction transaction, IEnumerable<string> readOnlyFields)
        {
            DataObject dataObject = (DataObject)Activator.CreateInstance(dataObjectType);
            dataObject.isInsert = false;
            SchemaObject schemaObject = Schema.Schema.GetSchemaObject(dataObjectType);

            int tableAliasCounter = 1;
            Dictionary<string, string> tableAliasesByFieldPath = new Dictionary<string, string>()
            {
                { "", "table000" }
            };

            ISelectQuery selectQuery = SQLProviderFactory.GetSelectQuery();
            foreach (Schema.Field field in schemaObject.GetFields())
            {
                Select select = new Select() { SelectOperand = (Base.Data.Operand.Field)$"table000.{field.FieldName}", Alias = $"table000_{field.FieldName}" };
                selectQuery.SelectList.Add(select);
            }

            IOrderedEnumerable<string> sortedFields = readOnlyFields.Where(f => f.Contains('.')).OrderBy(f => f);
            foreach (string readOnlyField in sortedFields)
            {
                string[] parts = readOnlyField.Split('.');
                string checkedPath = "";
                DataObject lastDataObject = dataObject;
                SchemaObject lastSchemaObject = schemaObject;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string myAlias = tableAliasesByFieldPath[checkedPath];

                    if (!string.IsNullOrEmpty(checkedPath))
                    {
                        checkedPath += ".";
                    }

                    checkedPath += parts[i];
                    if (tableAliasesByFieldPath.ContainsKey(checkedPath))
                    {
                        continue;
                    }

                    tableAliasCounter++;
                    string newAlias = $"table{tableAliasCounter.ToString("D3")}";
                    tableAliasesByFieldPath.Add(checkedPath, newAlias);
                    Relationship relationship = lastSchemaObject.GetRelationship(parts[i]);
                    DataObject relatedDataObject = relationship.GetValue(lastDataObject);

                    if (relatedDataObject == null)
                    {
                        relatedDataObject = (DataObject)Activator.CreateInstance(relationship.RelatedObjectType);
                        relatedDataObject.isEditable = false;
                        relationship.SetPrivateDataCallback(lastDataObject, relatedDataObject);
                    }

                    Join join = new Join();
                    join.JoinType = Join.JoinTypes.Left;
                    join.Table = new Table(relationship.RelatedSchemaObject.SchemaName, relationship.RelatedSchemaObject.ObjectName, newAlias);
                    join.Condition = lastDataObject.GetRelationshipCondition(relationship, myAlias, newAlias);
                    selectQuery.JoinList.Add(join);

                    lastDataObject = relatedDataObject;
                    lastSchemaObject = relationship.RelatedSchemaObject;
                }

                string path = readOnlyField.Substring(0, readOnlyField.LastIndexOf('.'));
                string pathField = readOnlyField.Substring(readOnlyField.LastIndexOf('.') + 1);
                string finalAlias = tableAliasesByFieldPath[path];

                Select readOnlySelect = new Select() { SelectOperand = (Base.Data.Operand.Field)$"{path}.{pathField}", Alias = $"{path}_{pathField}" };
                selectQuery.SelectList.Add(readOnlySelect);
            }

            selectQuery.WhereCondition = new Condition()
            {
                Left = (Base.Data.Operand.Field)$"table000_{schemaObject.PrimaryKeyField.FieldName}",
                ConditionType = Condition.ConditionTypes.Equal,
                Right = new Literal(primaryKey)
            };

            DataTable dataTable = selectQuery.Execute(transaction);
            if (dataTable.Rows.Count <= 0)
            {
                return null;
            }
            DataRow row = dataTable.Rows[0];

            foreach (Schema.Field field in schemaObject.GetFields())
            {
                object value = row[$"table000_{field.FieldName}"];
                field.SetPrivateDataCallback(dataObject, value);
                dataObject.retrievedPaths.Add(field.FieldName);
            }

            foreach (string readOnlyField in sortedFields)
            {
                string path = readOnlyField.Substring(0, readOnlyField.LastIndexOf('.'));
                string pathField = readOnlyField.Substring(readOnlyField.LastIndexOf('.') + 1);
                string alias = tableAliasesByFieldPath[path];

                object value = row[$"{alias}_{pathField}"];

                DataObject lastObject = dataObject;
                SchemaObject lastSchemaObject = schemaObject;
                string[] parts = path.Split('.');
                for (int i = 0; i < parts.Length; i++)
                {
                    lastObject.retrievedPaths.Add(parts[i]);
                    Relationship relationship = lastSchemaObject.GetRelationship(parts[i]);
                    lastObject = relationship.GetValue(lastObject);
                    lastSchemaObject = relationship.RelatedSchemaObject;
                }

                Schema.Field field = lastSchemaObject.GetField(pathField);
                field.SetPrivateDataCallback(lastObject, value);
                lastObject.retrievedPaths.Add(pathField);
            }

            return dataObject;
        }

        public static TDataObject GetEditableByPrimaryKey<TDataObject>(long? primaryKey, ITransaction transaction, IEnumerable<string> readOnlyFields) where TDataObject:DataObject
        {
            return (TDataObject)GetEditableByPrimaryKey(typeof(TDataObject), primaryKey, transaction, readOnlyFields);
        }

        public virtual ICondition GetRelationshipCondition(Relationship relationship, string myAlias, string otherAlias)
        {
            return new Condition()
            {
                Left = (Base.Data.Operand.Field)$"{myAlias}.{relationship.ForeignKeyField}",
                ConditionType = Condition.ConditionTypes.Equal,
                Right = (Base.Data.Operand.Field)$"{otherAlias}.{relationship.RelatedSchemaObject.PrimaryKeyField}"
            };
        }

        public void Copy(DataObject destination)
        {
            if (destination.GetType() != GetType())
            {
                throw new InvalidOperationException("Cannot copy to Data Object of different type");
            }

            SchemaObject schemaObject = Schema.Schema.GetSchemaObject(GetType());
            
            foreach(Schema.Field field in schemaObject.GetFields().Where(f => f != schemaObject.PrimaryKeyField))
            {
                field.SetPrivateDataCallback(destination, field.GetPrivateDataCallback(this));
            }
        }

        protected void CheckGet([CallerMemberName]string fieldName = "")
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("Could not determine field name for get validation");
            }

            if (!isEditable && !retrievedPaths.Contains(fieldName))
            {
                throw new InvalidOperationException($"The field {fieldName} was not retrieved");
            }
        }

        protected void CheckSet([CallerMemberName]string fieldName = "")
        {
            if (!isEditable)
            {
                throw new InvalidOperationException($"Cannot call set on field {fieldName} - this is not an editable data object");
            }
        }

        public class FKConstraintConflict
        {
            public long? ForeignKey { get; set; }
            public Type ConflictType { get; set; }
            public string ForeignKeyName { get; set; }

            public ActionTypes ActionType { get; set; } = ActionTypes.Conflict;

            public enum ActionTypes
            {
                AutoRemoveReference,
                AutoDeleteReference,
                Conflict
            }
        }
    }
}
