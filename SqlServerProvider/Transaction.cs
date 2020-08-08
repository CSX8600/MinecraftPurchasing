using ClussPro.Base.Data.Query;
using System.Data.SqlClient;

namespace ClussPro.SqlServerProvider
{
    public class Transaction : ITransaction
    {
        private bool isDisposed;
        private bool isActive = true;
        internal SqlTransaction SQLTransaction { get; set; }

        public bool IsActive => isActive;

        public void Commit()
        {
            SqlConnection backingConnection = SQLTransaction.Connection;
            SQLTransaction?.Commit();
            backingConnection.Close();
            backingConnection.Dispose();
            isActive = false;
            Dispose();
        }

        public void Rollback()
        {
            SqlConnection backingConnection = SQLTransaction.Connection;
            SQLTransaction?.Rollback();
            backingConnection.Close();
            backingConnection.Dispose();
            isActive = false;
            Dispose();
        }

        public void Dispose()
        {
            SqlConnection backingConnection = SQLTransaction?.Connection;

            if (isActive)
            {
                SQLTransaction?.Rollback();
            }

            backingConnection?.Close();
            backingConnection?.Dispose();
            SQLTransaction?.Dispose();
            isDisposed = true;
        }

        public bool IsDisposed => isDisposed;
    }
}
