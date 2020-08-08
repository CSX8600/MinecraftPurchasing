using ClussPro.Base.Data.Operand;

namespace ClussPro.Base.Data.Conditions
{
    public class Condition : ICondition
    {
        public IOperand Left { get; set; }
        public ConditionTypes ConditionType { get; set; }
        public IOperand Right { get; set; }

        public enum ConditionTypes
        {
            Equal,
            NotEqual,
            Greater,
            Less,
            GreaterEqual,
            LessEqual,
            List,
            NotList
        }
    }
}
