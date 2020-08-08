namespace ClussPro.Base.Data.Query
{
    public interface IDropSchema
    {
        string Schema { get; set; }
        void Execute(ITransaction transaction);
    }
}
