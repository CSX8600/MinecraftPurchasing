using ClussPro.Base.Data;
using ClussPro.Base.Data.Conditions;
using ClussPro.Base.Data.Operand;
using ClussPro.Base.Data.Query;
using ClussPro.Base.Extensions;
using ClussPro.ObjectBasedFramework.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.ObjectBasedFramework.DataSearch
{
    public class Search
    {
        public Type DataObjectType { get; set; }
        public ISearchCondition SearchCondition { get; set; }

        public Search(Type dataObjectType)
        {
            DataObjectType = dataObjectType;
        }

        public Search(Type dataObjectType, ISearchCondition searchCondition) : this(dataObjectType)
        {
            SearchCondition = searchCondition;
        }

        public IEnumerable<DataObject> GetUntypedEditableReader(ITransaction transaction, IEnumerable<string> readOnlyFields = null)
        {
            SchemaObject schemaObject = Schema.Schema.GetSchemaObject(DataObjectType);

            HashSet<string> fields = new HashSet<string>();
            foreach(Schema.Field field in schemaObject.GetFields())
            {
                fields.Add(field.FieldName);
            }

            fields.AddRange(readOnlyFields);

            ISelectQuery selectQuery = GetBaseQuery(schemaObject, fields, out Dictionary<string, string> tableAliasesByFieldPath);

            DataTable table = selectQuery.Execute(transaction);
            FieldInfo isEditableField = DataObjectType.GetField("isEditable", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach(DataRow row in table.Rows)
            {
                DataObject dataObject = (DataObject)Activator.CreateInstance(DataObjectType);
                isEditableField.SetValue(dataObject, true);

                foreach(IGrouping<string, string> fieldByPath in fields.GroupBy(field =>
                                                                    {
                                                                        if (field.Contains("."))
                                                                        {
                                                                            return field.Substring(0, field.LastIndexOf('.'));
                                                                        }

                                                                        return string.Empty;
                                                                    }))
                {
                    DataObject objectToSetValueOn = dataObject;
                    if (fieldByPath.Key.Contains("."))
                    {
                        string[] parts = fieldByPath.Key.Split('.');

                        SchemaObject lastSchemaObject = schemaObject;
                        for(int i = 0; i < parts.Length - 1; i++)
                        {
                            Relationship relationship = lastSchemaObject.GetRelationship(parts[i]);
                            DataObject relatedDataObject = relationship.GetValue(objectToSetValueOn);
                            
                            if (relatedDataObject == null)
                            {
                                relatedDataObject = (DataObject)Activator.CreateInstance(relationship.RelatedObjectType);
                                relationship.SetPrivateDataCallback(objectToSetValueOn, relatedDataObject);
                            }

                            objectToSetValueOn = relatedDataObject;
                            lastSchemaObject = relationship.RelatedSchemaObject;
                        }
                    }

                    string fieldAlias = tableAliasesByFieldPath[fieldByPath.Key];
                    foreach(string field in fieldByPath)
                    {
                        string fieldName = field;
                        if (fieldName.Contains('.'))
                        {
                            fieldName = fieldName.Substring(fieldName.LastIndexOf('.') + 1);
                        }

                        string columnName = $"{fieldAlias}_{fieldName}";
                        object databaseValue = row[columnName];

                        Schema.Field schemaField = schemaObject.GetField(field);
                        schemaField.SetPrivateDataCallback(objectToSetValueOn, databaseValue);
                    }
                }

                yield return dataObject;
            }
        }

        private ISelectQuery GetBaseQuery(SchemaObject thisSchemaObject, IEnumerable<string> fields, out Dictionary<string, string> tableAliasesByFieldPath)
        {
            DataObject dataObject = (DataObject)Activator.CreateInstance(thisSchemaObject.DataObjectType);

            IOrderedEnumerable<string> sortedFields = fields.Where(f => f.Contains(".")).OrderBy(str => str);
            tableAliasesByFieldPath = new Dictionary<string, string>()
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
                for (int i = 0; i < fieldPathParts.Length - 1; i++)
                {
                    string myAlias = tableAliasesByFieldPath[checkedFieldPath];

                    if (!string.IsNullOrEmpty(checkedFieldPath))
                    {
                        checkedFieldPath += ".";
                    }

                    checkedFieldPath += fieldPathParts[i];


                    Relationship relationship = lastSchemaObject.GetRelationship(checkedFieldPath);
                    SchemaObject relatedSchemaObject = relationship.RelatedSchemaObject;
                    DataObject relatedDataObject = (DataObject)Activator.CreateInstance(relatedSchemaObject.DataObjectType);

                    if (tableAliasesByFieldPath.ContainsKey(checkedFieldPath))
                    {
                        lastObject = relatedDataObject;
                        lastSchemaObject = relatedSchemaObject;

                        continue;
                    }

                    tableAliasCounter++;
                    string otherAlias = $"table{tableAliasCounter.ToString("D3")}";
                    tableAliasesByFieldPath.Add(checkedFieldPath, otherAlias);

                    Join join = new Join();
                    join.Table = new Table(relatedSchemaObject.SchemaName, relatedSchemaObject.ObjectName, otherAlias);
                    join.JoinType = Join.JoinTypes.Left;
                    join.Condition = lastObject.GetRelationshipCondition(relationship, myAlias, otherAlias);

                    lastObject = relatedDataObject;
                    lastSchemaObject = relatedSchemaObject;
                }
            }

            foreach(string conditionFieldPath in SearchCondition.GetFieldPaths())
            {
                if (string.IsNullOrEmpty(conditionFieldPath))
                {
                    continue;
                }

                if (tableAliasesByFieldPath.ContainsKey(conditionFieldPath))
                {
                    continue;
                }

                string[] parts = conditionFieldPath.Split('.');
                DataObject lastObject = dataObject;
                SchemaObject lastSchemaObject = thisSchemaObject;
                string workingPath = "";
                foreach(string part in parts)
                {
                    string myAlias = tableAliasesByFieldPath[workingPath];

                    if (!string.IsNullOrEmpty(workingPath))
                    {
                        workingPath += ".";
                    }

                    workingPath += part;
                    Relationship relationship = lastSchemaObject.GetRelationship(part);
                    DataObject relatedObject = relationship.GetValue(lastObject);

                    if (relatedObject == null)
                    {
                        relatedObject = (DataObject)Activator.CreateInstance(relationship.RelatedObjectType);
                    }

                    lastSchemaObject = relationship.RelatedSchemaObject;

                    if (tableAliasesByFieldPath.ContainsKey(workingPath))
                    {
                        lastObject = relatedObject;
                        continue;
                    }

                    string newAlias = $"table{tableAliasCounter.ToString("D3")}";
                    tableAliasCounter++;

                    Join join = new Join();
                    join.JoinType = Join.JoinTypes.Left;
                    join.Table = new Table(lastSchemaObject.SchemaName, lastSchemaObject.ObjectName, newAlias);
                    join.Condition = lastObject.GetRelationshipCondition(relationship, myAlias, newAlias);
                    joinList.Add(join);

                    lastObject = relatedObject;
                }
            }

            ISelectQuery selectQuery = SQLProviderFactory.GetSelectQuery();
            selectQuery.JoinList = joinList;

            foreach (string field in sortedFields)
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

                string alias = tableAliasesByFieldPath[path];

                Select select = new Select() { SelectOperand = (Base.Data.Operand.Field)$"{alias}.{fieldName}", Alias = $"{alias}_{fieldName}" };
                selectQuery.SelectList.Add(select);
            }

            selectQuery.WhereCondition = SearchCondition?.GetCondition(tableAliasesByFieldPath);

            return selectQuery;
        }
    }

    public class Search<T> : Search where T:DataObject
    {
        public Search() : base(typeof(T)) { }
        public Search(ISearchCondition searchCondition) : base(typeof(T), searchCondition) { }

        public IEnumerable<T> GetEditableReader(ITransaction transaction)
        {
            return (IEnumerable<T>)GetUntypedEditableReader(transaction);
        }
    }
}
