using ClussPro.Base.Data.Query;
using System.Data.SqlClient;

namespace ClussPro.SqlServerProvider
{
    public class DropTableQuery : BaseTransactionalQuery, IDropTable
    {
        public string Schema { get; set; }
        public string Table { get; set; }

        public void Execute(ITransaction transaction)
        {
            CheckedTransactionExecute(transaction, localTransaction =>
            {
                string sql = $"DROP TABLE [{Schema}].[{Table}]";

                using (SqlCommand sqlCommand = new SqlCommand(sql, localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            });
        }
    }
}
