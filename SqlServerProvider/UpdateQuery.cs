using ClussPro.Base.Data;
using ClussPro.Base.Data.Conditions;
using ClussPro.Base.Data.Query;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ClussPro.SqlServerProvider
{
    public class UpdateQuery : BaseTransactionalQuery, IUpdateQuery
    {
        public Table Table { get; set; }
        public List<FieldValue> FieldValueList { get; set; } = new List<FieldValue>();
        public ICondition Condition { get; set; }

        public void Execute(ITransaction transaction)
        {
            CheckedTransactionExecute(transaction, localTransaction =>
            {
                using (SqlCommand command = new SqlCommand(null, localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    string sql = GetSQL(command.Parameters);

                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            });
        }

        private string GetSQL(SqlParameterCollection parameters)
        {
            StringBuilder sqlBuilder = new StringBuilder("UPDATE ");
            sqlBuilder.Append(ScriptWriters.TableWriter.WriteTable(Table));
            sqlBuilder.Append(" SET ");

            bool first = true;
            foreach(FieldValue fieldValue in FieldValueList)
            {
                if (!first)
                {
                    sqlBuilder.Append(", ");
                }

                first = false;

                string parameterNumber = parameters.Count.ToString();

                sqlBuilder.Append($"[{fieldValue.FieldName}]=@{parameterNumber}");
                parameters.Add(new SqlParameter(parameterNumber, fieldValue.Value));
            }

            if (Condition != null)
            {
                sqlBuilder.Append(" WHERE ");
                sqlBuilder.Append(ScriptWriters.ConditionWriter.WriteCondition(Condition, parameters));
            }

            return sqlBuilder.ToString();
        }
    }
}
