using ClussPro.Base.Data.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClussPro.Base.Data.Query
{
    public class Select
    {
        public IOperand SelectOperand { get; set; }
        public string Alias { get; set; }

        public static implicit operator Select(string field)
        {
            string alias = null;
            if (field.Contains("."))
            {
                alias = field.Substring(0, field.IndexOf("."));
            }
            
            return new Select()
            {
                SelectOperand = new Field()
                {
                    TableAlias = alias,
                    FieldName = field
                }
            };
        }
    }
}
