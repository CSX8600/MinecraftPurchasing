using ClussPro.Base.Data;
using ClussPro.Base.Data.Query;
using System;
using System.Data.SqlClient;
using System.Text;

namespace ClussPro.SqlServerProvider
{
    public class AlterTableQuery : BaseTransactionalQuery, IAlterTable
    {
        public string Schema { get; set; }
        public string Table { get; set; }

        public void AddForeignKey(string constraintName, string foreignKeyName, string parentSchema, string parentTable, string parentField, ITransaction transaction = null)
        {
            CheckedTransactionExecute(transaction, (localTransaction) =>
            {
                StringBuilder sqlBuilder = new StringBuilder("ALTER TABLE ");
                sqlBuilder.Append($"[{Schema}].[{Table}] ADD CONSTRAINT {constraintName} FOREIGN KEY");
                sqlBuilder.Append($"({foreignKeyName}) REFERENCES [{parentSchema}].[{parentTable}] ({parentField})");

                using (SqlCommand command = new SqlCommand(sqlBuilder.ToString(), localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    command.ExecuteNonQuery();
                }
            });
        }

        public void DropConstraint(string constraintName, ITransaction transaction = null)
        {
            CheckedTransactionExecute(transaction, localTransaction =>
            {
                StringBuilder sqlBuilder = new StringBuilder("ALTER TABLE ");
                sqlBuilder.Append($"[{Schema}].[{Table}] DROP CONSTRAINT {constraintName}");

                using (SqlCommand command = new SqlCommand(sqlBuilder.ToString(), localTransaction.SQLTransaction.Connection, localTransaction.SQLTransaction))
                {
                    command.ExecuteNonQuery();
                }
            });
        }
    }
}
