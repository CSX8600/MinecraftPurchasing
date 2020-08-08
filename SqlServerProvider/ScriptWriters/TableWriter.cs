using ClussPro.Base.Data;

namespace ClussPro.SqlServerProvider.ScriptWriters
{
    internal static class TableWriter
    {
        public static string WriteTable(Table table)
        {
            string sql = $"[{table.Schema}].[{table.Name}]";

            if (!string.IsNullOrEmpty(table.Alias))
            {
                sql += $" AS {table.Alias}";
            }

            return sql;
        }
    }
}
