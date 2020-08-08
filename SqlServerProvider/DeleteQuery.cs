using ClussPro.Base.Data;
using ClussPro.Base.Data.Conditions;
using ClussPro.Base.Data.Query;
using System.Data.SqlClient;
using System.Text;

namespace ClussPro.SqlServerProvider
{
    public class DeleteQuery : BaseTransactionalQuery, IDeleteQuery
    {
        public Table Table { get; set; }
        public ICondition Condition { get; set; }

        public void Execute(ITransaction transaction)
        {
            CheckedTransactionExecute(transaction, (localTransaction) =>
            {
                using (SqlCommand command = new SqlCommand(null, localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    command.CommandText = GetSQL(command.Parameters);
                    command.ExecuteNonQuery();
                }
            });
        }

        private string GetSQL(SqlParameterCollection parameters)
        {
            StringBuilder sqlBuilder = new StringBuilder("DELETE FROM ");
            sqlBuilder.Append(ScriptWriters.TableWriter.WriteTable(Table));

            if (Condition != null)
            {
                sqlBuilder.Append(" WHERE ");
                sqlBuilder.Append(ScriptWriters.ConditionWriter.WriteCondition(Condition, parameters));
            }

            return sqlBuilder.ToString();
        }
    }
}
