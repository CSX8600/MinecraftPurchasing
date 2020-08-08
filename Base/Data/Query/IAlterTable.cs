namespace ClussPro.Base.Data.Query
{
    public interface IAlterTable
    {
        string Schema { get; set; }
        string Table { get; set; }

        void AddForeignKey(string constraintName, string foreignKeyName, string parentSchema, string parentTable, string parentField, ITransaction transaction = null);
        void DropConstraint(string constraintName, ITransaction transaction = null);
    }
}
