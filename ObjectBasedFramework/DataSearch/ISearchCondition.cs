using ClussPro.Base.Data.Conditions;
using System;
using System.Collections.Generic;

namespace ClussPro.ObjectBasedFramework.DataSearch
{
    public interface ISearchCondition
    {
        IEnumerable<string> GetFieldPaths();
        ICondition GetCondition(Dictionary<string, string> tableAliasesByFieldPath);
    }
}
