using ClussPro.Base.Data.Query;

namespace ClussPro.Base.Data
{
    public interface ISQLProvider
    {
        ISelectQuery GetSelectQuery();
        IDeleteQuery GetDeleteQuery();
        IUpdateQuery GetUpdateQuery();
        IInsertQuery GetInsertQuery();
        ITransaction GenerateTransaction();
        ICreateSchema GetCreateSchemaQuery();
        IDropSchema GetDropSchemaQuery();
        ICreateTable GetCreateTableQuery();
        IDropTable GetDropTableQuery();
        IAlterTable GetAlterTableQuery();
    }
}
