namespace ClussPro.Base.Data
{
    public class Table
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }

        public Table() { }
        public Table(string schema, string name, string alias = null)
        {
            Schema = schema;
            Name = name;
            Alias = alias;
        }
    }
}
