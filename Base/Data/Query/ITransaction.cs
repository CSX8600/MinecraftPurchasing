using System;

namespace ClussPro.Base.Data.Query
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
        bool IsActive { get; }
    }
}
