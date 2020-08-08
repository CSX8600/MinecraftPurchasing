using ClussPro.Base.Data.Conditions;
using System.Collections.Generic;
using System.Data;

namespace ClussPro.Base.Data.Query
{
    public interface ISelectQuery
    {
        List<Select> SelectList { get; set; }
        Table Table { get; set; }
        ICondition WhereCondition { get; set; }
        List<Join> JoinList { get; set; }

        DataTable Execute(ITransaction transaction);
    }
}
