using ClussPro.Base.Data;
using ClussPro.Base.Data.Query;
using System;
using System.Data.SqlClient;

namespace ClussPro.SqlServerProvider
{
    public class MSSqlServerProvider : ISQLProvider
    {
        public ITransaction GenerateTransaction()
        {
            Transaction transaction = new Transaction();

            SqlConnection connection = new SqlConnection(MSSqlServerProviderConfig.ConnectionString);
            connection.Open();
            SqlTransaction sqlTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, Guid.NewGuid().ToString("N"));
            transaction.SQLTransaction = sqlTransaction;

            return transaction;
        }

        public IDeleteQuery GetDeleteQuery()
        {
            return new DeleteQuery();
        }

        public IInsertQuery GetInsertQuery()
        {
            return new InsertQuery();
        }

        public ISelectQuery GetSelectQuery()
        {
            return new SelectQuery();
        }

        public IUpdateQuery GetUpdateQuery()
        {
            return new UpdateQuery();
        }

        public ICreateSchema GetCreateSchemaQuery()
        {
            return new CreateSchemaQuery();
        }

        public IDropSchema GetDropSchemaQuery()
        {
            return new DropSchemaQuery();
        }

        public ICreateTable GetCreateTableQuery()
        {
            return new CreateTableQuery();
        }

        public IDropTable GetDropTableQuery()
        {
            return new DropTableQuery();
        }

        public IAlterTable GetAlterTableQuery()
        {
            return new AlterTableQuery();
        }
    }
}
