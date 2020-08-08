using ClussPro.Base.Data;
using ClussPro.Base.Data.Query;
using System;

namespace ClussPro.SqlServerProvider
{
    public abstract class BaseTransactionalQuery
    {
        protected object CheckedTransactionExecuteWithResult(ITransaction transaction, Func<Transaction, object> work)
        {
            if (transaction != null && !(transaction is Transaction))
            {
                throw new InvalidCastException("Expected Transaction of type " + typeof(Transaction).ToString() + ", got " + transaction.GetType().ToString());
            }

            Transaction localTransaction = null;
            try
            {
                if (transaction == null)
                {
                    localTransaction = (Transaction)SQLProviderFactory.GenerateTransaction();
                }
                else
                {
                    localTransaction = (Transaction)transaction;
                }

                object result = work(localTransaction);

                if (transaction == null)
                {
                    localTransaction.Commit();
                }

                return result;
            }
            finally
            {
                if (transaction == null && localTransaction != null && !localTransaction.IsDisposed)
                {
                    transaction.Dispose();
                }
            }
        }

        protected void CheckedTransactionExecute(ITransaction transaction, Action<Transaction> work)
        {
            CheckedTransactionExecuteWithResult(transaction, (trans) =>
            {
                work(trans);
                return null;
            });
        }
    }
}
