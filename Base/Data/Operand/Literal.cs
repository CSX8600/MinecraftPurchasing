namespace ClussPro.Base.Data.Operand
{
    public class Literal : IOperand
    {
        public object Value { get; private set; }
        public Literal() { }
        public Literal(object value)
        {
            Value = value;
        }
    }
}
