using ClussPro.Base.Data.Operand;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClussPro.ObjectBasedFramework.DataSearch
{
    public class LongSearchCondition : SearchCondition
    {
        public LongSearchCondition(Type dataObjectType) : base(dataObjectType) { }

        public long? Value { get; set; }
        public List<long> List { get; set; } = new List<long>();

        protected override IOperand GetRightOperand()
        {
            if (List != null && List.Any())
            {
                return (CSV<long>)List;
            }

            return new Literal(Value);
        }
    }
}
