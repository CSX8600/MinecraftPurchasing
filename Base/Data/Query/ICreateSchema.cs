namespace ClussPro.Base.Data.Query
{
    public interface ICreateSchema
    {
        string SchemaName { get; set; }

        void Execute(ITransaction transaction);
    }
}
