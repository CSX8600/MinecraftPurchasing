namespace ClussPro.Base.Data.Query
{
    public interface IDropTable
    {
        string Schema { get; set; }
        string Table { get; set; }
        void Execute(ITransaction transaction);
    }
}
