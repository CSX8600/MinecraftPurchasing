using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClussPro.Base.Data.Conditions;
using ClussPro.Base.Data.Operand;

namespace ClussPro.ObjectBasedFramework.DataSearch
{
    public abstract class SearchCondition : ISearchCondition
    {
        public SearchCondition(Type dataObjectType)
        {
            DataObjectType = dataObjectType;
        }

        public Type DataObjectType { get; set; }

        public string Field { get; set; }

        public enum SearchConditionTypes
        {
            Equals,
            NotEquals,
            Greater,
            GreaterEquals,
            Less,
            LessEquals,
            List,
            NotList
        }

        public SearchConditionTypes SearchConditionType { get; set; }

        protected abstract IOperand GetRightOperand();

        public ICondition GetCondition(Dictionary<string, string> tableAliasesByFieldPath)
        {
            Condition condition = new Condition();
            condition.Left = (Base.Data.Operand.Field)Field;
            
            switch(SearchConditionType)
            {
                case SearchConditionTypes.Equals:
                    condition.ConditionType = Condition.ConditionTypes.Equal;
                    break;
                case SearchConditionTypes.Greater:
                    condition.ConditionType = Condition.ConditionTypes.Greater;
                    break;
                case SearchConditionTypes.GreaterEquals:
                    condition.ConditionType = Condition.ConditionTypes.GreaterEqual;
                    break;
                case SearchConditionTypes.Less:
                    condition.ConditionType = Condition.ConditionTypes.Less;
                    break;
                case SearchConditionTypes.LessEquals:
                    condition.ConditionType = Condition.ConditionTypes.LessEqual;
                    break;
                case SearchConditionTypes.List:
                    condition.ConditionType = Condition.ConditionTypes.List;
                    break;
                case SearchConditionTypes.NotList:
                    condition.ConditionType = Condition.ConditionTypes.NotList;
                    break;
            }

            condition.Right = GetRightOperand();

            return condition;
        }

        public IEnumerable<string> GetFieldPaths()
        {
            if (!Field.Contains("."))
            {
                yield return "";
                yield break;
            }

            yield return Field.Substring(0, Field.LastIndexOf('.'));
        }
    }
}
