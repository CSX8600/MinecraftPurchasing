using ClussPro.Base.Data.Conditions;

namespace ClussPro.Base.Data
{
    public class Join
    {
        public Table Table { get; set; }
        public JoinTypes JoinType { get; set; }
        public ICondition Condition { get; set; }

        public enum JoinTypes
        {
            Inner,
            Left
        }
    }
}
