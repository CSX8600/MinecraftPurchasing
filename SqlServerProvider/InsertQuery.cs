using ClussPro.Base.Data;
using ClussPro.Base.Data.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.SqlServerProvider
{
    public class InsertQuery : BaseTransactionalQuery, IInsertQuery
    {

        public Table Table { get; set; }
        public List<FieldValue> FieldValueList { get; set; } = new List<FieldValue>();

        public long? Execute(ITransaction transaction)
        {
            return CheckedTransactionExecuteWithResult(transaction, localTransaction =>
            {
                long? primaryKey = null;
                using (SqlCommand command = new SqlCommand(null, localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    command.CommandText = GetSQL(command.Parameters);
                    primaryKey = Convert.ToInt64(command.ExecuteScalar());
                }

                return primaryKey;
            }) as long?;
        }

        private string GetSQL(SqlParameterCollection parameters)
        {
            StringBuilder sqlBuilder = new StringBuilder("INSERT INTO ");
            sqlBuilder.Append(ScriptWriters.TableWriter.WriteTable(Table));
            sqlBuilder.Append(" (");

            bool first = true;
            foreach(FieldValue fieldValue in FieldValueList)
            {
                if (!first)
                {
                    sqlBuilder.Append(", ");
                }

                first = false;

                sqlBuilder.Append($"[{fieldValue.FieldName}]");
            }

            sqlBuilder.Append(") VALUES (");

            first = true;
            foreach(FieldValue fieldValue in FieldValueList)
            {
                if (!first)
                {
                    sqlBuilder.Append(", ");
                }

                first = false;

                string parameterNumber = parameters.Count.ToString();
                sqlBuilder.Append($"@{parameterNumber}");
                parameters.AddWithValue(parameterNumber, fieldValue.Value);
            }

            sqlBuilder.Append("); SELECT SCOPE_IDENTITY() AS [SCOPE_IDENTITY]");

            return sqlBuilder.ToString();
        }
    }
}
