using ClussPro.Base.Data.Conditions;
using System.Collections.Generic;
using System.Linq;

namespace ClussPro.ObjectBasedFramework.DataSearch
{
    public class SearchConditionGroup : ISearchCondition
    {
        public SearchConditionGroup() { }
        public SearchConditionGroup(SearchConditionGroupTypes searchConditionGroupType, params ISearchCondition[] searchConditions)
        {
            SearchConditionGroupType = searchConditionGroupType;
            SearchConditions = searchConditions.ToList();
        }

        public SearchConditionGroupTypes SearchConditionGroupType { get; set; }
        public List<ISearchCondition> SearchConditions { get; set; }

        public enum SearchConditionGroupTypes
        {
            And,
            Or
        }

        public ICondition GetCondition(Dictionary<string, string> tableAliasesByFieldPath)
        {
            ConditionGroup conditionGroup = new ConditionGroup();
            if (SearchConditionGroupType == SearchConditionGroupTypes.And)
            {
                conditionGroup.ConditionGroupType = ConditionGroup.ConditionGroupTypes.And;
            }
            else
            {
                conditionGroup.ConditionGroupType = ConditionGroup.ConditionGroupTypes.Or;
            }

            conditionGroup.Conditions.AddRange(SearchConditions.Select(sc => sc.GetCondition(tableAliasesByFieldPath)));

            return conditionGroup;
        }

        public IEnumerable<string> GetFieldPaths()
        {
            return SearchConditions.SelectMany(sc => sc.GetFieldPaths());
        }
    }
}
