using ClussPro.Base.Data.Query;
using System.Data.SqlClient;

namespace ClussPro.SqlServerProvider
{
    public class DropSchemaQuery : BaseTransactionalQuery, IDropSchema
    {
        public string Schema { get; set; }

        public void Execute(ITransaction transaction)
        {
            CheckedTransactionExecute(transaction, localTransaction =>
            {
                string sql = $"DROP SCHEMA [{Schema}]";

                using (SqlCommand command = new SqlCommand(sql, localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    command.ExecuteNonQuery();
                }
            });
        }
    }
}
