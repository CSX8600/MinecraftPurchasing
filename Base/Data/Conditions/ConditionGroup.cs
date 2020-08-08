using System.Collections.Generic;

namespace ClussPro.Base.Data.Conditions
{
    public class ConditionGroup : ICondition
    {
        public ConditionGroupTypes ConditionGroupType { get; set; }
        public List<ICondition> Conditions { get; set; } = new List<ICondition>();
        public enum ConditionGroupTypes
        {
            And,
            Or
        }
    }
}
