using System.Collections.Generic;

namespace ClussPro.Base.Data.Operand
{
    public class CSV : IOperand
    {
        protected CSV() { }
        public virtual System.Collections.IEnumerable Values { get; set; }

        public static implicit operator CSV(List<object> enumerable)
        {
            CSV csv = new CSV();
            csv.Values = enumerable;
            return csv;
        }
    }

    public class CSV<T> : CSV
    {
        public static implicit operator CSV<T>(List<T> enumerable)
        {
            CSV<T> csv = new CSV<T>();
            csv.Values = enumerable;
            return csv;
        }
    }
}
